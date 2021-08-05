using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Отсутствуют данные в поле Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Отсутствует данные в поле пароля")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Отсутствует данные в поле подтверждения пароля")]
        [DataType(DataType.Password)]
        public string VerifyPassword { get; set; }
    }
}
