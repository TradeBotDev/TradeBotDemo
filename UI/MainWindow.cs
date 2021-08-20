using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TradeBot.Common.v1;
using TradeBot.Facade.FacadeService.v1;
using ZedGraph;

namespace UI
{
    public partial class TradeBotUi : Form
    {
        private readonly List<string> _listOfAllSlots;
        private Dictionary<string, Configuration> _configurations;
        private readonly Dictionary<string, Duration> _intervalMap;
        private readonly Dictionary<string, int> _sensitivityMap;
        private readonly FacadeClient _facadeClient;
        private bool _loggedIn;
        private PointPairList _balanceList = new PointPairList();
        private PointPairList _orderList = new PointPairList();
        private DateTime lastDateBalance = new DateTime();
        private DateTime lastDateOrder = new DateTime();
        private struct IncomingMessage
        {
            public string SlotName;
            public double Qty;
            public double Price;
            public OrderType Type;
            public OrderStatus Status;
            public string Time;
            public string Id;
            public string Message;
        }
        public class Configuration
        {
            public string AlgorithmSensitivity;
            public string AlgorithmInterval;
            public string AvailableBalance;
            public string RequiredProfit;
            public string VolumeOfContracts;
            public string UpdatePriceRange;
        }

        public TradeBotUi()
        {
            InitializeComponent();
            _facadeClient = new FacadeClient();
            _facadeClient.HandleBalanceUpdate += HandleBalanceUpdate;
            _facadeClient.HandleOrderUpdate += HandleOrderUpdate;
            _intervalMap = InitIntervalMap();
            _sensitivityMap = InitSensitivityMap();
            _listOfAllSlots = InitListOfAllSlots();
            FormClosing += MainWindow_FormClosing;
            ConfigUpdatePriceRangeTxb.TextChanged += ConfigUpdatePriceRangeOnTextChanged;
            SlotsComboBox.DataSource = _listOfAllSlots;
            SlotsComboBox.SelectedIndex = 0;
            foreach (var control in MainMenuGroupBox.Controls)
            {
                var fullName = control.GetType().FullName;
                if (fullName != null && fullName.Contains("TextBox"))
                    ((TextBox)control).Validating += OnValidatingTextBox;
            }
            foreach (var control in SignUpPanel.Controls)
            {
                var fullName = control.GetType().FullName;
                if (fullName != null && fullName.Contains("TextBox"))
                    ((TextBox)control).Validating += OnValidating;
            }
            SetConfigurationBySlots(ReadFromJson());
            
        }

        private Configuration GetConfiguration()
        {
            return new Configuration
            {
                AlgorithmSensitivity = ConfigAlgorithmSensivityTxb.Text,
                AlgorithmInterval = ConfigIntervalOfAnalysisTxb.Text,
                AvailableBalance = ConfigAvailableBalanceTxb.Text,
                RequiredProfit = ConfigRequiredProfitTxb.Text,
                VolumeOfContracts = ConfigVolumeOfContractsTxb.Text,
                UpdatePriceRange = ConfigUpdatePriceRangeTxb.Text,
            };
        }

        private void SetConfigurationBySlots(Dictionary<string, Configuration> saved)
        {
            _configurations = saved;
            if (_configurations.Count > 0)
            {
                SetConfiguration(_configurations["XBTUSD"]);
                SetSlots();
            }
        }

        private void SetSlots()
        {
            foreach (var rows in _configurations)
            {
                AddToActiveSlots(rows.Key);
            }
        }

        private void SetConfiguration(Configuration configuration)
        {
            ConfigAlgorithmSensivityTxb.Text = configuration.AlgorithmSensitivity;
            ConfigIntervalOfAnalysisTxb.Text = configuration.AlgorithmInterval;
            ConfigAvailableBalanceTxb.Text = configuration.AvailableBalance;
            ConfigRequiredProfitTxb.Text = configuration.RequiredProfit;
            ConfigVolumeOfContractsTxb.Text = configuration.VolumeOfContracts;
            ConfigUpdatePriceRangeTxb.Text = configuration.UpdatePriceRange;
        }

