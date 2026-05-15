using GradeApp.Data;
using GradeApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GradeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly GradeContext _context;

        public StudentsController(GradeContext context)
        {
            _context = context;
        }

        // GET: api/Students
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudent()
        {
            return await _context.Students.ToListAsync();
        }

        // GET: api/Students/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            return student;
        }

        // PUT: api/Students/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(int id, Student student)
        {
            if (id != student.Id)
            {
                return BadRequest();
            }

            _context.Entry(student).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Students
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Student>> PostStudent(Student student)
        {
            var departmentExists = await _context.Departments.AnyAsync(d => d.Id == student.DepartmentId);
            if (!departmentExists)
            {
                return BadRequest("Обраної кафедри не існує в базі даних!");
            }
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStudent", new { id = student.Id }, student);
        }

        // DELETE: api/Students/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            bool hasGrades = await _context.Grades.AnyAsync(g => g.StudentId == id);
            if (hasGrades)
            {
                return BadRequest("Неможливо видалити студента, він завжди з нами.");
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound("Студента не знайдено.");
            }

            try
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                return BadRequest("Неможливо видалити студента.");
            }
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }

        // filter
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Student>>> SearchStudents(
        [FromQuery] string? name,
        [FromQuery] string? department)
        {
            var query = _context.Students.Include(s => s.Department).AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(s => s.FullName.Contains(name));
            }

            if (!string.IsNullOrEmpty(department))
            {
                query = query.Where(s => s.Department.Name.Contains(department));
            }

            return await query.ToListAsync();
        }

        // довідка
        [HttpGet("{id}/certificate")]
        public async Task<ActionResult<object>> GetStudentCertificate(int id)
        {
            var student = await _context.Students
                .Include(s => s.Department)
                    .ThenInclude(d => d.Faculty)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound("Студента не знайдено.");
            }

            var averageGrade = await _context.Grades
                .Where(g => g.StudentId == id)
                .Select(g => g.Value)
                .DefaultIfEmpty(0)
                .AverageAsync();

            var certificate = new
            {
                DocumentTitle = "ДОВІДКА ПРО НАВЧАННЯ",
                IssueDate = DateTime.Now.ToString("dd.MM.yyyy"),
                StudentName = student.FullName,
                Faculty = student.Department.Faculty.Name,
                Department = student.Department.Name,
                AcademicPerformance = new
                {
                    AverageScore = Math.Round(averageGrade, 2),
                    Status = averageGrade >= 60 ? "Успішно виконує навчальний план" : "Має академічну заборгованість"
                },
                IssuedBy = "Інформаційна система GradeApp"
            };

            return Ok(certificate);
        }

        [HttpGet("me")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<object>> GetCurrentStudentInfo()
        {
            var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            var student = await _context.Students
                .Include(s => s.Department)
                .ThenInclude(d => d.Faculty)
                .FirstOrDefaultAsync(s => s.FullName == userName);

            if (student == null) return NotFound("Студента не знайдено.");

            return Ok(new
            {
                FullName = student.FullName,
                DepartmentName = student.Department.Name,
                FacultyName = student.Department.Faculty.Name
            });
        }
    }
}
