using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Security.Claims;

namespace JournalApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StudentController : ControllerBase
    {
        [Authorize(Roles = "student")]
        [HttpGet("grades")]
        public IActionResult GetGrades()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var grades = new List<object>();

            using var connection = new SqliteConnection("Data Source=Data/диплом.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
        SELECT 
            sub.name AS subject,
            g.grade,
            g.date
        FROM Grades g
        JOIN Students s ON g.student_id = s.id
        JOIN Journals j ON g.journal_id = j.id
        JOIN Subjects sub ON j.subject_id = sub.id
        WHERE s.user_id = $userId
        ORDER BY g.date DESC
    ";

            command.Parameters.AddWithValue("$userId", userId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                grades.Add(new
                {
                    subject = reader["subject"],
                    grade = reader["grade"],
                    date = reader["date"]
                });
            }

            return Ok(grades);
        }
        [Authorize(Roles = "student")]
        [HttpGet("attendance")]
        public IActionResult GetAttendance()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var attendance = new List<object>();

            using var connection = new SqliteConnection("Data Source=Data/диплом.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
    SELECT 
        sub.name AS subject,
        a.date,
        a.status
    FROM Attendance a
    JOIN Students s ON a.student_id = s.id
    JOIN Journals j ON a.journal_id = j.id
    JOIN Subjects sub ON j.subject_id = sub.id
    WHERE s.user_id = $userId
    ORDER BY a.date DESC
";

            command.Parameters.AddWithValue("$userId", userId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                attendance.Add(new
                {
                    date = reader["date"],
                    status = reader["status"],
                    subject = reader["subject"]
                });
            }

            return Ok(attendance);
        }
    }
}