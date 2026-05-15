using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GradeApp.Data;
using GradeApp.Models;

namespace GradeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly GradeContext _context;

        public DepartmentsController(GradeContext context)
        {
            _context = context;
        }

        // GET: api/Departments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartment()
        {
            return await _context.Departments.ToListAsync();
        }

        // GET: api/Departments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetDepartment(int id)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                return NotFound();
            }

            return department;
        }

        // PUT: api/Departments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepartment(int id, Department department)
        {
            bool hasProfessors = await _context.Professors.AnyAsync(p => p.DepartmentId == id);
            bool hasStudents = await _context.Students.AnyAsync(s => s.DepartmentId == id);

            if (hasProfessors || hasStudents)
            {
                return BadRequest("До кафедри все ще прив'язані викладачі або студенти.");
            }

            if (id != department.Id)
            {
                return BadRequest();
            }

            _context.Entry(department).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartmentExists(id))
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

        // POST: api/Departments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Department>> PostDepartment(Department department)
        {
            bool facultyExists = await _context.Faculties.AnyAsync(f => f.Id == department.FacultyId);
            if (!facultyExists)
            {
                return BadRequest("Факультету не існує.");
            }

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetDepartment", new { id = department.Id }, department);
        }

        // DELETE: api/Departments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            bool hasProfessors = await _context.Professors.AnyAsync(p => p.DepartmentId == id);
            bool hasStudents = await _context.Students.AnyAsync(s => s.DepartmentId == id);

            if (hasProfessors || hasStudents)
            {
                return BadRequest("Тут ще є люди, ну кудиии.");
            }

            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound("Запис не знайдено.");
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            try
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                return BadRequest("Неможливо видалити цей запис.");
            }
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.Id == id);
        }
    }
}
