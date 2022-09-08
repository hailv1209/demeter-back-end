using Demeter.DTOs;
using FluentEmail.Core;
using Microsoft.AspNetCore.Authorization;
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
                    var bookTable = GetBookTable(connection, request);
                    if (bookTable == null)
                    {
                        connection.Close();
                        return BadRequest();
                    }
                    string baseUrl = GetBaseUrl();
                    var template = @$"
                    Hi @Model.Name!
                    <br/><br/>
                    You have made a reservation at our restaurant at @Model.Time on @Model.Date. To be sure you want to reserve a table, please click the following link. 
                    <a href='{baseUrl}/BookingConfirmation?bookTableId={bookTable.Id}'>Here</a>
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

    private string GetBaseUrl()
    {
        var request = HttpContext.Request;

        var host = request.Host.ToUriComponent();

        var pathBase = request.PathBase.ToUriComponent();

        return $"{request.Scheme}://{host}{pathBase}";
    }

    private BookTableResponseDto? GetBookTable(MySqlConnection connection, BookTableDto request)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT * FROM booktable WHERE Status = @Status AND Email = @Email AND Phone = @Phone ORDER BY Id DESC;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@Status", 0);
        command.Parameters.AddWithValue("@Email", request.Email);
        command.Parameters.AddWithValue("@Phone", request.Phone);
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

    [HttpGet]
    [Authorize]
    public IActionResult Get([FromQuery] BookTableRequestDto request)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        connection.Open();
        if (connection.State == ConnectionState.Open)
        {
            var response = new PaginationResponseDto<BookTableResponseDto>();
            var count = CountListBookTable(connection, request);
            if (count == null || count == 0)
            {
                response.Data = new List<BookTableResponseDto>();
                response.Total = 0;
                connection.Close();
                return Ok(response);
            }
            var list = GetListBookTable(connection, request);
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

    private List<BookTableResponseDto>? GetListBookTable(MySqlConnection connection, BookTableRequestDto request)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT * FROM booktable ORDER BY Id DESC LIMIT @Limit OFFSET @Offset;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@Limit", request.PageSize);
        command.Parameters.AddWithValue("@Offset", request.PageSize * (request.PageNumber - 1));
        var response = new List<BookTableResponseDto>();
        try
        {
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var table = new BookTableResponseDto
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
                        response.Add(table);
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

    private int? CountListBookTable(MySqlConnection connection, BookTableRequestDto request)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT COUNT(Id) as NumberOfBookTables FROM booktable;";

        command.CommandText = queryString;
        try
        {
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var count = reader.GetInt32("NumberOfBookTables");
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