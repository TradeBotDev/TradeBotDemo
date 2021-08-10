using System.ComponentModel.DataAnnotations;

namespace Website.Models
{
    public class CreditCardModel
    {
        [Required(ErrorMessage = "Ввод номера карты является обязательным.")]
        [CreditCard(ErrorMessage = "Введенный код не является номером кредитной карты.")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Отсутствуют данные в поле ввода даты.")]
        public int Date { get; set; }

        [Required(ErrorMessage = "Отсутствуют данные в поле ввода CVV.")]
        public int CVV { get; set; }
    }
}
