using System.ComponentModel.DataAnnotations;

namespace Website.Models.Authorization
{
	public class LoginModel
	{
		[Required(ErrorMessage = "Отсутствуют данные в поле Email.")]
		[EmailAddress(ErrorMessage = "Введенные данные не являются адресом электронной почты.")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Отсутствуют данные в поле пароля.")]
		[DataType(DataType.Password, ErrorMessage = "Введенные деннаые не являются паролем.")]
		public string Password { get; set; }
	}
}