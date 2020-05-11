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
        Telescope telescope;
        Telescope.Starbook.Place place;
        DateTime dateTime;

        public SetupDialogForm(Telescope telescope)
        {
            this.telescope = telescope;
            this.place = new Telescope.Starbook.Place();
            this.dateTime = DateTime.MinValue;

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

            comboBoxPredefinedGuideRates.SelectedIndex = 0;

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

            HashSet<TextBox> dotKeyTextBoxes = new HashSet<TextBox>();
            HashSet<TextBox> integerTextBoxes = new HashSet<TextBox>();
            HashSet<TextBox> floatTextBoxes = new HashSet<TextBox>();

            dotKeyTextBoxes.Add(this.textBoxIPAddress1);
            dotKeyTextBoxes.Add(this.textBoxIPAddress2);
            dotKeyTextBoxes.Add(this.textBoxIPAddress3);

            integerTextBoxes.Add(this.textBoxIPAddress1);
            integerTextBoxes.Add(this.textBoxIPAddress2);
            integerTextBoxes.Add(this.textBoxIPAddress3);
            integerTextBoxes.Add(this.textBoxIPAddress4);
            integerTextBoxes.Add(this.textBoxLatitudeDegree);
            integerTextBoxes.Add(this.textBoxLatitudeMinute);
            integerTextBoxes.Add(this.textBoxLongitudeDegree);
            integerTextBoxes.Add(this.textBoxLongitudeMinute);
            integerTextBoxes.Add(this.textBoxElevation);
            integerTextBoxes.Add(this.textBoxYear);
            integerTextBoxes.Add(this.textBoxMonth);
            integerTextBoxes.Add(this.textBoxDay);
            integerTextBoxes.Add(this.textBoxHour);
            integerTextBoxes.Add(this.textBoxMinute);
            integerTextBoxes.Add(this.textBoxSecond);

            floatTextBoxes.Add(this.textBoxGuideRate0);
            floatTextBoxes.Add(this.textBoxGuideRate1);
            floatTextBoxes.Add(this.textBoxGuideRate2);
            floatTextBoxes.Add(this.textBoxGuideRate3);
            floatTextBoxes.Add(this.textBoxGuideRate4);
            floatTextBoxes.Add(this.textBoxGuideRate5);
            floatTextBoxes.Add(this.textBoxGuideRate6);
            floatTextBoxes.Add(this.textBoxGuideRate7);
            floatTextBoxes.Add(this.textBoxGuideRate8);

            Queue<Control> controls = new Queue<Control>(); 

            for (controls.Enqueue(this); controls.Count > 0; )
            {
                Control control = controls.Dequeue();

                if (control is TextBox textBox)
                {
                    textBox.Enter += textBox_FocusMonitored;
                    textBox.MouseClick += textBox_MouseMonitored;

                    if (dotKeyTextBoxes.Contains(textBox))
                    {
                        textBox.KeyPress += textBox_DotKeyMonitored;
                    }

                    if (integerTextBoxes.Contains(textBox))
                    {
                        textBox.KeyPress += textBox_IntegerMonitored;
                    }

                    if (floatTextBoxes.Contains(textBox))
                    {
                        textBox.KeyPress += textBox_FloatMonitored;
                    }

                    textBox.TextChanged += buttonApply_Monitored;
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.SelectedIndexChanged += buttonApply_Monitored;
                }
                else if (control is CheckBox checkBox)
                {
                    checkBox.CheckedChanged += buttonApply_Monitored;
                }
                else
                {
                    foreach (Control ctrl in control.Controls)
                    {
                        controls.Enqueue(ctrl);
                    }
                }
            }
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

        private bool CheckComponentLocation(ref Telescope.Starbook.Place place)
        {
            Telescope.Starbook.Direction direction;

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

            place.Latitude = new Telescope.Starbook.DMS(direction, degree, minute, 0);

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

            place.Longitude = new Telescope.Starbook.DMS(direction, degree, minute, 0);

            if (!int.TryParse(textBoxElevation.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out int elevation))
            {
                MessageBox.Show(this, "Elevation should be a valid integer.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxElevation.SelectAll();
                textBoxElevation.Focus();
                return false;
            }

            telescope.transform.SiteElevation = elevation;

            return true;
        }

        private bool CheckComponentDateTime(ref Telescope.Starbook.Place place, ref DateTime dateTime)
        {
            place.Timezone = comboBoxTimezone.SelectedIndex - 12;

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
            if (!CheckComponentIPAddress(out IPAddress ipAddressCheck))
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
                labelStatus.Text = "Disconnected";

                textBoxLatitudeDegree.Clear();
                textBoxLatitudeMinute.Clear();
                comboBoxLatitudeDirection.SelectedItem = string.Empty;

                textBoxLongitudeDegree.Clear();
                textBoxLongitudeMinute.Clear();
                comboBoxLongitudeDirection.SelectedItem = string.Empty;

                textBoxElevation.Clear();

                textBoxYear.Clear();
                textBoxMonth.Clear();
                textBoxDay.Clear();
                textBoxHour.Clear();
                textBoxMinute.Clear();
                textBoxSecond.Clear();
                comboBoxTimezone.SelectedItem = string.Empty;

                labelFirmwareVersion.Text = "Firmware Version: --";
            }
            else
            {
                labelStatus.Text = "Connected"; connected = true;

                if (status.State == Telescope.Starbook.State.Init)
                {
                    initializing = true;
                }

                response = Telescope.starbook.GetPlace(out Telescope.Starbook.Place place);

                if (response == Telescope.Starbook.Response.OK)
                {
                    Telescope.LogMessage("SetupDialogForm", "Check: Starbook.GetPlace()={0} / Latitude={1}, Longitude={2}, Timezone={3}", response, place.Latitude, place.Longitude, place.Timezone); this.place = place;
                }
                else
                {
                    Telescope.LogMessage("SetupDialogForm", "Check: Starbook.GetPlace()={0}", response); place = new Telescope.Starbook.Place();
                }

                response = Telescope.starbook.GetTime(out DateTime dateTime);

                if (response == Telescope.Starbook.Response.OK)
                {
                    Telescope.LogMessage("SetupDialogForm", "Check: Starbook.GetTime()={0} / Time={1:yyyy/MM/dd HH:mm:ss}", response, dateTime); this.dateTime = dateTime;
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

                textBoxElevation.Text = telescope.transform.SiteElevation.ToString(CultureInfo.InvariantCulture);

                textBoxYear.Text = dateTime.Year.ToString(CultureInfo.InvariantCulture);
                textBoxMonth.Text = dateTime.Month.ToString(CultureInfo.InvariantCulture);
                textBoxDay.Text = dateTime.Day.ToString(CultureInfo.InvariantCulture);
                textBoxHour.Text = dateTime.Hour.ToString(CultureInfo.InvariantCulture);
                textBoxMinute.Text = dateTime.Minute.ToString(CultureInfo.InvariantCulture);
                textBoxSecond.Text = dateTime.Second.ToString(CultureInfo.InvariantCulture);

                comboBoxTimezone.SelectedIndex = place.Timezone + 12;

                response = Telescope.starbook.GetVersion(out string version);

                if (response == Telescope.Starbook.Response.OK)
                {
                    Telescope.LogMessage("SetupDialogForm", "Check: Starbook.GetVersion()={0} / Version={1}", response, version);
                }
                else
                {
                    Telescope.LogMessage("SetupDialogForm", "Check: Starbook.GetVersion()={0}", response); version = "N/A";
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

            textBoxElevation.ReadOnly = !checkBoxSetLocation.Checked;
            textBoxElevation.Enabled = connected && initializing;

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

            comboBoxTimezone.Enabled = connected && initializing && checkBoxSetDateTime.Checked;

            checkBoxSetDateTime.Enabled = connected && initializing;
            checkBoxSyncSystemTime.Enabled = connected && initializing && checkBoxSetDateTime.Checked;

            // Pulse Guide

            comboBoxGuideRate.Enabled = connected && checkBoxSetGuideRate.Checked;

            textBoxGuideRate0.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate1.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate2.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate3.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate4.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate5.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate6.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate7.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate8.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;

            textBoxGuideRate0.Enabled = connected;
            textBoxGuideRate1.Enabled = connected;
            textBoxGuideRate2.Enabled = connected;
            textBoxGuideRate3.Enabled = connected;
            textBoxGuideRate4.Enabled = connected;
            textBoxGuideRate5.Enabled = connected;
            textBoxGuideRate6.Enabled = connected;
            textBoxGuideRate7.Enabled = connected;
            textBoxGuideRate8.Enabled = connected;

            comboBoxPredefinedGuideRates.Enabled = connected && checkBoxSetGuideRate.Checked;

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

            textBoxElevation.ReadOnly = !checkBoxSetLocation.Checked;
        }

        private void checkBoxSetDateTime_CheckedChanged(object sender, EventArgs e)
        {
            textBoxYear.ReadOnly = !checkBoxSetDateTime.Checked || checkBoxSyncSystemTime.Checked;
            textBoxMonth.ReadOnly = !checkBoxSetDateTime.Checked || checkBoxSyncSystemTime.Checked;
            textBoxDay.ReadOnly = !checkBoxSetDateTime.Checked || checkBoxSyncSystemTime.Checked;

            textBoxHour.ReadOnly = !checkBoxSetDateTime.Checked || checkBoxSyncSystemTime.Checked;
            textBoxMinute.ReadOnly = !checkBoxSetDateTime.Checked || checkBoxSyncSystemTime.Checked;
            textBoxSecond.ReadOnly = !checkBoxSetDateTime.Checked || checkBoxSyncSystemTime.Checked;

            comboBoxTimezone.Enabled = checkBoxSetDateTime.Checked && !checkBoxSyncSystemTime.Checked;

            checkBoxSyncSystemTime.Enabled = checkBoxSetDateTime.Checked;
        }

        private void checkBoxSyncSystemTime_CheckedChanged(object sender, EventArgs e)
        {
            textBoxYear.ReadOnly = !checkBoxSetDateTime.Checked || checkBoxSyncSystemTime.Checked;
            textBoxMonth.ReadOnly = !checkBoxSetDateTime.Checked || checkBoxSyncSystemTime.Checked;
            textBoxDay.ReadOnly = !checkBoxSetDateTime.Checked || checkBoxSyncSystemTime.Checked;

            textBoxHour.ReadOnly = !checkBoxSetDateTime.Checked || checkBoxSyncSystemTime.Checked;
            textBoxMinute.ReadOnly = !checkBoxSetDateTime.Checked || checkBoxSyncSystemTime.Checked;
            textBoxSecond.ReadOnly = !checkBoxSetDateTime.Checked || checkBoxSyncSystemTime.Checked;

            comboBoxTimezone.Enabled = checkBoxSetDateTime.Checked && !checkBoxSyncSystemTime.Checked;
        }

        private void comboBoxPredefinedGuideRates_SelectedIndexChanged(object sender, EventArgs e)
        {
            double[] guideRates;

            switch (comboBoxPredefinedGuideRates.SelectedIndex)
            {
                case 0:
                default:
                    guideRates = Telescope.guideRates; break;
                case 1:
                    guideRates = Telescope.guideRatesStarbook; break;
                case 2:
                    guideRates = Telescope.guideRatesStarbookS; break;
                case 3:
                    guideRates = Telescope.guideRatesStarbookTen; break;
            }

            textBoxGuideRate0.Text = guideRates[0].ToString(CultureInfo.InvariantCulture);
            textBoxGuideRate1.Text = guideRates[1].ToString(CultureInfo.InvariantCulture);
            textBoxGuideRate2.Text = guideRates[2].ToString(CultureInfo.InvariantCulture);
            textBoxGuideRate3.Text = guideRates[3].ToString(CultureInfo.InvariantCulture);
            textBoxGuideRate4.Text = guideRates[4].ToString(CultureInfo.InvariantCulture);
            textBoxGuideRate5.Text = guideRates[5].ToString(CultureInfo.InvariantCulture);
            textBoxGuideRate6.Text = guideRates[6].ToString(CultureInfo.InvariantCulture);
            textBoxGuideRate7.Text = guideRates[7].ToString(CultureInfo.InvariantCulture);
            textBoxGuideRate8.Text = guideRates[8].ToString(CultureInfo.InvariantCulture);

            textBoxGuideRate0.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate1.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate2.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate3.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate4.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate5.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate6.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate7.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate8.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
        }

        private void checkBoxSetGuideRate_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxGuideRate.Enabled = checkBoxSetGuideRate.Checked;

            textBoxGuideRate0.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate1.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate2.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate3.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate4.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate5.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate6.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate7.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;
            textBoxGuideRate8.ReadOnly = !checkBoxSetGuideRate.Checked || comboBoxPredefinedGuideRates.SelectedIndex != 0;

            comboBoxPredefinedGuideRates.Enabled = checkBoxSetGuideRate.Checked;
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

                double timezone = Math.Min(Math.Max(TimeZoneInfo.Local.GetUtcOffset(dateTime).TotalHours, -12), 14);

                comboBoxTimezone.SelectedIndex = (int)timezone + 12;
            }
        }

        private void pictureBox_Click(object sender, EventArgs e) // Click on ASCOM logo event handler
        {
            try
            {
                System.Diagnostics.Process.Start("https://github.com/lkcheng89/starbook");
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

            bool setLocation = false, setDateTime = false, setGuideRate = false;

            Telescope.Starbook.Place place = this.place;
            DateTime dateTime = this.dateTime;
            int guideRate = Telescope.guideRate;
            double[] guideRates = new double[9];
            Array.Copy(Telescope.guideRates, guideRates, 9);

            if (checkBoxSetLocation.Enabled && checkBoxSetLocation.Checked)
            {
                if (!CheckComponentLocation(ref place))
                {
                    this.DialogResult = DialogResult.None; return;
                }

                setLocation = true;
            }

            if (checkBoxSetDateTime.Enabled && checkBoxSetDateTime.Checked)
            {
                if (!CheckComponentDateTime(ref place, ref dateTime))
                {
                    this.DialogResult = DialogResult.None; return;
                }

                setDateTime = true;
            }

            if (checkBoxSetGuideRate.Enabled && checkBoxSetGuideRate.Checked)
            {
                guideRate = comboBoxGuideRate.SelectedIndex;

                if (!CheckComponentGuideRate(textBoxGuideRate1, 1, out guideRates[1])) { this.DialogResult = DialogResult.None; return; }
                if (!CheckComponentGuideRate(textBoxGuideRate2, 2, out guideRates[2])) { this.DialogResult = DialogResult.None; return; }
                if (!CheckComponentGuideRate(textBoxGuideRate3, 3, out guideRates[3])) { this.DialogResult = DialogResult.None; return; }
                if (!CheckComponentGuideRate(textBoxGuideRate4, 4, out guideRates[4])) { this.DialogResult = DialogResult.None; return; }
                if (!CheckComponentGuideRate(textBoxGuideRate5, 5, out guideRates[5])) { this.DialogResult = DialogResult.None; return; }
                if (!CheckComponentGuideRate(textBoxGuideRate6, 6, out guideRates[6])) { this.DialogResult = DialogResult.None; return; }
                if (!CheckComponentGuideRate(textBoxGuideRate7, 7, out guideRates[7])) { this.DialogResult = DialogResult.None; return; }
                if (!CheckComponentGuideRate(textBoxGuideRate8, 8, out guideRates[8])) { this.DialogResult = DialogResult.None; return; }

                setGuideRate = true;
            }

            if (setLocation || setDateTime)
            {
                Telescope.Starbook.Response response = Telescope.starbook.SetPlace(place);
                Telescope.LogMessage("SetupDialogForm", "Apply: Starbook.SetPlace({0},{1},{2})={3}", place.Latitude, place.Longitude, place.Timezone, response);

                if (response == Telescope.Starbook.Response.OK)
                {
                    this.place = place;
                }
                else
                {
                    MessageBox.Show(this, string.Format(CultureInfo.InvariantCulture, "Cannot set location: {0}", response), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (setDateTime)
            {
                Telescope.Starbook.Response response = Telescope.starbook.SetTime(dateTime);
                Telescope.LogMessage("SetupDialogForm", "Apply: Starbook.SetTime({0:yyyy/MM/dd HH:mm:ss})={1}", dateTime, response);

                if (response == Telescope.Starbook.Response.OK)
                {
                    this.dateTime = dateTime;
                }
                else
                {
                    MessageBox.Show(this, string.Format(CultureInfo.InvariantCulture, "Cannot set date & time: {0}", response), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (setGuideRate)
            {
                Telescope.guideRate = guideRate;
                Array.Copy(guideRates, Telescope.guideRates, 9);
            }

            if (setLocation || setDateTime || setGuideRate)
            {
                Telescope.Starbook.Response response = Telescope.starbook.Save();
                Telescope.LogMessage("SetupDialogForm", "Apply: Starbook.Save()={0}", response);

                if (response != Telescope.Starbook.Response.OK)
                {
                    MessageBox.Show(this, string.Format(CultureInfo.InvariantCulture, "Cannot save setting: {0}", response), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            Telescope.j2000 = checkBoxJ2000.Checked;
            Telescope.autoMeridianFlip = checkBoxAutoMeridianFlip.Checked ? 60 : 0;
            Telescope.traceLogger.Enabled = checkBoxTraceLogger.Checked;

            buttonApply.Enabled = false;
        }

        private void textBox_FocusMonitored(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox != null)
            {
                textBox.SelectAll();
            }
        }

        private void textBox_MouseMonitored(object sender, MouseEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox != null)
            {
                textBox.SelectAll();
            }
        }

        private void textBox_DotKeyMonitored(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '.')
            {
                e.Handled = true; SelectNextControl(ActiveControl, true, true, true, true);
            }
        }

        private void textBox_IntegerMonitored(object sender, KeyPressEventArgs e)
        {
            char keyChar = e.KeyChar;

            if (!char.IsControl(keyChar) && !char.IsDigit(keyChar))
            {
                e.Handled = true;
            }
        }

        private void textBox_FloatMonitored(object sender, KeyPressEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                char keyChar = e.KeyChar;

                if (!char.IsControl(keyChar) && ((keyChar != '.' && !char.IsDigit(keyChar)) || (keyChar == '.' && textBox.Text.Contains("."))))
                {
                    e.Handled = true;
                }
            }
        }

        private void buttonApply_Monitored(object sender, EventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (!textBox.Enabled || textBox.ReadOnly)
                {
                    return;
                }
            }
            else if (sender is ComboBox comboBox)
            {
                if (!comboBox.Enabled)
                {
                    return;
                }
            }
            else if (sender is CheckBox checkBox)
            {
                if (!checkBox.Enabled)
                {
                    return;
                }
            }

            buttonApply.Enabled = true;
        }
    }
}