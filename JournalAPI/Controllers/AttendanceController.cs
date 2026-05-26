using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Security.Claims;

namespace JournalApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AttendanceController : ControllerBase
    {
        [Authorize(Roles = "teacher")]
        [HttpPost]
        public IActionResult AddAttendance([FromBody] AddAttendanceRequest request)
        {
            using var connection = new SqliteConnection("Data Source=Data/диплом.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Attendance (student_id, journal_id, date, status)
                VALUES ($studentId, $journalId, $date, $status)
            ";

            command.Parameters.AddWithValue("$studentId", request.StudentId);
            command.Parameters.AddWithValue("$journalId", request.JournalId);
            command.Parameters.AddWithValue("$date", string.IsNullOrWhiteSpace(request.Date)
        ? DateTime.Now.ToString("yyyy-MM-dd")
        : request.Date
);
            command.Parameters.AddWithValue("$status", request.Status);

            command.ExecuteNonQuery();

            return Ok(new { message = "Посещаемость сохранена" });
        }

        [Authorize(Roles = "teacher")]
        [HttpGet("journal/{journalId}")]
        public IActionResult GetAttendanceByJournal(int journalId)
        {
            var attendance = new List<object>();

            using var connection = new SqliteConnection("Data Source=Data/диплом.db");
            connection.Open();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    a.id,
                    st.name AS student,
                    a.date,
                    a.status
                FROM Attendance a
                JOIN Students st ON a.student_id = st.id
                WHERE a.journal_id = $journalId
AND EXISTS (
    SELECT 1
    FROM Journals j
    WHERE j.id = a.journal_id
    AND j.user_id = $userId
)
                ORDER BY st.name, a.date DESC
            ";

            command.Parameters.AddWithValue("$journalId", journalId);
            command.Parameters.AddWithValue("$userId", userId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                attendance.Add(new
                {
                    id = reader["id"],
                    student = reader["student"],
                    date = reader["date"],
                    status = reader["status"]
                });
            }

            return Ok(attendance);
        }
    }

    public class AddAttendanceRequest
    {
        public int StudentId { get; set; }
        public int JournalId { get; set; }
        public string? Date { get; set; }
        public string Status { get; set; } = "";
    }
}