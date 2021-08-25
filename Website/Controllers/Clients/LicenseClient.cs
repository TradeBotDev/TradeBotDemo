using Serilog;
using System;
using System.Threading.Tasks;
using TradeBot.Facade.FacadeService.v1;
using Website.Models;

namespace Website.Controllers.Clients
{
	public static class LicenseClient
	{
		// Логгирование.
		private static readonly ILogger logger = Log.ForContext("Where", "Website");

		// Клиент Facade для того, чтобы можно было получить доступ к LicenseService.
		private static readonly FacadeService.FacadeServiceClient client = new(AccountServiceConnection.GetConnection());

		// Метод установки лицензии для пользователя по Id сессии.
		public static async Task<SetLicenseResponse> SetLicense(string sessionId, ProductCode product, CreditCardModel model)
		{
			Log.Information("{@Class}: метод {@Method} принял запрос: " +
				$"sessionId - {sessionId}, " +
				$"product - {product}, " +
				$"CardNumber - {model.CardNumber}, " +
				$"Date - {model.Date}, " +
				$"CVV - {model.CVV}.", "LicenseClient", "SetLicense");

			var request = new SetLicenseRequest
			{
				SessionId = sessionId,
				Product = product,
				CardNumber = model.CardNumber,
				Date = Convert.ToInt32(model.Date),
				Cvv = Convert.ToInt32(model.CVV)
			};
			return await client.SetLicenseAsync(request);
		}

		// Метод проверки на то, есть ли лицензия у текущего пользователя.
		public static async Task<CheckLicenseResponse> CheckLicense(string sessionId, ProductCode product)
		{
			Log.Information("{@Class}: метод {@Method} принял запрос: " +
				$"sessionId - {sessionId}, " +
				$"product - {product}", "LicenseClient", "CheckLicense");
			
			// Если Id сессии является null, в запросе отправляется просто пустая строка (gRPC не умеет пересылать null).
			// В таком случае сработает валидация уже в самом сервисе и вернется сообщение об ошибке.
			if (sessionId == null) sessionId = "";
			var request = new CheckLicenseRequest
			{
				SessionId = sessionId,
				Product = product
			};
			return await client.CheckLicenseAsync(request);
		}
	}
}