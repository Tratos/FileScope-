// WizardDlg.cs
// Copyright (C) 2002 Matt Zyzik (www.FileScope.com)
// 
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace FileScope
{
	/// <summary>
	/// Initial wizard configuration window for FileScope.
	/// </summary>
	public class WizardDlg : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.Button button6;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TabPage tabPage4;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Button button7;
		private System.Windows.Forms.Button button8;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.RadioButton radioDialup;
		private System.Windows.Forms.RadioButton radioCable;
		private System.Windows.Forms.RadioButton radioT3;
		private System.Windows.Forms.RadioButton radioT1;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button buttonBrowse;
		private System.Windows.Forms.TextBox textDownload;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Button button9;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button button10;
		private System.Windows.Forms.ListBox listShares;
		private System.Windows.Forms.GroupBox groupBox22;
		private System.Windows.Forms.GroupBox groupBox17;
		private System.Windows.Forms.CheckBox chkFirewall;
		private System.Windows.Forms.Label label2343;
		private System.Windows.Forms.TextBox textBoxPort;
		private System.ComponentModel.Container components = null;

		public WizardDlg()
		{
			InitializeComponent();
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.button2 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.groupBox17 = new System.Windows.Forms.GroupBox();
			this.textBoxPort = new System.Windows.Forms.TextBox();
			this.label2343 = new System.Windows.Forms.Label();
			this.chkFirewall = new System.Windows.Forms.CheckBox();
			this.groupBox22 = new System.Windows.Forms.GroupBox();
			this.radioT3 = new System.Windows.Forms.RadioButton();
			this.radioT1 = new System.Windows.Forms.RadioButton();
			this.radioCable = new System.Windows.Forms.RadioButton();
			this.radioDialup = new System.Windows.Forms.RadioButton();
			this.button3 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.listShares = new System.Windows.Forms.ListBox();
			this.button10 = new System.Windows.Forms.Button();
			this.button9 = new System.Windows.Forms.Button();
			this.label6 = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.buttonBrowse = new System.Windows.Forms.Button();
			this.textDownload = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.button5 = new System.Windows.Forms.Button();
			this.button6 = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.tabPage4 = new System.Windows.Forms.TabPage();
			this.label7 = new System.Windows.Forms.Label();
			this.button7 = new System.Windows.Forms.Button();
			this.button8 = new System.Windows.Forms.Button();
			this.label8 = new System.Windows.Forms.Label();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.groupBox17.SuspendLayout();
			this.groupBox22.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.tabPage4.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Appearance = System.Windows.Forms.TabAppearance.Buttons;
			this.tabControl1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.tabPage1,
																					  this.tabPage2,
																					  this.tabPage3,
																					  this.tabPage4});
			this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.tabControl1.Location = new System.Drawing.Point(0, -24);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(432, 376);
			this.tabControl1.TabIndex = 2;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.AddRange(new System.Windows.Forms.Control[] {
																				   this.button2,
																				   this.button1,
																				   this.label3});
			this.tabPage1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.tabPage1.Location = new System.Drawing.Point(4, 25);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(424, 347);
			this.tabPage1.TabIndex = 0;
			// 
			// button2
			// 
			this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.button2.Location = new System.Drawing.Point(224, 312);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(88, 24);
			this.button2.TabIndex = 2;
			this.button2.Text = "Next";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// button1
			// 
			this.button1.Enabled = false;
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.button1.Location = new System.Drawing.Point(112, 312);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(88, 24);
			this.button1.TabIndex = 3;
			this.button1.Text = "Previous";
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label3.Location = new System.Drawing.Point(24, 72);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(376, 192);
			this.label3.TabIndex = 1;
			this.label3.Text = "Welcome to the FileScope Wizard. This wizard will provide the necessary steps to " +
				"configure your FileScope program. You will only have to run this wizard once. Cl" +
				"ick the \"Next\" button at the bottom of this window when you are ready to continu" +
				"e.";
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.AddRange(new System.Windows.Forms.Control[] {
																				   this.groupBox17,
																				   this.groupBox22,
																				   this.button3,
																				   this.button4,
																				   this.label1});
			this.tabPage2.Location = new System.Drawing.Point(4, 25);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(424, 347);
			this.tabPage2.TabIndex = 1;
			// 
			// groupBox17
			// 
			this.groupBox17.Controls.AddRange(new System.Windows.Forms.Control[] {
																					 this.textBoxPort,
																					 this.label2343,
																					 this.chkFirewall});
			this.groupBox17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.groupBox17.Location = new System.Drawing.Point(56, 200);
			this.groupBox17.Name = "groupBox17";
			this.groupBox17.Size = new System.Drawing.Size(312, 88);
			this.groupBox17.TabIndex = 10;
			this.groupBox17.TabStop = false;
			this.groupBox17.Text = "Incoming Connections";
			// 
			// textBoxPort
			// 
			this.textBoxPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.textBoxPort.Location = new System.Drawing.Point(170, 61);
			this.textBoxPort.Name = "textBoxPort";
			this.textBoxPort.Size = new System.Drawing.Size(46, 20);
			this.textBoxPort.TabIndex = 2;
			this.textBoxPort.Text = "6346";
			// 
			// label2343
			// 
			this.label2343.AutoSize = true;
			this.label2343.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label2343.Location = new System.Drawing.Point(32, 64);
			this.label2343.Name = "label2343";
			this.label2343.Size = new System.Drawing.Size(139, 13);
			this.label2343.TabIndex = 1;
			this.label2343.Text = "Incoming connections port:";
			// 
			// chkFirewall
			// 
			this.chkFirewall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.chkFirewall.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.chkFirewall.Location = new System.Drawing.Point(32, 24);
			this.chkFirewall.Name = "chkFirewall";
			this.chkFirewall.Size = new System.Drawing.Size(256, 32);
			this.chkFirewall.TabIndex = 0;
			this.chkFirewall.Text = "I know that I am behind a firewall and I cannot accept incoming connections";
			// 
			// groupBox22
			// 
			this.groupBox22.Controls.AddRange(new System.Windows.Forms.Control[] {
																					 this.radioT3,
																					 this.radioT1,
																					 this.radioCable,
																					 this.radioDialup});
			this.groupBox22.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.groupBox22.Location = new System.Drawing.Point(56, 64);
			this.groupBox22.Name = "groupBox22";
			this.groupBox22.Size = new System.Drawing.Size(312, 120);
			this.groupBox22.TabIndex = 9;
			this.groupBox22.TabStop = false;
			this.groupBox22.Text = "Connection Type";
			// 
			// radioT3
			// 
			this.radioT3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioT3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.radioT3.Location = new System.Drawing.Point(96, 96);
			this.radioT3.Name = "radioT3";
			this.radioT3.Size = new System.Drawing.Size(120, 16);
			this.radioT3.TabIndex = 4;
			this.radioT3.Text = "T3 Line or higher";
			// 
			// radioT1
			// 
			this.radioT1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioT1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.radioT1.Location = new System.Drawing.Point(96, 72);
			this.radioT1.Name = "radioT1";
			this.radioT1.Size = new System.Drawing.Size(120, 16);
			this.radioT1.TabIndex = 3;
			this.radioT1.Text = "T1 Line";
			// 
			// radioCable
			// 
			this.radioCable.Checked = true;
			this.radioCable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioCable.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.radioCable.Location = new System.Drawing.Point(96, 48);
			this.radioCable.Name = "radioCable";
			this.radioCable.Size = new System.Drawing.Size(120, 16);
			this.radioCable.TabIndex = 1;
			this.radioCable.TabStop = true;
			this.radioCable.Text = "Cable/DSL Modem";
			// 
			// radioDialup
			// 
			this.radioDialup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioDialup.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.radioDialup.Location = new System.Drawing.Point(96, 24);
			this.radioDialup.Name = "radioDialup";
			this.radioDialup.Size = new System.Drawing.Size(120, 16);
			this.radioDialup.TabIndex = 0;
			this.radioDialup.Text = "Dialup Modem";
			// 
			// button3
			// 
			this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.button3.Location = new System.Drawing.Point(224, 312);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(88, 24);
			this.button3.TabIndex = 4;
			this.button3.Text = "Next";
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// button4
			// 
			this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.button4.Location = new System.Drawing.Point(112, 312);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(88, 24);
			this.button4.TabIndex = 5;
			this.button4.Text = "Previous";
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(192, 22);
			this.label1.TabIndex = 1;
			this.label1.Text = "Internet Connection";
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.AddRange(new System.Windows.Forms.Control[] {
																				   this.groupBox4,
																				   this.groupBox3,
																				   this.button5,
																				   this.button6,
																				   this.label4});
			this.tabPage3.Location = new System.Drawing.Point(4, 25);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(424, 347);
			this.tabPage3.TabIndex = 2;
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.listShares,
																					this.button10,
																					this.button9,
																					this.label6});
			this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.groupBox4.Location = new System.Drawing.Point(24, 120);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(376, 184);
			this.groupBox4.TabIndex = 10;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Shared Directories";
			// 
			// listShares
			// 
			this.listShares.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.listShares.Location = new System.Drawing.Point(8, 56);
			this.listShares.Name = "listShares";
			this.listShares.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.listShares.Size = new System.Drawing.Size(360, 121);
			this.listShares.TabIndex = 4;
			// 
			// button10
			// 
			this.button10.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.button10.Location = new System.Drawing.Point(192, 32);
			this.button10.Name = "button10";
			this.button10.Size = new System.Drawing.Size(144, 20);
			this.button10.TabIndex = 3;
			this.button10.Text = "Remove selected folders";
			this.button10.Click += new System.EventHandler(this.button10_Click);
			// 
			// button9
			// 
			this.button9.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.button9.Location = new System.Drawing.Point(40, 32);
			this.button9.Name = "button9";
			this.button9.Size = new System.Drawing.Size(136, 20);
			this.button9.TabIndex = 2;
			this.button9.Text = "Add a folder to share";
			this.button9.Click += new System.EventHandler(this.button9_Click);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label6.Location = new System.Drawing.Point(96, 16);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(192, 13);
			this.label6.TabIndex = 0;
			this.label6.Text = "What folders would you like to share?";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.buttonBrowse,
																					this.textDownload,
																					this.label5});
			this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.groupBox3.Location = new System.Drawing.Point(24, 48);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(376, 64);
			this.groupBox3.TabIndex = 9;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Download Directory";
			// 
			// buttonBrowse
			// 
			this.buttonBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonBrowse.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.buttonBrowse.Location = new System.Drawing.Point(304, 40);
			this.buttonBrowse.Name = "buttonBrowse";
			this.buttonBrowse.Size = new System.Drawing.Size(56, 20);
			this.buttonBrowse.TabIndex = 2;
			this.buttonBrowse.Text = "Browse";
			this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
			// 
			// textDownload
			// 
			this.textDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.textDownload.Location = new System.Drawing.Point(16, 40);
			this.textDownload.Name = "textDownload";
			this.textDownload.Size = new System.Drawing.Size(288, 20);
			this.textDownload.TabIndex = 1;
			this.textDownload.Text = "";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label5.Location = new System.Drawing.Point(16, 22);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(242, 13);
			this.label5.TabIndex = 0;
			this.label5.Text = "Directory where downloaded files will be saved:";
			// 
			// button5
			// 
			this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.button5.Location = new System.Drawing.Point(224, 312);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(88, 24);
			this.button5.TabIndex = 7;
			this.button5.Text = "Next";
			this.button5.Click += new System.EventHandler(this.button5_Click);
			// 
			// button6
			// 
			this.button6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.button6.Location = new System.Drawing.Point(112, 312);
			this.button6.Name = "button6";
			this.button6.Size = new System.Drawing.Size(88, 24);
			this.button6.TabIndex = 8;
			this.button6.Text = "Previous";
			this.button6.Click += new System.EventHandler(this.button6_Click);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.BackColor = System.Drawing.Color.Transparent;
			this.label4.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label4.Location = new System.Drawing.Point(16, 16);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(109, 22);
			this.label4.TabIndex = 6;
			this.label4.Text = "Directories";
			// 
			// tabPage4
			// 
			this.tabPage4.Controls.AddRange(new System.Windows.Forms.Control[] {
																				   this.label7,
																				   this.button7,
																				   this.button8,
																				   this.label8});
			this.tabPage4.Location = new System.Drawing.Point(4, 25);
			this.tabPage4.Name = "tabPage4";
			this.tabPage4.Size = new System.Drawing.Size(424, 347);
			this.tabPage4.TabIndex = 3;
			// 
			// label7
			// 
			this.label7.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label7.Location = new System.Drawing.Point(56, 120);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(328, 96);
			this.label7.TabIndex = 13;
			this.label7.Text = "Just remember that you can change any of these settings in the \"Options\" window a" +
				"ccessible from the Main menu of FileScope.";
			// 
			// button7
			// 
			this.button7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.button7.Location = new System.Drawing.Point(224, 312);
			this.button7.Name = "button7";
			this.button7.Size = new System.Drawing.Size(88, 24);
			this.button7.TabIndex = 11;
			this.button7.Text = "Finish";
			this.button7.Click += new System.EventHandler(this.button7_Click);
			// 
			// button8
			// 
			this.button8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.button8.Location = new System.Drawing.Point(112, 312);
			this.button8.Name = "button8";
			this.button8.Size = new System.Drawing.Size(88, 24);
			this.button8.TabIndex = 12;
			this.button8.Text = "Previous";
			this.button8.Click += new System.EventHandler(this.button8_Click);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.BackColor = System.Drawing.Color.Transparent;
			this.label8.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label8.Location = new System.Drawing.Point(16, 13);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(128, 22);
			this.label8.TabIndex = 10;
			this.label8.Text = "You\'re Done!";
			// 
			// WizardDlg
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(432, 354);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.tabControl1});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "WizardDlg";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "FileScope Wizard";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.wizard_Closing);
			this.Load += new System.EventHandler(this.wizard_Load);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.groupBox17.ResumeLayout(false);
			this.groupBox22.ResumeLayout(false);
			this.tabPage3.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.tabPage4.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void button2_Click(object sender, System.EventArgs e)
		{
			//Next button on Page1
			tabControl1.SelectedTab = tabPage2;
		}

		private void button3_Click(object sender, System.EventArgs e)
		{
			//Next button on Page2
			try
			{
				int portNum = Convert.ToInt32(textBoxPort.Text);
				if(portNum < 10 || portNum > 65530)
				{
					MessageBox.Show("Invalid port number!");
					return;
				}
			}
			catch
			{
				MessageBox.Show("Invalid port number!");
				return;
			}
			tabControl1.SelectedTab = tabPage3;
		}

		private void button5_Click(object sender, System.EventArgs e)
		{
			//Next button on Page3
			if(!System.IO.Directory.Exists(textDownload.Text))
			{
				MessageBox.Show("The download directory does not exist!");
				return;
			}
			if(!listShares.Items.Contains(textDownload.Text))
				listShares.Items.Add(textDownload.Text);
			tabControl1.SelectedTab = tabPage4;
		}

		private void button4_Click(object sender, System.EventArgs e)
		{
			//Previous button on Page2
			tabControl1.SelectedTab = tabPage1;
		}

		private void button6_Click(object sender, System.EventArgs e)
		{
			//Previous button on Page3
			tabControl1.SelectedTab = tabPage2;
		}

		private void button8_Click(object sender, System.EventArgs e)
		{
			//Previous button on Page4
			tabControl1.SelectedTab = tabPage3;
		}

		private void wizard_Load(object sender, System.EventArgs e)
		{
			textDownload.Text = Utils.GetCurrentPath("downloads");
		}

		private void buttonBrowse_Click(object sender, System.EventArgs e)
		{
			//browse for download folder
			BrowseForFolder pickFolder = new BrowseForFolder();
			string strDownload = pickFolder.Show("Pick your download directory");
			if(strDownload.Length > 0)
				textDownload.Text = strDownload;
		}

		private void button9_Click(object sender, System.EventArgs e)
		{
			//browse for share folders
			BrowseForFolder pickFolder = new BrowseForFolder();
			string strDir = pickFolder.Show("Pick a folder to share");
			if(strDir.Length > 0)
				listShares.Items.Add(strDir);
		}

		private void button10_Click(object sender, System.EventArgs e)
		{
			//Remove selected shares
			for(int count = listShares.SelectedIndices.Count - 1; count >= 0; count--)
				listShares.Items.RemoveAt(listShares.SelectedIndices[count]);
		}

		private void button7_Click(object sender, System.EventArgs e)
		{
			//Finish button on Page4
			if(radioDialup.Checked)
			{
				Stats.settings.connectionType = EnumConnectionType.dialUp;
				Stats.settings.maxUploads = 2;
			}
			else if(radioCable.Checked)
			{
				Stats.settings.connectionType = EnumConnectionType.cable;
				Stats.settings.maxUploads = 10;
			}
			else if(radioT1.Checked)
			{
				Stats.settings.connectionType = EnumConnectionType.t1;
				Stats.settings.maxUploads = 20;
			}
			else
			{
				Stats.settings.connectionType = EnumConnectionType.t3;
				Stats.settings.maxUploads = 30;
			}
			//not ultrapeer by default
			Stats.settings.ultrapeerCapable = Stats.Updated.Gnutella2.ultrapeer = Stats.Updated.Gnutella.ultrapeer = false;
			//send all of the shares to Stats.shareList
			Stats.shareList.AddRange(listShares.Items);
			//download directory
			Stats.settings.dlDirectory = textDownload.Text;
			//we don't know our public IP yet
			System.Net.IPHostEntry ipEntry = System.Net.Dns.Resolve(System.Net.Dns.GetHostName());
			Stats.settings.ipAddress = ipEntry.AddressList[0].ToString();
			//get permanent GUID
			Stats.settings.myGUID = GUID.newGUID();
			//initial connections to keep when ultrapeer
			Stats.settings.gConnectionsToKeep = 200;
			//behind firewall?
			Stats.settings.firewall = chkFirewall.Checked;
			//enable filescope update notifications
			Stats.settings.updateNotify = true;
			//not always on top
			Stats.settings.alwaysOnTop = false;
			//switch to transfers view on a new download
			Stats.settings.switchTransfers = true;
			//minimizing and closing
			Stats.settings.minimizeNormal = false;
			Stats.settings.closeNormal = true;
			//incoming connections port
			Stats.settings.port = Convert.ToInt32(textBoxPort.Text);
			//allow ultrapeer by default
			Stats.settings.allowUltrapeer = true;
			//allow chat by default
			Stats.settings.allowChats = true;
			//connect to both services by default
			Stats.settings.autoGnutella = false;
			Stats.settings.autoGnutella2 = true;
			Stats.settings.autoEDonkey = false;
			Stats.settings.autoOpenNap = false;
			//alert by default about dangerous files
			Stats.settings.fileAlert = true;
			//alert by default when cancelling downloads
			Stats.settings.cancelDLAlert = true;
			//don't clear transfers by default
			Stats.settings.clearDl = false;
			Stats.settings.clearUp = false;
			//main window settings
			Stats.settings.mainHeight = 0;
			Stats.settings.mainMax = false;
			Stats.settings.mainWidth = 0;
			//good splitter percentage
			Stats.settings.transSplitPerc = .6F;
			//setup colors
			Themes.SetColors("Default Colors");
			//serialize the settings
			Stats.LoadSave.SaveSettings();
			//serialize the share list
			Stats.LoadSave.SaveShares();
			//serialize the empty host list
			Stats.LoadSave.SaveHosts();
			//serialize the empty blocked host list
			Stats.LoadSave.SaveBlockedChatHosts();
			//web caches
			Stats.gnutellaWebCache.Add("http://a.gnu.crysm.net/gcache.php");
			Stats.gnutellaWebCache.Add("http://cfx.domainchen.de/gwebc/gcache.php");
			Stats.gnutellaWebCache.Add("http://gwebcache.skysys.org/gcache.php");
			Stats.gnutellaWebCache.Add("http://kjellman.com/gcache/gcache.php");
			Stats.gnutellaWebCache.Add("http://serverbeest.mine.nu/heinjan/gwebcache/gcache.php");
			Stats.gnutellaWebCache.Add("http://webpages.charter.net/mlatin/gwebcache/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.arne-nickel.de/gnucleus/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.cultiv8r.com/gcache/gcache.cfm");
			Stats.gnutellaWebCache.Add("http://www.dors.de/gcache/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.geeknik.net/gnucache/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.gnucleus.net/gcache/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.padar.com/gwebcache/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.puterdocs.com/gcache/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.sweeveworld.com/~tolkien/gnucache/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.techdudez.com/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.wgops.com/gwebcache/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.x3k6a2.net/GnuCache_0.5.2/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.zero-g.net/gcache/gcache.php");
			Stats.gnutellaWebCache.Add("http://andrewhitchcock.org/gnucache/index.php");
			Stats.gnutellaWebCache.Add("http://suspiciousdisco.com/gnucache/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.apexo.de/gnucache/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.frission.net/gcache/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.sven-herrmann.net/doetsch.info/gnucleus/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.mfux.ch/GnuCache/gcache.php");
			Stats.gnutellaWebCache.Add("http://jh-webservice.de/gnucache/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.tripadelic.com/gnucache/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.johnellis.net/gcache/gcache.php");
			Stats.gnutellaWebCache.Add("http://arras.sytes.net/gwebcache/gcache.php");
			Stats.gnutellaWebCache.Add("http://inerciasensorial.com.br/gwebcache/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.nuffsed.net/gnuc/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.php50.com/gwebcache/index.php");
			Stats.gnutellaWebCache.Add("http://www.php50.com/godfella/gcache.php");
			Stats.gnutellaWebCache.Add("http://gc.ibel.de/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.blar.de/gwebcache/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.blar.de/gnetcache/index.php");
			Stats.gnutellaWebCache.Add("http://www.riverstyx.com/gcache.php");
			Stats.gnutellaWebCache.Add("http://conny.cistron.de/gnutella/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.wordreserve.com/gnucache/gcache.php");
			Stats.gnutellaWebCache.Add("http://yosoy.problematico.com/index.php");
			Stats.gnutellaWebCache.Add("http://www.sermo.net/gnuwebcache/gcache.php");
			Stats.gnutellaWebCache.Add("http://gnucache.l33t.ca/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.perser.org/gcache/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.cgamalaysia.org/gweb/gcache.php");
			Stats.gnutellaWebCache.Add("http://contextcraft.com/gwebcache/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.nickcatalano.com/GNUCACHE/gcache.php");
			Stats.gnutellaWebCache.Add("http://burneyfireems.org/111/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.likaos.com/gnucleus/gcache.php");
			Stats.gnutellaWebCache.Add("http://gnutella-cache.no-ip.com/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.vertsync.com/gwebcache/gcache.php");
			Stats.gnutellaWebCache.Add("http://zetafleet.com/gWebCache.php");
			Stats.gnutellaWebCache.Add("http://www.miamimicro.com/GWebCache/bequiet.asp");
			Stats.gnutellaWebCache.Add("http://www.zetafleet.com/gWebCache.php");
			Stats.gnutellaWebCache.Add("http://www.php.ee/gcache/index.php");
			Stats.gnutellaWebCache.Add("http://www.looten.net/gwebcache/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.stevesnexus.com/gcache.php");
			Stats.gnutellaWebCache.Add("http://www.blackgate.net/gcache.php");
			Stats.gnutella2WebCache.Add("http://www20.brinkster.com/dgc2/lynnx.asp");
			Stats.gnutella2WebCache.Add("http://user1.7host.com/dgwc2/lynnx.asp");
			Stats.gnutella2WebCache.Add("http://gwebcache2.jonatkins.com/cgi-bin/gwebcache.cgi");
			Stats.gnutella2WebCache.Add("http://www.newsability.com/Services/GWC2/lynn.asp");
			Stats.gnutella2WebCache.Add("http://g2cache.theg2.net/gwcache/lynnx.asp");
			Stats.gnutella2WebCache.Add("http://gwebcache5.jonatkins.com/cgi-bin/perlgcache.cgi");
			//serialize the web cache we just made
			Stats.LoadSave.SaveWebCache();
			//prepare everything
			Stats.InitializeVariables();
			Stats.LoadSave.UpdateShares();
			//open main window
			StartApp.CreateMainWindow();
			//close this window
			closingTime = true;
			this.Close();
		}

		bool closingTime = false;

		private void wizard_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			//can't close the wizard
			if(!closingTime)
				e.Cancel = true;
		}
	}
}
