
namespace UI
{
    partial class TradeBotUI
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
            this.ConfigVolumeOfContracts = new System.Windows.Forms.TextBox();
            this.ConfigUpdatePriceRange = new System.Windows.Forms.TextBox();
            this.ConfigIntervalOfAnalysis = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ConfigRequiredProfitl = new System.Windows.Forms.Label();
            this.ConfigVolumeOfContractsl = new System.Windows.Forms.Label();
            this.ConfigUpdatePriceRangel = new System.Windows.Forms.Label();
            this.ConfigIntervalOfAnalysisl = new System.Windows.Forms.Label();
            this.MainMenuPanel = new System.Windows.Forms.Panel();
            this.ShowRegistrationPanel = new System.Windows.Forms.Button();
            this.ShowLoginPanel = new System.Windows.Forms.Button();
            this.ShowMainMenu = new System.Windows.Forms.Button();
            this.IsUserLogged = new System.Windows.Forms.Label();
            this.RegistrationPanel = new System.Windows.Forms.Panel();
            this.RegistrationButton = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.ConfigTokenl = new System.Windows.Forms.Label();
            this.RegKey = new System.Windows.Forms.TextBox();
            this.RegToken = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.RegPass = new System.Windows.Forms.TextBox();
            this.RegLog = new System.Windows.Forms.TextBox();
            this.LogginPanel = new System.Windows.Forms.Panel();
            this.LoginButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.LogLogTextBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.LogPassTextBox = new System.Windows.Forms.TextBox();
            this.MainMenuPanel.SuspendLayout();
            this.RegistrationPanel.SuspendLayout();
            this.LogginPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(0, 284);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(349, 60);
            this.StartButton.TabIndex = 0;
            this.StartButton.Text = "Launch Bot";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // ConfigAvailableBalance
            // 
            this.ConfigAvailableBalance.Location = new System.Drawing.Point(155, 19);
            this.ConfigAvailableBalance.Name = "ConfigAvailableBalance";
            this.ConfigAvailableBalance.Size = new System.Drawing.Size(77, 27);
            this.ConfigAvailableBalance.TabIndex = 1;
            this.ConfigAvailableBalance.Text = "0,5";
            // 
            // ConfigRequiredProfit
            // 
            this.ConfigRequiredProfit.Location = new System.Drawing.Point(155, 52);
            this.ConfigRequiredProfit.Name = "ConfigRequiredProfit";
            this.ConfigRequiredProfit.Size = new System.Drawing.Size(77, 27);
            this.ConfigRequiredProfit.TabIndex = 2;
            this.ConfigRequiredProfit.Text = "0,005";
            // 
            // ConfigVolumeOfContracts
            // 
            this.ConfigVolumeOfContracts.Location = new System.Drawing.Point(155, 84);
            this.ConfigVolumeOfContracts.Name = "ConfigVolumeOfContracts";
            this.ConfigVolumeOfContracts.Size = new System.Drawing.Size(77, 27);
            this.ConfigVolumeOfContracts.TabIndex = 3;
            this.ConfigVolumeOfContracts.Text = "100";
            // 
            // ConfigUpdatePriceRange
            // 
            this.ConfigUpdatePriceRange.Location = new System.Drawing.Point(155, 117);
            this.ConfigUpdatePriceRange.Name = "ConfigUpdatePriceRange";
            this.ConfigUpdatePriceRange.Size = new System.Drawing.Size(77, 27);
            this.ConfigUpdatePriceRange.TabIndex = 4;
            this.ConfigUpdatePriceRange.Text = "0,1";
            // 
            // ConfigIntervalOfAnalysis
            // 
            this.ConfigIntervalOfAnalysis.Location = new System.Drawing.Point(155, 149);
            this.ConfigIntervalOfAnalysis.Name = "ConfigIntervalOfAnalysis";
            this.ConfigIntervalOfAnalysis.Size = new System.Drawing.Size(77, 27);
            this.ConfigIntervalOfAnalysis.TabIndex = 5;
            this.ConfigIntervalOfAnalysis.Text = "0,1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(127, 20);
            this.label1.TabIndex = 7;
            this.label1.Text = "Available balance";
            // 
            // ConfigRequiredProfitl
            // 
            this.ConfigRequiredProfitl.AutoSize = true;
            this.ConfigRequiredProfitl.Location = new System.Drawing.Point(3, 53);
            this.ConfigRequiredProfitl.Name = "ConfigRequiredProfitl";
            this.ConfigRequiredProfitl.Size = new System.Drawing.Size(110, 20);
            this.ConfigRequiredProfitl.TabIndex = 8;
            this.ConfigRequiredProfitl.Text = "Required profit";
            // 
            // ConfigVolumeOfContractsl
            // 
            this.ConfigVolumeOfContractsl.AutoSize = true;
            this.ConfigVolumeOfContractsl.Location = new System.Drawing.Point(3, 85);
            this.ConfigVolumeOfContractsl.Name = "ConfigVolumeOfContractsl";
            this.ConfigVolumeOfContractsl.Size = new System.Drawing.Size(141, 20);
            this.ConfigVolumeOfContractsl.TabIndex = 9;
            this.ConfigVolumeOfContractsl.Text = "Volume of contracts";
            // 
            // ConfigUpdatePriceRangel
            // 
            this.ConfigUpdatePriceRangel.AutoSize = true;
            this.ConfigUpdatePriceRangel.Location = new System.Drawing.Point(3, 120);
            this.ConfigUpdatePriceRangel.Name = "ConfigUpdatePriceRangel";
            this.ConfigUpdatePriceRangel.Size = new System.Drawing.Size(137, 20);
            this.ConfigUpdatePriceRangel.TabIndex = 10;
            this.ConfigUpdatePriceRangel.Text = "Update price range";
            // 
            // ConfigIntervalOfAnalysisl
            // 
            this.ConfigIntervalOfAnalysisl.AutoSize = true;
            this.ConfigIntervalOfAnalysisl.Location = new System.Drawing.Point(3, 153);
            this.ConfigIntervalOfAnalysisl.Name = "ConfigIntervalOfAnalysisl";
            this.ConfigIntervalOfAnalysisl.Size = new System.Drawing.Size(131, 20);
            this.ConfigIntervalOfAnalysisl.TabIndex = 11;
            this.ConfigIntervalOfAnalysisl.Text = "Interval of analysis";
            // 
            // MainMenuPanel
            // 
            this.MainMenuPanel.Controls.Add(this.ConfigIntervalOfAnalysis);
            this.MainMenuPanel.Controls.Add(this.StartButton);
            this.MainMenuPanel.Controls.Add(this.ConfigIntervalOfAnalysisl);
            this.MainMenuPanel.Controls.Add(this.ConfigAvailableBalance);
            this.MainMenuPanel.Controls.Add(this.ConfigUpdatePriceRangel);
            this.MainMenuPanel.Controls.Add(this.ConfigRequiredProfit);
            this.MainMenuPanel.Controls.Add(this.ConfigVolumeOfContractsl);
            this.MainMenuPanel.Controls.Add(this.ConfigVolumeOfContracts);
            this.MainMenuPanel.Controls.Add(this.ConfigRequiredProfitl);
            this.MainMenuPanel.Controls.Add(this.ConfigUpdatePriceRange);
            this.MainMenuPanel.Controls.Add(this.label1);
            this.MainMenuPanel.Location = new System.Drawing.Point(174, 12);
            this.MainMenuPanel.Name = "MainMenuPanel";
            this.MainMenuPanel.Size = new System.Drawing.Size(349, 344);
            this.MainMenuPanel.TabIndex = 13;
            // 
            // ShowRegistrationPanel
            // 
            this.ShowRegistrationPanel.Location = new System.Drawing.Point(11, 12);
            this.ShowRegistrationPanel.Name = "ShowRegistrationPanel";
            this.ShowRegistrationPanel.Size = new System.Drawing.Size(136, 61);
            this.ShowRegistrationPanel.TabIndex = 14;
            this.ShowRegistrationPanel.Text = "Registration";
            this.ShowRegistrationPanel.UseVisualStyleBackColor = true;
            this.ShowRegistrationPanel.Click += new System.EventHandler(this.ShowRegistrationPanel_Click);
            // 
            // ShowLoginPanel
            // 
            this.ShowLoginPanel.Location = new System.Drawing.Point(11, 79);
            this.ShowLoginPanel.Name = "ShowLoginPanel";
            this.ShowLoginPanel.Size = new System.Drawing.Size(136, 61);
            this.ShowLoginPanel.TabIndex = 15;
            this.ShowLoginPanel.Text = "Login";
            this.ShowLoginPanel.UseVisualStyleBackColor = true;
            this.ShowLoginPanel.Click += new System.EventHandler(this.ShowLoginPanel_Click);
            // 
            // ShowMainMenu
            // 
            this.ShowMainMenu.Location = new System.Drawing.Point(11, 147);
            this.ShowMainMenu.Name = "ShowMainMenu";
            this.ShowMainMenu.Size = new System.Drawing.Size(136, 61);
            this.ShowMainMenu.TabIndex = 16;
            this.ShowMainMenu.Text = "Main Menu";
            this.ShowMainMenu.UseVisualStyleBackColor = true;
            this.ShowMainMenu.Click += new System.EventHandler(this.ShowMainMenu_Click);
            // 
            // IsUserLogged
            // 
            this.IsUserLogged.AutoSize = true;
            this.IsUserLogged.Location = new System.Drawing.Point(11, 336);
            this.IsUserLogged.Name = "IsUserLogged";
            this.IsUserLogged.Size = new System.Drawing.Size(38, 20);
            this.IsUserLogged.TabIndex = 17;
            this.IsUserLogged.Text = "cyka";
            // 
            // RegistrationPanel
            // 
            this.RegistrationPanel.Controls.Add(this.RegistrationButton);
            this.RegistrationPanel.Controls.Add(this.label6);
            this.RegistrationPanel.Controls.Add(this.label5);
            this.RegistrationPanel.Controls.Add(this.label4);
            this.RegistrationPanel.Controls.Add(this.ConfigTokenl);
            this.RegistrationPanel.Controls.Add(this.RegKey);
            this.RegistrationPanel.Controls.Add(this.RegToken);
            this.RegistrationPanel.Controls.Add(this.label2);
            this.RegistrationPanel.Controls.Add(this.RegPass);
            this.RegistrationPanel.Controls.Add(this.RegLog);
            this.RegistrationPanel.Location = new System.Drawing.Point(174, 12);
            this.RegistrationPanel.Name = "RegistrationPanel";
            this.RegistrationPanel.Size = new System.Drawing.Size(349, 344);
            this.RegistrationPanel.TabIndex = 18;
            this.RegistrationPanel.Visible = false;
            // 
            // RegistrationButton
            // 
            this.RegistrationButton.Location = new System.Drawing.Point(0, 284);
            this.RegistrationButton.Name = "RegistrationButton";
            this.RegistrationButton.Size = new System.Drawing.Size(349, 60);
            this.RegistrationButton.TabIndex = 26;
            this.RegistrationButton.Text = "Registration";
            this.RegistrationButton.UseVisualStyleBackColor = true;
            this.RegistrationButton.Click += new System.EventHandler(this.RegistrationButton_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 104);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(33, 20);
            this.label6.TabIndex = 25;
            this.label6.Text = "Key";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 69);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 20);
            this.label5.TabIndex = 24;
            this.label5.Text = "Password";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 37);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(46, 20);
            this.label4.TabIndex = 23;
            this.label4.Text = "Login";
            // 
            // ConfigTokenl
            // 
            this.ConfigTokenl.AutoSize = true;
            this.ConfigTokenl.Location = new System.Drawing.Point(3, 137);
            this.ConfigTokenl.Name = "ConfigTokenl";
            this.ConfigTokenl.Size = new System.Drawing.Size(50, 20);
            this.ConfigTokenl.TabIndex = 21;
            this.ConfigTokenl.Text = "Secret";
            // 
            // RegKey
            // 
            this.RegKey.Location = new System.Drawing.Point(91, 101);
            this.RegKey.Name = "RegKey";
            this.RegKey.Size = new System.Drawing.Size(229, 27);
            this.RegKey.TabIndex = 3;
            this.RegKey.Text = "0n8sicC9Y8v3iuwtDDkJ44IO";
            // 
            // RegToken
            // 
            this.RegToken.Location = new System.Drawing.Point(91, 133);
            this.RegToken.Name = "RegToken";
            this.RegToken.Size = new System.Drawing.Size(229, 27);
            this.RegToken.TabIndex = 20;
            this.RegToken.Text = "PhVLNBRGA199lGgrQ2bbf59Ux7yRsgwkn-sfigW7rMOPoPWh";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(123, 1);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(108, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "REGISTRATION";
            // 
            // RegPass
            // 
            this.RegPass.Location = new System.Drawing.Point(91, 67);
            this.RegPass.Name = "RegPass";
            this.RegPass.Size = new System.Drawing.Size(229, 27);
            this.RegPass.TabIndex = 1;
            this.RegPass.UseSystemPasswordChar = true;
            // 
            // RegLog
            // 
            this.RegLog.Location = new System.Drawing.Point(91, 35);
            this.RegLog.Name = "RegLog";
            this.RegLog.Size = new System.Drawing.Size(229, 27);
            this.RegLog.TabIndex = 0;
            // 
            // LogginPanel
            // 
            this.LogginPanel.Controls.Add(this.LoginButton);
            this.LogginPanel.Controls.Add(this.label3);
            this.LogginPanel.Controls.Add(this.label8);
            this.LogginPanel.Controls.Add(this.LogLogTextBox);
            this.LogginPanel.Controls.Add(this.label9);
            this.LogginPanel.Controls.Add(this.LogPassTextBox);
            this.LogginPanel.Location = new System.Drawing.Point(174, 12);
            this.LogginPanel.Name = "LogginPanel";
            this.LogginPanel.Size = new System.Drawing.Size(349, 344);
            this.LogginPanel.TabIndex = 19;
            this.LogginPanel.Visible = false;
            // 
            // LoginButton
            // 
            this.LoginButton.Location = new System.Drawing.Point(0, 284);
            this.LoginButton.Name = "LoginButton";
            this.LoginButton.Size = new System.Drawing.Size(349, 60);
            this.LoginButton.TabIndex = 35;
            this.LoginButton.Text = "Login";
            this.LoginButton.UseVisualStyleBackColor = true;
            this.LoginButton.Click += new System.EventHandler(this.LoginButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(155, 1);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 20);
            this.label3.TabIndex = 0;
            this.label3.Text = "LOGIN";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(2, 103);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(70, 20);
            this.label8.TabIndex = 33;
            this.label8.Text = "Password";
            // 
            // LogLogTextBox
            // 
            this.LogLogTextBox.Location = new System.Drawing.Point(79, 67);
            this.LogLogTextBox.Name = "LogLogTextBox";
            this.LogLogTextBox.Size = new System.Drawing.Size(229, 27);
            this.LogLogTextBox.TabIndex = 27;
            this.LogLogTextBox.Text = "a@mail.ru";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(2, 69);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(46, 20);
            this.label9.TabIndex = 32;
            this.label9.Text = "Login";
            // 
            // LogPassTextBox
            // 
            this.LogPassTextBox.Location = new System.Drawing.Point(79, 100);
            this.LogPassTextBox.Name = "LogPassTextBox";
            this.LogPassTextBox.Size = new System.Drawing.Size(229, 27);
            this.LogPassTextBox.TabIndex = 28;
            this.LogPassTextBox.Text = "123";
            this.LogPassTextBox.UseSystemPasswordChar = true;
            // 
            // TradeBotUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(533, 365);
            this.Controls.Add(this.IsUserLogged);
            this.Controls.Add(this.ShowMainMenu);
            this.Controls.Add(this.ShowLoginPanel);
            this.Controls.Add(this.ShowRegistrationPanel);
            this.Controls.Add(this.MainMenuPanel);
            this.Controls.Add(this.RegistrationPanel);
            this.Controls.Add(this.LogginPanel);
            this.Name = "TradeBotUI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TradeBot";
            this.MainMenuPanel.ResumeLayout(false);
            this.MainMenuPanel.PerformLayout();
            this.RegistrationPanel.ResumeLayout(false);
            this.RegistrationPanel.PerformLayout();
            this.LogginPanel.ResumeLayout(false);
            this.LogginPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.TextBox ConfigAvailableBalance;
        private System.Windows.Forms.TextBox ConfigRequiredProfit;
        private System.Windows.Forms.TextBox ConfigVolumeOfContracts;
        private System.Windows.Forms.TextBox ConfigUpdatePriceRange;
        private System.Windows.Forms.TextBox ConfigIntervalOfAnalysis;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label ConfigRequiredProfitl;
        private System.Windows.Forms.Label ConfigVolumeOfContractsl;
        private System.Windows.Forms.Label ConfigUpdatePriceRangel;
        private System.Windows.Forms.Label ConfigIntervalOfAnalysisl;
        private System.Windows.Forms.Panel MainMenuPanel;
        private System.Windows.Forms.Button ShowRegistrationPanel;
        private System.Windows.Forms.Button ShowLoginPanel;
        private System.Windows.Forms.Button ShowMainMenu;
        private System.Windows.Forms.Label IsUserLogged;
        private System.Windows.Forms.Panel RegistrationPanel;
        private System.Windows.Forms.Panel LogginPanel;
        private System.Windows.Forms.Label ConfigTokenl;
        private System.Windows.Forms.TextBox RegKey;
        private System.Windows.Forms.TextBox RegToken;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox RegPass;
        private System.Windows.Forms.TextBox RegLog;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button RegistrationButton;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button LoginButton;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox LogLogTextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox LogPassTextBox;
    }
}

