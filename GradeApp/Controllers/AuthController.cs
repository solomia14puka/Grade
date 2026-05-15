using GradeApp.Data;
using GradeApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace GradeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly GradeContext _context;
        private readonly string _jwtSecret = "SuperSecretKeyThatIsVeryLongAndSecure123!";

        public AuthController(GradeContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (request.Role == "Professor")
            {
                if (_context.Professors.Any(p => p.FullName == request.FullName))
                    return BadRequest("Викладач з таким іменем вже існує.");

                var professor = new Professor
                {
                    FullName = request.FullName,
                    Password = request.Password
                };

                _context.Professors.Add(professor);
                _context.SaveChanges();
                return Ok("Викладача успішно зареєстровано!");
            }
            else if (request.Role == "Student")
            {
                if (request.DepartmentId == null)
                    return BadRequest("А де ж навчається студент??");

                if (_context.Students.Any(s => s.FullName == request.FullName))
                    return BadRequest("Студент з таким іменем вже існує.");

                var student = new Student
                {
                    FullName = request.FullName,
                    DepartmentId = request.DepartmentId.Value,
                    Password = request.Password
                };

                _context.Students.Add(student);
                _context.SaveChanges();
                return Ok("Студента успішно зареєстровано!");
            }

            return BadRequest("Невідома роль.");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var professor = _context.Professors.FirstOrDefault(p => p.FullName == request.FullName && p.Password == request.Password);
            if (professor != null)
            {
                var token = GenerateJwtToken(professor.FullName, "Professor");
                return Ok(new { Token = token, Role = "Professor", UserName = professor.FullName });
            }

            var student = _context.Students.FirstOrDefault(s => s.FullName == request.FullName && s.Password == request.Password);
            if (student != null)
            {
                var token = GenerateJwtToken(student.FullName, "Student");
                return Ok(new { Token = token, Role = "Student", UserName = student.FullName });
            }

            return Unauthorized("Невірне ім'я або пароль.");
        }

        private string GenerateJwtToken(string fullName, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, fullName),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}