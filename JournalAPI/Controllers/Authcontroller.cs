using Microsoft.AspNetCore.Mvc;
using JournalApi.Data;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JournalApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AuthController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _db.Users.FirstOrDefault(u => u.Login == request.Login);

            if (user == null)
                return Unauthorized("Пользователь не найден");

            if (user.Password != request.Password)
                return Unauthorized("Неверный пароль");

            var jwtKey = "super_secret_key_for_diplom_project_12345";

            var claims = new[]
            {
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Role, user.Role),
    new Claim("login", user.Login)
};

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                role = user.Role
            });
        }
    }

    public class LoginRequest
    {
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
    }
}