using GradeApp.Data;
using GradeApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace GradeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly GradeContext _context;

        public AuthController(GradeContext context)
        {
            _context = context;
        }

        // POST: api/auth/register
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
                    return BadRequest("Для студента обов'язково треба вказати DepartmentId.");

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

            return BadRequest("Невідома роль. Введіть Student або Professor.");
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var professor = _context.Professors.FirstOrDefault(p => p.FullName == request.FullName && p.Password == request.Password);
            if (professor != null)
            {
                return Ok(new { Message = "Вхід успішний!", Role = "Professor", UserId = professor.Id });
            }

            var student = _context.Students.FirstOrDefault(s => s.FullName == request.FullName && s.Password == request.Password);
            if (student != null)
            {
                return Ok(new { Message = "Вхід успішний!", Role = "Student", UserId = student.Id });
            }

            return Unauthorized("Невірне ім'я або пароль.");
        }
    }
}