using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace JournalApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SemestersController : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public IActionResult GetSemesters()
        {
            var semesters = new List<object>();

            using var connection =
                new SqliteConnection("Data Source=Data/диплом.db");

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT id, name, start_date, end_date
                FROM Semesters
            ";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                semesters.Add(new
                {
                    Id = reader["id"],
                    Name = reader["name"],
                    Start_date = reader["start_date"],
                    End_date = reader["end_date"],
                });
            }

            return Ok(semesters);
        }
    }
}