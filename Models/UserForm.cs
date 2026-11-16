using System.ComponentModel.DataAnnotations;
namespace AdminPanel.Models
{
    public class UserForm
    {
        [Required(ErrorMessage = "Введите логин.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Введите email.")]
        [EmailAddress(ErrorMessage = "Неверный email.")]
        public string Email { get; set; }
        public string Password { get; set; }
        [Required]
        public string Role { get; set; } = "User";
    }
}
