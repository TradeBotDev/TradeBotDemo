using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Windows.Forms;
using TradeBot.Common.v1;
using TradeBot.Facade.FacadeService.v1;
//using StartBotRequest = TradeBot.Relay.RelayService.v1.StartBotRequest;

namespace UI
{
    public partial class TradeBotUI : Form
    {
        private TradeBot.Facade.FacadeService.v1.FacadeService.FacadeServiceClient client;
        private Metadata meta;
        public TradeBotUI()
        {
            client = new TradeBot.Facade.FacadeService.v1.FacadeService.FacadeServiceClient(GrpcChannel.ForAddress("https://localhost:5002"));
            InitializeComponent();
        }

        private async void StartButton_Click(object sender, EventArgs e)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5002");
            var facadeClient = new TradeBot.Facade.FacadeService.v1.FacadeService.FacadeServiceClient(channel);

            Config config = new Config()
            {
                AvaibleBalance = double.Parse(ConfigAvailableBalance.Text),
                RequiredProfit = double.Parse(ConfigRequiredProfit.Text),
                ContractValue = double.Parse(ConfigVolumeOfContracts.Text),
                //AlgorithmInfo = new AlgorithmInfo() { Interval = new Google.Protobuf.WellKnownTypes.Timestamp() { Seconds = int.Parse(ConfigIntervalOfAnalysis.Text) } },
                OrderUpdatePriceRange = double.Parse(ConfigUpdatePriceRange.Text),
                SlotFee = 0.1D,
                TotalBalance = 100.0
            };

            var requestForRelay = new TradeBot.Facade.FacadeService.v1.SwitchBotRequest()
            {
                Config = config

            };
            //var requestForFacade = new AuthenticateTokenRequest()
            //{
            //    Token = ConfigTokenl.Text
            //};


            var call2 = await facadeClient.SwitchBotAsync(requestForRelay, meta);
            Console.WriteLine("Запустил бота с конфигом {0}", requestForRelay.Config);
        }

        private void ShowRegistrationPanel_Click(object sender, EventArgs e)
        {
            RegistrationPanel.Visible = true;
            RegistrationPanel.Enabled = true;
            LogginPanel.Visible = false;
            LogginPanel.Enabled = false;
            MainMenuPanel.Visible = false;
            MainMenuPanel.Enabled = false;
        }

        private void ShowLoginPanel_Click(object sender, EventArgs e)
        {
            RegistrationPanel.Visible = false;
            RegistrationPanel.Enabled = false;
            LogginPanel.Visible = true;
            LogginPanel.Enabled = true;
            MainMenuPanel.Visible = false;
            MainMenuPanel.Enabled = false;
        }

        private void ShowMainMenu_Click(object sender, EventArgs e)
        {
            RegistrationPanel.Visible = false;
            RegistrationPanel.Enabled = false;
            LogginPanel.Visible = false;
            LogginPanel.Enabled = false;
            MainMenuPanel.Visible = true;
            MainMenuPanel.Enabled = true;
        }

        private async void RegistrationButton_Click(object sender, EventArgs e)
        {
            var regResponse = await client.RegisterAsync(new RegisterRequest
            {
                Email = RegLog.Text,
                Password = RegPass.Text,
                VerifyPassword = RegPass.Text
            });


        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            var logResponse = await client.LoginAsync(new LoginRequest
            {
                Email = LogLogTextBox.Text,
                Password = LogPassTextBox.Text,
                SaveExchangesAfterLogout = true
            });
            meta = new Metadata();
            meta.Add("sessionid", logResponse.SessionId);
            meta.Add("slot", "XBTUSD");
            meta.Add("trademarket", "bitmex");

            var exchangeResponse = client.AddExchangeAccess(new AddExchangeAccessRequest
            {
                SessionId = logResponse.SessionId,
                Token = RegKey.Text,
                Secret = RegToken.Text,
                Code = ExchangeCode.Bitmex,
                ExchangeName = "BitMEX"
            });
        }
    }
}
