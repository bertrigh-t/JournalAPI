using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace JournalApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        [HttpGet("teachers")]
        public IActionResult GetTeachers()
        {
            var teachers = new List<object>();

            using var connection = new SqliteConnection("Data Source=Data/диплом.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, login
                FROM Users
                WHERE role = 'teacher'
            ";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                teachers.Add(new
                {
                    id = reader["id"],
                    login = reader["login"]
                });
            }

            return Ok(teachers);
        }

        [HttpGet("groups")]
        public IActionResult GetGroups()
        {
            var groups = new List<object>();

            using var connection = new SqliteConnection("Data Source=Data/диплом.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, name
                FROM Groups
            ";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                groups.Add(new
                {
                    id = reader["id"],
                    name = reader["name"]
                });
            }

            return Ok(groups);
        }

        [HttpGet("subjects")]
        public IActionResult GetSubjects()
        {
            var subjects = new List<object>();

            using var connection = new SqliteConnection("Data Source=Data/диплом.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, name
                FROM Subjects
            ";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                subjects.Add(new
                {
                    id = reader["id"],
                    name = reader["name"]
                });
            }

            return Ok(subjects);
        }

        [HttpPost("journals")]
        public IActionResult CreateJournal([FromBody] CreateJournalRequest request)
        {
            using var connection = new SqliteConnection("Data Source=Data/диплом.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Journals (
                    group_id,
                    subject_id,
                    user_id
                )
                VALUES (
                    $groupId,
                    $subjectId,
                    $userId
                )
            ";

            command.Parameters.AddWithValue("$groupId", request.GroupId);
            command.Parameters.AddWithValue("$subjectId", request.SubjectId);
            command.Parameters.AddWithValue("$userId", request.UserId);

            command.ExecuteNonQuery();

            return Ok(new
            {
                message = "Журнал создан"
            });
        }
    }

    public class CreateJournalRequest
    {
        public int GroupId { get; set; }
        public int SubjectId { get; set; }
        public int UserId { get; set; }
    }
}