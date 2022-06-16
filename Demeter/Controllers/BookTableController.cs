using Demeter.DTOs;
using FluentEmail.Core;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
namespace Demeter.Controllers;

[ApiController]
[Route("[controller]")]
public class BookTableController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IFluentEmail _mail;

    public BookTableController(IConfiguration configuration, IFluentEmail mail)
    {
        _mail = mail;
        _configuration = configuration;
    }

    [HttpPost]
    public IActionResult Post(BookTableDto request)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        connection.Open();
        if (connection.State == ConnectionState.Open)
        {
            var rows_affected = 0;
            using var command = new MySqlCommand();
            command.Connection = connection;

            string queryString = @"INSERT INTO booktable (Name, Email, Phone, Date, Time, NumPeople, Message) VALUES (@Name, @Email, @Phone, @Date, @Time, @NumPeople, @Message);
                                select last_insert_id();";

            command.CommandText = queryString;
            command.Parameters.AddWithValue("@Name", request.Name);
            command.Parameters.AddWithValue("@Email", request.Email);
            command.Parameters.AddWithValue("@Phone", request.Phone);
            command.Parameters.AddWithValue("@Date", request.Date);
            command.Parameters.AddWithValue("@Time", request.Time);
            command.Parameters.AddWithValue("@NumPeople", request.NumPeople);
            command.Parameters.AddWithValue("@Message", request.Message);

            try
            {
                rows_affected = command.ExecuteNonQuery();
                if (rows_affected > 0)
                {
                    var template = @"
                    Hi @Model.Name!
                    <br/><br/>
                    You have made a reservation at our restaurant at @Model.Time on @Model.Date. To be sure you want to reserve a table please send yes, or no if you change your mind.
                    <br/><br/>
                    Thank you very much for trusting our restaurant.";

                    _mail.To(request.Email)
                     .Subject("Booking Confirmation")
                     .UsingTemplate(template, request);

                    _mail.Send();

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