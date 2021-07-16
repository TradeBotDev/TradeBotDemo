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
            var relayClient = new RelayService.RelayServiceClient(channel);
            var facadeClient = new FacadeService.FacadeServiceClient(channel);

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
            
            var requestForRelay = new StartBotRequest() 
            {
                Config = config

            };
            var requestForFacade = new AuthenticateTokenRequest()
            {
                Token = ConfigToken.Text
            };
            var call1 = await facadeClient.AuthenticateTokenAsync(requestForFacade);
            Console.WriteLine("Выслал facade: {0}", requestForFacade.Token);

            var call2 = await relayClient.StartBotAsync(requestForRelay);
            Console.WriteLine("Запустил бота с конфигом {0}", requestForRelay.Config);
        }
    }
}
