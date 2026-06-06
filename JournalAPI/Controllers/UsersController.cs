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
        SELECT id, login, password, role
        FROM Users
        ORDER BY login
    ";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                users.Add(new
                {
                    Id = reader["id"],
                    Login = reader["login"],
                    Role = reader["role"],
                    Password = reader["password"]
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

            var command = connection.CreateCommand();

            command.CommandText = @"
        INSERT INTO Users
        (login, password, role)
        VALUES
        ($login, $password, $role)
    ";

            command.Parameters.AddWithValue("$login", request.Login);
            command.Parameters.AddWithValue("$password", request.Password);
            command.Parameters.AddWithValue("$role", request.Role);

            command.ExecuteNonQuery();

            return Ok(new
            {
                message = "Пользователь создан"
            });
        }
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            using var connection =
                new SqliteConnection("Data Source=Data/диплом.db");

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
        UPDATE Users
        SET login = $login,
            password = $password,
            role = $role
        WHERE id = $id
    ";

            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$login", request.Login);
            command.Parameters.AddWithValue("$password", request.Password);
            command.Parameters.AddWithValue("$role", request.Role);

            var rows = command.ExecuteNonQuery();

            if (rows == 0)
                return NotFound();

            return Ok(new
            {
                message = "Пользователь обновлен"
            });
        }
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            using var connection =
                new SqliteConnection("Data Source=Data/диплом.db");

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
        DELETE FROM Users
        WHERE id = $id
    ";

            command.Parameters.AddWithValue("$id", id);

            var rows = command.ExecuteNonQuery();

            if (rows == 0)
                return NotFound();

            return Ok(new
            {
                message = "Пользователь удалён"
            });
        }
    }
    public class AddUserRequest
    {
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "";
    }
    public class UpdateUserRequest
    {
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "";
    }

}