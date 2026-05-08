using System.ComponentModel.DataAnnotations;

namespace GradeApp.Models
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Введіть ім'я")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Введіть пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
