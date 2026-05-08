using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace GradeApp.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        public string FullName { get; set; }
        public int DepartmentId { get; set; }
        public string Password { get; set; }

        [JsonIgnore]
        public virtual Department? Department { get; set; }
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
}
