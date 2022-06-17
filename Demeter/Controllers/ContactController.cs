using Demeter.DTOs;
using FluentEmail.Core;
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
    public IActionResult Post(ContactDro request)
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
}