        private void WriteToJson(Dictionary<string, Configuration> save)
        {
            var serializer = new JsonSerializer();
            if (File.Exists("configuration.save")) File.Delete("configuration.save");
            var sw = new StreamWriter("configuration.save");
            var writer = new JsonTextWriter(sw);
            serializer.Serialize(writer, save);
            writer.Close();
            sw.Close();
        }

        private Dictionary<string, Configuration> ReadFromJson()
        {
            JObject obj;
            if (File.Exists("configuration.save"))
            {
                var serializer = new JsonSerializer();
                var sr = new StreamReader("configuration.save");
                JsonReader jsonReader = new JsonTextReader(sr);
                obj = serializer.Deserialize(jsonReader) as JObject;
                jsonReader.Close();
                sr.Close();
                return (Dictionary<string, Configuration>)obj?.ToObject(typeof(Dictionary<string, Configuration>));
            }
            return new Dictionary<string, Configuration>();
        }


        private static List<string> InitListOfAllSlots()
        {
            return new List<string>
            {
                "XBTUSD",
                "ETHUSD",
                "DOGEUSD",
                "LTCUSD",
                "ADAUSDT",
                "XRPUSD",
                "SOLUSDT"
            };
        }

        private static Dictionary<string, Duration> InitIntervalMap()
        {
            return new Dictionary<string, Duration>
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

        private static Dictionary<string, int> InitSensitivityMap()
        {
            return new Dictionary<string, int>
            {
                { "Minimal", 0 },
                { "Low", 1 },
                { "Medium", 2 },
                { "High", 3 },
                { "Ultra", 4 }
            };
        }

        private static bool CheckConnection(DefaultResponse response)
        {
            if (response.Code == ReplyCode.Succeed) return true;
            MessageBox.Show(@"There is no connection to the services", @"Connection lost");
            return false;
        }

        private bool CheckIfLogged()
        {
            if (!ValidateChildren(ValidationConstraints.Enabled))
            {
                MessageBox.Show(@"Wrong configuration!", @"Correct the fields");
                return false;
            }
            if (!_loggedIn)
            {
                MessageBox.Show(@"You are not signed in!", @"Signed in is required");
                return false;
            }
            return true;
        }

        private void InsertOrderToTable(int index, DataGridView table, IncomingMessage message)
        {
            table.Rows.Insert(index, message.SlotName, message.Qty, message.Price, message.Type, message.Time, message.Id);
        }

        private void UpdateTable(DataGridView table, IncomingMessage incomingMessage)
        {
            for (var i = 0; i < table.Rows.Count; i++)
            {
                if (string.Equals(table.Rows[i].Cells[5].Value as string, incomingMessage.Id))
                {
                    table.Rows.RemoveAt(i);
                    InsertOrderToTable(i, table, incomingMessage);
                }
            }
        }

        private void DeleteFromTable(DataGridView table, string id)
        {
            for (var i = 0; i < table.Rows.Count; i++)
            {
                if (string.Equals(table.Rows[i].Cells[5].Value as string, id))
                    table.Rows.RemoveAt(i);
            }
        }

        private void WriteMessageToEventConsole(IncomingMessage message)
        {
            if (!string.IsNullOrEmpty(message.Message))
            {
                var incomingString = $"[{message.Time}] {message.Message}\r\n" + $"Order {message.Id}, price: {message.Price}, quantity: {message.Qty}, type: {message.Type}\r\n\r\n";
                EventConsole.Text += incomingString;
            }
        }

        private void WriteMessageToEventConsole(string message)
        {
            if (!string.IsNullOrEmpty(message)) EventConsole.Text += $"[{DateTime.Now:HH:mm:ss}] {message}\r\n\r\n";
        }

        private void HandleOrderUpdate(PublishOrderEvent orderEvent)
        {
            var incomingMessage = new IncomingMessage
            {
                SlotName = orderEvent.SlotName,
                Qty = orderEvent.Order.Quantity,
                Price = orderEvent.Order.Price,
                Type = orderEvent.Order.Signature.Type,
                Status = orderEvent.Order.Signature.Status,
                Time = TimeZoneInfo.ConvertTime(orderEvent.Time.ToDateTime(), TimeZoneInfo.Local).ToString("HH:mm:ss dd.MM.yyyy"),
                Id = orderEvent.Order.Id,
                Message = orderEvent.Message
            };
            switch (orderEvent.ChangesType)
            {
                case ChangesType.Partitial:
                    InsertOrderToTable(0, incomingMessage.Status == OrderStatus.Open ? ActiveOrdersDataGridView : FilledOrdersDataGridView, incomingMessage);
                    UpdateList(orderEvent.Order.Price.ToString(), orderEvent.Time, _orderList, OrderGraph, lastDateOrder);
                    break;
                case ChangesType.Insert:
                    InsertOrderToTable(0, ActiveOrdersDataGridView, incomingMessage);
                    WriteMessageToEventConsole(incomingMessage);
                    UpdateList(orderEvent.Order.Price.ToString(), orderEvent.Time, _orderList, OrderGraph, lastDateOrder);
                    break;

                case ChangesType.Update:
                    UpdateTable(ActiveOrdersDataGridView, incomingMessage);
                    UpdateList(orderEvent.Order.Price.ToString(), orderEvent.Time, _orderList, OrderGraph, lastDateOrder);
                    break;

                case ChangesType.Delete:
                    InsertOrderToTable(0, FilledOrdersDataGridView, incomingMessage);
                    DeleteFromTable(ActiveOrdersDataGridView, incomingMessage.Id);
                    WriteMessageToEventConsole(incomingMessage);
                    break;

                case ChangesType.Undefiend:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleBalanceUpdate(PublishBalanceEvent balanceUpdate)
        {
            BalanceLabel.Text = $"{balanceUpdate.Balance.Value} {balanceUpdate.Balance.Currency}";
            UpdateList(balanceUpdate.Balance.Value, balanceUpdate.Time, _balanceList, BalanceGraph, lastDateBalance);
        }

        private async void Start(string slotName)
        {
            WriteToJson(_configurations);
            await _facadeClient.StartBot(slotName, GetConfig());
            WriteMessageToEventConsole("Bot has been started!");
        }

        private async void Stop(string slotName)
        {
            CheckConnection(await _facadeClient.StopBot(slotName,GetConfig()));
            WriteMessageToEventConsole("Bot has been stopped!");
        }

        private Config GetConfig()
        {
            return new Config
            {
                AvaibleBalance = double.Parse(ConfigAvailableBalanceTxb.Text),
                RequiredProfit = double.Parse(ConfigRequiredProfitTxb.Text),
                ContractValue = double.Parse(ConfigVolumeOfContractsTxb.Text),
                AlgorithmInfo = new AlgorithmInfo
                {
                    Interval = _intervalMap[ConfigIntervalOfAnalysisTxb.Text],
                    Sensitivity = _sensitivityMap[ConfigAlgorithmSensivityTxb.Text]
                },
                OrderUpdatePriceRange = double.Parse(ConfigUpdatePriceRangeTxb.Text),
            };
        }

        private void AddToActiveSlots(string comboboxElem)
        {
            ActiveSlotsDataGridView.Rows.Add(comboboxElem);
            _listOfAllSlots.Remove(comboboxElem);
            SlotsComboBox.DataSource = _listOfAllSlots.Except(_configurations.Keys.ToList()).ToList();
            SlotsComboBox.SelectedIndex = 0;
        }

        private void RemoveFromActiveSlots(string comboboxElem)
        {
            _listOfAllSlots.Insert(0, comboboxElem);
            _configurations.Remove(comboboxElem);
            SlotsComboBox.DataSource = _listOfAllSlots.Except(_configurations.Keys.ToList()).ToList();
            SlotsComboBox.SelectedIndex = 0;
            ActiveSlotsDataGridView.Rows.RemoveAt(ActiveSlotsDataGridView.Rows.Count - 1);
        }

        #region EventHandlers

        private void OnValidating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(((TextBox)sender).Text))
            {
                e.Cancel = true;
                ErrorProviderMainForm.SetError(((TextBox)sender), "The field must not be empty!");
            }
            else ErrorProviderMainForm.Clear();
        }

        private void OnValidatingTextBox(object sender, CancelEventArgs e)
        {
            if (ActiveOrdersDataGridView.RowCount <= 0) return;
            if (!double.TryParse(((TextBox)sender).Text, out _))
            {
                e.Cancel = true;
                ErrorProviderMainForm.SetError((TextBox)sender, "A number is required!");
            }
            else if (string.IsNullOrEmpty(((TextBox)sender).Text))
            {
                e.Cancel = true;
                ErrorProviderMainForm.SetError(((TextBox)sender), "The field must not be empty!");
            }
            else ErrorProviderMainForm.Clear();
        }

        private void ConfigUpdatePriceRangeOnTextChanged(object sender, EventArgs e)
        {
            ConfigUpdatePriceRangeTxb.TextChanged -= ConfigUpdatePriceRangeOnTextChanged;
            if (ConfigUpdatePriceRangeTxb.Text.IndexOf(',') == ConfigUpdatePriceRangeTxb.Text.Length - 1) return;
            if (!double.TryParse(ConfigUpdatePriceRangeTxb.Text, out var value)) return;

            var floor = Math.Floor(value);
            
            ConfigUpdatePriceRangeTxb.Text = (floor + (value - floor < 0.5 ? 0.0 : 0.5)).ToString(CultureInfo.InvariantCulture);
            ConfigUpdatePriceRangeTxb.TextChanged += ConfigUpdatePriceRangeOnTextChanged;
        }

        private void ActiveOrdersDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (_configurations.Count <= 0) return;
            if (ActiveSlotsDataGridView.CurrentRow is not null)
            SetConfiguration(_configurations[ActiveSlotsDataGridView.Rows[ActiveSlotsDataGridView.CurrentRow.Index].Cells[0].EditedFormattedValue.ToString()]);
        }


        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;
            if (e.ColumnIndex is -1 or 0) return;
            _configurations[ActiveSlotsDataGridView.Rows[ActiveSlotsDataGridView.CurrentRow.Index].Cells[0]
                .EditedFormattedValue.ToString()] = GetConfiguration();
            WriteToJson(_configurations);
            if (!CheckIfLogged()) return;
            var cellCheckBox = (DataGridViewCheckBoxCell)ActiveSlotsDataGridView.Rows[ActiveSlotsDataGridView.CurrentRow.Index].Cells[1];
            cellCheckBox.Value ??= false;
            if (Convert.ToBoolean(cellCheckBox.Value))
            {
                if (MessageBox.Show(@"Are you sure you want to stop bot?", @"Stop bot",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Stop(ActiveSlotsDataGridView.Rows[ActiveSlotsDataGridView.CurrentRow.Index].Cells[0]
                        .EditedFormattedValue.ToString());
                    cellCheckBox.Value = false;
                }
            }
            else
            {
                Start(ActiveSlotsDataGridView.Rows[ActiveSlotsDataGridView.CurrentRow.Index].Cells[0].EditedFormattedValue.ToString());
                cellCheckBox.Value = true;
            }
        }

