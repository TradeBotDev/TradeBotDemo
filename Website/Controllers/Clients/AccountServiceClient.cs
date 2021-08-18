using Grpc.Net.Client;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Website.Models.Authorization;

namespace Website.Controllers.Clients
{
	public static class AccountServiceClient
	{
		// Клиент сервиса аккаунтов для того, чтобы можно было получить к нему доступ.
		private static Account.AccountClient client = new(AccountServiceConnection.GetConnection());

		// Метод входа в аккаунт.
		public static async Task<LoginResponse> Login(LoginModel model)
		{
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
			var request = new IsValidSessionRequest
			{
				SessionId = sessionId
			};
			return await client.IsValidSessionAsync(request);
		}

		// Метод получения информации из аккаунта.
		public static async Task<AccountDataResponse> AccountData(string sessionId)
		{
			var request = new AccountDataRequest
			{
				SessionId = sessionId
			};
			return await client.AccountDataAsync(request);
		}
	}
}
