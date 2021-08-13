using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TradeBot.Common.v1;
using TradeBot.Facade.FacadeService.v1;
using UpdateServerConfigRequest = TradeBot.Facade.FacadeService.v1.UpdateServerConfigRequest;

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
            _client = new FacadeService.FacadeServiceClient(GrpcChannel.ForAddress("http://localhost:5002"));
            InitializeComponent();
            InitIntervalMap();
            InitSensitivityMap();
            FormClosing += MainWindow_FormClosing;
            ConfigUpdatePriceRange.TextChanged += ConfigUpdatePriceRangeOnTextChanged;
            dataGridView1.CellContentClick += dataGridViewSites_CellContentClick;
            dataGridView1.EditingControlShowing += DataGridView1OnEditingControlShowing;
        }

        private void DataGridView1OnEditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is ComboBox combo)
            {
                combo.SelectedIndexChanged -= ComboBox_SelectedIndexChanged;
                combo.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            }
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((ComboBox)sender).Text == "None") dataGridView1.Rows.RemoveAt(dataGridView1.CurrentCell.RowIndex);
        }

        private void ConfigUpdatePriceRangeOnTextChanged(object? sender, EventArgs e)
        {
            var str = ConfigUpdatePriceRange.Text;
            if (ConfigUpdatePriceRange.Text.IndexOf(',') == ConfigUpdatePriceRange.Text.Length - 1) return;
            if (!double.TryParse(ConfigUpdatePriceRange.Text, out var value)) return;

            var floor = Math.Floor(value);

            ConfigUpdatePriceRange.TextChanged -= ConfigUpdatePriceRangeOnTextChanged;
            ConfigUpdatePriceRange.Text = (floor += (value - floor) < 0.5 ? 0.0 : 0.5).ToString();
            ConfigUpdatePriceRange.TextChanged += ConfigUpdatePriceRangeOnTextChanged;
        }

        private void NewEventProcess(AsyncServerStreamingCall<SubscribeEventsResponse> response)
        {

            var slotName = response.ResponseStream.Current.Order.SlotName;
            var qty = response.ResponseStream.Current.Order.Order.Quantity;
            var price = response.ResponseStream.Current.Order.Order.Price;
            var type = response.ResponseStream.Current.Order.Order.Signature.Type;
            var time = TimeZoneInfo.ConvertTime(response.ResponseStream.Current.Order.Time.ToDateTime(), TimeZoneInfo.Local).ToString("HH:mm:ss dd.MM.yyyy");
            var id = response.ResponseStream.Current.Order.Order.Id;
            var message = response.ResponseStream.Current.Order.Message;
            switch (response.ResponseStream.Current.Order.ChangesType)
            {
                case ChangesType.Partitial:
                    {
                        if (response.ResponseStream.Current.Order.Order.Signature.Status == OrderStatus.Open)
                        {
                            ActiveOrdersDataGridView.Rows.Add(slotName, qty, price, type, time, id);
                        }

                        if (response.ResponseStream.Current.Order.Order.Signature.Status == OrderStatus.Closed)
                        {
                            FilledOrdersDataGridView.Rows.Add(slotName, qty, price, type, time, id);
                        }

                        break;
                    }
                case ChangesType.Insert:
                    {
                        ActiveOrdersDataGridView.Rows.Add(slotName, qty, price, type, time, id);
                        if (!string.IsNullOrEmpty(message))
                        {
                            var incomingString =
                                $"[{time}]: {response.ResponseStream.Current.Order.Message}\r\n" +
                                $"Order {id}, price: {price}, quantity: {qty}, type: {type}\r\n";
                            EventConsole.Text += incomingString;
                        }
                        break;
                    }
                case ChangesType.Update:
                    {
                        for (var i = 0; i < ActiveOrdersDataGridView.Rows.Count; i++)
                        {
                            if (string.Equals(ActiveOrdersDataGridView.Rows[i].Cells[5].Value as string, id))
                            {
                                ActiveOrdersDataGridView.Rows.RemoveAt(i);
                                ActiveOrdersDataGridView.Rows.Insert(i, slotName, qty, price, type, time, id);
                            }
                        }

                        break;
                    }
                case ChangesType.Delete:
                    {
                        FilledOrdersDataGridView.Rows.Add(slotName, qty, price, type, time, id);
                        for (var i = 0; i < ActiveOrdersDataGridView.Rows.Count; i++)
                        {
                            if (string.Equals(ActiveOrdersDataGridView.Rows[i].Cells[5].Value as string, id))
                                ActiveOrdersDataGridView.Rows.RemoveAt(i);
                        }

                        if (!string.IsNullOrEmpty(message))
                        {
                            var incomingString =
                                $"[{time}]: {response.ResponseStream.Current.Order.Message}\r\n" +
                                $"Order {id}, price: {price}, quantity: {qty}, type: {type}\r\n";
                            EventConsole.Text += incomingString;
                        }
                        break;
                    }
            }
        }

        private async void SubscribeEvents(string sessionId)
        {
            var response = _client.SubscribeEvents(new SubscribeEventsRequest { Sessionid = sessionId });

            while (await response.ResponseStream.MoveNext())
            {
                switch (response.ResponseStream.Current.EventTypeCase)
                {
                    case SubscribeEventsResponse.EventTypeOneofCase.Balance:
                        BalanceLabel.Text = $"{double.Parse(response.ResponseStream.Current.Balance.Balance.Value) / 100000000} {response.ResponseStream.Current.Balance.Balance.Currency}";
                        break;
                    case SubscribeEventsResponse.EventTypeOneofCase.Order:
                        NewEventProcess(response);
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
                { "Minimal", 0 },
                { "Low", 1 },
                { "Medium", 2 },
                { "High", 3 },
                { "Ultra", 4 }
            };
        }

        private async void Start(string slotName)
        {
            var configuration = GetConfig();
            _meta[1] = new Metadata.Entry("slot", slotName);
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
            Console.WriteLine("Запустил бота с конфигом {0}", configuration);
            EventConsole.Text += $"[{DateTime.Now:HH:mm:ss}]: Bot has been started!\r\n";
        }

        private Config GetConfig()
        {
            var config = new Config
            {
                AvaibleBalance = double.Parse(ConfigAvailableBalance.Text),
                RequiredProfit = double.Parse(ConfigRequiredProfit.Text),
                ContractValue = double.Parse(ConfigVolumeOfContracts.Text),
                AlgorithmInfo = new AlgorithmInfo
                {
                    Interval = _intervalMap[ConfigIntervalOfAnalysis.Text],
                    Sensivity = _sensitivityMap[ConfigAlgorithmSensivity.Text]
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
            EventConsole.Text += $"[{DateTime.Now:HH:mm:ss}]: You are signed in as {LogLogTextBox.Text}\r\n";
        }

        private async void SignOutButton_Click(object sender, EventArgs e)
        {
            var logOutResponse = await _client.LogoutAsync(new SessionRequest() { SessionId = _sessionId });
            SignInGroupBox.Visible = true;
            SignInGroupBox.Enabled = true;
            LoggedGroupBox.Visible = false;
            LoggedGroupBox.Enabled = false;
        }

        private async void RemoveMyOrdersButton_Click(object sender, EventArgs e)
        {
            var removeMyOrdersResponse = await _client.DeleteOrderAsync(new DeleteOrderRequest(), _meta);
        }

        private async void UpdateConfigButton_Click(object sender, EventArgs e)
        {
            var updateConfigResponse = await _client.UpdateServerConfigAsync(new UpdateServerConfigRequest { Request = new TradeBot.Common.v1.UpdateServerConfigRequest { Config = GetConfig(), Switch = false } }, _meta);
        }

        private async void Stop(string slotName)
        {
            _meta[1] = new Metadata.Entry("slot", slotName);
            var stopBotResponse = await _client.StopBotAsync(new StopBotRequest { Request = new TradeBot.Common.v1.UpdateServerConfigRequest { Config = GetConfig(), Switch = true } }, _meta);
            var removeMyOrdersResponse = await _client.DeleteOrderAsync(new DeleteOrderRequest(), _meta);
            EventConsole.Text += $"[{DateTime.Now:HH:mm:ss}]: Bot has been stopped!\r\n";
        }

        private async void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            var stopBotResponse = await _client.UpdateServerConfigAsync(new UpdateServerConfigRequest { Request = new TradeBot.Common.v1.UpdateServerConfigRequest { Config = GetConfig(), Switch = true } }, _meta);
        }
        private void dataGridViewSites_CellContentClick(object sender,
            DataGridViewCellEventArgs e)
        {
            if (Convert.ToBoolean(dataGridView1.Rows[e.RowIndex].Cells[1].EditedFormattedValue))
                Start(dataGridView1.Rows[e.RowIndex].Cells[0].EditedFormattedValue.ToString());
            else Stop(dataGridView1.Rows[e.RowIndex].Cells[0].EditedFormattedValue.ToString());

        }

        private void AddRowButton_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count < 7) dataGridView1.Rows.Add(SlotsComboBox.Text);
        }

        private void RemoveRowButton_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.RemoveAt(dataGridView1.Rows.Count - 1);
        }

        private void TradeBotUi_Load(object sender, EventArgs e)
        {
            dataGridView1.Rows.Add(SlotsComboBox.Text);
        }
    }
}
