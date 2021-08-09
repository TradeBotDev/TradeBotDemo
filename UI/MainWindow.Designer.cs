
namespace UI
{
    partial class TradeBotUi
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.StartButton = new System.Windows.Forms.Button();
            this.ConfigAvailableBalance = new System.Windows.Forms.TextBox();
            this.ConfigRequiredProfit = new System.Windows.Forms.TextBox();
            this.ConfigUpdatePriceRange = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ConfigRequiredProfitl = new System.Windows.Forms.Label();
            this.ConfigVolumeOfContractsl = new System.Windows.Forms.Label();
            this.ConfigUpdatePriceRangel = new System.Windows.Forms.Label();
            this.ConfigIntervalOfAnalysisl = new System.Windows.Forms.Label();
            this.ShowRegistrationPanel = new System.Windows.Forms.Button();
            this.ShowLoginPanel = new System.Windows.Forms.Button();
            this.ShowMainMenu = new System.Windows.Forms.Button();
            this.RegistrationButton = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.ConfigTokenl = new System.Windows.Forms.Label();
            this.RegKey = new System.Windows.Forms.TextBox();
            this.RegToken = new System.Windows.Forms.TextBox();
            this.RegPass = new System.Windows.Forms.TextBox();
            this.RegLog = new System.Windows.Forms.TextBox();
            this.LoginButton = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.LogLogTextBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.LogPassTextBox = new System.Windows.Forms.TextBox();
            this.MainMenuGroupBox = new System.Windows.Forms.GroupBox();
            this.RemoveMyOrdersButton = new System.Windows.Forms.Button();
            this.ConfigIntervalOfAnalysis = new System.Windows.Forms.ComboBox();
            this.ConfigVolumeOfContracts = new System.Windows.Forms.ComboBox();
            this.ConfigAlgorithmSensivity = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SignUpGroupBox = new System.Windows.Forms.GroupBox();
            this.SignInGroupBox = new System.Windows.Forms.GroupBox();
            this.EventConsole = new System.Windows.Forms.RichTextBox();
            this.LoggedGroupBox = new System.Windows.Forms.GroupBox();
            this.SignOutButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.MainMenuGroupBox.SuspendLayout();
            this.SignUpGroupBox.SuspendLayout();
            this.SignInGroupBox.SuspendLayout();
            this.LoggedGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(340, 244);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(349, 60);
            this.StartButton.TabIndex = 0;
            this.StartButton.Text = "Launch Bot";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // ConfigAvailableBalance
            // 
            this.ConfigAvailableBalance.Location = new System.Drawing.Point(161, 40);
            this.ConfigAvailableBalance.Name = "ConfigAvailableBalance";
            this.ConfigAvailableBalance.Size = new System.Drawing.Size(84, 27);
            this.ConfigAvailableBalance.TabIndex = 1;
            this.ConfigAvailableBalance.Text = "0,5";
            // 
            // ConfigRequiredProfit
            // 
            this.ConfigRequiredProfit.Location = new System.Drawing.Point(161, 73);
            this.ConfigRequiredProfit.Name = "ConfigRequiredProfit";
            this.ConfigRequiredProfit.Size = new System.Drawing.Size(84, 27);
            this.ConfigRequiredProfit.TabIndex = 2;
            this.ConfigRequiredProfit.Text = "0,005";
            // 
            // ConfigUpdatePriceRange
            // 
            this.ConfigUpdatePriceRange.Location = new System.Drawing.Point(161, 138);
            this.ConfigUpdatePriceRange.Name = "ConfigUpdatePriceRange";
            this.ConfigUpdatePriceRange.Size = new System.Drawing.Size(84, 27);
            this.ConfigUpdatePriceRange.TabIndex = 4;
            this.ConfigUpdatePriceRange.Text = "50";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(127, 20);
            this.label1.TabIndex = 7;
            this.label1.Text = "Available balance";
            // 
            // ConfigRequiredProfitl
            // 
            this.ConfigRequiredProfitl.AutoSize = true;
            this.ConfigRequiredProfitl.Location = new System.Drawing.Point(9, 74);
            this.ConfigRequiredProfitl.Name = "ConfigRequiredProfitl";
            this.ConfigRequiredProfitl.Size = new System.Drawing.Size(110, 20);
            this.ConfigRequiredProfitl.TabIndex = 8;
            this.ConfigRequiredProfitl.Text = "Required profit";
            // 
            // ConfigVolumeOfContractsl
            // 
            this.ConfigVolumeOfContractsl.AutoSize = true;
            this.ConfigVolumeOfContractsl.Location = new System.Drawing.Point(9, 106);
            this.ConfigVolumeOfContractsl.Name = "ConfigVolumeOfContractsl";
            this.ConfigVolumeOfContractsl.Size = new System.Drawing.Size(141, 20);
            this.ConfigVolumeOfContractsl.TabIndex = 9;
            this.ConfigVolumeOfContractsl.Text = "Volume of contracts";
            // 
            // ConfigUpdatePriceRangel
            // 
            this.ConfigUpdatePriceRangel.AutoSize = true;
            this.ConfigUpdatePriceRangel.Location = new System.Drawing.Point(9, 141);
            this.ConfigUpdatePriceRangel.Name = "ConfigUpdatePriceRangel";
            this.ConfigUpdatePriceRangel.Size = new System.Drawing.Size(137, 20);
            this.ConfigUpdatePriceRangel.TabIndex = 10;
            this.ConfigUpdatePriceRangel.Text = "Update price range";
            // 
            // ConfigIntervalOfAnalysisl
            // 
            this.ConfigIntervalOfAnalysisl.AutoSize = true;
            this.ConfigIntervalOfAnalysisl.Location = new System.Drawing.Point(9, 174);
            this.ConfigIntervalOfAnalysisl.Name = "ConfigIntervalOfAnalysisl";
            this.ConfigIntervalOfAnalysisl.Size = new System.Drawing.Size(131, 20);
            this.ConfigIntervalOfAnalysisl.TabIndex = 11;
            this.ConfigIntervalOfAnalysisl.Text = "Interval of analysis";
            // 
            // ShowRegistrationPanel
            // 
            this.ShowRegistrationPanel.Location = new System.Drawing.Point(11, 12);
            this.ShowRegistrationPanel.Name = "ShowRegistrationPanel";
            this.ShowRegistrationPanel.Size = new System.Drawing.Size(117, 61);
            this.ShowRegistrationPanel.TabIndex = 14;
            this.ShowRegistrationPanel.Text = "Sign Up";
            this.ShowRegistrationPanel.UseVisualStyleBackColor = true;
            this.ShowRegistrationPanel.Click += new System.EventHandler(this.ShowRegistrationPanel_Click);
            // 
            // ShowLoginPanel
            // 
            this.ShowLoginPanel.Location = new System.Drawing.Point(11, 79);
            this.ShowLoginPanel.Name = "ShowLoginPanel";
            this.ShowLoginPanel.Size = new System.Drawing.Size(117, 61);
            this.ShowLoginPanel.TabIndex = 15;
            this.ShowLoginPanel.Text = "Sign In";
            this.ShowLoginPanel.UseVisualStyleBackColor = true;
            this.ShowLoginPanel.Click += new System.EventHandler(this.ShowLoginPanel_Click);
            // 
            // ShowMainMenu
            // 
            this.ShowMainMenu.Location = new System.Drawing.Point(11, 147);
            this.ShowMainMenu.Name = "ShowMainMenu";
            this.ShowMainMenu.Size = new System.Drawing.Size(117, 61);
            this.ShowMainMenu.TabIndex = 16;
            this.ShowMainMenu.Text = "Main Menu";
            this.ShowMainMenu.UseVisualStyleBackColor = true;
            this.ShowMainMenu.Click += new System.EventHandler(this.ShowMainMenu_Click);
            // 
            // RegistrationButton
            // 
            this.RegistrationButton.Location = new System.Drawing.Point(340, 244);
            this.RegistrationButton.Name = "RegistrationButton";
            this.RegistrationButton.Size = new System.Drawing.Size(349, 60);
            this.RegistrationButton.TabIndex = 26;
            this.RegistrationButton.Text = "Sign Up";
            this.RegistrationButton.UseVisualStyleBackColor = true;
            this.RegistrationButton.Click += new System.EventHandler(this.RegistrationButton_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(17, 107);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(33, 20);
            this.label6.TabIndex = 25;
            this.label6.Text = "Key";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(17, 72);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 20);
            this.label5.TabIndex = 24;
            this.label5.Text = "Password";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(46, 20);
            this.label4.TabIndex = 23;
            this.label4.Text = "Login";
            // 
            // ConfigTokenl
            // 
            this.ConfigTokenl.AutoSize = true;
            this.ConfigTokenl.Location = new System.Drawing.Point(17, 140);
            this.ConfigTokenl.Name = "ConfigTokenl";
            this.ConfigTokenl.Size = new System.Drawing.Size(50, 20);
            this.ConfigTokenl.TabIndex = 21;
            this.ConfigTokenl.Text = "Secret";
            // 
            // RegKey
            // 
            this.RegKey.Location = new System.Drawing.Point(105, 104);
            this.RegKey.Name = "RegKey";
            this.RegKey.Size = new System.Drawing.Size(229, 27);
            this.RegKey.TabIndex = 3;
            this.RegKey.Text = "0n8sicC9Y8v3iuwtDDkJ44IO";
            // 
            // RegToken
            // 
            this.RegToken.Location = new System.Drawing.Point(105, 136);
            this.RegToken.Name = "RegToken";
            this.RegToken.Size = new System.Drawing.Size(229, 27);
            this.RegToken.TabIndex = 20;
            this.RegToken.Text = "PhVLNBRGA199lGgrQ2bbf59Ux7yRsgwkn-sfigW7rMOPoPWh";
            // 
            // RegPass
            // 
            this.RegPass.Location = new System.Drawing.Point(105, 70);
            this.RegPass.Name = "RegPass";
            this.RegPass.Size = new System.Drawing.Size(229, 27);
            this.RegPass.TabIndex = 1;
            this.RegPass.UseSystemPasswordChar = true;
            // 
            // RegLog
            // 
            this.RegLog.Location = new System.Drawing.Point(105, 38);
            this.RegLog.Name = "RegLog";
            this.RegLog.Size = new System.Drawing.Size(229, 27);
            this.RegLog.TabIndex = 0;
            // 
            // LoginButton
            // 
            this.LoginButton.Location = new System.Drawing.Point(340, 244);
            this.LoginButton.Name = "LoginButton";
            this.LoginButton.Size = new System.Drawing.Size(349, 60);
            this.LoginButton.TabIndex = 35;
            this.LoginButton.Text = "Sign In";
            this.LoginButton.UseVisualStyleBackColor = true;
            this.LoginButton.Click += new System.EventHandler(this.LoginButton_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 106);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(70, 20);
            this.label8.TabIndex = 33;
            this.label8.Text = "Password";
            // 
            // LogLogTextBox
            // 
            this.LogLogTextBox.Location = new System.Drawing.Point(83, 70);
            this.LogLogTextBox.Name = "LogLogTextBox";
            this.LogLogTextBox.Size = new System.Drawing.Size(229, 27);
            this.LogLogTextBox.TabIndex = 27;
            this.LogLogTextBox.Text = "a@mail.ru";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 72);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(46, 20);
            this.label9.TabIndex = 32;
            this.label9.Text = "Login";
            // 
            // LogPassTextBox
            // 
            this.LogPassTextBox.Location = new System.Drawing.Point(83, 103);
            this.LogPassTextBox.Name = "LogPassTextBox";
            this.LogPassTextBox.Size = new System.Drawing.Size(229, 27);
            this.LogPassTextBox.TabIndex = 28;
            this.LogPassTextBox.Text = "123";
            this.LogPassTextBox.UseSystemPasswordChar = true;
            // 
            // MainMenuGroupBox
            // 
            this.MainMenuGroupBox.Controls.Add(this.button1);
            this.MainMenuGroupBox.Controls.Add(this.RemoveMyOrdersButton);
            this.MainMenuGroupBox.Controls.Add(this.ConfigIntervalOfAnalysis);
            this.MainMenuGroupBox.Controls.Add(this.ConfigVolumeOfContracts);
            this.MainMenuGroupBox.Controls.Add(this.ConfigAlgorithmSensivity);
            this.MainMenuGroupBox.Controls.Add(this.label2);
            this.MainMenuGroupBox.Controls.Add(this.StartButton);
            this.MainMenuGroupBox.Controls.Add(this.label1);
            this.MainMenuGroupBox.Controls.Add(this.ConfigIntervalOfAnalysisl);
            this.MainMenuGroupBox.Controls.Add(this.ConfigUpdatePriceRange);
            this.MainMenuGroupBox.Controls.Add(this.ConfigAvailableBalance);
            this.MainMenuGroupBox.Controls.Add(this.ConfigRequiredProfitl);
            this.MainMenuGroupBox.Controls.Add(this.ConfigUpdatePriceRangel);
            this.MainMenuGroupBox.Controls.Add(this.ConfigRequiredProfit);
            this.MainMenuGroupBox.Controls.Add(this.ConfigVolumeOfContractsl);
            this.MainMenuGroupBox.Location = new System.Drawing.Point(145, 10);
            this.MainMenuGroupBox.Name = "MainMenuGroupBox";
            this.MainMenuGroupBox.Size = new System.Drawing.Size(695, 310);
            this.MainMenuGroupBox.TabIndex = 20;
            this.MainMenuGroupBox.TabStop = false;
            this.MainMenuGroupBox.Text = "Main Menu";
            // 
            // RemoveMyOrdersButton
            // 
            this.RemoveMyOrdersButton.Location = new System.Drawing.Point(9, 243);
            this.RemoveMyOrdersButton.Name = "RemoveMyOrdersButton";
            this.RemoveMyOrdersButton.Size = new System.Drawing.Size(144, 59);
            this.RemoveMyOrdersButton.TabIndex = 16;
            this.RemoveMyOrdersButton.Text = "Cancel my orders";
            this.RemoveMyOrdersButton.UseVisualStyleBackColor = true;
            this.RemoveMyOrdersButton.Click += new System.EventHandler(this.RemoveMyOrdersButton_Click);
            // 
            // ConfigIntervalOfAnalysis
            // 
            this.ConfigIntervalOfAnalysis.DisplayMember = "0";
            this.ConfigIntervalOfAnalysis.FormattingEnabled = true;
            this.ConfigIntervalOfAnalysis.Items.AddRange(new object[] {
            "5s",
            "30s",
            "1m",
            "3m",
            "5m",
            "15m",
            "30m",
            "1h",
            "2h",
            "3h",
            "4h",
            "6h",
            "12h",
            "1d",
            "3d",
            "1w",
            "2w",
            "1mon"});
            this.ConfigIntervalOfAnalysis.Location = new System.Drawing.Point(161, 169);
            this.ConfigIntervalOfAnalysis.Name = "ConfigIntervalOfAnalysis";
            this.ConfigIntervalOfAnalysis.Size = new System.Drawing.Size(84, 28);
            this.ConfigIntervalOfAnalysis.TabIndex = 15;
            this.ConfigIntervalOfAnalysis.Text = "5s";
            // 
            // ConfigVolumeOfContracts
            // 
            this.ConfigVolumeOfContracts.DisplayMember = "(none)";
            this.ConfigVolumeOfContracts.FormattingEnabled = true;
            this.ConfigVolumeOfContracts.Items.AddRange(new object[] {
            "100",
            "200",
            "300",
            "400",
            "500"});
            this.ConfigVolumeOfContracts.Location = new System.Drawing.Point(161, 106);
            this.ConfigVolumeOfContracts.Name = "ConfigVolumeOfContracts";
            this.ConfigVolumeOfContracts.Size = new System.Drawing.Size(84, 28);
            this.ConfigVolumeOfContracts.TabIndex = 14;
            this.ConfigVolumeOfContracts.Text = "100";
            // 
            // ConfigAlgorithmSensivity
            // 
            this.ConfigAlgorithmSensivity.DisplayMember = "0";
            this.ConfigAlgorithmSensivity.FormattingEnabled = true;
            this.ConfigAlgorithmSensivity.Items.AddRange(new object[] {
            "Minimal",
            "Medium",
            "High"});
            this.ConfigAlgorithmSensivity.Location = new System.Drawing.Point(161, 203);
            this.ConfigAlgorithmSensivity.Name = "ConfigAlgorithmSensivity";
            this.ConfigAlgorithmSensivity.Size = new System.Drawing.Size(84, 28);
            this.ConfigAlgorithmSensivity.TabIndex = 13;
            this.ConfigAlgorithmSensivity.Text = "Minimal";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 204);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(135, 20);
            this.label2.TabIndex = 12;
            this.label2.Text = "Algorithm sensivity";
            // 
            // SignUpGroupBox
            // 
            this.SignUpGroupBox.Controls.Add(this.RegistrationButton);
            this.SignUpGroupBox.Controls.Add(this.label6);
            this.SignUpGroupBox.Controls.Add(this.RegLog);
            this.SignUpGroupBox.Controls.Add(this.label5);
            this.SignUpGroupBox.Controls.Add(this.RegPass);
            this.SignUpGroupBox.Controls.Add(this.label4);
            this.SignUpGroupBox.Controls.Add(this.RegToken);
            this.SignUpGroupBox.Controls.Add(this.ConfigTokenl);
            this.SignUpGroupBox.Controls.Add(this.RegKey);
            this.SignUpGroupBox.Enabled = false;
            this.SignUpGroupBox.Location = new System.Drawing.Point(145, 10);
            this.SignUpGroupBox.Name = "SignUpGroupBox";
            this.SignUpGroupBox.Size = new System.Drawing.Size(695, 310);
            this.SignUpGroupBox.TabIndex = 21;
            this.SignUpGroupBox.TabStop = false;
            this.SignUpGroupBox.Text = "Sign Up";
            this.SignUpGroupBox.Visible = false;
            // 
            // SignInGroupBox
            // 
            this.SignInGroupBox.Controls.Add(this.LoginButton);
            this.SignInGroupBox.Controls.Add(this.LogLogTextBox);
            this.SignInGroupBox.Controls.Add(this.LogPassTextBox);
            this.SignInGroupBox.Controls.Add(this.label8);
            this.SignInGroupBox.Controls.Add(this.label9);
            this.SignInGroupBox.Enabled = false;
            this.SignInGroupBox.Location = new System.Drawing.Point(145, 10);
            this.SignInGroupBox.Name = "SignInGroupBox";
            this.SignInGroupBox.Size = new System.Drawing.Size(695, 310);
            this.SignInGroupBox.TabIndex = 22;
            this.SignInGroupBox.TabStop = false;
            this.SignInGroupBox.Text = "Sign In";
            this.SignInGroupBox.Visible = false;
            // 
            // EventConsole
            // 
            this.EventConsole.Location = new System.Drawing.Point(11, 335);
            this.EventConsole.Name = "EventConsole";
            this.EventConsole.ReadOnly = true;
            this.EventConsole.Size = new System.Drawing.Size(835, 408);
            this.EventConsole.TabIndex = 23;
            this.EventConsole.Text = "";
            // 
            // LoggedGroupBox
            // 
            this.LoggedGroupBox.Controls.Add(this.SignOutButton);
            this.LoggedGroupBox.Enabled = false;
            this.LoggedGroupBox.Location = new System.Drawing.Point(145, 12);
            this.LoggedGroupBox.Name = "LoggedGroupBox";
            this.LoggedGroupBox.Size = new System.Drawing.Size(695, 308);
            this.LoggedGroupBox.TabIndex = 24;
            this.LoggedGroupBox.TabStop = false;
            this.LoggedGroupBox.Text = "Signed in as ///";
            this.LoggedGroupBox.Visible = false;
            // 
            // SignOutButton
            // 
            this.SignOutButton.Location = new System.Drawing.Point(509, 241);
            this.SignOutButton.Name = "SignOutButton";
            this.SignOutButton.Size = new System.Drawing.Size(180, 61);
            this.SignOutButton.TabIndex = 0;
            this.SignOutButton.Text = "Sign Out";
            this.SignOutButton.UseVisualStyleBackColor = true;
            this.SignOutButton.Click += new System.EventHandler(this.SignOutButton_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(159, 243);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(175, 59);
            this.button1.TabIndex = 17;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // TradeBotUi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(858, 755);
            this.Controls.Add(this.EventConsole);
            this.Controls.Add(this.ShowMainMenu);
            this.Controls.Add(this.ShowLoginPanel);
            this.Controls.Add(this.ShowRegistrationPanel);
            this.Controls.Add(this.MainMenuGroupBox);
            this.Controls.Add(this.SignUpGroupBox);
            this.Controls.Add(this.LoggedGroupBox);
            this.Controls.Add(this.SignInGroupBox);
            this.Name = "TradeBotUi";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TradeBot";
            this.MainMenuGroupBox.ResumeLayout(false);
            this.MainMenuGroupBox.PerformLayout();
            this.SignUpGroupBox.ResumeLayout(false);
            this.SignUpGroupBox.PerformLayout();
            this.SignInGroupBox.ResumeLayout(false);
            this.SignInGroupBox.PerformLayout();
            this.LoggedGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.TextBox ConfigAvailableBalance;
        private System.Windows.Forms.TextBox ConfigRequiredProfit;
        private System.Windows.Forms.TextBox ConfigUpdatePriceRange;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label ConfigRequiredProfitl;
        private System.Windows.Forms.Label ConfigVolumeOfContractsl;
        private System.Windows.Forms.Label ConfigUpdatePriceRangel;
        private System.Windows.Forms.Label ConfigIntervalOfAnalysisl;
        private System.Windows.Forms.Button ShowRegistrationPanel;
        private System.Windows.Forms.Button ShowLoginPanel;
        private System.Windows.Forms.Button ShowMainMenu;
        private System.Windows.Forms.Label ConfigTokenl;
        private System.Windows.Forms.TextBox RegKey;
        private System.Windows.Forms.TextBox RegToken;
        private System.Windows.Forms.TextBox RegPass;
        private System.Windows.Forms.TextBox RegLog;
        private System.Windows.Forms.Button RegistrationButton;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button LoginButton;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox LogLogTextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox LogPassTextBox;
        private System.Windows.Forms.GroupBox MainMenuGroupBox;
        private System.Windows.Forms.GroupBox SignUpGroupBox;
        private System.Windows.Forms.GroupBox SignInGroupBox;
        private System.Windows.Forms.RichTextBox EventConsole;
        private System.Windows.Forms.GroupBox LoggedGroupBox;
        private System.Windows.Forms.Button SignOutButton;
        private System.Windows.Forms.ComboBox ConfigAlgorithmSensivity;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox ConfigIntervalOfAnalysis;
        private System.Windows.Forms.ComboBox ConfigVolumeOfContracts;
        private System.Windows.Forms.Button RemoveMyOrdersButton;
        private System.Windows.Forms.Button button1;
    }
}

