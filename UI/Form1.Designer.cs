
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
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(12, 235);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(281, 60);
            this.StartButton.TabIndex = 0;
            this.StartButton.Text = "Launch Bot";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // ConfigAvailableBalance
            // 
            this.ConfigAvailableBalance.Location = new System.Drawing.Point(163, 25);
            this.ConfigAvailableBalance.Name = "ConfigAvailableBalance";
            this.ConfigAvailableBalance.Size = new System.Drawing.Size(125, 27);
            this.ConfigAvailableBalance.TabIndex = 1;
            this.ConfigAvailableBalance.Text = "1,0";
            // 
            // ConfigRequiredProfit
            // 
            this.ConfigRequiredProfit.Location = new System.Drawing.Point(163, 58);
            this.ConfigRequiredProfit.Name = "ConfigRequiredProfit";
            this.ConfigRequiredProfit.Size = new System.Drawing.Size(125, 27);
            this.ConfigRequiredProfit.TabIndex = 2;
            this.ConfigRequiredProfit.Text = "0,01";
            // 
            // ConfigVolumeOfContracts
            // 
            this.ConfigVolumeOfContracts.Location = new System.Drawing.Point(163, 91);
            this.ConfigVolumeOfContracts.Name = "ConfigVolumeOfContracts";
            this.ConfigVolumeOfContracts.Size = new System.Drawing.Size(125, 27);
            this.ConfigVolumeOfContracts.TabIndex = 3;
            this.ConfigVolumeOfContracts.Text = "20,0";
            // 
            // ConfigUpdatePriceRange
            // 
            this.ConfigUpdatePriceRange.Location = new System.Drawing.Point(163, 124);
            this.ConfigUpdatePriceRange.Name = "ConfigUpdatePriceRange";
            this.ConfigUpdatePriceRange.Size = new System.Drawing.Size(125, 27);
            this.ConfigUpdatePriceRange.TabIndex = 4;
            this.ConfigUpdatePriceRange.Text = "0,08";
            // 
            // ConfigIntervalOfAnalysis
            // 
            this.ConfigIntervalOfAnalysis.Location = new System.Drawing.Point(163, 157);
            this.ConfigIntervalOfAnalysis.Name = "ConfigIntervalOfAnalysis";
            this.ConfigIntervalOfAnalysis.Size = new System.Drawing.Size(125, 27);
            this.ConfigIntervalOfAnalysis.TabIndex = 5;
            this.ConfigIntervalOfAnalysis.Text = "3600,0";
            // 
            // ConfigToken
            // 
            this.ConfigToken.Location = new System.Drawing.Point(163, 190);
            this.ConfigToken.Name = "ConfigToken";
            this.ConfigToken.Size = new System.Drawing.Size(125, 27);
            this.ConfigToken.TabIndex = 6;
            this.ConfigToken.Text = "token)";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(127, 20);
            this.label1.TabIndex = 7;
            this.label1.Text = "Available balance";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 20);
            this.label2.TabIndex = 8;
            this.label2.Text = "Required profit";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 94);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(141, 20);
            this.label3.TabIndex = 9;
            this.label3.Text = "Volume of contracts";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 127);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(137, 20);
            this.label4.TabIndex = 10;
            this.label4.Text = "Update price range";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 160);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(131, 20);
            this.label5.TabIndex = 11;
            this.label5.Text = "Interval of analysis";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 193);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(48, 20);
            this.label6.TabIndex = 12;
            this.label6.Text = "Token";
            // 
            // TradeBotUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(302, 304);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ConfigToken);
            this.Controls.Add(this.ConfigIntervalOfAnalysis);
            this.Controls.Add(this.ConfigUpdatePriceRange);
            this.Controls.Add(this.ConfigVolumeOfContracts);
            this.Controls.Add(this.ConfigRequiredProfit);
            this.Controls.Add(this.ConfigAvailableBalance);
            this.Controls.Add(this.StartButton);
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
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
    }
}

