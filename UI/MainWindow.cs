using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Google.Protobuf.WellKnownTypes;
using TradeBot.Common.v1;
using TradeBot.Facade.FacadeService.v1;
using UpdateServerConfigRequest = TradeBot.Facade.FacadeService.v1.UpdateServerConfigRequest;
using static TradeBot.Facade.FacadeService.v1.SubscribeEventsResponse;

namespace UI
{
    public partial class TradeBotUi : Form
    {
        private readonly FacadeService.FacadeServiceClient _client;
        private Metadata _meta;
        private string _sessionId;

        private Dictionary<string, Duration> _intervalMap;
        private Dictionary<string, int> _sensitivityMap;

        public TradeBotUi()
        {
            _client = new FacadeService.FacadeServiceClient(GrpcChannel.ForAddress("https://localhost:5002"));
            InitializeComponent();
            InitIntervalMap();
            InitSensitivityMap();
            this.FormClosing += MainWindow_FormClosing;
        }

        private async void SubscribeEvents(string sessionId)
        {
            var response = _client.SubscribeEvents(new SubscribeEventsRequest { Sessionid = sessionId });

            while (await response.ResponseStream.MoveNext())
                {
                    switch (response.ResponseStream.Current.EventTypeCase)
                    {
                        case EventTypeOneofCase.Balance:
                            BalanceLabel.Text = response.ResponseStream.Current.Balance.Balance.Value;
                            break;
                        case EventTypeOneofCase.Order:
                            EventConsole.Text += response.ResponseStream.Current.Order.Message;
                            EventConsole.Text += response.ResponseStream.Current.Order.Order;
                            break;
                    }
                }
        }


        private void InitIntervalMap()
        {
            _intervalMap = new Dictionary<string, Duration>
            {
                { "5s", Duration.FromTimeSpan(new TimeSpan(0, 0, 0, 5)) },
                { "30s", Duration.FromTimeSpan(new TimeSpan(0, 0, 0, 30)) },
                { "1m", Duration.FromTimeSpan(new TimeSpan(0, 0, 1, 0)) },
                { "3m", Duration.FromTimeSpan(new TimeSpan(0, 0, 3, 0)) },
                { "5m", Duration.FromTimeSpan(new TimeSpan(0, 0, 5, 0)) },
                { "15m", Duration.FromTimeSpan(new TimeSpan(0, 0, 15, 0)) },
                { "30m", Duration.FromTimeSpan(new TimeSpan(0, 0, 30, 0)) },
                { "1h", Duration.FromTimeSpan(new TimeSpan(0, 1, 0, 0)) },
                { "2h", Duration.FromTimeSpan(new TimeSpan(0, 2, 0, 0)) },
                { "3h", Duration.FromTimeSpan(new TimeSpan(0, 3, 0, 0)) },
                { "4h", Duration.FromTimeSpan(new TimeSpan(0, 4, 0, 0)) },
                { "6h", Duration.FromTimeSpan(new TimeSpan(0, 6, 0, 0)) },
                { "12h", Duration.FromTimeSpan(new TimeSpan(0, 12, 0, 0)) },
                { "1d", Duration.FromTimeSpan(new TimeSpan(1, 0, 0, 0)) },
                { "2d", Duration.FromTimeSpan(new TimeSpan(2, 0, 0, 0)) },
                { "1w", Duration.FromTimeSpan(new TimeSpan(7, 0, 0, 0)) },
                { "2w", Duration.FromTimeSpan(new TimeSpan(14, 0, 0, 0)) },
                { "1M", Duration.FromTimeSpan(new TimeSpan(30, 0, 0, 0)) }
            };
        }

        private void InitSensitivityMap()
        {
            _sensitivityMap = new Dictionary<string, int>
            {
                { "Minimal", 1 },
                { "Medium", 2 },
                { "High", 3 }
            };
        }

        private async void StartButton_Click(object sender, EventArgs e)
        {
            var configuration = GetConfig();

            var startBotResponse = await _client.StartBotAsync(new SwitchBotRequest
            {
                Config = configuration
            }, _meta);
            var updateConfigResponse = await _client.UpdateServerConfigAsync(
                new UpdateServerConfigRequest
                {
                    Request = new TradeBot.Common.v1.UpdateServerConfigRequest
                        { Config = configuration, Switch = false }
                }, _meta);
            SubscribeEvents(_meta.GetValue("sessionid"));

            StartButton.Enabled = false;
            StartButton.Visible = false;
            StopButton.Enabled = true;
            StopButton.Visible = true;

            Console.WriteLine("Запустил бота с конфигом {0}", configuration);
        }

