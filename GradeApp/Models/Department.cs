using System.ComponentModel.DataAnnotations;

namespace GradeApp.Models
{
    public class Department
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        public string Name { get; set; }
        public int FacultyId { get; set; }
        public virtual Faculty Faculty { get; set; }

        public virtual ICollection<Professor> Professors { get; set; } = new List<Professor>();
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }
}