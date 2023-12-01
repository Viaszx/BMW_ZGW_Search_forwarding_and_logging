/*
 * Created in SharpDevelop.
 * User: Vias
 * Date: 01/01/2023
 */
 
namespace BMW_ZGW_Search
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Button btnSearch;
		private System.Windows.Forms.Button CloseButton;
		private System.Windows.Forms.Button StartStopButton_;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.CheckBox OutputCheckBox;
		private System.Windows.Forms.Button MinimizeButton;

		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
				private void InitializeComponent()
		{
			this.btnSearch = new System.Windows.Forms.Button();
			this.CloseButton = new System.Windows.Forms.Button();
			this.StartStopButton_ = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.OutputCheckBox = new System.Windows.Forms.CheckBox();
			this.MinimizeButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnSearch
			// 
			this.btnSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.btnSearch.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnSearch.ForeColor = System.Drawing.SystemColors.Window;
			this.btnSearch.Location = new System.Drawing.Point(6, 2);
			this.btnSearch.Name = "btnSearch";
			this.btnSearch.Size = new System.Drawing.Size(107, 29);
			this.btnSearch.TabIndex = 1;
			this.btnSearch.Text = "Search ZGW";
			this.btnSearch.UseVisualStyleBackColor = false;
			this.btnSearch.Click += new System.EventHandler(this.BtnSearchClick);
			// 
			// CloseButton
			// 
			this.CloseButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.CloseButton.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.CloseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.CloseButton.Font = new System.Drawing.Font("Tahoma", 8.129032F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.CloseButton.ForeColor = System.Drawing.SystemColors.Window;
			this.CloseButton.Location = new System.Drawing.Point(375, 2);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = new System.Drawing.Size(28, 29);
			this.CloseButton.TabIndex = 127;
			this.CloseButton.Text = "X";
			this.CloseButton.UseVisualStyleBackColor = false;
			this.CloseButton.Click += new System.EventHandler(this.CloseButtonClick);
			// 
			// StartStopButton_
			// 
			this.StartStopButton_.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.StartStopButton_.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.StartStopButton_.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.StartStopButton_.ForeColor = System.Drawing.SystemColors.Window;
			this.StartStopButton_.Location = new System.Drawing.Point(119, 2);
			this.StartStopButton_.Name = "StartStopButton_";
			this.StartStopButton_.Size = new System.Drawing.Size(145, 29);
			this.StartStopButton_.TabIndex = 128;
			this.StartStopButton_.Text = "Start Forwarding";
			this.StartStopButton_.UseVisualStyleBackColor = false;
			this.StartStopButton_.Visible = false;
			this.StartStopButton_.Click += new System.EventHandler(this.StartStopButton_Click);
			// 
			// textBox1
			// 
			this.textBox1.Enabled = false;
			this.textBox1.Location = new System.Drawing.Point(6, 35);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.Size = new System.Drawing.Size(399, 218);
			this.textBox1.TabIndex = 130;
			// 
			// OutputCheckBox
			// 
			this.OutputCheckBox.Location = new System.Drawing.Point(270, 2);
			this.OutputCheckBox.Name = "OutputCheckBox";
			this.OutputCheckBox.Size = new System.Drawing.Size(60, 27);
			this.OutputCheckBox.TabIndex = 131;
			this.OutputCheckBox.Text = "Log";
			this.OutputCheckBox.UseVisualStyleBackColor = true;
			this.OutputCheckBox.Visible = false;
			// 
			// MinimizeButton
			// 
			this.MinimizeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.MinimizeButton.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.MinimizeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.MinimizeButton.Font = new System.Drawing.Font("Tahoma", 8.129032F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.MinimizeButton.ForeColor = System.Drawing.SystemColors.Window;
			this.MinimizeButton.Location = new System.Drawing.Point(341, 2);
			this.MinimizeButton.Name = "MinimizeButton";
			this.MinimizeButton.Size = new System.Drawing.Size(28, 29);
			this.MinimizeButton.TabIndex = 132;
			this.MinimizeButton.Text = "—";
			this.MinimizeButton.UseVisualStyleBackColor = false;
			this.MinimizeButton.Click += new System.EventHandler(this.MinimizeButtonClick);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.ClientSize = new System.Drawing.Size(411, 259);
			this.Controls.Add(this.MinimizeButton);
			this.Controls.Add(this.OutputCheckBox);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.StartStopButton_);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.btnSearch);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MainForm";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "BMW ZGW";
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
