using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace JournalAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class TeacherSubjectsController : ControllerBase
    {
        [Authorize(Roles = "admin")]
        [HttpGet("{userId}")]
        public IActionResult GetTeacherSubjects(int userId)
        {
            var subjects = new List<object>();

            using var connection =
                new SqliteConnection("Data Source=Data/диплом.db");

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
        SELECT
            ts.id,
            s.id as subject_id,
            s.name
        FROM TeacherSubjects ts
        JOIN Subjects s
            ON s.id = ts.subject_id
        WHERE ts.user_id = $userId
    ";

            command.Parameters.AddWithValue("$userId", userId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                subjects.Add(new
                {
                    Id = reader["id"],
                    SubjectId = reader["subject_id"],
                    Name = reader["name"]
                });
            }

            return Ok(subjects);
        }
        [Authorize(Roles = "admin")]
        [HttpPost]
        public IActionResult AddTeacherSubject([FromBody] AddTeacherSubjectRequest request)
        {
            using var connection =
                new SqliteConnection("Data Source=Data/диплом.db");

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
        INSERT INTO TeacherSubjects
        (
            user_id,
            subject_id
        )
        VALUES
        (
            $userId,
            $subjectId
        )
    ";

            command.Parameters.AddWithValue(
                "$userId",
                request.UserId
            );

            command.Parameters.AddWithValue(
                "$subjectId",
                request.SubjectId
            );

            command.ExecuteNonQuery();

            return Ok();
        }
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteTeacherSubject(int id)
        {
            using var connection =
                new SqliteConnection("Data Source=Data/диплом.db");

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText =
                "DELETE FROM TeacherSubjects WHERE id = $id";

            command.Parameters.AddWithValue("$id", id);

            command.ExecuteNonQuery();

            return Ok();
        }
    }
    public class AddTeacherSubjectRequest
    {
        public int UserId { get; set; }

        public int SubjectId { get; set; }
    }
}
