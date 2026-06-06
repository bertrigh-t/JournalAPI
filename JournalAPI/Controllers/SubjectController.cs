using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace JournalAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SubjectController : ControllerBase
    {
        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult GetSubjects()
        {
            var subjects = new List<object>();

            using var connection =
                new SqliteConnection("Data Source=Data/диплом.db");

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
        SELECT id, name
        FROM Subjects
        ORDER BY name
    ";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                subjects.Add(new
                {
                    Id = reader["id"],
                    Name = reader["name"]
                });
            }

            return Ok(subjects);
        }
        [Authorize(Roles = "admin")]
        [HttpPost]
        public IActionResult AddSubject([FromBody] SubjectRequest request)
        {
            using var connection =
                new SqliteConnection("Data Source=Data/диплом.db");

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
        INSERT INTO Subjects(name)
        VALUES($name)
    ";

            command.Parameters.AddWithValue(
                "$name",
                request.Name
            );

            command.ExecuteNonQuery();

            return Ok();
        }
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public IActionResult UpdateSubject(int id, [FromBody] SubjectRequest request)
        {
            using var connection =
                new SqliteConnection("Data Source=Data/диплом.db");

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
        UPDATE Subjects
        SET name = $name
        WHERE id = $id
    ";

            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$name", request.Name);

            command.ExecuteNonQuery();

            return Ok();
        }
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteSubject(int id)
        {
            using var connection =
                new SqliteConnection("Data Source=Data/диплом.db");

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
        DELETE FROM Subjects
        WHERE id = $id
    ";

            command.Parameters.AddWithValue("$id", id);

            command.ExecuteNonQuery();

            return Ok();
        }
    }
    public class SubjectRequest
    {
        public string Name { get; set; } = "";
    }
}
