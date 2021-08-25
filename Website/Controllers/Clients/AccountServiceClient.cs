using Serilog;
using System.Threading.Tasks;
using TradeBot.Facade.FacadeService.v1;
using Website.Models.Authorization;

namespace Website.Controllers.Clients
{
	public static class AccountServiceClient
	{
		// Логгирование.
		private static readonly ILogger logger = Log.ForContext("Where", "Website");

		// Клиент Афсфву для того, чтобы можно было получить доступ к AccountService.
		private static readonly FacadeService.FacadeServiceClient client = new(AccountServiceConnection.GetConnection());

		// Метод входа в аккаунт.
		public static async Task<LoginResponse> Login(LoginModel model)
		{
			logger.Information("{@Class}: метод {@Method} принял запрос: " +
				$"Email - {model.Email}, " +
				$"Password - {model.Password}.", "AccountServiceClient", "Register");

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
			logger.Information("{@Class}: метод {@Method} принял запрос: " +
				$"sessionId - {sessionId}, " +
				$"saveExchangeAccesses - {saveExchangeAccesses}.", "AccountServiceClient", "Logout");

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
			logger.Information("{@Class}: метод {@Method} принял запрос: " +
				$"Email - {model.Email}, " +
				$"Password - {model.Password}, " +
				$"VerifyPassword - {model.VerifyPassword}.", "AccountServiceClient", "Register");

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
			logger.Information("{@Class}: метод {@Method} принял запрос: " +
				$"sessionId - {sessionId}.", "AccountServiceClient", "Register");

			var request = new IsValidSessionRequest { SessionId = sessionId };
			return await client.IsValidSessionAsync(request);
		}

		// Метод получения информации из аккаунта.
		public static async Task<AccountDataResponse> AccountData(string sessionId)
		{
			logger.Information("{@Class}: метод {@Method} принял запрос: " +
				$"sessionId - {sessionId}.", "AccountServiceClient", "Register");

			var request = new AccountDataRequest { SessionId = sessionId };
			return await client.AccountDataAsync(request);
		}
	}
}