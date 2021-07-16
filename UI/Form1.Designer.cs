
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
            this.ConfigToken = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ConfigRequiredProfitl = new System.Windows.Forms.Label();
            this.ConfigVolumeOfContractsl = new System.Windows.Forms.Label();
            this.ConfigUpdatePriceRangel = new System.Windows.Forms.Label();
            this.ConfigIntervalOfAnalysisl = new System.Windows.Forms.Label();
            this.ConfigTokenl = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(10, 176);
            this.StartButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(246, 45);
            this.StartButton.TabIndex = 0;
            this.StartButton.Text = "Launch Bot";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // ConfigAvailableBalance
            // 
            this.ConfigAvailableBalance.Location = new System.Drawing.Point(143, 19);
            this.ConfigAvailableBalance.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ConfigAvailableBalance.Name = "ConfigAvailableBalance";
            this.ConfigAvailableBalance.Size = new System.Drawing.Size(110, 23);
            this.ConfigAvailableBalance.TabIndex = 1;
            this.ConfigAvailableBalance.Text = "0,1";
            // 
            // ConfigRequiredProfit
            // 
            this.ConfigRequiredProfit.Location = new System.Drawing.Point(143, 44);
            this.ConfigRequiredProfit.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ConfigRequiredProfit.Name = "ConfigRequiredProfit";
            this.ConfigRequiredProfit.Size = new System.Drawing.Size(110, 23);
            this.ConfigRequiredProfit.TabIndex = 2;
            this.ConfigRequiredProfit.Text = "0,1";
            // 
            // ConfigVolumeOfContracts
            // 
            this.ConfigVolumeOfContracts.Location = new System.Drawing.Point(143, 68);
            this.ConfigVolumeOfContracts.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ConfigVolumeOfContracts.Name = "ConfigVolumeOfContracts";
            this.ConfigVolumeOfContracts.Size = new System.Drawing.Size(110, 23);
            this.ConfigVolumeOfContracts.TabIndex = 3;
            this.ConfigVolumeOfContracts.Text = "0,1";
            // 
            // ConfigUpdatePriceRange
            // 
            this.ConfigUpdatePriceRange.Location = new System.Drawing.Point(143, 93);
            this.ConfigUpdatePriceRange.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ConfigUpdatePriceRange.Name = "ConfigUpdatePriceRange";
            this.ConfigUpdatePriceRange.Size = new System.Drawing.Size(110, 23);
            this.ConfigUpdatePriceRange.TabIndex = 4;
            this.ConfigUpdatePriceRange.Text = "0,1";
            // 
            // ConfigIntervalOfAnalysis
            // 
            this.ConfigIntervalOfAnalysis.Location = new System.Drawing.Point(143, 118);
            this.ConfigIntervalOfAnalysis.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ConfigIntervalOfAnalysis.Name = "ConfigIntervalOfAnalysis";
            this.ConfigIntervalOfAnalysis.Size = new System.Drawing.Size(110, 23);
            this.ConfigIntervalOfAnalysis.TabIndex = 5;
            this.ConfigIntervalOfAnalysis.Text = "0,1";
            // 
            // ConfigToken
            // 
            this.ConfigToken.Location = new System.Drawing.Point(143, 142);
            this.ConfigToken.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ConfigToken.Name = "ConfigToken";
            this.ConfigToken.Size = new System.Drawing.Size(110, 23);
            this.ConfigToken.TabIndex = 6;
            this.ConfigToken.Text = "tokencool";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 15);
            this.label1.TabIndex = 7;
            this.label1.Text = "Available balance";
            // 
            // ConfigRequiredProfitl
            // 
            this.ConfigRequiredProfitl.AutoSize = true;
            this.ConfigRequiredProfitl.Location = new System.Drawing.Point(10, 46);
            this.ConfigRequiredProfitl.Name = "ConfigRequiredProfitl";
            this.ConfigRequiredProfitl.Size = new System.Drawing.Size(86, 15);
            this.ConfigRequiredProfitl.TabIndex = 8;
            this.ConfigRequiredProfitl.Text = "Required profit";
            // 
            // ConfigVolumeOfContractsl
            // 
            this.ConfigVolumeOfContractsl.AutoSize = true;
            this.ConfigVolumeOfContractsl.Location = new System.Drawing.Point(10, 70);
            this.ConfigVolumeOfContractsl.Name = "ConfigVolumeOfContractsl";
            this.ConfigVolumeOfContractsl.Size = new System.Drawing.Size(113, 15);
            this.ConfigVolumeOfContractsl.TabIndex = 9;
            this.ConfigVolumeOfContractsl.Text = "Volume of contracts";
            // 
            // ConfigUpdatePriceRangel
            // 
            this.ConfigUpdatePriceRangel.AutoSize = true;
            this.ConfigUpdatePriceRangel.Location = new System.Drawing.Point(10, 95);
            this.ConfigUpdatePriceRangel.Name = "ConfigUpdatePriceRangel";
            this.ConfigUpdatePriceRangel.Size = new System.Drawing.Size(107, 15);
            this.ConfigUpdatePriceRangel.TabIndex = 10;
            this.ConfigUpdatePriceRangel.Text = "Update price range";
            // 
            // ConfigIntervalOfAnalysisl
            // 
            this.ConfigIntervalOfAnalysisl.AutoSize = true;
            this.ConfigIntervalOfAnalysisl.Location = new System.Drawing.Point(10, 120);
            this.ConfigIntervalOfAnalysisl.Name = "ConfigIntervalOfAnalysisl";
            this.ConfigIntervalOfAnalysisl.Size = new System.Drawing.Size(104, 15);
            this.ConfigIntervalOfAnalysisl.TabIndex = 11;
            this.ConfigIntervalOfAnalysisl.Text = "Interval of analysis";
            // 
            // ConfigTokenl
            // 
            this.ConfigTokenl.AutoSize = true;
            this.ConfigTokenl.Location = new System.Drawing.Point(10, 145);
            this.ConfigTokenl.Name = "ConfigTokenl";
            this.ConfigTokenl.Size = new System.Drawing.Size(38, 15);
            this.ConfigTokenl.TabIndex = 12;
            this.ConfigTokenl.Text = "Token";
            // 
            // TradeBotUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(264, 228);
            this.Controls.Add(this.ConfigTokenl);
            this.Controls.Add(this.ConfigIntervalOfAnalysisl);
            this.Controls.Add(this.ConfigUpdatePriceRangel);
            this.Controls.Add(this.ConfigVolumeOfContractsl);
            this.Controls.Add(this.ConfigRequiredProfitl);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ConfigToken);
            this.Controls.Add(this.ConfigIntervalOfAnalysis);
            this.Controls.Add(this.ConfigUpdatePriceRange);
            this.Controls.Add(this.ConfigVolumeOfContracts);
            this.Controls.Add(this.ConfigRequiredProfit);
            this.Controls.Add(this.ConfigAvailableBalance);
            this.Controls.Add(this.StartButton);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "TradeBotUI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TradeBot";
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
        private System.Windows.Forms.TextBox ConfigToken;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label ConfigRequiredProfitl;
        private System.Windows.Forms.Label ConfigVolumeOfContractsl;
        private System.Windows.Forms.Label ConfigUpdatePriceRangel;
        private System.Windows.Forms.Label ConfigIntervalOfAnalysisl;
        private System.Windows.Forms.Label ConfigTokenl;
    }
}

