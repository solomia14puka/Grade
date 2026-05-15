using GradeApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GradeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GradesController : ControllerBase
    {
        private readonly GradeContext _context;

        public GradesController(GradeContext context)
        {
            _context = context;
        }

        // GET: api/Grades
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Grade>>> GetGrade()
        {
            return await _context.Grades.ToListAsync();
        }

        // GET: api/Grades/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Grade>> GetGrade(int id)
        {
            var grade = await _context.Grades.FindAsync(id);

            if (grade == null)
            {
                return NotFound();
            }

            return grade;
        }

        // PUT: api/Grades/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGrade(int id, Grade grade)
        {
            if (id != grade.Id)
            {
                return BadRequest();
            }

            var valid = await _context.Students.AnyAsync(s => s.Id == grade.StudentId) &&
                await _context.Subjects.AnyAsync(s => s.Id == grade.SubjectId) &&
                await _context.Professors.AnyAsync(p => p.Id == grade.ProfessorId);

            if (!valid) return BadRequest("Помилка введення");
            _context.Entry(grade).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GradeExists(id))
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

        // POST: api/Grades
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Professor")]
        [HttpPost]
        public async Task<ActionResult<Grade>> PostGrade([FromBody] Grade grade)
        {
            if (grade.Value < 0 || grade.Value > 100)
            {
                return BadRequest("Оцінка повинна бути в межах від 0 до 100.");
            }

            var studentExists = await _context.Students.AnyAsync(s => s.Id == grade.StudentId);
            if (!studentExists)
            {
                return BadRequest($"Студента не існує.");
            }

            var subjectExists = await _context.Subjects.AnyAsync(s => s.Id == grade.SubjectId);
            if (!subjectExists)
            {
                return BadRequest($"Предмета не існує.");
            }

            var professorExists = await _context.Professors.AnyAsync(p => p.Id == grade.ProfessorId);
            if (!professorExists)
            {
                return BadRequest($"Викладача не існує.");
            }
            if (grade.Date == default)
            {
                grade.Date = DateTime.Now;
            }

            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGrade", new { id = grade.Id }, grade);
        }

        // DELETE: api/Grades/5
        [Authorize(Roles = "Professor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            var grade = await _context.Grades.FindAsync(id);
            if (grade == null)
            {
                return NotFound("Оцінку не знайдено.");
            }

            try
            {
                _context.Grades.Remove(grade);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                return BadRequest("Помилка при видаленні оцінки.");
            }
        }

        private bool GradeExists(int id)
        {
            return _context.Grades.Any(e => e.Id == id);
        }

        // filter
        [Authorize]
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<object>>> GetFilteredGrades(
        [FromQuery] string? facultyName,
        [FromQuery] string? departmentName,
        [FromQuery] string? subjectName,
        [FromQuery] string? studentName,
        [FromQuery] string? professorName)
        {
            var query = _context.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.Department)
                        .ThenInclude(d => d.Faculty)
                .Include(g => g.Subject)
                .Include(g => g.Professor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(facultyName))
            {
                query = query.Where(g => g.Student.Department.Faculty.Name.Contains(facultyName));
            }

            if (!string.IsNullOrEmpty(departmentName))
            {
                query = query.Where(g => g.Student.Department.Name.Contains(departmentName));
            }

            if (!string.IsNullOrEmpty(subjectName))
            {
                query = query.Where(g => g.Subject.Name.Contains(subjectName));
            }

            if (!string.IsNullOrEmpty(studentName))
            {
                query = query.Where(g => g.Student.FullName.Contains(studentName));
            }

            if (!string.IsNullOrEmpty(professorName))
            {
                query = query.Where(g => g.Professor.FullName.Contains(professorName));
            }

            var result = await query.Select(g => new
            {
                GradeId = g.Id,
                Value = g.Value,
                Date = g.Date,
                Student = g.Student.FullName,
                Department = g.Student.Department.Name,
                Faculty = g.Student.Department.Faculty.Name,
                Subject = g.Subject.Name,
                Professor = g.Professor.FullName
            }).ToListAsync();

            return Ok(result);
        }

        [Authorize(Roles = "Student")]
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<object>>> GetMyGrades()
        {
            var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var student = await _context.Students.FirstOrDefaultAsync(s => s.FullName == userName);

            if (student == null)
            {
                return NotFound("Студента не знайдено.");
            }

            var grades = await _context.Grades
                .Where(g => g.StudentId == student.Id)
                .Include(g => g.Subject)
                .Include(g => g.Professor)
                .OrderByDescending(g => g.Date)
                .Select(g => new
                {
                    GradeId = g.Id,
                    SubjectName = g.Subject.Name,
                    Value = g.Value,
                    Date = g.Date,
                    ProfessorName = g.Professor.FullName
                })
                .ToListAsync();

            return Ok(grades);
        }

        [Authorize(Roles = "Student")]
        [HttpGet("my/dynamics")]
        public async Task<ActionResult<object>> GetMyDynamics()
        {
            var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var student = await _context.Students.FirstOrDefaultAsync(s => s.FullName == userName);

            if (student == null)
            {
                return NotFound("Студента не знайдено.");
            }

            var grades = await _context.Grades
                .Where(g => g.StudentId == student.Id)
                .OrderBy(g => g.Date)
                .ToListAsync();

            if (!grades.Any())
            {
                return Ok(new List<object>());
            }

            var monthlyProgression = grades
                .GroupBy(g => new { g.Date.Year, g.Date.Month })
                .Select(group => new
                {
                    Period = $"{group.Key.Month:D2}/{group.Key.Year}",
                    AverageValue = Math.Round(group.Average(g => g.Value), 2)
                })
                .ToList();
            return Ok(monthlyProgression);
        }

        [HttpGet("student/{id}")]
        [Authorize(Roles = "Professor")]
        public async Task<ActionResult<IEnumerable<object>>> GetStudentGradesAdmin(int id)
        {
            var grades = await _context.Grades
                .Include(g => g.Subject)
                .Include(g => g.Professor)
                .Where(g => g.StudentId == id)
                .Select(g => new {
                    g.Value,
                    SubjectName = g.Subject.Name,
                    Date = g.Date,
                    ProfessorName = g.Professor.FullName
                })
                .OrderByDescending(g => g.Date)
                .ToListAsync();

            return Ok(grades);
        }
    }
}