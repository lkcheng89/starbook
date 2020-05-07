using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ASCOM.Utilities;
using ASCOM.Starbook;
using System.Net;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace ASCOM.Starbook
{
    [ComVisible(false)]					// Form not registered for COM!
    public partial class SetupDialogForm : Form
    {
        public SetupDialogForm()
        {
            InitializeComponent();
            InitializeComponentProfile();
        }

        private void InitializeComponentProfile()
        {
            // set the list of com ports to those that are currently available

            byte[] ipAddress = Telescope.starbook.IPAddress.GetAddressBytes();

            if (ipAddress.Length == 4)
            {
                this.textBoxIPAddress1.Text = ipAddress[0].ToString(CultureInfo.InvariantCulture);
                this.textBoxIPAddress2.Text = ipAddress[1].ToString(CultureInfo.InvariantCulture);
                this.textBoxIPAddress3.Text = ipAddress[2].ToString(CultureInfo.InvariantCulture);
                this.textBoxIPAddress4.Text = ipAddress[3].ToString(CultureInfo.InvariantCulture);
            }

            comboBoxGuideRate.SelectedIndex = Telescope.guideRate;

            textBoxGuideRate0.Text = Telescope.guideRates[0].ToString(CultureInfo.InvariantCulture);
            textBoxGuideRate1.Text = Telescope.guideRates[1].ToString(CultureInfo.InvariantCulture);
            textBoxGuideRate2.Text = Telescope.guideRates[2].ToString(CultureInfo.InvariantCulture);
            textBoxGuideRate3.Text = Telescope.guideRates[3].ToString(CultureInfo.InvariantCulture);
            textBoxGuideRate4.Text = Telescope.guideRates[4].ToString(CultureInfo.InvariantCulture);
            textBoxGuideRate5.Text = Telescope.guideRates[5].ToString(CultureInfo.InvariantCulture);
            textBoxGuideRate6.Text = Telescope.guideRates[6].ToString(CultureInfo.InvariantCulture);
            textBoxGuideRate7.Text = Telescope.guideRates[7].ToString(CultureInfo.InvariantCulture);
            textBoxGuideRate8.Text = Telescope.guideRates[8].ToString(CultureInfo.InvariantCulture);

            checkBoxJ2000.Checked = Telescope.j2000;
            checkBoxAutoMeridianFlip.Checked = Telescope.autoMeridianFlip > 0;
            checkBoxTraceLogger.Checked = Telescope.traceLogger.Enabled;

            Util util = new Util();

            if (util.ServicePack > 0)
            {
                labelPlatformVersion.Text = string.Format(CultureInfo.InvariantCulture, "Platform Version: {0}.{1} SP{2}", util.MajorVersion, util.MinorVersion, util.ServicePack);
            }
            else
            {
                labelPlatformVersion.Text = string.Format(CultureInfo.InvariantCulture, "Platform Version: {0}.{1}", util.MajorVersion, util.MinorVersion);
            }

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            labelDriverVersion.Text = string.Format(CultureInfo.InvariantCulture, "Driver Version: {0}.{1}", version.Major, version.Minor);

            /////

            textBoxIPAddress1.TextChanged += buttonApply_Monitored;
            textBoxIPAddress2.TextChanged += buttonApply_Monitored;
            textBoxIPAddress3.TextChanged += buttonApply_Monitored;
            textBoxIPAddress4.TextChanged += buttonApply_Monitored;

            textBoxLatitudeDegree.TextChanged += buttonApply_Monitored;
            textBoxLatitudeMinute.TextChanged += buttonApply_Monitored;
            comboBoxLatitudeDirection.SelectedIndexChanged += buttonApply_Monitored;
            textBoxLongitudeDegree.TextChanged += buttonApply_Monitored;
            textBoxLongitudeMinute.TextChanged += buttonApply_Monitored;
            comboBoxLongitudeDirection.SelectedIndexChanged += buttonApply_Monitored;
            checkBoxSetLocation.CheckedChanged += buttonApply_Monitored;

            textBoxYear.TextChanged += buttonApply_Monitored;
            textBoxMonth.TextChanged += buttonApply_Monitored;
            textBoxDay.TextChanged += buttonApply_Monitored;
            textBoxHour.TextChanged += buttonApply_Monitored;
            textBoxMinute.TextChanged += buttonApply_Monitored;
            textBoxSecond.TextChanged += buttonApply_Monitored;
            checkBoxSetDateTime.CheckedChanged += buttonApply_Monitored;
            checkBoxSyncSystemTime.CheckedChanged += buttonApply_Monitored;

            comboBoxGuideRate.SelectedIndexChanged += buttonApply_Monitored;
            textBoxGuideRate0.TextChanged += buttonApply_Monitored;
            textBoxGuideRate1.TextChanged += buttonApply_Monitored;
            textBoxGuideRate2.TextChanged += buttonApply_Monitored;
            textBoxGuideRate3.TextChanged += buttonApply_Monitored;
            textBoxGuideRate4.TextChanged += buttonApply_Monitored;
            textBoxGuideRate5.TextChanged += buttonApply_Monitored;
            textBoxGuideRate6.TextChanged += buttonApply_Monitored;
            textBoxGuideRate7.TextChanged += buttonApply_Monitored;
            textBoxGuideRate8.TextChanged += buttonApply_Monitored;
            checkBoxSetGuideRate.CheckedChanged += buttonApply_Monitored;

            checkBoxJ2000.CheckedChanged += buttonApply_Monitored;
            checkBoxAutoMeridianFlip.CheckedChanged += buttonApply_Monitored;
            checkBoxTraceLogger.CheckedChanged += buttonApply_Monitored;
        }

        private bool CheckComponentIPAddress(out IPAddress ipAddress)
        {
            ipAddress = IPAddress.None;

            if (!int.TryParse(textBoxIPAddress1.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out int b1) || b1 < 0 || 255 < b1)
            {
                MessageBox.Show(this, "The 1st byte of IP address should be ranged from 0 to 255.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxIPAddress1.SelectAll();
                textBoxIPAddress1.Focus();
                return false;
            }

            if (!int.TryParse(textBoxIPAddress2.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out int b2) || b2 < 0 || 255 < b2)
            {
                MessageBox.Show(this, "The 2nd byte of IP address should be ranged from 0 to 255.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxIPAddress2.SelectAll();
                textBoxIPAddress2.Focus();
                return false;
            }

            if (!int.TryParse(textBoxIPAddress3.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out int b3) || b3 < 0 || 255 < b3)
            {
                MessageBox.Show(this, "The 3rd byte of IP address should be ranged from 0 to 255.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxIPAddress3.SelectAll();
                textBoxIPAddress3.Focus();
                return false;
            }

            if (!int.TryParse(textBoxIPAddress4.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out int b4) || b4 < 0 || 255 < b4)
            {
                MessageBox.Show(this, "The 4th byte of IP address should be ranged from 0 to 255.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxIPAddress4.SelectAll();
                textBoxIPAddress4.Focus();
                return false;
            }

            ipAddress = new IPAddress(new byte[] { (byte)b1, (byte)b2, (byte)b3, (byte)b4 });

            return true;
        }

        private bool CheckComponentLocation(out Telescope.Starbook.Place location)
        {
            location = new Telescope.Starbook.Place(); Telescope.Starbook.Direction direction;

            if (!int.TryParse(textBoxLatitudeDegree.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out int degree) || degree < 0 || 90 < degree)
            {
                MessageBox.Show(this, "Degree of latitude should be ranged from 0 to 90.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxLatitudeDegree.SelectAll();
                textBoxLatitudeDegree.Focus();
                return false;
            }

            if (!int.TryParse(textBoxLatitudeMinute.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out int minute) || minute < 0 || 59 < minute)
            {
                MessageBox.Show(this, "Minute of latitude should be ranged from 0 to 59.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxLatitudeMinute.SelectAll();
                textBoxLatitudeMinute.Focus();
                return false;
            }

            switch (comboBoxLatitudeDirection.SelectedItem as string)
            {
                case "N":
                    direction = Telescope.Starbook.Direction.North; break;
                case "S":
                    direction = Telescope.Starbook.Direction.South; break;
                default:
                    MessageBox.Show(this, "Direction of latitude should be N or S.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    comboBoxLatitudeDirection.Focus();
                    return false;
            }

            location.Latitude = new Telescope.Starbook.DMS(direction, degree, minute, 0);

            if (!int.TryParse(textBoxLongitudeDegree.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out degree) || degree < 0 || 180 < degree)
            {
                MessageBox.Show(this, "Degree of longitude should be ranged from 0 to 180.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxLongitudeDegree.SelectAll();
                textBoxLongitudeDegree.Focus();
                return false;
            }

            if (!int.TryParse(textBoxLongitudeMinute.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out minute) || minute < 0 || 59 < minute)
            {
                MessageBox.Show(this, "Minute of longitude should be ranged from 0 to 59.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxLongitudeMinute.SelectAll();
                textBoxLongitudeMinute.Focus();
                return false;
            }

            switch (comboBoxLongitudeDirection.SelectedItem as string)
            {
                case "E":
                    direction = Telescope.Starbook.Direction.East; break;
                case "W":
                    direction = Telescope.Starbook.Direction.West; break;
                default:
                    MessageBox.Show(this, "Direction of longitude should be E or W.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    comboBoxLongitudeDirection.Focus();
                    return false;
            }

            location.Longitude = new Telescope.Starbook.DMS(direction, degree, minute, 0);

            if (!int.TryParse(textBoxTimezone.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out int timezone) || timezone < -12 || 14 < timezone)
            {
                MessageBox.Show(this, "Time zone should be ranged from -12 to 14.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxTimezone.SelectAll();
                textBoxTimezone.Focus();
                return false;
            }

            location.Timezone = timezone;

            return true;
        }

        private bool CheckComponentDateTime(out DateTime dateTime)
        {
            dateTime = DateTime.MinValue;

            if (!int.TryParse(textBoxYear.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out int year) || year < 1 || 9999 < year)
            {
                MessageBox.Show(this, "Year of date should be ranged from 1 to 9999.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxYear.SelectAll();
                textBoxYear.Focus();
                return false;
            }

            if (!int.TryParse(textBoxMonth.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out int month) || month < 1 || 12 < month)
            {
                MessageBox.Show(this, "Month of date should be ranged from 1 to 12.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxMonth.SelectAll();
                textBoxMonth.Focus();
                return false;
            }

            if (!int.TryParse(textBoxDay.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out int day) || day < 1 || 31 < day)
            {
                MessageBox.Show(this, "Day of date should be ranged from 1 to 31.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxDay.SelectAll();
                textBoxDay.Focus();
                return false;
            }

            if (!int.TryParse(textBoxHour.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out int hour) || hour < 0 || 23 < hour)
            {
                MessageBox.Show(this, "Hour of time should be ranged from 0 to 23.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxHour.SelectAll();
                textBoxHour.Focus();
                return false;
            }

            if (!int.TryParse(textBoxMinute.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out int minute) || minute < 0 || 59 < minute)
            {
                MessageBox.Show(this, "Minute of time should be ranged from 0 to 59.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxMinute.SelectAll();
                textBoxMinute.Focus();
                return false;
            }

            if (!int.TryParse(textBoxSecond.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out int second) || second < 0 || 59 < second)
            {
                MessageBox.Show(this, "Second of time should be ranged from 0 to 59.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxSecond.SelectAll();
                textBoxSecond.Focus();
                return false;
            }

            dateTime = new DateTime(year, month, day, hour, minute, second);

            return true;
        }

        private bool CheckComponentGuideRate(TextBox textBoxGuideRate, int textBoxGuideRateIndex, out double guideRate)
        {
            if (textBoxGuideRate.Text == "NaN")
            {
                guideRate = double.NaN; return true;
            }

            if (!double.TryParse(textBoxGuideRate.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out guideRate))
            {
                MessageBox.Show(this, string.Format(CultureInfo.InvariantCulture, "Format of guide rate {0} is incorrect.", textBoxGuideRateIndex), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxGuideRate.SelectAll();
                textBoxGuideRate.Focus();
                return false;
            }

            return true;
        }

        private void buttonCheck_Click(object sender, EventArgs e)
        {
            IPAddress ipAddressCheck;

            if (!CheckComponentIPAddress(out ipAddressCheck))
            {
                this.DialogResult = DialogResult.None; return;
            }

            IPAddress ipAddress = Telescope.starbook.IPAddress; Telescope.starbook.IPAddress = ipAddressCheck;

            Telescope.Starbook.Response response = Telescope.starbook.GetStatus(out Telescope.Starbook.Status status);

            if (response == Telescope.Starbook.Response.OK)
            {
                Telescope.LogMessage("SetupDialogForm", "Check: Starbook.GetStatus()={0} / RA={1}, Dec={2}, State={3}, Goto={4}", response, status.RA, status.Dec, status.State, status.Goto);
            }
            else
            {
                Telescope.LogMessage("SetupDialogForm", "Check: Starbook.GetStatus()={0}", response); status = new Telescope.Starbook.Status();
            }

            bool connected = false, initializing = false;

            if (status.State == Telescope.Starbook.State.Unknown)
            {
                textBoxStatus.Text = "Disconnected";

                textBoxLatitudeDegree.Clear();
                textBoxLatitudeMinute.Clear();
                comboBoxLatitudeDirection.SelectedItem = string.Empty;

                textBoxLongitudeDegree.Clear();
                textBoxLongitudeMinute.Clear();
                comboBoxLongitudeDirection.SelectedItem = string.Empty;

                textBoxTimezone.Clear();

                textBoxYear.Clear();
                textBoxMonth.Clear();
                textBoxDay.Clear();
                textBoxHour.Clear();
                textBoxMinute.Clear();
                textBoxSecond.Clear();

                labelFirmwareVersion.Text = "Firmware Version:";
            }
            else
            {
                textBoxStatus.Text = "Connected"; connected = true;

                if (status.State == Telescope.Starbook.State.Init)
                {
                    initializing = true;
                }

                response = Telescope.starbook.GetPlace(out Telescope.Starbook.Place place);

                if (response == Telescope.Starbook.Response.OK)
                {
                    Telescope.LogMessage("SetupDialogForm", "Check: Starbook.GetPlace()={0} / Latitude={1}, Longitude={2}, Timezone={3}", response, place.Latitude, place.Longitude, place.Timezone);
                }
                else
                {
                    Telescope.LogMessage("SetupDialogForm", "Check: Starbook.GetPlace()={0}", response); place = new Telescope.Starbook.Place();
                }

                response = Telescope.starbook.GetTime(out DateTime dateTime);

                if (response == Telescope.Starbook.Response.OK)
                {
                    Telescope.LogMessage("SetupDialogForm", "Check: Starbook.GetTime()={0} / Time={1:yyyy/MM/dd HH:mm:ss}", response, dateTime);
                }
                else
                {
                    Telescope.LogMessage("SetupDialogForm", "Check: Starbook.GetTime()={0}", response); dateTime = DateTime.MinValue;
                }

                textBoxLatitudeDegree.Text = place.Latitude.Degree.ToString(CultureInfo.InvariantCulture);
                textBoxLatitudeMinute.Text = place.Latitude.Minute.ToString(CultureInfo.InvariantCulture);

                if (place.Latitude.Direction == Telescope.Starbook.Direction.North)
                {
                    comboBoxLatitudeDirection.SelectedItem = "N";
                }
                else if (place.Latitude.Direction == Telescope.Starbook.Direction.South)
                {
                    comboBoxLatitudeDirection.SelectedItem = "S";
                }

                textBoxLongitudeDegree.Text = place.Longitude.Degree.ToString(CultureInfo.InvariantCulture);
                textBoxLongitudeMinute.Text = place.Longitude.Minute.ToString(CultureInfo.InvariantCulture);

                if (place.Longitude.Direction == Telescope.Starbook.Direction.East)
                {
                    comboBoxLongitudeDirection.SelectedItem = "E";
                }
                else if (place.Longitude.Direction == Telescope.Starbook.Direction.West)
                {
                    comboBoxLongitudeDirection.SelectedItem = "W";
                }

                textBoxTimezone.Text = place.Timezone.ToString(CultureInfo.InvariantCulture);

                textBoxYear.Text = dateTime.Year.ToString(CultureInfo.InvariantCulture);
                textBoxMonth.Text = dateTime.Month.ToString(CultureInfo.InvariantCulture);
                textBoxDay.Text = dateTime.Day.ToString(CultureInfo.InvariantCulture);
                textBoxHour.Text = dateTime.Hour.ToString(CultureInfo.InvariantCulture);
                textBoxMinute.Text = dateTime.Minute.ToString(CultureInfo.InvariantCulture);
                textBoxSecond.Text = dateTime.Second.ToString(CultureInfo.InvariantCulture);

                response = Telescope.starbook.GetVersion(out string version);

                if (response == Telescope.Starbook.Response.OK)
                {
                    Telescope.LogMessage("SetupDialogForm", "Check: Starbook.GetVersion()={0} / Version={1}", response, version);
                }
                else
                {
                    Telescope.LogMessage("SetupDialogForm", "Check: Starbook.GetVersion()={0}", response); version = string.Empty;
                }

                labelFirmwareVersion.Text = string.Format(CultureInfo.InvariantCulture, "Firmware Version: {0}", version);
            }

            // Location & Timezone

            textBoxLatitudeDegree.ReadOnly = !checkBoxSetLocation.Checked;
            textBoxLatitudeMinute.ReadOnly = !checkBoxSetLocation.Checked;

            textBoxLatitudeDegree.Enabled = connected && initializing;
            textBoxLatitudeMinute.Enabled = connected && initializing;
            comboBoxLatitudeDirection.Enabled = connected && initializing && checkBoxSetLocation.Checked;

            textBoxLongitudeDegree.ReadOnly = !checkBoxSetLocation.Checked;
            textBoxLongitudeMinute.ReadOnly = !checkBoxSetLocation.Checked;

            textBoxLongitudeDegree.Enabled = connected && initializing;
            textBoxLongitudeMinute.Enabled = connected && initializing;
            comboBoxLongitudeDirection.Enabled = connected && initializing && checkBoxSetLocation.Checked;

            textBoxTimezone.ReadOnly = !checkBoxSetLocation.Checked;
            textBoxTimezone.Enabled = connected && initializing;

            checkBoxSetLocation.Enabled = connected && initializing;

            // Date & Time

            textBoxYear.ReadOnly = !checkBoxSetDateTime.Checked;
            textBoxMonth.ReadOnly = !checkBoxSetDateTime.Checked;
            textBoxDay.ReadOnly = !checkBoxSetDateTime.Checked;

            textBoxHour.ReadOnly = !checkBoxSetDateTime.Checked;
            textBoxMinute.ReadOnly = !checkBoxSetDateTime.Checked;
            textBoxSecond.ReadOnly = !checkBoxSetDateTime.Checked;

            textBoxYear.Enabled = connected && initializing;
            textBoxMonth.Enabled = connected && initializing;
            textBoxDay.Enabled = connected && initializing;

            textBoxHour.Enabled = connected && initializing;
            textBoxMinute.Enabled = connected && initializing;
            textBoxSecond.Enabled = connected && initializing;

            checkBoxSetDateTime.Enabled = connected && initializing;
            checkBoxSyncSystemTime.Enabled = connected && initializing && checkBoxSetDateTime.Checked;

            // Pulse Guide

            comboBoxGuideRate.Enabled = connected && checkBoxSetGuideRate.Checked;

            textBoxGuideRate0.ReadOnly = !checkBoxSetGuideRate.Checked;
            textBoxGuideRate1.ReadOnly = !checkBoxSetGuideRate.Checked;
            textBoxGuideRate2.ReadOnly = !checkBoxSetGuideRate.Checked;
            textBoxGuideRate3.ReadOnly = !checkBoxSetGuideRate.Checked;
            textBoxGuideRate4.ReadOnly = !checkBoxSetGuideRate.Checked;
            textBoxGuideRate5.ReadOnly = !checkBoxSetGuideRate.Checked;
            textBoxGuideRate6.ReadOnly = !checkBoxSetGuideRate.Checked;
            textBoxGuideRate7.ReadOnly = !checkBoxSetGuideRate.Checked;
            textBoxGuideRate8.ReadOnly = !checkBoxSetGuideRate.Checked;

            textBoxGuideRate0.Enabled = connected;
            textBoxGuideRate1.Enabled = connected;
            textBoxGuideRate2.Enabled = connected;
            textBoxGuideRate3.Enabled = connected;
            textBoxGuideRate4.Enabled = connected;
            textBoxGuideRate5.Enabled = connected;
            textBoxGuideRate6.Enabled = connected;
            textBoxGuideRate7.Enabled = connected;
            textBoxGuideRate8.Enabled = connected;

            checkBoxSetGuideRate.Enabled = connected;

            Telescope.starbook.IPAddress = ipAddress;
        }

        private void checkBoxSetLocation_CheckedChanged(object sender, EventArgs e)
        {
            textBoxLatitudeDegree.ReadOnly = !checkBoxSetLocation.Checked;
            textBoxLatitudeMinute.ReadOnly = !checkBoxSetLocation.Checked;
            comboBoxLatitudeDirection.Enabled = checkBoxSetLocation.Checked;

            textBoxLongitudeDegree.ReadOnly = !checkBoxSetLocation.Checked;
            textBoxLongitudeMinute.ReadOnly = !checkBoxSetLocation.Checked;
            comboBoxLongitudeDirection.Enabled = checkBoxSetLocation.Checked;

            textBoxTimezone.ReadOnly = !checkBoxSetLocation.Checked;
        }

        private void checkBoxSetDateTime_CheckedChanged(object sender, EventArgs e)
        {
            textBoxYear.ReadOnly = !checkBoxSetDateTime.Checked;
            textBoxMonth.ReadOnly = !checkBoxSetDateTime.Checked;
            textBoxDay.ReadOnly = !checkBoxSetDateTime.Checked;

            textBoxHour.ReadOnly = !checkBoxSetDateTime.Checked;
            textBoxMinute.ReadOnly = !checkBoxSetDateTime.Checked;
            textBoxSecond.ReadOnly = !checkBoxSetDateTime.Checked;

            checkBoxSyncSystemTime.Enabled = checkBoxSetDateTime.Checked;
        }

        private void checkBoxSyncSystemTime_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void checkBoxSetGuideRate_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxGuideRate.Enabled = checkBoxSetGuideRate.Checked;

            textBoxGuideRate0.ReadOnly = !checkBoxSetGuideRate.Checked;
            textBoxGuideRate1.ReadOnly = !checkBoxSetGuideRate.Checked;
            textBoxGuideRate2.ReadOnly = !checkBoxSetGuideRate.Checked;
            textBoxGuideRate3.ReadOnly = !checkBoxSetGuideRate.Checked;
            textBoxGuideRate4.ReadOnly = !checkBoxSetGuideRate.Checked;
            textBoxGuideRate5.ReadOnly = !checkBoxSetGuideRate.Checked;
            textBoxGuideRate6.ReadOnly = !checkBoxSetGuideRate.Checked;
            textBoxGuideRate7.ReadOnly = !checkBoxSetGuideRate.Checked;
            textBoxGuideRate8.ReadOnly = !checkBoxSetGuideRate.Checked;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (checkBoxSyncSystemTime.Enabled && checkBoxSyncSystemTime.Checked)
            {
                DateTime dateTime = DateTime.Now;

                textBoxYear.Text = dateTime.Year.ToString(CultureInfo.InvariantCulture);
                textBoxMonth.Text = dateTime.Month.ToString(CultureInfo.InvariantCulture);
                textBoxDay.Text = dateTime.Day.ToString(CultureInfo.InvariantCulture);

                textBoxHour.Text = dateTime.Hour.ToString(CultureInfo.InvariantCulture);
                textBoxMinute.Text = dateTime.Minute.ToString(CultureInfo.InvariantCulture);
                textBoxSecond.Text = dateTime.Second.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void pictureBox_Click(object sender, EventArgs e) // Click on ASCOM logo event handler
        {
            try
            {
                System.Diagnostics.Process.Start("http://ascom-standards.org/");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        private void labelEmail_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(string.Format(CultureInfo.InvariantCulture, "mailto:{0}", labelEmail.Text));
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e) // OK button event handler
        {
            // Place any validation constraint checks here
            // Update the state variables with results from the dialogue

            if (buttonApply.Enabled && MessageBox.Show(this, "Do you want to apply the changes?", string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                buttonApply_Click(sender, e);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e) // Cancel button event handler
        {
            
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            IPAddress ipAddress;

            if (!CheckComponentIPAddress(out ipAddress))
            {
                this.DialogResult = DialogResult.None; return;
            }

            Telescope.starbook.IPAddress = ipAddress;

            if (checkBoxSetLocation.Enabled && checkBoxSetLocation.Checked)
            {
                Telescope.Starbook.Place location;

                if (!CheckComponentLocation(out location))
                {
                    this.DialogResult = DialogResult.None; return;
                }

                Telescope.Starbook.Response response = Telescope.starbook.SetPlace(location);

                if (response != Telescope.Starbook.Response.OK)
                {
                    MessageBox.Show(this, string.Format(CultureInfo.InvariantCulture, "Cannot set location: {0}", response), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (checkBoxSetDateTime.Enabled && checkBoxSetDateTime.Checked)
            {
                DateTime dateTime;

                if (!CheckComponentDateTime(out dateTime))
                {
                    this.DialogResult = DialogResult.None; return;
                }

                Telescope.Starbook.Response response = Telescope.starbook.SetTime(dateTime);

                if (response != Telescope.Starbook.Response.OK)
                {
                    MessageBox.Show(this, string.Format(CultureInfo.InvariantCulture, "Cannot set date & time: {0}", response), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (checkBoxSetGuideRate.Enabled && checkBoxSetGuideRate.Checked)
            {
                Telescope.Starbook.Response response = Telescope.starbook.SetSpeed(comboBoxGuideRate.SelectedIndex);

                if (response != Telescope.Starbook.Response.OK)
                {
                    MessageBox.Show(this, string.Format(CultureInfo.InvariantCulture, "Cannot set guide rate: {0}", response), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if ((checkBoxSetLocation.Enabled && checkBoxSetLocation.Checked) || (checkBoxSetDateTime.Enabled && checkBoxSetDateTime.Checked) || (checkBoxSetGuideRate.Enabled && checkBoxSetGuideRate.Checked))
            {
                Telescope.Starbook.Response response = Telescope.starbook.Save();

                if (response != Telescope.Starbook.Response.OK)
                {
                    MessageBox.Show(this, string.Format(CultureInfo.InvariantCulture, "Cannot save setting: {0}", response), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (checkBoxSetGuideRate.Enabled && checkBoxSetGuideRate.Checked)
            {
                Telescope.guideRate = comboBoxGuideRate.SelectedIndex;

                if (!CheckComponentGuideRate(textBoxGuideRate0, 0, out Telescope.guideRates[0])) { this.DialogResult = DialogResult.None; return; }
                if (!CheckComponentGuideRate(textBoxGuideRate1, 1, out Telescope.guideRates[1])) { this.DialogResult = DialogResult.None; return; }
                if (!CheckComponentGuideRate(textBoxGuideRate2, 2, out Telescope.guideRates[2])) { this.DialogResult = DialogResult.None; return; }
                if (!CheckComponentGuideRate(textBoxGuideRate3, 3, out Telescope.guideRates[3])) { this.DialogResult = DialogResult.None; return; }
                if (!CheckComponentGuideRate(textBoxGuideRate4, 4, out Telescope.guideRates[4])) { this.DialogResult = DialogResult.None; return; }
                if (!CheckComponentGuideRate(textBoxGuideRate5, 5, out Telescope.guideRates[5])) { this.DialogResult = DialogResult.None; return; }
                if (!CheckComponentGuideRate(textBoxGuideRate6, 6, out Telescope.guideRates[6])) { this.DialogResult = DialogResult.None; return; }
                if (!CheckComponentGuideRate(textBoxGuideRate7, 7, out Telescope.guideRates[7])) { this.DialogResult = DialogResult.None; return; }
                if (!CheckComponentGuideRate(textBoxGuideRate8, 8, out Telescope.guideRates[8])) { this.DialogResult = DialogResult.None; return; }
            }

            Telescope.j2000 = checkBoxJ2000.Checked;
            Telescope.autoMeridianFlip = checkBoxAutoMeridianFlip.Checked ? 60 : 0;
            Telescope.traceLogger.Enabled = checkBoxTraceLogger.Checked;

            buttonApply.Enabled = false;
        }

        private void buttonApply_Monitored(object sender, EventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox textBox = sender as TextBox;

                if (textBox == null || !textBox.Enabled || textBox.ReadOnly)
                {
                    return;
                }
            }
            else if (sender is ComboBox)
            {
                ComboBox comboBox = sender as ComboBox;

                if (comboBox == null || !comboBox.Enabled)
                {
                    return;
                }
            }
            else if (sender is CheckBox)
            {
                CheckBox checkBox = sender as CheckBox;

                if (checkBox == null || !checkBox.Enabled)
                {
                    return;
                }
            }

            buttonApply.Enabled = true;
        }
    }
}