        private void ShowSignUpPanel_Click(object sender, EventArgs e)
        {
            SignUpGroupBox.Visible = true;
            SignUpGroupBox.Enabled = true;
            SignInGroupBox.Visible = false;
            SignInGroupBox.Enabled = false;
            MainMenuGroupBox.Visible = false;
            MainMenuGroupBox.Enabled = false;
        }

        private void ShowSignInPanel_Click(object sender, EventArgs e)
        {
            if (_loggedIn)
            {
                LoggedGroupBox.Visible = true;
                LoggedGroupBox.Enabled = true;
                SignUpGroupBox.Visible = false;
                SignUpGroupBox.Enabled = false;
                MainMenuGroupBox.Visible = false;
                MainMenuGroupBox.Enabled = false;
            }
            else
            {
                SignUpGroupBox.Visible = false;
                SignUpGroupBox.Enabled = false;
                SignInGroupBox.Visible = true;
                SignInGroupBox.Enabled = true;
                MainMenuGroupBox.Visible = false;
                MainMenuGroupBox.Enabled = false;
            }

        }

        private void ShowMainMenu_Click(object sender, EventArgs e)
        {
            SignUpGroupBox.Visible = false;
            SignUpGroupBox.Enabled = false;
            SignInGroupBox.Visible = false;
            SignInGroupBox.Enabled = false;
            MainMenuGroupBox.Visible = true;
            MainMenuGroupBox.Enabled = true;
            LoggedGroupBox.Visible = false;
            LoggedGroupBox.Enabled = false;
        }

