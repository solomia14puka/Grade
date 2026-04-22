using System.Diagnostics;

namespace GradeApp.Models
{
    public class Professor
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Position { get; set; }

        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; }

        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
}