        private Config GetConfig()
        {
            _intervalMap.TryGetValue(ConfigIntervalOfAnalysisl.Text, out var interval);
            _sensitivityMap.TryGetValue(ConfigAlgorithmSensivity.Text, out var sensitivity);
            var config = new Config
            {
                AvaibleBalance = double.Parse(ConfigAvailableBalance.Text),
                RequiredProfit = double.Parse(ConfigRequiredProfit.Text),
                ContractValue = double.Parse(ConfigVolumeOfContracts.Text),
                AlgorithmInfo = new AlgorithmInfo
                {
                    Interval = interval,
                    Sensivity = sensitivity
                },
                OrderUpdatePriceRange = double.Parse(ConfigUpdatePriceRange.Text),
            };
            return config;
        }

        private void ShowRegistrationPanel_Click(object sender, EventArgs e)
        {
            SignUpGroupBox.Visible = true;
            SignUpGroupBox.Enabled = true;
            SignInGroupBox.Visible = false;
            SignInGroupBox.Enabled = false;
            MainMenuGroupBox.Visible = false;
            MainMenuGroupBox.Enabled = false;
        }

        private void ShowLoginPanel_Click(object sender, EventArgs e)
        {
            SignUpGroupBox.Visible = false;
            SignUpGroupBox.Enabled = false;
            SignInGroupBox.Visible = true;
            SignInGroupBox.Enabled = true;
            MainMenuGroupBox.Visible = false;
            MainMenuGroupBox.Enabled = false;
        }

        private void ShowMainMenu_Click(object sender, EventArgs e)
        {
            SignUpGroupBox.Visible = false;
            SignUpGroupBox.Enabled = false;
            SignInGroupBox.Visible = false;
            SignInGroupBox.Enabled = false;
            MainMenuGroupBox.Visible = true;
            MainMenuGroupBox.Enabled = true;
        }

        private async void RegistrationButton_Click(object sender, EventArgs e)
        {
            var regResponse = await _client.RegisterAsync(new RegisterRequest
            {
                Email = RegLog.Text,
                Password = RegPass.Text,
                VerifyPassword = RegPass.Text
            });
        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            var logResponse = await _client.LoginAsync(new LoginRequest
            {
                Email = LogLogTextBox.Text,
                Password = LogPassTextBox.Text,
                SaveExchangesAfterLogout = true
            });
            _sessionId = logResponse.SessionId;
            _meta = new Metadata
            {
                { "sessionid", logResponse.SessionId },
                { "slot", "XBTUSD" },
                { "trademarket", "bitmex" }
            };

            var exchangeResponse = _client.AddExchangeAccess(new AddExchangeAccessRequest
            {
                SessionId = logResponse.SessionId,
                Token = RegKey.Text,
                Secret = RegToken.Text,
                Code = ExchangeCode.Bitmex,
                ExchangeName = "BitMEX"
            });
            LoggedGroupBox.Visible = true;
            LoggedGroupBox.Enabled = true;
            LoggedGroupBox.Text = "Signed in as " + LogLogTextBox.Text;
            SignInGroupBox.Visible = false;
            SignInGroupBox.Enabled = false;
        }

        private async void SignOutButton_Click(object sender, EventArgs e)
        {
            var logOutResponse = await _client.LogoutAsync( new SessionRequest() { SessionId = _sessionId});
            SignInGroupBox.Visible = true;
            SignInGroupBox.Enabled = true;
            LoggedGroupBox.Visible = false;
            LoggedGroupBox.Enabled = false;
        }

        private async void RemoveMyOrdersButton_Click(object sender, EventArgs e)
        {
            var removeMyOrdersResponse = await _client.DeleteOrderAsync(new DeleteOrderRequest(),_meta); 
        }

        private async void UpdateConfigButton_Click(object sender, EventArgs e)
        {
            var updateConfigResponse = await _client.UpdateServerConfigAsync(new UpdateServerConfigRequest { Request = new TradeBot.Common.v1.UpdateServerConfigRequest { Config = GetConfig(), Switch = false}},_meta);
        }

        private async void StopButton_Click(object sender, EventArgs e)
        {
            StartButton.Enabled = true;
            StartButton.Visible = true;
            StopButton.Enabled = false;
            StopButton.Visible = false;
            var stopBotResponse = await _client.StopBotAsync(new StopBotRequest { Request=new TradeBot.Common.v1.UpdateServerConfigRequest {Config=GetConfig(),Switch=true } },_meta);
        }

        private async void MainWindow_FormClosing(object sender, FormClosingEventArgs e) 
        {
            var stopBotResponse = await _client.UpdateServerConfigAsync(new UpdateServerConfigRequest { Request = new TradeBot.Common.v1.UpdateServerConfigRequest { Config = GetConfig(), Switch = true}},_meta);
        }
    }
}
