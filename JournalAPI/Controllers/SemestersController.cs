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
        [Authorize(Roles = "admin")]
        [HttpPost]
        public IActionResult AddSemester([FromBody] SemesterRequest request)
        {
            using var connection =
                new SqliteConnection("Data Source=Data/диплом.db");

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
        INSERT INTO Semesters
        (name, start_date, end_date)
        VALUES
        ($name, $startDate, $endDate)
    ";

            command.Parameters.AddWithValue("$name", request.Name);
            command.Parameters.AddWithValue("$startDate", request.Start_date);
            command.Parameters.AddWithValue("$endDate", request.End_date);

            command.ExecuteNonQuery();

            return Ok(new
            {
                message = "Семестр добавлен"
            });
        }
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public IActionResult UpdateSemester(int id,[FromBody] SemesterRequest request)
        {
            using var connection =
                new SqliteConnection("Data Source=Data/диплом.db");

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
        UPDATE Semesters
        SET
            name = $name,
            start_date = $startDate,
            end_date = $endDate
        WHERE id = $id
    ";

            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$name", request.Name);
            command.Parameters.AddWithValue("$startDate", request.Start_date);
            command.Parameters.AddWithValue("$endDate", request.End_date);

            var rows = command.ExecuteNonQuery();

            if (rows == 0)
                return NotFound();

            return Ok(new
            {
                message = "Семестр изменен"
            });
        }
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteSemester(int id)
        {
            using var connection =
                new SqliteConnection("Data Source=Data/диплом.db");

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
        DELETE FROM Semesters
        WHERE id = $id
    ";

            command.Parameters.AddWithValue("$id", id);

            var rows = command.ExecuteNonQuery();

            if (rows == 0)
                return NotFound();

            return Ok(new
            {
                message = "Семестр удален"
            });
        }
    }
    public class SemesterRequest
    {
        public string Name { get; set; } = "";
        public string Start_date { get; set; } = "";
        public string End_date { get; set; } = "";
    }
}