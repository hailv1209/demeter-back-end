using Demeter.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
namespace Demeter.Controllers;

[ApiController]
[Route("[controller]")]
public class ContactController : ControllerBase
{
    private readonly IConfiguration _configuration;
   

    public ContactController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost]
    public IActionResult Post(ContactDto request)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        connection.Open();
        if (connection.State == ConnectionState.Open)
        {
            var rows_affected = 0;
            using var command = new MySqlCommand();
            command.Connection = connection;

            string queryString = @"INSERT INTO contact (Name, Email, Subject, Message) VALUES (@Name, @Email,@Subject, @Message);
                                select last_insert_id();";

            command.CommandText = queryString;
            command.Parameters.AddWithValue("@Name", request.Name);
            command.Parameters.AddWithValue("@Email", request.Email);
            command.Parameters.AddWithValue("@Subject", request.Subject);
            command.Parameters.AddWithValue("@Message", request.Message);

            try
            {
                rows_affected = command.ExecuteNonQuery();
                if (rows_affected > 0)
                {
                   
                    connection.Close();
                    return Ok();
                }

                connection.Close();
                return BadRequest();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                connection.Close();
                return BadRequest();
            }
        }
        return BadRequest();
    }

    [HttpGet]
    [Authorize]
    public IActionResult Get([FromQuery] ContactRequestDto request)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        connection.Open();
        if (connection.State == ConnectionState.Open)
        {
            var response = new PaginationResponseDto<ContactResponseDto>();
            var count = CountListContact(connection, request);
            if (count == null || count == 0)
            {
                response.Data = new List<ContactResponseDto>();
                response.Total = 0;
                connection.Close();
                return Ok(response);
            }
            var list = GetListContact(connection, request);
            if (list == null)
            {
                connection.Close();
                return BadRequest();
            }
            response.Data = list;
            response.Total = (int)count;
            connection.Close();
            return Ok(response);
        }
        return BadRequest();
    }

    private List<ContactResponseDto>? GetListContact(MySqlConnection connection, ContactRequestDto request)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT * FROM contact ORDER BY Id DESC LIMIT @Limit OFFSET @Offset;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@Limit", request.PageSize);
        command.Parameters.AddWithValue("@Offset", request.PageSize * (request.PageNumber - 1));
        var response = new List<ContactResponseDto>();
        try
        {
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var contact = new ContactResponseDto
                        {
                            Id = reader.GetInt32("Id"),
                            Name = reader.GetString("Name"),
                            Email = reader.GetString("Email"),
                            Subject = reader.GetString("Subject"),                            
                            Message = reader.GetString("Message"),
                        };
                        response.Add(contact);
                    }
                    return response;
                }
                return response;
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    private int? CountListContact(MySqlConnection connection, ContactRequestDto request)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT COUNT(Id) as NumberOfContacts FROM contact;";

        command.CommandText = queryString;
        try
        {
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var count = reader.GetInt32("NumberOfContacts");
                        return count;
                    }
                }
                return 0;
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
}