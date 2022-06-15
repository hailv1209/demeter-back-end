using Demeter.DTOs;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
namespace Demeter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookTableController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public BookTableController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult Post(BookTableDto request)
        {
            var sqlconnectstring =  _configuration.GetConnectionString("DefaultConnection");
            Console.WriteLine(sqlconnectstring);
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
                        connection.Close();
                        return Ok();
                    }
                    return BadRequest();

                }
                catch (System.Exception)
                {
                    return BadRequest();
                }
            }
            return BadRequest();
        }
    }
}