using Serilog;
using System.Threading.Tasks;
//using TradeBot.Account.AccountService.v1;
using TradeBot.Facade.FacadeService.v1;
using Website.Models.Authorization;

namespace Website.Controllers.Clients
{
	public static class AccountServiceClient
	{
		// Клиент сервиса аккаунтов для того, чтобы можно было получить к нему доступ.
		//private static Account.AccountClient client = new(AccountServiceConnection.GetConnection());
		private static FacadeService.FacadeServiceClient client = new(AccountServiceConnection.GetConnection());

		// Метод входа в аккаунт.
		public static async Task<LoginResponse> Login(LoginModel model)
		{
			Log.Information($"AccountServiceClient: метод Login принял запрос: " +
				$"Email - {model.Email}, " +
				$"Password - {model.Password}.");

			var request = new LoginRequest
			{
				Email = model.Email,
				Password = model.Password
			};
			return await client.LoginAsync(request);
		}

		// Метод выхода из аккаунта.
		public static async Task<LogoutResponse> Logout(string sessionId, bool saveExchangeAccesses)
		{
			Log.Information($"AccountServiceClient: метод Logout принял запрос: " +
				$"sessionId - {sessionId}, " +
				$"saveExchangeAccesses - {saveExchangeAccesses}.");

			var request = new LogoutRequest
			{
				SessionId = sessionId,
				SaveExchangeAccesses = saveExchangeAccesses
			};
			return await client.LogoutAsync(request);
		}

		// Метод регистрации.
		public static async Task<RegisterResponse> Register(RegisterModel model)
		{
			Log.Information($"AccountServiceClient: метод Register принял запрос: " +
				$"Email - {model.Email}, " +
				$"Password - {model.Password}, " +
				$"VerifyPassword - {model.VerifyPassword}.");

			var request = new RegisterRequest
			{
				Email = model.Email,
				Password = model.Password,
				VerifyPassword = model.VerifyPassword
			};
			return await client.RegisterAsync(request);
		}

		// Метод проверки сессии на валидность.
		public static async Task<IsValidSessionResponse> IsValidSession(string sessionId)
		{
			Log.Information($"AccountServiceClient: метод IsValidSession принял запрос: sessionId - {sessionId}.");
			var request = new IsValidSessionRequest
			{
				SessionId = sessionId
			};
			return await client.IsValidSessionAsync(request);
		}

		// Метод получения информации из аккаунта.
		public static async Task<AccountDataResponse> AccountData(string sessionId)
		{
			Log.Information($"AccountServiceClient: метод AccountData принял запрос: sessionId - {sessionId}.");
			var request = new AccountDataRequest
			{
				SessionId = sessionId
			};
			return await client.AccountDataAsync(request);
		}
	}
}