using System.Data;
using Demeter.DTOs;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace Demeter.Controllers;

[Route("[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class BookingConfirmationController : Controller
{
    private readonly ILogger<BookingConfirmationController> _logger;
    private readonly IConfiguration _configuration;

    public BookingConfirmationController(ILogger<BookingConfirmationController> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public IActionResult Index(int bookTableId)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        connection.Open();
        if (connection.State == ConnectionState.Open)
        {
            var booktable = GetBookTable(connection, bookTableId);
            if (booktable == null) {
                connection.Close();
                return BadRequest();
            }
            var result = UpdateBookTable(connection, booktable!);
            if (result) {
                connection.Close();
                return View(booktable);
            }
            connection.Close();
            return BadRequest();
        }
        connection.Close();
        return BadRequest();
    }

    private BookTableResponseDto? GetBookTable(MySqlConnection connection, int id)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT * FROM booktable WHERE Id = @id;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@id", id);
        try
        {
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var bookTable = new BookTableResponseDto
                        {
                            Id = reader.GetInt32("Id"),
                            Name = reader.GetString("Name"),
                            Email = reader.GetString("Email"),
                            Phone = reader.GetString("Phone"),
                            Date = reader.GetInt32("Date"),
                            Time = reader.GetString("Time"),
                            NumPeople = reader.GetInt32("NumPeople"),
                            Message = reader.GetString("Message"),
                            Status = reader.GetInt32("Status")
                        };
                        return bookTable;
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

    private bool UpdateBookTable(MySqlConnection connection, BookTableResponseDto bookTable)
    {
        var rows_affected = 0;
            using var command = new MySqlCommand();
            command.Connection = connection;

            string queryString = @"UPDATE booktable SET Status=@Status WHERE ID=@Id;";

            command.CommandText = queryString;
            command.Parameters.AddWithValue("@Status", 1);
            command.Parameters.AddWithValue("@Id", bookTable.Id);

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