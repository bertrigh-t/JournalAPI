using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace JournalApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = new List<object>();

            using var connection =
                new SqliteConnection("Data Source=Data/диплом.db");

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
        SELECT 
            u.id,
            u.login,
            u.password,
            u.role,
            CASE
                WHEN u.role = 'teacher' THEN t.name
                WHEN u.role = 'student' THEN s.name
                ELSE ''
            END AS name
        FROM Users u
        LEFT JOIN Teachers t ON t.user_id = u.id
        LEFT JOIN Students s ON s.user_id = u.id
        ORDER BY u.login
    ";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                users.Add(new
                {
                    Id = reader["id"],
                    Login = reader["login"],
                    Role = reader["role"],
                    Password = reader["password"],
                    Name = reader["name"]
                });
            }

            return Ok(users);
        }
        [Authorize(Roles = "admin")]
        [HttpPost]
        public IActionResult AddUser([FromBody] AddUserRequest request)
        {
            using var connection =
                new SqliteConnection("Data Source=Data/диплом.db");

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Добавляем в Users
                var userCommand = connection.CreateCommand();
                userCommand.Transaction = transaction;
                userCommand.CommandText = @"
            INSERT INTO Users (login, password, role)
            VALUES ($login, $password, $role);

            SELECT last_insert_rowid();
        ";

                userCommand.Parameters.AddWithValue("$login", request.Login);
                userCommand.Parameters.AddWithValue("$password", request.Password);
                userCommand.Parameters.AddWithValue("$role", request.Role);

                var userId = Convert.ToInt32(userCommand.ExecuteScalar());

                // 2. Если teacher -> добавляем в Teachers
                if (request.Role == "teacher")
                {
                    var teacherCommand = connection.CreateCommand();
                    teacherCommand.Transaction = transaction;
                    teacherCommand.CommandText = @"
                INSERT INTO Teachers (name, user_id)
                VALUES ($name, $userId)
            ";

                    teacherCommand.Parameters.AddWithValue("$name", request.Name);
                    teacherCommand.Parameters.AddWithValue("$userId", userId);

                    teacherCommand.ExecuteNonQuery();
                }

                // 3. Если student -> добавляем в Students
                if (request.Role == "student")
                {
                    var studentCommand = connection.CreateCommand();
                    studentCommand.Transaction = transaction;
                    studentCommand.CommandText = @"
                INSERT INTO Students (name, user_id)
                VALUES ($name, $userId)
            ";

                    studentCommand.Parameters.AddWithValue("$name", request.Name);
                    studentCommand.Parameters.AddWithValue("$userId", userId);

                    studentCommand.ExecuteNonQuery();
                }

                transaction.Commit();

                return Ok(new
                {
                    message = "Пользователь создан"
                });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            using var connection = new SqliteConnection("Data Source=Data/диплом.db");
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Обновляем Users
                var updateUser = connection.CreateCommand();
                updateUser.Transaction = transaction;
                updateUser.CommandText = @"
            UPDATE Users
            SET login = $login,
                password = $password,
                role = $role
            WHERE id = $id
        ";

                updateUser.Parameters.AddWithValue("$id", id);
                updateUser.Parameters.AddWithValue("$login", request.Login);
                updateUser.Parameters.AddWithValue("$password", request.Password);
                updateUser.Parameters.AddWithValue("$role", request.Role);

                var rows = updateUser.ExecuteNonQuery();

                if (rows == 0)
                {
                    transaction.Rollback();
                    return NotFound();
                }

                // 2. ВСЕГДА чистим старые связи
                var deleteTeacher = connection.CreateCommand();
                deleteTeacher.Transaction = transaction;
                deleteTeacher.CommandText = "DELETE FROM Teachers WHERE user_id = $id";
                deleteTeacher.Parameters.AddWithValue("$id", id);
                deleteTeacher.ExecuteNonQuery();

                var deleteStudent = connection.CreateCommand();
                deleteStudent.Transaction = transaction;
                deleteStudent.CommandText = "DELETE FROM Students WHERE user_id = $id";
                deleteStudent.Parameters.AddWithValue("$id", id);
                deleteStudent.ExecuteNonQuery();

                // 3. Создаём заново по роли
                if (request.Role == "teacher")
                {
                    var cmd = connection.CreateCommand();
                    cmd.Transaction = transaction;

                    cmd.CommandText = @"
                INSERT INTO Teachers (name, user_id)
                VALUES ($name, $userId)
            ";

                    cmd.Parameters.AddWithValue("$name", request.Name ?? request.Login);
                    cmd.Parameters.AddWithValue("$userId", id);

                    cmd.ExecuteNonQuery();
                }
                else if (request.Role == "student")
                {
                    var cmd = connection.CreateCommand();
                    cmd.Transaction = transaction;

                    cmd.CommandText = @"
                INSERT INTO Students (name, user_id)
                VALUES ($name, $userId)
            ";

                    cmd.Parameters.AddWithValue("$name", request.Name ?? request.Login);
                    cmd.Parameters.AddWithValue("$userId", id);

                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();

                return Ok(new { message = "Пользователь обновлен" });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            using var connection =
                new SqliteConnection("Data Source=Data/диплом.db");

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                var deleteTeacher = connection.CreateCommand();
                deleteTeacher.Transaction = transaction;
                deleteTeacher.CommandText = "DELETE FROM Teachers WHERE user_id = $id";
                deleteTeacher.Parameters.AddWithValue("$id", id);
                deleteTeacher.ExecuteNonQuery();

                var deleteStudent = connection.CreateCommand();
                deleteStudent.Transaction = transaction;
                deleteStudent.CommandText = "DELETE FROM Students WHERE user_id = $id";
                deleteStudent.Parameters.AddWithValue("$id", id);
                deleteStudent.ExecuteNonQuery();

                var deleteUser = connection.CreateCommand();
                deleteUser.Transaction = transaction;
                deleteUser.CommandText = "DELETE FROM Users WHERE id = $id";
                deleteUser.Parameters.AddWithValue("$id", id);

                var rows = deleteUser.ExecuteNonQuery();

                if (rows == 0)
                {
                    transaction.Rollback();
                    return NotFound();
                }

                transaction.Commit();

                return Ok(new
                {
                    message = "Пользователь удалён"
                });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "admin")]
        [HttpGet("teachers")]
        public IActionResult GetTeachers()
        {
            var teachers = new List<object>();

            using var connection =
                new SqliteConnection("Data Source=Data/диплом.db");

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
                    Id = reader["id"],
                    Name = reader["login"]
                });
            }

            return Ok(teachers);
        }
    }
    public class AddUserRequest
    {
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "";
        public string Name { get; set; } = "";
    }
    public class UpdateUserRequest
    {
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "";
        public string? Name { get; set; }

    }

}