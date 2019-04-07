﻿namespace ASCOM.Starbook
{
    partial class SetupDialogForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.cmdOK = new System.Windows.Forms.Button();
            this.labelIPAddress = new System.Windows.Forms.Label();
            this.picASCOM = new System.Windows.Forms.PictureBox();
            this.chkTraceLogger = new System.Windows.Forms.CheckBox();
            this.textBoxIPAddress1 = new System.Windows.Forms.TextBox();
            this.textBoxStatus = new System.Windows.Forms.TextBox();
            this.textBoxIPAddress2 = new System.Windows.Forms.TextBox();
            this.textBoxIPAddress3 = new System.Windows.Forms.TextBox();
            this.textBoxIPAddress4 = new System.Windows.Forms.TextBox();
            this.labelIPAddressDot1 = new System.Windows.Forms.Label();
            this.labelIPAddressDot2 = new System.Windows.Forms.Label();
            this.labelIPAddressDot3 = new System.Windows.Forms.Label();
            this.labelLatitude = new System.Windows.Forms.Label();
            this.labelLongitude = new System.Windows.Forms.Label();
            this.labelLatitudeDegree = new System.Windows.Forms.Label();
            this.labelLatitudeMinute = new System.Windows.Forms.Label();
            this.textBoxLatitudeDegree = new System.Windows.Forms.TextBox();
            this.textBoxLatitudeMinute = new System.Windows.Forms.TextBox();
            this.labelLongitudeDegree = new System.Windows.Forms.Label();
            this.labelLongitudeMinute = new System.Windows.Forms.Label();
            this.textBoxLongitudeDegree = new System.Windows.Forms.TextBox();
            this.textBoxLongitudeMinute = new System.Windows.Forms.TextBox();
            this.comboBoxLatitudeDirection = new System.Windows.Forms.ComboBox();
            this.comboBoxLongitudeDirection = new System.Windows.Forms.ComboBox();
            this.labelTimezone = new System.Windows.Forms.Label();
            this.textBoxTimezone = new System.Windows.Forms.TextBox();
            this.groupBoxConnection = new System.Windows.Forms.GroupBox();
            this.buttonCheck = new System.Windows.Forms.Button();
            this.groupBoxLocation = new System.Windows.Forms.GroupBox();
            this.checkBoxSetLocation = new System.Windows.Forms.CheckBox();
            this.labelTimezoneHour = new System.Windows.Forms.Label();
            this.groupBoxDateTime = new System.Windows.Forms.GroupBox();
            this.checkBoxSyncSystemTime = new System.Windows.Forms.CheckBox();
            this.checkBoxSetDateTime = new System.Windows.Forms.CheckBox();
            this.labelTime = new System.Windows.Forms.Label();
            this.labelDate = new System.Windows.Forms.Label();
            this.textBoxMinute = new System.Windows.Forms.TextBox();
            this.textBoxMonth = new System.Windows.Forms.TextBox();
            this.textBoxSecond = new System.Windows.Forms.TextBox();
            this.textBoxHour = new System.Windows.Forms.TextBox();
            this.textBoxDay = new System.Windows.Forms.TextBox();
            this.labelColon1 = new System.Windows.Forms.Label();
            this.textBoxYear = new System.Windows.Forms.TextBox();
            this.labelColon2 = new System.Windows.Forms.Label();
            this.labelSlash1 = new System.Windows.Forms.Label();
            this.labelSlash2 = new System.Windows.Forms.Label();
            this.labelFirmwareVersion = new System.Windows.Forms.Label();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.labelPlatformVersion = new System.Windows.Forms.Label();
            this.labelDriverVersion = new System.Windows.Forms.Label();
            this.labelCopyright = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.groupBoxGuideRate = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.labelGuideRate = new System.Windows.Forms.Label();
            this.textBoxGuideRate1 = new System.Windows.Forms.TextBox();
            this.textBoxGuideRate2 = new System.Windows.Forms.TextBox();
            this.textBoxGuideRate0 = new System.Windows.Forms.TextBox();
            this.comboBoxGuideRate = new System.Windows.Forms.ComboBox();
            this.textBoxGuideRate3 = new System.Windows.Forms.TextBox();
            this.textBoxGuideRate4 = new System.Windows.Forms.TextBox();
            this.textBoxGuideRate5 = new System.Windows.Forms.TextBox();
            this.textBoxGuideRate6 = new System.Windows.Forms.TextBox();
            this.textBoxGuideRate7 = new System.Windows.Forms.TextBox();
            this.textBoxGuideRate8 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBoxSetGuideRate = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).BeginInit();
            this.groupBoxConnection.SuspendLayout();
            this.groupBoxLocation.SuspendLayout();
            this.groupBoxDateTime.SuspendLayout();
            this.groupBoxGuideRate.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOK.Location = new System.Drawing.Point(586, 659);
            this.cmdOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(88, 42);
            this.cmdOK.TabIndex = 0;
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // labelIPAddress
            // 
            this.labelIPAddress.AutoSize = true;
            this.labelIPAddress.Location = new System.Drawing.Point(25, 43);
            this.labelIPAddress.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelIPAddress.Name = "labelIPAddress";
            this.labelIPAddress.Size = new System.Drawing.Size(85, 22);
            this.labelIPAddress.TabIndex = 0;
            this.labelIPAddress.Text = "IP Address";
            // 
            // picASCOM
            // 
            this.picASCOM.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.picASCOM.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picASCOM.Image = global::ASCOM.Starbook.Properties.Resources.ASCOM;
            this.picASCOM.Location = new System.Drawing.Point(283, 576);
            this.picASCOM.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.picASCOM.Name = "picASCOM";
            this.picASCOM.Size = new System.Drawing.Size(48, 56);
            this.picASCOM.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picASCOM.TabIndex = 3;
            this.picASCOM.TabStop = false;
            this.picASCOM.Click += new System.EventHandler(this.BrowseToAscom);
            this.picASCOM.DoubleClick += new System.EventHandler(this.BrowseToAscom);
            // 
            // chkTraceLogger
            // 
            this.chkTraceLogger.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.chkTraceLogger.AutoSize = true;
            this.chkTraceLogger.Location = new System.Drawing.Point(449, 668);
            this.chkTraceLogger.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkTraceLogger.Name = "chkTraceLogger";
            this.chkTraceLogger.Size = new System.Drawing.Size(129, 26);
            this.chkTraceLogger.TabIndex = 6;
            this.chkTraceLogger.Text = "Trace Logger";
            this.chkTraceLogger.UseVisualStyleBackColor = true;
            // 
            // textBoxIPAddress1
            // 
            this.textBoxIPAddress1.Location = new System.Drawing.Point(138, 40);
            this.textBoxIPAddress1.Name = "textBoxIPAddress1";
            this.textBoxIPAddress1.Size = new System.Drawing.Size(48, 29);
            this.textBoxIPAddress1.TabIndex = 2;
            // 
            // textBoxStatus
            // 
            this.textBoxStatus.Location = new System.Drawing.Point(400, 40);
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ReadOnly = true;
            this.textBoxStatus.Size = new System.Drawing.Size(127, 29);
            this.textBoxStatus.TabIndex = 9;
            this.textBoxStatus.Text = "Unknown";
            this.textBoxStatus.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxIPAddress2
            // 
            this.textBoxIPAddress2.Location = new System.Drawing.Point(204, 40);
            this.textBoxIPAddress2.Name = "textBoxIPAddress2";
            this.textBoxIPAddress2.Size = new System.Drawing.Size(48, 29);
            this.textBoxIPAddress2.TabIndex = 4;
            // 
            // textBoxIPAddress3
            // 
            this.textBoxIPAddress3.Location = new System.Drawing.Point(270, 40);
            this.textBoxIPAddress3.Name = "textBoxIPAddress3";
            this.textBoxIPAddress3.Size = new System.Drawing.Size(48, 29);
            this.textBoxIPAddress3.TabIndex = 6;
            // 
            // textBoxIPAddress4
            // 
            this.textBoxIPAddress4.Location = new System.Drawing.Point(336, 40);
            this.textBoxIPAddress4.Name = "textBoxIPAddress4";
            this.textBoxIPAddress4.Size = new System.Drawing.Size(48, 29);
            this.textBoxIPAddress4.TabIndex = 8;
            // 
            // labelIPAddressDot1
            // 
            this.labelIPAddressDot1.AutoSize = true;
            this.labelIPAddressDot1.Location = new System.Drawing.Point(188, 50);
            this.labelIPAddressDot1.Margin = new System.Windows.Forms.Padding(0);
            this.labelIPAddressDot1.Name = "labelIPAddressDot1";
            this.labelIPAddressDot1.Size = new System.Drawing.Size(15, 22);
            this.labelIPAddressDot1.TabIndex = 3;
            this.labelIPAddressDot1.Text = ".";
            // 
            // labelIPAddressDot2
            // 
            this.labelIPAddressDot2.AutoSize = true;
            this.labelIPAddressDot2.Location = new System.Drawing.Point(254, 50);
            this.labelIPAddressDot2.Margin = new System.Windows.Forms.Padding(0);
            this.labelIPAddressDot2.Name = "labelIPAddressDot2";
            this.labelIPAddressDot2.Size = new System.Drawing.Size(15, 22);
            this.labelIPAddressDot2.TabIndex = 5;
            this.labelIPAddressDot2.Text = ".";
            // 
            // labelIPAddressDot3
            // 
            this.labelIPAddressDot3.AutoSize = true;
            this.labelIPAddressDot3.Location = new System.Drawing.Point(320, 50);
            this.labelIPAddressDot3.Margin = new System.Windows.Forms.Padding(0);
            this.labelIPAddressDot3.Name = "labelIPAddressDot3";
            this.labelIPAddressDot3.Size = new System.Drawing.Size(15, 22);
            this.labelIPAddressDot3.TabIndex = 7;
            this.labelIPAddressDot3.Text = ".";
            // 
            // labelLatitude
            // 
            this.labelLatitude.AutoSize = true;
            this.labelLatitude.Location = new System.Drawing.Point(25, 40);
            this.labelLatitude.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelLatitude.Name = "labelLatitude";
            this.labelLatitude.Size = new System.Drawing.Size(70, 22);
            this.labelLatitude.TabIndex = 0;
            this.labelLatitude.Text = "Latitude";
            // 
            // labelLongitude
            // 
            this.labelLongitude.AutoSize = true;
            this.labelLongitude.Location = new System.Drawing.Point(25, 75);
            this.labelLongitude.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelLongitude.Name = "labelLongitude";
            this.labelLongitude.Size = new System.Drawing.Size(82, 22);
            this.labelLongitude.TabIndex = 0;
            this.labelLongitude.Text = "Longitude";
            // 
            // labelLatitudeDegree
            // 
            this.labelLatitudeDegree.AutoSize = true;
            this.labelLatitudeDegree.Location = new System.Drawing.Point(188, 40);
            this.labelLatitudeDegree.Margin = new System.Windows.Forms.Padding(0);
            this.labelLatitudeDegree.Name = "labelLatitudeDegree";
            this.labelLatitudeDegree.Size = new System.Drawing.Size(16, 22);
            this.labelLatitudeDegree.TabIndex = 3;
            this.labelLatitudeDegree.Text = "°";
            // 
            // labelLatitudeMinute
            // 
            this.labelLatitudeMinute.AutoSize = true;
            this.labelLatitudeMinute.Location = new System.Drawing.Point(254, 40);
            this.labelLatitudeMinute.Margin = new System.Windows.Forms.Padding(0);
            this.labelLatitudeMinute.Name = "labelLatitudeMinute";
            this.labelLatitudeMinute.Size = new System.Drawing.Size(14, 22);
            this.labelLatitudeMinute.TabIndex = 5;
            this.labelLatitudeMinute.Text = "\'";
            // 
            // textBoxLatitudeDegree
            // 
            this.textBoxLatitudeDegree.Location = new System.Drawing.Point(138, 37);
            this.textBoxLatitudeDegree.Name = "textBoxLatitudeDegree";
            this.textBoxLatitudeDegree.ReadOnly = true;
            this.textBoxLatitudeDegree.Size = new System.Drawing.Size(48, 29);
            this.textBoxLatitudeDegree.TabIndex = 2;
            this.textBoxLatitudeDegree.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxLatitudeMinute
            // 
            this.textBoxLatitudeMinute.Location = new System.Drawing.Point(204, 37);
            this.textBoxLatitudeMinute.Name = "textBoxLatitudeMinute";
            this.textBoxLatitudeMinute.ReadOnly = true;
            this.textBoxLatitudeMinute.Size = new System.Drawing.Size(48, 29);
            this.textBoxLatitudeMinute.TabIndex = 4;
            this.textBoxLatitudeMinute.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // labelLongitudeDegree
            // 
            this.labelLongitudeDegree.AutoSize = true;
            this.labelLongitudeDegree.Location = new System.Drawing.Point(188, 75);
            this.labelLongitudeDegree.Margin = new System.Windows.Forms.Padding(0);
            this.labelLongitudeDegree.Name = "labelLongitudeDegree";
            this.labelLongitudeDegree.Size = new System.Drawing.Size(16, 22);
            this.labelLongitudeDegree.TabIndex = 3;
            this.labelLongitudeDegree.Text = "°";
            // 
            // labelLongitudeMinute
            // 
            this.labelLongitudeMinute.AutoSize = true;
            this.labelLongitudeMinute.Location = new System.Drawing.Point(254, 75);
            this.labelLongitudeMinute.Margin = new System.Windows.Forms.Padding(0);
            this.labelLongitudeMinute.Name = "labelLongitudeMinute";
            this.labelLongitudeMinute.Size = new System.Drawing.Size(14, 22);
            this.labelLongitudeMinute.TabIndex = 5;
            this.labelLongitudeMinute.Text = "\'";
            // 
            // textBoxLongitudeDegree
            // 
            this.textBoxLongitudeDegree.Location = new System.Drawing.Point(138, 72);
            this.textBoxLongitudeDegree.Name = "textBoxLongitudeDegree";
            this.textBoxLongitudeDegree.ReadOnly = true;
            this.textBoxLongitudeDegree.Size = new System.Drawing.Size(48, 29);
            this.textBoxLongitudeDegree.TabIndex = 2;
            this.textBoxLongitudeDegree.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxLongitudeMinute
            // 
            this.textBoxLongitudeMinute.Location = new System.Drawing.Point(204, 72);
            this.textBoxLongitudeMinute.Name = "textBoxLongitudeMinute";
            this.textBoxLongitudeMinute.ReadOnly = true;
            this.textBoxLongitudeMinute.Size = new System.Drawing.Size(48, 29);
            this.textBoxLongitudeMinute.TabIndex = 4;
            this.textBoxLongitudeMinute.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // comboBoxLatitudeDirection
            // 
            this.comboBoxLatitudeDirection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLatitudeDirection.Enabled = false;
            this.comboBoxLatitudeDirection.FormattingEnabled = true;
            this.comboBoxLatitudeDirection.Items.AddRange(new object[] {
            "N",
            "S"});
            this.comboBoxLatitudeDirection.Location = new System.Drawing.Point(270, 37);
            this.comboBoxLatitudeDirection.Name = "comboBoxLatitudeDirection";
            this.comboBoxLatitudeDirection.Size = new System.Drawing.Size(65, 30);
            this.comboBoxLatitudeDirection.TabIndex = 10;
            // 
            // comboBoxLongitudeDirection
            // 
            this.comboBoxLongitudeDirection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLongitudeDirection.Enabled = false;
            this.comboBoxLongitudeDirection.FormattingEnabled = true;
            this.comboBoxLongitudeDirection.Items.AddRange(new object[] {
            "E",
            "W"});
            this.comboBoxLongitudeDirection.Location = new System.Drawing.Point(270, 72);
            this.comboBoxLongitudeDirection.Name = "comboBoxLongitudeDirection";
            this.comboBoxLongitudeDirection.Size = new System.Drawing.Size(65, 30);
            this.comboBoxLongitudeDirection.TabIndex = 10;
            // 
            // labelTimezone
            // 
            this.labelTimezone.AutoSize = true;
            this.labelTimezone.Location = new System.Drawing.Point(25, 109);
            this.labelTimezone.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTimezone.Name = "labelTimezone";
            this.labelTimezone.Size = new System.Drawing.Size(81, 22);
            this.labelTimezone.TabIndex = 0;
            this.labelTimezone.Text = "Timezone";
            // 
            // textBoxTimezone
            // 
            this.textBoxTimezone.Location = new System.Drawing.Point(138, 106);
            this.textBoxTimezone.Name = "textBoxTimezone";
            this.textBoxTimezone.ReadOnly = true;
            this.textBoxTimezone.Size = new System.Drawing.Size(48, 29);
            this.textBoxTimezone.TabIndex = 2;
            this.textBoxTimezone.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // groupBoxConnection
            // 
            this.groupBoxConnection.Controls.Add(this.buttonCheck);
            this.groupBoxConnection.Controls.Add(this.textBoxIPAddress4);
            this.groupBoxConnection.Controls.Add(this.labelIPAddress);
            this.groupBoxConnection.Controls.Add(this.labelIPAddressDot1);
            this.groupBoxConnection.Controls.Add(this.textBoxStatus);
            this.groupBoxConnection.Controls.Add(this.labelIPAddressDot2);
            this.groupBoxConnection.Controls.Add(this.labelIPAddressDot3);
            this.groupBoxConnection.Controls.Add(this.textBoxIPAddress1);
            this.groupBoxConnection.Controls.Add(this.textBoxIPAddress3);
            this.groupBoxConnection.Controls.Add(this.textBoxIPAddress2);
            this.groupBoxConnection.Location = new System.Drawing.Point(12, 13);
            this.groupBoxConnection.Name = "groupBoxConnection";
            this.groupBoxConnection.Size = new System.Drawing.Size(659, 88);
            this.groupBoxConnection.TabIndex = 11;
            this.groupBoxConnection.TabStop = false;
            this.groupBoxConnection.Text = "Connection Setting";
            // 
            // buttonCheck
            // 
            this.buttonCheck.Location = new System.Drawing.Point(539, 37);
            this.buttonCheck.Margin = new System.Windows.Forms.Padding(0);
            this.buttonCheck.Name = "buttonCheck";
            this.buttonCheck.Size = new System.Drawing.Size(106, 39);
            this.buttonCheck.TabIndex = 10;
            this.buttonCheck.Text = "Check";
            this.buttonCheck.UseVisualStyleBackColor = true;
            this.buttonCheck.Click += new System.EventHandler(this.buttonCheck_Click);
            // 
            // groupBoxLocation
            // 
            this.groupBoxLocation.Controls.Add(this.checkBoxSetLocation);
            this.groupBoxLocation.Controls.Add(this.textBoxTimezone);
            this.groupBoxLocation.Controls.Add(this.labelLatitude);
            this.groupBoxLocation.Controls.Add(this.labelLongitude);
            this.groupBoxLocation.Controls.Add(this.comboBoxLongitudeDirection);
            this.groupBoxLocation.Controls.Add(this.labelTimezone);
            this.groupBoxLocation.Controls.Add(this.comboBoxLatitudeDirection);
            this.groupBoxLocation.Controls.Add(this.labelLatitudeDegree);
            this.groupBoxLocation.Controls.Add(this.textBoxLongitudeMinute);
            this.groupBoxLocation.Controls.Add(this.labelTimezoneHour);
            this.groupBoxLocation.Controls.Add(this.labelLongitudeDegree);
            this.groupBoxLocation.Controls.Add(this.textBoxLatitudeMinute);
            this.groupBoxLocation.Controls.Add(this.labelLatitudeMinute);
            this.groupBoxLocation.Controls.Add(this.labelLongitudeMinute);
            this.groupBoxLocation.Controls.Add(this.textBoxLongitudeDegree);
            this.groupBoxLocation.Controls.Add(this.textBoxLatitudeDegree);
            this.groupBoxLocation.Location = new System.Drawing.Point(12, 107);
            this.groupBoxLocation.Name = "groupBoxLocation";
            this.groupBoxLocation.Size = new System.Drawing.Size(659, 154);
            this.groupBoxLocation.TabIndex = 12;
            this.groupBoxLocation.TabStop = false;
            this.groupBoxLocation.Text = "Location Setting";
            // 
            // checkBoxSetLocation
            // 
            this.checkBoxSetLocation.AutoSize = true;
            this.checkBoxSetLocation.Enabled = false;
            this.checkBoxSetLocation.Location = new System.Drawing.Point(464, 108);
            this.checkBoxSetLocation.Name = "checkBoxSetLocation";
            this.checkBoxSetLocation.Size = new System.Drawing.Size(127, 26);
            this.checkBoxSetLocation.TabIndex = 6;
            this.checkBoxSetLocation.Text = "Set Location";
            this.checkBoxSetLocation.UseVisualStyleBackColor = true;
            this.checkBoxSetLocation.CheckedChanged += new System.EventHandler(this.checkBoxSetLocation_CheckedChanged);
            // 
            // labelTimezoneHour
            // 
            this.labelTimezoneHour.AutoSize = true;
            this.labelTimezoneHour.Location = new System.Drawing.Point(188, 113);
            this.labelTimezoneHour.Margin = new System.Windows.Forms.Padding(0);
            this.labelTimezoneHour.Name = "labelTimezoneHour";
            this.labelTimezoneHour.Size = new System.Drawing.Size(19, 22);
            this.labelTimezoneHour.TabIndex = 3;
            this.labelTimezoneHour.Text = "h";
            // 
            // groupBoxDateTime
            // 
            this.groupBoxDateTime.Controls.Add(this.checkBoxSyncSystemTime);
            this.groupBoxDateTime.Controls.Add(this.checkBoxSetDateTime);
            this.groupBoxDateTime.Controls.Add(this.labelTime);
            this.groupBoxDateTime.Controls.Add(this.labelDate);
            this.groupBoxDateTime.Controls.Add(this.textBoxMinute);
            this.groupBoxDateTime.Controls.Add(this.textBoxMonth);
            this.groupBoxDateTime.Controls.Add(this.textBoxSecond);
            this.groupBoxDateTime.Controls.Add(this.textBoxHour);
            this.groupBoxDateTime.Controls.Add(this.textBoxDay);
            this.groupBoxDateTime.Controls.Add(this.labelColon1);
            this.groupBoxDateTime.Controls.Add(this.textBoxYear);
            this.groupBoxDateTime.Controls.Add(this.labelColon2);
            this.groupBoxDateTime.Controls.Add(this.labelSlash1);
            this.groupBoxDateTime.Controls.Add(this.labelSlash2);
            this.groupBoxDateTime.Location = new System.Drawing.Point(12, 280);
            this.groupBoxDateTime.Name = "groupBoxDateTime";
            this.groupBoxDateTime.Size = new System.Drawing.Size(659, 123);
            this.groupBoxDateTime.TabIndex = 12;
            this.groupBoxDateTime.TabStop = false;
            this.groupBoxDateTime.Text = "Date && Time Setting";
            // 
            // checkBoxSyncSystemTime
            // 
            this.checkBoxSyncSystemTime.AutoSize = true;
            this.checkBoxSyncSystemTime.Enabled = false;
            this.checkBoxSyncSystemTime.Location = new System.Drawing.Point(464, 75);
            this.checkBoxSyncSystemTime.Name = "checkBoxSyncSystemTime";
            this.checkBoxSyncSystemTime.Size = new System.Drawing.Size(165, 26);
            this.checkBoxSyncSystemTime.TabIndex = 6;
            this.checkBoxSyncSystemTime.Text = "Sync System Time";
            this.checkBoxSyncSystemTime.UseVisualStyleBackColor = true;
            this.checkBoxSyncSystemTime.CheckedChanged += new System.EventHandler(this.checkBoxSyncSystemTime_CheckedChanged);
            // 
            // checkBoxSetDateTime
            // 
            this.checkBoxSetDateTime.AutoSize = true;
            this.checkBoxSetDateTime.Enabled = false;
            this.checkBoxSetDateTime.Location = new System.Drawing.Point(464, 40);
            this.checkBoxSetDateTime.Name = "checkBoxSetDateTime";
            this.checkBoxSetDateTime.Size = new System.Drawing.Size(154, 26);
            this.checkBoxSetDateTime.TabIndex = 6;
            this.checkBoxSetDateTime.Text = "Set Date && Time";
            this.checkBoxSetDateTime.UseVisualStyleBackColor = true;
            this.checkBoxSetDateTime.CheckedChanged += new System.EventHandler(this.checkBoxSetDateTime_CheckedChanged);
            // 
            // labelTime
            // 
            this.labelTime.AutoSize = true;
            this.labelTime.Location = new System.Drawing.Point(25, 75);
            this.labelTime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTime.Name = "labelTime";
            this.labelTime.Size = new System.Drawing.Size(46, 22);
            this.labelTime.TabIndex = 0;
            this.labelTime.Text = "Time";
            // 
            // labelDate
            // 
            this.labelDate.AutoSize = true;
            this.labelDate.Location = new System.Drawing.Point(25, 40);
            this.labelDate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDate.Name = "labelDate";
            this.labelDate.Size = new System.Drawing.Size(45, 22);
            this.labelDate.TabIndex = 0;
            this.labelDate.Text = "Date";
            // 
            // textBoxMinute
            // 
            this.textBoxMinute.Location = new System.Drawing.Point(204, 72);
            this.textBoxMinute.Name = "textBoxMinute";
            this.textBoxMinute.ReadOnly = true;
            this.textBoxMinute.Size = new System.Drawing.Size(48, 29);
            this.textBoxMinute.TabIndex = 4;
            this.textBoxMinute.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxMonth
            // 
            this.textBoxMonth.Location = new System.Drawing.Point(204, 37);
            this.textBoxMonth.Name = "textBoxMonth";
            this.textBoxMonth.ReadOnly = true;
            this.textBoxMonth.Size = new System.Drawing.Size(48, 29);
            this.textBoxMonth.TabIndex = 4;
            this.textBoxMonth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxSecond
            // 
            this.textBoxSecond.Location = new System.Drawing.Point(271, 72);
            this.textBoxSecond.Name = "textBoxSecond";
            this.textBoxSecond.ReadOnly = true;
            this.textBoxSecond.Size = new System.Drawing.Size(48, 29);
            this.textBoxSecond.TabIndex = 2;
            this.textBoxSecond.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxHour
            // 
            this.textBoxHour.Location = new System.Drawing.Point(138, 72);
            this.textBoxHour.Name = "textBoxHour";
            this.textBoxHour.ReadOnly = true;
            this.textBoxHour.Size = new System.Drawing.Size(48, 29);
            this.textBoxHour.TabIndex = 2;
            this.textBoxHour.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxDay
            // 
            this.textBoxDay.Location = new System.Drawing.Point(271, 37);
            this.textBoxDay.Name = "textBoxDay";
            this.textBoxDay.ReadOnly = true;
            this.textBoxDay.Size = new System.Drawing.Size(48, 29);
            this.textBoxDay.TabIndex = 2;
            this.textBoxDay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // labelColon1
            // 
            this.labelColon1.AutoSize = true;
            this.labelColon1.Location = new System.Drawing.Point(188, 75);
            this.labelColon1.Margin = new System.Windows.Forms.Padding(0);
            this.labelColon1.Name = "labelColon1";
            this.labelColon1.Size = new System.Drawing.Size(15, 22);
            this.labelColon1.TabIndex = 3;
            this.labelColon1.Text = ":";
            // 
            // textBoxYear
            // 
            this.textBoxYear.Location = new System.Drawing.Point(138, 37);
            this.textBoxYear.Name = "textBoxYear";
            this.textBoxYear.ReadOnly = true;
            this.textBoxYear.Size = new System.Drawing.Size(48, 29);
            this.textBoxYear.TabIndex = 2;
            this.textBoxYear.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // labelColon2
            // 
            this.labelColon2.AutoSize = true;
            this.labelColon2.Location = new System.Drawing.Point(254, 75);
            this.labelColon2.Margin = new System.Windows.Forms.Padding(0);
            this.labelColon2.Name = "labelColon2";
            this.labelColon2.Size = new System.Drawing.Size(15, 22);
            this.labelColon2.TabIndex = 5;
            this.labelColon2.Text = ":";
            // 
            // labelSlash1
            // 
            this.labelSlash1.AutoSize = true;
            this.labelSlash1.Location = new System.Drawing.Point(188, 40);
            this.labelSlash1.Margin = new System.Windows.Forms.Padding(0);
            this.labelSlash1.Name = "labelSlash1";
            this.labelSlash1.Size = new System.Drawing.Size(17, 22);
            this.labelSlash1.TabIndex = 3;
            this.labelSlash1.Text = "/";
            // 
            // labelSlash2
            // 
            this.labelSlash2.AutoSize = true;
            this.labelSlash2.Location = new System.Drawing.Point(254, 40);
            this.labelSlash2.Margin = new System.Windows.Forms.Padding(0);
            this.labelSlash2.Name = "labelSlash2";
            this.labelSlash2.Size = new System.Drawing.Size(17, 22);
            this.labelSlash2.TabIndex = 5;
            this.labelSlash2.Text = "/";
            // 
            // labelFirmwareVersion
            // 
            this.labelFirmwareVersion.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.labelFirmwareVersion.AutoSize = true;
            this.labelFirmwareVersion.Location = new System.Drawing.Point(13, 576);
            this.labelFirmwareVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelFirmwareVersion.Name = "labelFirmwareVersion";
            this.labelFirmwareVersion.Size = new System.Drawing.Size(142, 22);
            this.labelFirmwareVersion.TabIndex = 0;
            this.labelFirmwareVersion.Text = "Firmware Version:";
            // 
            // timer
            // 
            this.timer.Interval = 500;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // labelPlatformVersion
            // 
            this.labelPlatformVersion.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.labelPlatformVersion.AutoSize = true;
            this.labelPlatformVersion.Location = new System.Drawing.Point(13, 607);
            this.labelPlatformVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPlatformVersion.Name = "labelPlatformVersion";
            this.labelPlatformVersion.Size = new System.Drawing.Size(136, 22);
            this.labelPlatformVersion.TabIndex = 0;
            this.labelPlatformVersion.Text = "Platform Version:";
            // 
            // labelDriverVersion
            // 
            this.labelDriverVersion.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.labelDriverVersion.AutoSize = true;
            this.labelDriverVersion.Location = new System.Drawing.Point(13, 635);
            this.labelDriverVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDriverVersion.Name = "labelDriverVersion";
            this.labelDriverVersion.Size = new System.Drawing.Size(117, 22);
            this.labelDriverVersion.TabIndex = 0;
            this.labelDriverVersion.Text = "Driver Version:";
            // 
            // labelCopyright
            // 
            this.labelCopyright.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.labelCopyright.AutoSize = true;
            this.labelCopyright.Location = new System.Drawing.Point(363, 576);
            this.labelCopyright.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelCopyright.Name = "labelCopyright";
            this.labelCopyright.Size = new System.Drawing.Size(254, 22);
            this.labelCopyright.TabIndex = 0;
            this.labelCopyright.Text = "Copyright © 2019 Lung-Kai Cheng";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label1.AutoSize = true;
            this.label1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label1.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.Highlight;
            this.label1.Location = new System.Drawing.Point(363, 606);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(175, 22);
            this.label1.TabIndex = 0;
            this.label1.Text = "lkcheng89@gmail.com";
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(586, 659);
            this.cmdCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(88, 42);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Visible = false;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // groupBoxGuideRate
            // 
            this.groupBoxGuideRate.Controls.Add(this.label3);
            this.groupBoxGuideRate.Controls.Add(this.checkBoxSetGuideRate);
            this.groupBoxGuideRate.Controls.Add(this.label4);
            this.groupBoxGuideRate.Controls.Add(this.label2);
            this.groupBoxGuideRate.Controls.Add(this.labelGuideRate);
            this.groupBoxGuideRate.Controls.Add(this.textBoxGuideRate1);
            this.groupBoxGuideRate.Controls.Add(this.textBoxGuideRate8);
            this.groupBoxGuideRate.Controls.Add(this.textBoxGuideRate7);
            this.groupBoxGuideRate.Controls.Add(this.textBoxGuideRate6);
            this.groupBoxGuideRate.Controls.Add(this.textBoxGuideRate5);
            this.groupBoxGuideRate.Controls.Add(this.textBoxGuideRate4);
            this.groupBoxGuideRate.Controls.Add(this.textBoxGuideRate3);
            this.groupBoxGuideRate.Controls.Add(this.textBoxGuideRate2);
            this.groupBoxGuideRate.Controls.Add(this.comboBoxGuideRate);
            this.groupBoxGuideRate.Controls.Add(this.textBoxGuideRate0);
            this.groupBoxGuideRate.Location = new System.Drawing.Point(12, 409);
            this.groupBoxGuideRate.Name = "groupBoxGuideRate";
            this.groupBoxGuideRate.Size = new System.Drawing.Size(659, 155);
            this.groupBoxGuideRate.TabIndex = 12;
            this.groupBoxGuideRate.TabStop = false;
            this.groupBoxGuideRate.Text = "Pulse Guide Setting";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 75);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 22);
            this.label2.TabIndex = 0;
            this.label2.Text = "Guide Rates";
            // 
            // labelGuideRate
            // 
            this.labelGuideRate.AutoSize = true;
            this.labelGuideRate.Location = new System.Drawing.Point(25, 40);
            this.labelGuideRate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelGuideRate.Name = "labelGuideRate";
            this.labelGuideRate.Size = new System.Drawing.Size(90, 22);
            this.labelGuideRate.TabIndex = 0;
            this.labelGuideRate.Text = "Guide Rate";
            // 
            // textBoxGuideRate1
            // 
            this.textBoxGuideRate1.Location = new System.Drawing.Point(192, 72);
            this.textBoxGuideRate1.Name = "textBoxGuideRate1";
            this.textBoxGuideRate1.Size = new System.Drawing.Size(48, 29);
            this.textBoxGuideRate1.TabIndex = 4;
            this.textBoxGuideRate1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxGuideRate2
            // 
            this.textBoxGuideRate2.Location = new System.Drawing.Point(246, 72);
            this.textBoxGuideRate2.Name = "textBoxGuideRate2";
            this.textBoxGuideRate2.Size = new System.Drawing.Size(48, 29);
            this.textBoxGuideRate2.TabIndex = 2;
            this.textBoxGuideRate2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxGuideRate0
            // 
            this.textBoxGuideRate0.Location = new System.Drawing.Point(138, 72);
            this.textBoxGuideRate0.Name = "textBoxGuideRate0";
            this.textBoxGuideRate0.Size = new System.Drawing.Size(48, 29);
            this.textBoxGuideRate0.TabIndex = 2;
            this.textBoxGuideRate0.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // comboBoxGuideRate
            // 
            this.comboBoxGuideRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxGuideRate.FormattingEnabled = true;
            this.comboBoxGuideRate.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8"});
            this.comboBoxGuideRate.Location = new System.Drawing.Point(138, 37);
            this.comboBoxGuideRate.Name = "comboBoxGuideRate";
            this.comboBoxGuideRate.Size = new System.Drawing.Size(48, 30);
            this.comboBoxGuideRate.TabIndex = 10;
            // 
            // textBoxGuideRate3
            // 
            this.textBoxGuideRate3.Location = new System.Drawing.Point(300, 72);
            this.textBoxGuideRate3.Name = "textBoxGuideRate3";
            this.textBoxGuideRate3.Size = new System.Drawing.Size(48, 29);
            this.textBoxGuideRate3.TabIndex = 2;
            this.textBoxGuideRate3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxGuideRate4
            // 
            this.textBoxGuideRate4.Location = new System.Drawing.Point(354, 72);
            this.textBoxGuideRate4.Name = "textBoxGuideRate4";
            this.textBoxGuideRate4.Size = new System.Drawing.Size(48, 29);
            this.textBoxGuideRate4.TabIndex = 2;
            this.textBoxGuideRate4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxGuideRate5
            // 
            this.textBoxGuideRate5.Location = new System.Drawing.Point(408, 72);
            this.textBoxGuideRate5.Name = "textBoxGuideRate5";
            this.textBoxGuideRate5.Size = new System.Drawing.Size(48, 29);
            this.textBoxGuideRate5.TabIndex = 2;
            this.textBoxGuideRate5.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxGuideRate6
            // 
            this.textBoxGuideRate6.Location = new System.Drawing.Point(462, 72);
            this.textBoxGuideRate6.Name = "textBoxGuideRate6";
            this.textBoxGuideRate6.Size = new System.Drawing.Size(48, 29);
            this.textBoxGuideRate6.TabIndex = 2;
            this.textBoxGuideRate6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxGuideRate7
            // 
            this.textBoxGuideRate7.Location = new System.Drawing.Point(516, 72);
            this.textBoxGuideRate7.Name = "textBoxGuideRate7";
            this.textBoxGuideRate7.Size = new System.Drawing.Size(48, 29);
            this.textBoxGuideRate7.TabIndex = 2;
            this.textBoxGuideRate7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxGuideRate8
            // 
            this.textBoxGuideRate8.Location = new System.Drawing.Point(570, 72);
            this.textBoxGuideRate8.Name = "textBoxGuideRate8";
            this.textBoxGuideRate8.Size = new System.Drawing.Size(48, 29);
            this.textBoxGuideRate8.TabIndex = 2;
            this.textBoxGuideRate8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(134, 113);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(199, 22);
            this.label3.TabIndex = 0;
            this.label3.Text = "X = Times of Sidereal Rate";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(622, 79);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(19, 22);
            this.label4.TabIndex = 0;
            this.label4.Text = "X";
            // 
            // checkBoxSetGuideRate
            // 
            this.checkBoxSetGuideRate.AutoSize = true;
            this.checkBoxSetGuideRate.Checked = true;
            this.checkBoxSetGuideRate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSetGuideRate.Enabled = false;
            this.checkBoxSetGuideRate.Location = new System.Drawing.Point(464, 112);
            this.checkBoxSetGuideRate.Name = "checkBoxSetGuideRate";
            this.checkBoxSetGuideRate.Size = new System.Drawing.Size(143, 26);
            this.checkBoxSetGuideRate.TabIndex = 6;
            this.checkBoxSetGuideRate.Text = "Set Guide Rate";
            this.checkBoxSetGuideRate.UseVisualStyleBackColor = true;
            this.checkBoxSetGuideRate.CheckedChanged += new System.EventHandler(this.checkBoxSetDateTime_CheckedChanged);
            // 
            // SetupDialogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(687, 718);
            this.Controls.Add(this.groupBoxGuideRate);
            this.Controls.Add(this.groupBoxDateTime);
            this.Controls.Add(this.groupBoxLocation);
            this.Controls.Add(this.labelDriverVersion);
            this.Controls.Add(this.labelPlatformVersion);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelCopyright);
            this.Controls.Add(this.labelFirmwareVersion);
            this.Controls.Add(this.groupBoxConnection);
            this.Controls.Add(this.chkTraceLogger);
            this.Controls.Add(this.picASCOM);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.cmdCancel);
            this.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetupDialogForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Starbook Driver Setup";
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).EndInit();
            this.groupBoxConnection.ResumeLayout(false);
            this.groupBoxConnection.PerformLayout();
            this.groupBoxLocation.ResumeLayout(false);
            this.groupBoxLocation.PerformLayout();
            this.groupBoxDateTime.ResumeLayout(false);
            this.groupBoxDateTime.PerformLayout();
            this.groupBoxGuideRate.ResumeLayout(false);
            this.groupBoxGuideRate.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Label labelIPAddress;
        private System.Windows.Forms.PictureBox picASCOM;
        private System.Windows.Forms.CheckBox chkTraceLogger;
        private System.Windows.Forms.TextBox textBoxIPAddress1;
        private System.Windows.Forms.TextBox textBoxStatus;
        private System.Windows.Forms.TextBox textBoxIPAddress2;
        private System.Windows.Forms.TextBox textBoxIPAddress3;
        private System.Windows.Forms.TextBox textBoxIPAddress4;
        private System.Windows.Forms.Label labelIPAddressDot1;
        private System.Windows.Forms.Label labelIPAddressDot2;
        private System.Windows.Forms.Label labelIPAddressDot3;
        private System.Windows.Forms.Label labelLatitude;
        private System.Windows.Forms.Label labelLongitude;
        private System.Windows.Forms.Label labelLatitudeDegree;
        private System.Windows.Forms.Label labelLatitudeMinute;
        private System.Windows.Forms.TextBox textBoxLatitudeDegree;
        private System.Windows.Forms.TextBox textBoxLatitudeMinute;
        private System.Windows.Forms.Label labelLongitudeDegree;
        private System.Windows.Forms.Label labelLongitudeMinute;
        private System.Windows.Forms.TextBox textBoxLongitudeDegree;
        private System.Windows.Forms.TextBox textBoxLongitudeMinute;
        private System.Windows.Forms.ComboBox comboBoxLatitudeDirection;
        private System.Windows.Forms.ComboBox comboBoxLongitudeDirection;
        private System.Windows.Forms.Label labelTimezone;
        private System.Windows.Forms.TextBox textBoxTimezone;
        private System.Windows.Forms.GroupBox groupBoxConnection;
        private System.Windows.Forms.GroupBox groupBoxLocation;
        private System.Windows.Forms.GroupBox groupBoxDateTime;
        private System.Windows.Forms.Label labelTime;
        private System.Windows.Forms.Label labelDate;
        private System.Windows.Forms.TextBox textBoxMinute;
        private System.Windows.Forms.TextBox textBoxMonth;
        private System.Windows.Forms.TextBox textBoxSecond;
        private System.Windows.Forms.TextBox textBoxHour;
        private System.Windows.Forms.TextBox textBoxDay;
        private System.Windows.Forms.Label labelColon1;
        private System.Windows.Forms.TextBox textBoxYear;
        private System.Windows.Forms.Label labelColon2;
        private System.Windows.Forms.Label labelSlash1;
        private System.Windows.Forms.Label labelSlash2;
        private System.Windows.Forms.CheckBox checkBoxSetDateTime;
        private System.Windows.Forms.Button buttonCheck;
        private System.Windows.Forms.CheckBox checkBoxSetLocation;
        private System.Windows.Forms.CheckBox checkBoxSyncSystemTime;
        private System.Windows.Forms.Label labelFirmwareVersion;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.Label labelPlatformVersion;
        private System.Windows.Forms.Label labelDriverVersion;
        private System.Windows.Forms.Label labelTimezoneHour;
        private System.Windows.Forms.Label labelCopyright;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.GroupBox groupBoxGuideRate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelGuideRate;
        private System.Windows.Forms.TextBox textBoxGuideRate1;
        private System.Windows.Forms.TextBox textBoxGuideRate2;
        private System.Windows.Forms.TextBox textBoxGuideRate0;
        private System.Windows.Forms.TextBox textBoxGuideRate7;
        private System.Windows.Forms.TextBox textBoxGuideRate6;
        private System.Windows.Forms.TextBox textBoxGuideRate5;
        private System.Windows.Forms.TextBox textBoxGuideRate4;
        private System.Windows.Forms.TextBox textBoxGuideRate3;
        private System.Windows.Forms.ComboBox comboBoxGuideRate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxGuideRate8;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBoxSetGuideRate;
    }
}