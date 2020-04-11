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
            chkTraceLogger.Checked = Telescope.traceLogger.Enabled;
            // set the list of com ports to those that are currently available

            byte[] ipAddress = Telescope.starbook.IPAddress.GetAddressBytes();

            if (ipAddress.Length == 4)
            {
                this.textBoxIPAddress1.Text = ipAddress[0].ToString();
                this.textBoxIPAddress2.Text = ipAddress[1].ToString();
                this.textBoxIPAddress3.Text = ipAddress[2].ToString();
                this.textBoxIPAddress4.Text = ipAddress[3].ToString();
            }

            comboBoxGuideRate.SelectedIndex = Telescope.guideRate;

            textBoxGuideRate0.Text = Telescope.guideRates[0].ToString();
            textBoxGuideRate1.Text = Telescope.guideRates[1].ToString();
            textBoxGuideRate2.Text = Telescope.guideRates[2].ToString();
            textBoxGuideRate3.Text = Telescope.guideRates[3].ToString();
            textBoxGuideRate4.Text = Telescope.guideRates[4].ToString();
            textBoxGuideRate5.Text = Telescope.guideRates[5].ToString();
            textBoxGuideRate6.Text = Telescope.guideRates[6].ToString();
            textBoxGuideRate7.Text = Telescope.guideRates[7].ToString();
            textBoxGuideRate8.Text = Telescope.guideRates[8].ToString();

            Util util = new Util();
            labelPlatformVersion.Text = string.Format("Platform Version: {0}.{1}", util.MajorVersion, util.MinorVersion);
            //labelPlatformVersion.Text = string.Format("Platform Version: {0}.{1} {4}, Build {0}.{1}.{2}.{3}", util.MajorVersion, util.MinorVersion, util.ServicePack, util.BuildNumber, util.ServicePack > 0 ? string.Format("SP{0}", util.ServicePack) : string.Empty);

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            labelDriverVersion.Text = String.Format("Driver Version: {0}.{1}", version.Major, version.Minor);
        }

        private bool CheckComponentIPAddress(out IPAddress ipAddress)
        {
            ipAddress = IPAddress.None; int n;

            if (!int.TryParse(textBoxIPAddress1.Text, out n) || n < 0 || 255 < n)
            {
                MessageBox.Show(this, "The 1st byte of IP address should be ranged from 0 to 255.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxIPAddress1.SelectAll();
                textBoxIPAddress1.Focus();
                return false;
            }

            if (!int.TryParse(textBoxIPAddress2.Text, out n) || n < 0 || 255 < n)
            {
                MessageBox.Show(this, "The 2nd byte of IP address should be ranged from 0 to 255.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxIPAddress2.SelectAll();
                textBoxIPAddress2.Focus();
                return false;
            }

            if (!int.TryParse(textBoxIPAddress3.Text, out n) || n < 0 || 255 < n)
            {
                MessageBox.Show(this, "The 3rd byte of IP address should be ranged from 0 to 255.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxIPAddress3.SelectAll();
                textBoxIPAddress3.Focus();
                return false;
            }

            if (!int.TryParse(textBoxIPAddress4.Text, out n) || n < 0 || 255 < n)
            {
                MessageBox.Show(this, "The 4th byte of IP address should be ranged from 0 to 255.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxIPAddress4.SelectAll();
                textBoxIPAddress4.Focus();
                return false;
            }

            ipAddress = IPAddress.Parse(string.Format("{0}.{1}.{2}.{3}", textBoxIPAddress1.Text, textBoxIPAddress2.Text, textBoxIPAddress3.Text, textBoxIPAddress4.Text));

            return true;
        }

        private bool CheckComponentLocation(out Telescope.Starbook.Place location)
        {
            location = new Telescope.Starbook.Place(); Telescope.Starbook.Direction direction;

            if (!int.TryParse(textBoxLatitudeDegree.Text, out int degree) || degree < 0 || 90 < degree)
            {
                MessageBox.Show(this, "Degree of latitude should be ranged from 0 to 90.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxLatitudeDegree.SelectAll();
                textBoxLatitudeDegree.Focus();
                return false;
            }

            if (!int.TryParse(textBoxLatitudeMinute.Text, out int minute) || minute < 0 || 59 < minute)
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

            if (!int.TryParse(textBoxLongitudeDegree.Text, out degree) || degree < 0 || 180 < degree)
            {
                MessageBox.Show(this, "Degree of longitude should be ranged from 0 to 180.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxLongitudeDegree.SelectAll();
                textBoxLongitudeDegree.Focus();
                return false;
            }

            if (!int.TryParse(textBoxLongitudeMinute.Text, out minute) || minute < 0 || 59 < minute)
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

            if (!int.TryParse(textBoxTimezone.Text, out int timezone) || timezone < -12 || 14 < timezone)
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

            if (!int.TryParse(textBoxYear.Text, out int year) || year < 1 || 9999 < year)
            {
                MessageBox.Show(this, "Year of date should be ranged from 1 to 9999.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxYear.SelectAll();
                textBoxYear.Focus();
                return false;
            }

            if (!int.TryParse(textBoxMonth.Text, out int month) || month < 1 || 12 < month)
            {
                MessageBox.Show(this, "Month of date should be ranged from 1 to 12.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxMonth.SelectAll();
                textBoxMonth.Focus();
                return false;
            }

            if (!int.TryParse(textBoxDay.Text, out int day) || day < 1 || 31 < day)
            {
                MessageBox.Show(this, "Day of date should be ranged from 1 to 31.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxDay.SelectAll();
                textBoxDay.Focus();
                return false;
            }

            if (!int.TryParse(textBoxHour.Text, out int hour) || hour < 0 || 23 < hour)
            {
                MessageBox.Show(this, "Hour of time should be ranged from 0 to 23.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxHour.SelectAll();
                textBoxHour.Focus();
                return false;
            }

            if (!int.TryParse(textBoxMinute.Text, out int minute) || minute < 0 || 59 < minute)
            {
                MessageBox.Show(this, "Minute of time should be ranged from 0 to 59.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxMinute.SelectAll();
                textBoxMinute.Focus();
                return false;
            }

            if (!int.TryParse(textBoxSecond.Text, out int second) || second < 0 || 59 < second)
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

            if (!double.TryParse(textBoxGuideRate.Text, out guideRate))
            {
                MessageBox.Show(this, string.Format("Format of guide rate {0} is incorrect.", textBoxGuideRateIndex), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                textBoxLatitudeDegree.Text = place.Latitude.Degree.ToString();
                textBoxLatitudeMinute.Text = place.Latitude.Minute.ToString();

                if (place.Latitude.Direction == Telescope.Starbook.Direction.North)
                {
                    comboBoxLatitudeDirection.SelectedItem = "N";
                }
                else if (place.Latitude.Direction == Telescope.Starbook.Direction.South)
                {
                    comboBoxLatitudeDirection.SelectedItem = "S";
                }

                textBoxLongitudeDegree.Text = place.Longitude.Degree.ToString();
                textBoxLongitudeMinute.Text = place.Longitude.Minute.ToString();

                if (place.Longitude.Direction == Telescope.Starbook.Direction.East)
                {
                    comboBoxLongitudeDirection.SelectedItem = "E";
                }
                else if (place.Longitude.Direction == Telescope.Starbook.Direction.West)
                {
                    comboBoxLongitudeDirection.SelectedItem = "W";
                }

                textBoxTimezone.Text = place.Timezone.ToString();

                textBoxYear.Text = dateTime.Year.ToString();
                textBoxMonth.Text = dateTime.Month.ToString();
                textBoxDay.Text = dateTime.Day.ToString();
                textBoxHour.Text = dateTime.Hour.ToString();
                textBoxMinute.Text = dateTime.Minute.ToString();
                textBoxSecond.Text = dateTime.Second.ToString();

                response = Telescope.starbook.GetVersion(out string version);

                if (response == Telescope.Starbook.Response.OK)
                {
                    Telescope.LogMessage("SetupDialogForm", "Check: Starbook.GetVersion()={0} / Version={1}", response, version);
                }
                else
                {
                    Telescope.LogMessage("SetupDialogForm", "Check: Starbook.GetVersion()={0}", response); version = string.Empty;
                }

                labelFirmwareVersion.Text = string.Format("Firmware Version: {0}", version);
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

        private void buttonOK_Click(object sender, EventArgs e) // OK button event handler
        {
            // Place any validation constraint checks here
            // Update the state variables with results from the dialogue

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
                    MessageBox.Show(this, string.Format("Cannot set location: {0}", response), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show(this, string.Format("Cannot set date & time: {0}", response), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (checkBoxSetGuideRate.Enabled && checkBoxSetGuideRate.Checked)
            {
                Telescope.Starbook.Response response = Telescope.starbook.SetSpeed(comboBoxGuideRate.SelectedIndex);

                if (response != Telescope.Starbook.Response.OK)
                {
                    MessageBox.Show(this, string.Format("Cannot set guide rate: {0}", response), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if ((checkBoxSetLocation.Enabled && checkBoxSetLocation.Checked) || (checkBoxSetDateTime.Enabled && checkBoxSetDateTime.Checked) || (checkBoxSetGuideRate.Enabled && checkBoxSetGuideRate.Checked))
            {
                Telescope.Starbook.Response response = Telescope.starbook.Save();

                if (response != Telescope.Starbook.Response.OK)
                {
                    MessageBox.Show(this, string.Format("Cannot save setting: {0}", response), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            Telescope.traceLogger.Enabled = chkTraceLogger.Checked;
        }

        private void buttonCancel_Click(object sender, EventArgs e) // Cancel button event handler
        {
            Close();
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

                textBoxYear.Text = dateTime.Year.ToString();
                textBoxMonth.Text = dateTime.Month.ToString();
                textBoxDay.Text = dateTime.Day.ToString();

                textBoxHour.Text = dateTime.Hour.ToString();
                textBoxMinute.Text = dateTime.Minute.ToString();
                textBoxSecond.Text = dateTime.Second.ToString();
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
                System.Diagnostics.Process.Start(string.Format("mailto:{0}", labelEmail.Text));
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }
    }
}