        private async void RegistrationButton_Click(object sender, EventArgs e)
        {
            if (!ValidateChildren(ValidationConstraints.Enabled))
            {
                MessageBox.Show(@"Wrong login or password!", @"Correct the fields");
                return;
            }

            if (CheckConnection(await _facadeClient.RegisterAccount(RegLog.Text, RegPass.Text, RegPass.Text)))
                WriteMessageToEventConsole($"You have registered an account {RegLog.Text}");
        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            DefaultResponse sessionId;
            if (!CheckConnection(sessionId = await _facadeClient.SigningIn(LogLogTextBox.Text, LogPassTextBox.Text, KeyTxb.Text, SecretTxb.Text))) return;
            if (sessionId.Message.Contains("Отсутствует"))
            {
                MessageBox.Show(@"Account with this username and password was not found.",@"Account not found");
                return;
            }
            SessionIDLbl.Text = sessionId.Message;
            LoggedGroupBox.Visible = true;
            LoggedGroupBox.Enabled = true;
            ShowSignUpPanel.Enabled = false;
            LoggedGroupBox.Text = $"Signed in as {LogLogTextBox.Text}";
            SignInGroupBox.Visible = false;
            SignInGroupBox.Enabled = false;
            _loggedIn = true;
            WriteMessageToEventConsole($"You are signed in as {LogLogTextBox.Text}");
        }

