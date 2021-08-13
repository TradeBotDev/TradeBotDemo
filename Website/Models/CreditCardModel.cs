using System.ComponentModel.DataAnnotations;

namespace Website.Models
{
    public class CreditCardModel
    {
        [Required(ErrorMessage = "Ввод номера карты является обязательным.")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Длина номера карты должна быть равна 16 символам.")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Отсутствуют данные в поле ввода даты.")]
        public int Date { get; set; }

        [Required(ErrorMessage = "Отсутствуют данные в поле ввода CVV.")]
        public int CVV { get; set; }
    }
}
