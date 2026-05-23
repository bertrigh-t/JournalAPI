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
        [Authorize]
        [HttpGet("group/{groupId}")]
        public IActionResult GetScheduleByGroup(int groupId)
        {
            var schedule = new List<object>();

            using var connection = new SqliteConnection("Data Source=Data/диплом.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    sch.id,
                    sch.day_of_week,
                    sch.lesson_number,
                    sch.start_time,
                    sch.end_time,
                    sch.classroom,
                    sub.name AS subject
                FROM Schedule sch
                JOIN Journals j ON sch.journal_id = j.id
                JOIN Subjects sub ON j.subject_id = sub.id
                WHERE j.group_id = $groupId
                ORDER BY sch.day_of_week, sch.lesson_number
            ";

            command.Parameters.AddWithValue("$groupId", groupId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                schedule.Add(new
                {
                    id = reader["id"],
                    dayOfWeek = reader["day_of_week"],
                    lessonNumber = reader["lesson_number"],
                    startTime = reader["start_time"],
                    endTime = reader["end_time"],
                    classroom = reader["classroom"],
                    subject = reader["subject"]
                });
            }

            return Ok(schedule);
        }
        [Authorize(Roles = "student")]
        [HttpGet("student")]
        public IActionResult GetStudentSchedule()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var schedule = new List<object>();

            using var connection = new SqliteConnection("Data Source=Data/диплом.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
        SELECT 
            sch.id,
            sch.day_of_week,
            sch.lesson_number,
            sch.start_time,
            sch.end_time,
            sch.classroom,
            sub.name AS subject
        FROM Schedule sch
        JOIN Journals j ON sch.journal_id = j.id
        JOIN Subjects sub ON j.subject_id = sub.id
        JOIN Students st ON j.group_id = st.group_id
        WHERE st.user_id = $userId
        ORDER BY sch.day_of_week, sch.lesson_number
    ";

            command.Parameters.AddWithValue("$userId", userId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                schedule.Add(new
                {
                    id = reader["id"],
                    dayOfWeek = reader["day_of_week"],
                    lessonNumber = reader["lesson_number"],
                    startTime = reader["start_time"],
                    endTime = reader["end_time"],
                    classroom = reader["classroom"],
                    subject = reader["subject"]
                });
            }

            return Ok(schedule);
        }
    }
}