        private async void SignOutButton_Click(object sender, EventArgs e)
        {
            if (!CheckConnection(await _facadeClient.SigningOut())) return;
            SignInGroupBox.Visible = true;
            SignInGroupBox.Enabled = true;
            ShowSignUpPanel.Enabled = true;
            LoggedGroupBox.Visible = false;
            LoggedGroupBox.Enabled = false;
            _loggedIn = false;
        }

        private async void RemoveMyOrdersButton_Click(object sender, EventArgs e)
        {
            if (!CheckIfLogged()) return;
            if (MessageBox.Show(@"Are you sure you want to remove your orders?", @"Remove orders", MessageBoxButtons.YesNo) == DialogResult.Yes)
                CheckConnection(await _facadeClient.RemoveMyOrders());
        }

        private async void UpdateConfigButton_Click(object sender, EventArgs e)
        {
            if (!CheckIfLogged()) return;
            if (MessageBox.Show(@"Are you sure you want to update the configuration?", @"Update configuration",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                WriteToJson(_configurations);
                CheckConnection(await _facadeClient.UpdateConfig(GetConfig()));
            }
        }

        private async void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show(@"Are you sure you want to close this window? Algorithm progress will be reset.",
                @"Close window",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                WriteToJson(_configurations);
                for (var i = 0; i < ActiveSlotsDataGridView.Rows.Count; i++)
                {
                    if (Convert.ToBoolean(ActiveSlotsDataGridView.Rows[i].Cells[1].EditedFormattedValue))
                        await _facadeClient.StopBot(
                            ActiveSlotsDataGridView.Rows[i].Cells[0].EditedFormattedValue.ToString(), GetConfig());
                }
            }
            else e.Cancel = true;
        }

        private void AddRowButton_Click(object sender, EventArgs e)
        {
            if (ActiveSlotsDataGridView.Rows.Count >= 7 || _listOfAllSlots.Count == 1) return;
            ClearConfigsTextBoxes();
            AddToActiveSlots(SlotsComboBox.Text);
        }

        private void ClearConfigsTextBoxes()
        {
            ConfigAlgorithmSensivityTxb.Text = "";
            ConfigAvailableBalanceTxb.Text = "";
            ConfigIntervalOfAnalysisTxb.SelectedIndex = 0;
            ConfigRequiredProfitTxb.Text = "";
            ConfigUpdatePriceRangeTxb.Text = "";
            ConfigVolumeOfContractsTxb.SelectedIndex = 0;
        }

