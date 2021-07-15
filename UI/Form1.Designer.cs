
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
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ConfigRequiredProfit = new System.Windows.Forms.Label();
            this.ConfigVolumeOfContracts = new System.Windows.Forms.Label();
            this.ConfigUpdatePriceRange = new System.Windows.Forms.Label();
            this.ConfigIntervalOfAnalysis = new System.Windows.Forms.Label();
            this.ConfigToken = new System.Windows.Forms.Label();
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
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(163, 58);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(125, 27);
            this.textBox2.TabIndex = 2;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(163, 91);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(125, 27);
            this.textBox3.TabIndex = 3;
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(163, 124);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(125, 27);
            this.textBox4.TabIndex = 4;
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(163, 157);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(125, 27);
            this.textBox5.TabIndex = 5;
            // 
            // textBox6
            // 
            this.textBox6.Location = new System.Drawing.Point(163, 190);
            this.textBox6.Name = "textBox6";
            this.textBox6.Size = new System.Drawing.Size(125, 27);
            this.textBox6.TabIndex = 6;
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
            // ConfigRequiredProfit
            // 
            this.ConfigRequiredProfit.AutoSize = true;
            this.ConfigRequiredProfit.Location = new System.Drawing.Point(12, 61);
            this.ConfigRequiredProfit.Name = "ConfigRequiredProfit";
            this.ConfigRequiredProfit.Size = new System.Drawing.Size(110, 20);
            this.ConfigRequiredProfit.TabIndex = 8;
            this.ConfigRequiredProfit.Text = "Required profit";
            // 
            // ConfigVolumeOfContracts
            // 
            this.ConfigVolumeOfContracts.AutoSize = true;
            this.ConfigVolumeOfContracts.Location = new System.Drawing.Point(12, 94);
            this.ConfigVolumeOfContracts.Name = "ConfigVolumeOfContracts";
            this.ConfigVolumeOfContracts.Size = new System.Drawing.Size(141, 20);
            this.ConfigVolumeOfContracts.TabIndex = 9;
            this.ConfigVolumeOfContracts.Text = "Volume of contracts";
            // 
            // ConfigUpdatePriceRange
            // 
            this.ConfigUpdatePriceRange.AutoSize = true;
            this.ConfigUpdatePriceRange.Location = new System.Drawing.Point(12, 127);
            this.ConfigUpdatePriceRange.Name = "ConfigUpdatePriceRange";
            this.ConfigUpdatePriceRange.Size = new System.Drawing.Size(137, 20);
            this.ConfigUpdatePriceRange.TabIndex = 10;
            this.ConfigUpdatePriceRange.Text = "Update price range";
            // 
            // ConfigIntervalOfAnalysis
            // 
            this.ConfigIntervalOfAnalysis.AutoSize = true;
            this.ConfigIntervalOfAnalysis.Location = new System.Drawing.Point(12, 160);
            this.ConfigIntervalOfAnalysis.Name = "ConfigIntervalOfAnalysis";
            this.ConfigIntervalOfAnalysis.Size = new System.Drawing.Size(131, 20);
            this.ConfigIntervalOfAnalysis.TabIndex = 11;
            this.ConfigIntervalOfAnalysis.Text = "Interval of analysis";
            // 
            // ConfigToken
            // 
            this.ConfigToken.AutoSize = true;
            this.ConfigToken.Location = new System.Drawing.Point(12, 193);
            this.ConfigToken.Name = "ConfigToken";
            this.ConfigToken.Size = new System.Drawing.Size(48, 20);
            this.ConfigToken.TabIndex = 12;
            this.ConfigToken.Text = "Token";
            // 
            // TradeBotUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(302, 304);
            this.Controls.Add(this.ConfigToken);
            this.Controls.Add(this.ConfigIntervalOfAnalysis);
            this.Controls.Add(this.ConfigUpdatePriceRange);
            this.Controls.Add(this.ConfigVolumeOfContracts);
            this.Controls.Add(this.ConfigRequiredProfit);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox6);
            this.Controls.Add(this.textBox5);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.textBox2);
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
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label ConfigRequiredProfit;
        private System.Windows.Forms.Label ConfigVolumeOfContracts;
        private System.Windows.Forms.Label ConfigUpdatePriceRange;
        private System.Windows.Forms.Label ConfigIntervalOfAnalysis;
        private System.Windows.Forms.Label ConfigToken;
    }
}

