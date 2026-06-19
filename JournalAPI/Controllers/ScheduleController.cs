using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Security.Claims;

namespace JournalApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ScheduleController : ControllerBase
    {
        private const string ConnStr = "Data Source=Data/диплом.db";
        [Authorize]
        [HttpGet]
        public IActionResult GetSchedule(int groupId, int semesterId)
        {
            var result = new List<object>();

            using var connection = new SqliteConnection(ConnStr);
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
    SELECT 
        sc.id,
        sc.day_of_week,
        sc.time,
        sc.number,                -- новое поле
        g.name AS group_name,
        s.name AS subject_name,
        t.name AS teacher_name
    FROM Schedule sc
    JOIN Groups g ON sc.group_id = g.id
    JOIN Subjects s ON sc.subject_id = s.id
    JOIN Teachers t ON sc.teacher_id = t.id
    WHERE sc.group_id = $groupId
      AND sc.semester_id = $semesterId
    ORDER BY sc.day_of_week, sc.number   -- сортировка по номеру
";
            command.Parameters.AddWithValue("$groupId", groupId);
            command.Parameters.AddWithValue("$semesterId", semesterId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                result.Add(new  
                {
                    Id = reader["id"],
                    DayOfWeek = reader["day_of_week"],
                    Time = reader["time"],
                    Number = reader["number"],
                    Group = reader["group_name"],
                    Subject = reader["subject_name"],
                    Teacher = reader["teacher_name"]
                });
            }

            return Ok(result);
        }
        [Authorize(Roles = "admin")]
        [HttpPost]
        public IActionResult AddSchedule([FromBody] AddScheduleRequest request)
        {
            using var connection = new SqliteConnection(ConnStr);
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
            INSERT INTO Schedule
            (group_id, subject_id, teacher_id, semester_id, day_of_week, time, number)
            VALUES
            ($groupId, $subjectId, $teacherId, $semesterId, $day, $time, $number)
        ";

            command.Parameters.AddWithValue("$groupId", request.GroupId);
            command.Parameters.AddWithValue("$subjectId", request.SubjectId);
            command.Parameters.AddWithValue("$teacherId", request.TeacherId);
            command.Parameters.AddWithValue("$semesterId", request.SemesterId);
            command.Parameters.AddWithValue("$day", request.DayOfWeek);
            command.Parameters.AddWithValue("$time", request.Time);
            command.Parameters.AddWithValue("$number", request.Number);

            command.ExecuteNonQuery();

            return Ok(new { message = "Расписание добавлено" });
        }
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public IActionResult UpdateSchedule(int id, [FromBody] AddScheduleRequest request)
        {
            using var connection = new SqliteConnection(ConnStr);
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
            UPDATE Schedule
            SET group_id = $groupId,
                subject_id = $subjectId,
                teacher_id = $teacherId,
                semester_id = $semesterId,
                day_of_week = $day,
                time = $time
                number = $number
            WHERE id = $id
        ";

            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$groupId", request.GroupId);
            command.Parameters.AddWithValue("$subjectId", request.SubjectId);
            command.Parameters.AddWithValue("$teacherId", request.TeacherId);
            command.Parameters.AddWithValue("$semesterId", request.SemesterId);
            command.Parameters.AddWithValue("$day", request.DayOfWeek);
            command.Parameters.AddWithValue("$time", request.Time);
            command.Parameters.AddWithValue("$number", request.Number);


            var rows = command.ExecuteNonQuery();

            if (rows == 0)
                return NotFound();

            return Ok(new { message = "Обновлено" });
        }
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteSchedule(int id)
        {
            using var connection = new SqliteConnection(ConnStr);
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
            DELETE FROM Schedule
            WHERE id = $id
        ";

            command.Parameters.AddWithValue("$id", id);

            var rows = command.ExecuteNonQuery();

            if (rows == 0)
                return NotFound();

            return Ok(new { message = "Удалено" });
        }
    }
    public class AddScheduleRequest
    {
        public int GroupId { get; set; }
        public int SubjectId { get; set; }
        public int TeacherId { get; set; }
        public int SemesterId { get; set; }
        public int DayOfWeek { get; set; }
        public string Time { get; set; } = "";
        public int Number { get; set; }
    }
}
