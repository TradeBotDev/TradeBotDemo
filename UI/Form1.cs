using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TradeBot.Facade.FacadeService.v1;
using TradeBot.Relay.RelayService.v1;
using TradeBot.Common.v1;
using Grpc.Core;
//using StartBotRequest = TradeBot.Relay.RelayService.v1.StartBotRequest;

namespace UI
{
    public partial class TradeBotUI : Form
    {
        private TradeBot.Facade.FacadeService.v1.FacadeService.FacadeServiceClient client;
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
                AlgorithmInfo = new AlgorithmInfo() { Interval = new Google.Protobuf.WellKnownTypes.Timestamp() { Seconds = int.Parse(ConfigIntervalOfAnalysis.Text) } },
                OrderUpdatePriceRange = double.Parse(ConfigUpdatePriceRange.Text),
                SlotFee = 0.1D,
                TotalBalance = 100.0
            };

            var requestForRelay = new TradeBot.Facade.FacadeService.v1.SwitchBotRequest()
            {
                Config = config

            };
            var requestForFacade = new AuthenticateTokenRequest()
            {
                Token = ConfigTokenl.Text
            };
            var call = await facadeClient.AuthenticateTokenAsync(requestForFacade);
            Console.WriteLine("Выслал facade: {0}", requestForFacade.Token);

            var call2 =await facadeClient.SwitchBotAsync(requestForRelay);
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
            try
            {
                var response1 = await client.RegisterAsync(new RegisterRequest
                {
                    Email = RegLog.Text,
                    Password = RegPass.Text
                });
            }
            catch (RpcException ex)
            {
                IsUserLogged.Text = ex.Message;
            }
        } 
    }
}
