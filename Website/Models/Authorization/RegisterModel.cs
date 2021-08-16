using System.ComponentModel.DataAnnotations;

namespace Website.Models.Authorization
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Отсутствуют данные в поле Email.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Отсутствует данные в поле пароля.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Отсутствует данные в поле подтверждения пароля.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
        public string VerifyPassword { get; set; }
    }
}
