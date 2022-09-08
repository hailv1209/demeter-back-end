using System.Data;
using System.Security.Cryptography;
using System.Text;
using Demeter.DTOs;
using Demeter.Entities;
using Demeter.Enums;
using Demeter.Services;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace Demeter.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly TokenService _service;
    public AccountController(IConfiguration configuration)
    {
        _configuration = configuration;
        _service = new TokenService(_configuration);
    }

    [HttpPost("login")]
    public ActionResult<UserDto> Login(LoginDto request)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        connection.Open();
        if (connection.State == ConnectionState.Open)
        {
            var user = GetUser(connection, request.Username!);
            if (user == null)
            {
                return BadRequest();
            }
            using var hmac = new HMACSHA512(user.PasswordSalt!);

            var computerHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password!));

            for (int i = 0; i < computerHash.Length; i++)
            {
                if (computerHash[i] != user.PasswordHash![i]) return Unauthorized();
            }
            connection.Close();
            var userToReturn = new UserDto
            {
                DisplayName = user.DisplayName,
                Username = user.Username,
                Role = user.Role,
                Token = _service.GetToken(user)
            };
            return Ok(userToReturn);
        }
        connection.Close();
        return BadRequest();
    }

    [HttpPost("register")]
    public ActionResult<bool> Register(RegisterDto request)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        connection.Open();
        if (connection.State == ConnectionState.Open)
        {
            var user = GetUser(connection, request.Username!);
            if (user != null)
            {
                connection.Close();
                return BadRequest("Username already exists");
            }
            using var hmac = new HMACSHA512();

            var computerHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password!));

            var newUser = new User
            {
                Username = request.Username,
                DisplayName = request.DisplayName,
                PasswordHash = computerHash,
                PasswordSalt = hmac.Key,
                Role = request.Role,
            };

            var result = CreateUser(connection, newUser);
            if (result)
            {
                connection.Close();
                return Ok(true);
            }
            connection.Close();
            return BadRequest();
        }
        connection.Close();
        return BadRequest();
    }


    private User? GetUser(MySqlConnection connection, string username)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT * FROM user WHERE Username = @username;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@username", username);
        try
        {
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var user = new User
                        {
                            Id = reader.GetInt32("Id"),
                            Username = reader.GetString("Username"),
                            DisplayName = reader.GetString("DisplayName"),
                            PasswordHash = (byte[]) reader["PasswordHash"],
                            PasswordSalt = (byte[]) reader["PasswordSalt"],
                            Role = (Role) reader.GetInt32("Role")
                        };
                        return user;
                    }
                    return null;
                }
                return null;
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    private bool CreateUser(MySqlConnection connection, User user)
    {
        var rows_affected = 0;
            using var command = new MySqlCommand();
            command.Connection = connection;

            string queryString = @"INSERT INTO user (Username, DisplayName, PasswordHash, PasswordSalt, Role) VALUES (@Username, @DisplayName, @PasswordHash, @PasswordSalt, @Role);
                                select last_insert_id();";

            command.CommandText = queryString;
            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@DisplayName", user.DisplayName);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@PasswordSalt", user.PasswordSalt);
            command.Parameters.AddWithValue("@Role", user.Role);

            try
            {
                rows_affected = command.ExecuteNonQuery();
                if (rows_affected > 0)
                {
                    
                    return true;
                }
                return false;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
    }
}