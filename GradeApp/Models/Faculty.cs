using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace GradeApp.Models
{
    public class Faculty
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        public string Name { get; set; }

        public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
    }
}