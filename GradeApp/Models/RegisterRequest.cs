using System.ComponentModel.DataAnnotations;

namespace GradeApp.Models
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Ім'я обов'язкове для заповнення")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Вкажіть вашу роль: Student або Professor")]
        public string Role { get; set; }

        public int? DepartmentId { get; set; }

        [Required(ErrorMessage = "Пароль обов'язковий")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        [DataType(DataType.Password)]
        public string PasswordConfirm { get; set; }
    }
}