using System.ComponentModel.DataAnnotations;
namespace AdminPanel.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Введите логин.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Введите пароль.")]
        public string Password { get; set; }
    }
}
