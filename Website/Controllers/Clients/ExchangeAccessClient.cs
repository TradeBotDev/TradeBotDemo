using Serilog;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Website.Models;

namespace Website.Controllers.Clients
{
	public class ExchangeAccessClient
	{
		// Клиент сервиса бирж для того, чтобы можно было получить к нему доступ.
		private static ExchangeAccess.ExchangeAccessClient client = new(AccountServiceConnection.GetConnection());

		// Метод добавления новой биржи в аккаунт.
		public static async Task<AddExchangeAccessResponse> AddExchangeAccess(string sessionId, AddExchangeAccessModel model)
		{
			Log.Information($"ExchangeAccessClient: метод AddExchangeAccess принял запрос: " +
				$"sessionId - {sessionId}, " +
				$"ExchangeCode - {model.ExchangeCode}, " +
				$"Secret - {model.Secret}, " +
				$"Token - {model.Token}.");

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
			Log.Information($"ExchangeAccessClient: метод AllExchangesBySession принял запрос: sessionId - {sessionId}.");
			var request = new AllExchangesBySessionRequest
			{
				SessionId = sessionId
			};
			return await client.AllExchangesBySessionAsync(request);
		}

		// Метод удаления выбранной биржи по Id сессии.
		public static async Task<DeleteExchangeAccessResponse> DeleteExchangeAccess(string sessionId, ExchangeAccessCode code)
		{
			Log.Information($"ExchangeAccessClient: метод DeleteExchangeAccess принял запрос:" +
				$"sessionId - {sessionId}, " +
				$"code - {code}.");

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
			Log.Information($"ExchangeAccessClient: метод ExchangeBySession принял запрос:" +
				$"sessionId - {sessionId}, " +
				$"code - {code}.");

			var request = new ExchangeBySessionRequest
			{
				SessionId = sessionId,
				Code = code
			};
			return await client.ExchangeBySessionAsync(request);
		}
	}
}