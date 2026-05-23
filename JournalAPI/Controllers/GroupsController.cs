using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Security.Claims;

namespace JournalApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GroupsController : ControllerBase
    {
        [Authorize(Roles = "teacher")]
        [HttpGet("{id}/students")]
        public IActionResult GetStudents(int id)
        {
            using var connection = new SqliteConnection("Data Source=Data/диплом.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, name
                FROM Students
                WHERE group_id = $groupId
            ";

            command.Parameters.AddWithValue("$groupId", id);

            var students = new List<object>();

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                students.Add(new
                {
                    id = reader["id"],
                    name = reader["name"]
                });
            }

            return Ok(students);
        }
        [Authorize(Roles = "teacher")]
        [HttpGet]
        public IActionResult GetGroups()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var groups = new List<object>();

            using var connection = new SqliteConnection("Data Source=Data/диплом.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
    SELECT id, name, teacher_id
    FROM Groups
";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                int groupTeacherId = Convert.ToInt32(reader["teacher_id"]);

                groups.Add(new
                {
                    Id = reader["id"],
                    Name = reader["name"]
                });
            }
            return Ok(groups);
        }
        [Authorize(Roles = "teacher")]
        [HttpGet("{groupId}/journals")]
        public IActionResult GetJournals(int groupId)
        {
            var journals = new List<object>();

            using var connection = new SqliteConnection("Data Source=Data/диплом.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
        SELECT j.id, s.name AS subject
        FROM Journals j
        JOIN Subjects s ON j.subject_id = s.id
        WHERE j.group_id = $groupId
    ";
            command.Parameters.AddWithValue("$groupId", groupId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                journals.Add(new
                {
                    Id = reader["id"],
                    Subject = reader["subject"]
                });
            }

            return Ok(journals);
        }
    }
}