using System.ComponentModel.DataAnnotations;

namespace Website.Models
{
	public class CreditCardModel
	{
		[Required(ErrorMessage = "Ввод номера карты является обязательным.")]
		[StringLength(19, MinimumLength = 19, ErrorMessage = "Длина номера карты должна содержать в себе 16 чисел.")]
		public string CardNumber { get; set; }

		[Required(ErrorMessage = "Отсутствуют данные в поле ввода даты.")]
		public string Date { get; set; }

		[Required(ErrorMessage = "Отсутствуют данные в поле ввода CVV.")]
		public string CVV { get; set; }
	}
}
