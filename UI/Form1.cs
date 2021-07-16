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
using StartBotRequest = TradeBot.Relay.RelayService.v1.StartBotRequest;

namespace UI
{
    public partial class TradeBotUI : Form
    {
        public TradeBotUI()
        {
            InitializeComponent();
        }

        private async void StartButton_Click(object sender, EventArgs e)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5002");
            //var relayClient = new RelayService.RelayServiceClient(channel);
            var facadeClient = new TradeBot.Facade.FacadeService.v1.FacadeService.FacadeServiceClient(channel);

            var per1 = double.Parse(ConfigAvailableBalance.Text);
            var per2 = double.Parse(ConfigRequiredProfit.Text);
            var per3 = double.Parse(ConfigVolumeOfContracts.Text);
            var per4 = double.Parse(ConfigUpdatePriceRange.Text);

            Config config = new Config()
            {
                AvaibleBalance = per1,
                RequiredProfit = per2,
                ContractValue = per3,
                //AlgorithmInfo = new AlgorithmInfo() { Interval = new Google.Protobuf.WellKnownTypes.Timestamp() { Seconds = int.Parse(ConfigIntervalOfAnalysis.Text) } },
                OrderUpdatePriceRange = per4,
                SlotFee = 0.1D,
                TotalBalance = 100.0
            };

            var requestForRelay = new TradeBot.Facade.FacadeService.v1.StartBotRequest()
            {
                Config = config

            };
            var requestForFacade = new AuthenticateTokenRequest()
            {
                Token = ConfigTokenl.Text
            };
            var call = await facadeClient.AuthenticateTokenAsync(requestForFacade);
            Console.WriteLine("Выслал facade: {0}", requestForFacade.Token);

            var call2 = facadeClient.StartBotRPC(requestForRelay);
            Console.WriteLine("Запустил бота с конфигом {0}", requestForRelay.Config);
        }

        private void TradeBotUI_Load(object sender, EventArgs e)
        {

        }
    }
}
