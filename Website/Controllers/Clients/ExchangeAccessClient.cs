using Serilog;
using System.Threading.Tasks;
using TradeBot.Facade.FacadeService.v1;
using Website.Models;

namespace Website.Controllers.Clients
{
	public class ExchangeAccessClient
	{
		// Логгирование.
		private static readonly ILogger logger = Log.ForContext("Where", "Website");

		// Клиент Facade для того, чтобы можно было получить доступ к сервису ExchangeAccessService.
		private static readonly FacadeService.FacadeServiceClient client = new(AccountServiceConnection.GetConnection());

		// Метод добавления новой биржи в аккаунт.
		public static async Task<AddExchangeAccessResponse> AddExchangeAccess(string sessionId, AddExchangeAccessModel model)
		{
			logger.Information("{@Class}: метод {@Method} принял запрос: " +
				$"sessionId - {sessionId}, " +
				$"ExchangeCode - {model.ExchangeCode}, " +
				$"Secret - {model.Secret}, " +
				$"Token - {model.Token}.", "ExchangeAccessClient", "AddExchangeAccess");

			var request = new AddExchangeAccessRequest
			{
				SessionId = sessionId,
				Code = model.ExchangeCode,
				ExchangeName = model.ExchangeCode.ToString(),
				Token = model.Token,
				Secret = model.Secret
			};
			return await client.AddExchangeAccessAsync(request);
		}

		// Метод получения информации о всех биржах по Id сессии.
		public static async Task<AllExchangesBySessionResponse> AllExchangesBySession(string sessionId)
		{
			logger.Information("ExchangeAccessClient: метод AllExchangesBySession принял запрос: sessionId - " +
				$"{sessionId}.", "ExchangeAccessClient", "AllExchangesBySession");
			var request = new AllExchangesBySessionRequest { SessionId = sessionId };
			return await client.AllExchangesBySessionAsync(request);
		}

		// Метод удаления выбранной биржи по Id сессии.
		public static async Task<DeleteExchangeAccessResponse> DeleteExchangeAccess(string sessionId, ExchangeAccessCode code)
		{
			logger.Information("{@Class}: метод {@Method} принял запрос:" +
				$"sessionId - {sessionId}, " +
				$"code - {code}.", "ExchangeAccessClient", "DeleteExchangeAccess");

			var request = new DeleteExchangeAccessRequest
			{
				SessionId = sessionId,
				Code = code
			};
			return await client.DeleteExchangeAccessAsync(request);
		}

		// Метод получения информации конкретной биржи по Id сессии.
		public static async Task<ExchangeBySessionResponse> ExchangeBySession(string sessionId, ExchangeAccessCode code)
		{
			logger.Information("{@Class}: метод {@Method} принял запрос:" +
				$"sessionId - {sessionId}, " +
				$"code - {code}.", "ExchangeAccessClient", "ExchangeBySession");

			var request = new ExchangeBySessionRequest
			{
				SessionId = sessionId,
				Code = code
			};
			return await client.ExchangeBySessionAsync(request);
		}
	}
}