        private void RemoveRowButton_Click(object sender, EventArgs e)
        {
            RemoveFromActiveSlots(ActiveSlotsDataGridView.Rows[^1].Cells[0].Value.ToString());
        }

        private void OurWebsiteLnkLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var parameter = new ProcessStartInfo { Verb = "open", FileName = "explorer", Arguments = "http://23.88.34.174:5008/" };
            Process.Start(parameter);
            OurWebsiteLnkLbl1.LinkVisited = true;
        }

        private void OurWebsiteLnkLbl1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var parameter = new ProcessStartInfo { Verb = "open", FileName = "explorer", Arguments = "http://23.88.34.174:5008/" };
            Process.Start(parameter);
            OurWebsiteLnkLbl1.LinkVisited = true;
        }

        private void BitmexWebsiteLnkLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var parameter = new ProcessStartInfo { Verb = "open", FileName = "explorer", Arguments = "https://testnet.bitmex.com/app/trade/XBTUSD" };
            Process.Start(parameter);
            OurWebsiteLnkLbl1.LinkVisited = true;
        }

        private async void SetLicense_ButtonClick(object sender, EventArgs e)
        {
            if (_loggedIn) {
                await _facadeClient.RegisterLicense();
            }
        }

        #endregion

        #region DrawGraphs
        private void UpdateList(string b, Timestamp time, PointPairList list, ZedGraph.ZedGraphControl graphControl, DateTime ld)
        {
            DateTime tm = TimeZoneInfo.ConvertTime(time.ToDateTime(), TimeZoneInfo.Local);//.ToString("HH:mm:ss dd.MM.yyyy");
            b = b.Replace('.', ',');
            if (double.TryParse(b, out double value))
            {
                if (ld.Day == tm.Day)
                {
                    list.RemoveAt(list.Count - 1);
                }
                list.Add(new XDate(tm), value);
                ld = tm;
                if (list.Count > 40)
                {
                    list.RemoveAt(0);
                }
                if (graphControl == BalanceGraph)
                {
                    lastDateBalance = tm;
                }
                else
                {
                    lastDateOrder = tm;
                }
                DrawGraph(graphControl, list);
            }
        }
        private void DrawGraph(ZedGraph.ZedGraphControl graphControl, PointPairList list)
        {
            GraphPane pane = graphControl.GraphPane;

            pane.CurveList.Clear();

            LineItem myCurve = pane.AddCurve("", list, System.Drawing.Color.Blue, SymbolType.Circle);
            if(graphControl.Name.Contains("Balance"))
            {
                pane.Title.Text = "Balance graph";
                pane.YAxis.Title.Text = "Balance, XBT";
            }
            else
            {
                pane.Title.Text = "Order graph";
                pane.YAxis.Title.Text = "Filled orders";
            }
            pane.XAxis.Type = AxisType.Date;
            pane.XAxis.Title.Text = "Date";
            
            try
            {
                pane.YAxis.Scale.Min = list.Last().Y - 0.01;
                pane.YAxis.Scale.Max = list.Last().Y + 0.01;

                pane.XAxis.Scale.Min = new XDate(list.Last().X - 1);
                pane.XAxis.Scale.Max = new XDate(list.Last().X + 1);
            }
            catch (Exception e)
            {

            }
            
            BalanceGraph.AxisChange();

            BalanceGraph.Invalidate();
        }
        void zedGraph_ZoomEvent(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {
            GraphPane pane = sender.GraphPane;


            if (pane.XAxis.Scale.Min <= 0)
            {
                pane.XAxis.Scale.Min = 0;
            }

            if (pane.XAxis.Scale.Max >= 10)
            {
                pane.XAxis.Scale.Max = 10;
            }

            //if (pane.YAxis.Scale.Min <= -1)
            //{
            //    pane.YAxis.Scale.Min = -1;
            //}

            //if (pane.YAxis.Scale.Max >= 2)
            //{
            //    pane.YAxis.Scale.Max = 2;
            //}
        }

        #endregion


    }
}
