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
        private FacadeService.FacadeServiceClient client;
        private Metadata meta;
        private bool IsSub=false;
        public TradeBotUI()
        {
            client = new FacadeService.FacadeServiceClient(GrpcChannel.ForAddress("https://localhost:5002"));
            InitializeComponent();
        }

        private async void StartButton_Click(object sender, EventArgs e)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5002");
            var facadeClient = new TradeBot.Facade.FacadeService.v1.FacadeService.FacadeServiceClient(channel);

            var config = new Config
            {
                AvaibleBalance = double.Parse(ConfigAvailableBalance.Text),
                RequiredProfit = double.Parse(ConfigRequiredProfit.Text),
                ContractValue = double.Parse(ConfigVolumeOfContracts.Text),
                //AlgorithmInfo = new AlgorithmInfo() { Interval = new Google.Protobuf.WellKnownTypes.Timestamp() { Seconds = int.Parse(ConfigIntervalOfAnalysis.Text) } },
                OrderUpdatePriceRange = double.Parse(ConfigUpdatePriceRange.Text),
                SlotFee = 0.1D,
                TotalBalance = 100.0
            };

            var requestForRelay = new SwitchBotRequest()
            {
                Config = config

            };
            //var requestForFacade = new AuthenticateTokenRequest()
            //{
            //    Token = ConfigTokenl.Text
            //};


            var call2 = await facadeClient.SwitchBotAsync(requestForRelay, meta);
            Console.WriteLine("Запустил бота с конфигом {0}", requestForRelay.Config);

            if (!IsSub)
            {
                IsSub = true;
                var call3 = facadeClient.SubscribeLogsRelay(new TradeBot.Facade.FacadeService.v1.SubscribeLogsRequest() { R = new TradeBot.Common.v1.SubscribeLogsRequest() { Level = LogLevel.Information } },meta);

                while (await call3.ResponseStream.MoveNext())
                {
                    EventConsole.Text += call3.ResponseStream.Current.Message.Message + "\r\n";


                }
            }
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
