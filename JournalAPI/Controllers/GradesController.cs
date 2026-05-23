using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace JournalApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GradesController : ControllerBase
    {
        [Authorize(Roles = "teacher")]
        [HttpPost]
        public IActionResult AddGrade([FromBody] AddGradeRequest request)
        {
            using var connection = new SqliteConnection("Data Source=Data/диплом.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Grades (student_id, journal_id, grade, date)
                VALUES ($studentId, $journalId, $grade, $date)
            ";

            command.Parameters.AddWithValue("$studentId", request.StudentId);
            command.Parameters.AddWithValue("$journalId", request.JournalId);
            command.Parameters.AddWithValue("$grade", request.Grade);
            command.Parameters.AddWithValue("$date",
    string.IsNullOrWhiteSpace(request.Date)
        ? DateTime.Now.ToString("yyyy-MM-dd")
        : request.Date
);

            command.ExecuteNonQuery();

            return Ok(new
            {
                message = "Оценка добавлена"
            });
        }
        [Authorize(Roles = "teacher")]
        [HttpGet("journal/{journalId}")]
        public IActionResult GetGradesByJournal(int journalId)
        {
            var grades = new List<object>();

            using var connection = new SqliteConnection("Data Source=Data/диплом.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
        SELECT 
            g.id,
            st.name AS student,
            g.grade,
            g.date
        FROM Grades g
        JOIN Students st ON g.student_id = st.id
        WHERE g.journal_id = $journalId
        ORDER BY st.name, g.date DESC
    ";

            command.Parameters.AddWithValue("$journalId", journalId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                grades.Add(new
                {
                    id = reader["id"],
                    student = reader["student"],
                    grade = reader["grade"],
                    date = reader["date"]
                });
            }

            return Ok(grades);
        }
        [Authorize(Roles = "teacher")]
        [HttpPut("{id}")]
        public IActionResult UpdateGrade(int id, [FromBody] UpdateGradeRequest request)
        {
            using var connection = new SqliteConnection("Data Source=Data/диплом.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
        UPDATE Grades
        SET grade = $grade,
            date = $date
        WHERE id = $id
    ";

            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$grade", request.Grade);
            command.Parameters.AddWithValue("$date",
    string.IsNullOrWhiteSpace(request.Date)
        ? DateTime.Now.ToString("yyyy-MM-dd")
        : request.Date
);

            var rows = command.ExecuteNonQuery();

            if (rows == 0)
                return NotFound("Оценка не найдена");

            return Ok(new { message = "Оценка обновлена" });
        }

        public class UpdateGradeRequest
        {
            public int Grade { get; set; }
            public string Date { get; set; } = "";
        }
    }

    public class AddGradeRequest
    {
        public int StudentId { get; set; }
        public int JournalId { get; set; }
        public int Grade { get; set; }
        public string Date { get; set; } = "";
    }
}