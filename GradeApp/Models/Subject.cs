using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace GradeApp.Models
{
    public class Subject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        public string Name { get; set; }
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
}
