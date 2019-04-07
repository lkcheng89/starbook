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
            // Initialise current values of user settings from the ASCOM Profile
            InitUI();
        }

        private void cmdOK_Click(object sender, EventArgs e) // OK button event handler
        {
            // Place any validation constraint checks here
            // Update the state variables with results from the dialogue

            Telescope.starbook.IPAddress = IPAddress.Parse(string.Format("{0}.{1}.{2}.{3}",
                textBoxIPAddress1.Text, textBoxIPAddress2.Text, textBoxIPAddress3.Text, textBoxIPAddress4.Text));

            if (checkBoxSetLocation.Enabled && checkBoxSetLocation.Checked)
            {
                Telescope.Starbook.DMS latitude = new Telescope.Starbook.DMS();

                latitude.Degree = int.Parse(textBoxLatitudeDegree.Text);
                latitude.Minute = int.Parse(textBoxLatitudeMinute.Text);

                switch (comboBoxLatitudeDirection.SelectedItem as string)
                {
                    case "N":
                        latitude.Direction = Telescope.Starbook.Direction.North; break;
                    case "S":
                        latitude.Direction = Telescope.Starbook.Direction.South; break;
                    default:
                        break;
                }

                Telescope.Starbook.DMS longitude = new Telescope.Starbook.DMS();

                longitude.Degree = int.Parse(textBoxLongitudeDegree.Text);
                longitude.Minute = int.Parse(textBoxLongitudeMinute.Text);

                switch (comboBoxLongitudeDirection.SelectedItem as string)
                {
                    case "E":
                        longitude.Direction = Telescope.Starbook.Direction.East; break;
                    case "W":
                        longitude.Direction = Telescope.Starbook.Direction.West; break;
                    default:
                        break;
                }

                int timezone = int.Parse(textBoxTimezone.Text);

                Telescope.Starbook.Place place = new Telescope.Starbook.Place();

                place.Latitude = latitude;
                place.Longitude = longitude;
                place.Timezone = timezone;

                Telescope.Starbook.Response response = Telescope.starbook.SetPlace(place);

                if (response != Telescope.Starbook.Response.OK)
                {
                    MessageBox.Show(string.Format("Cannot set location: {0}", response));
                }
            }

            if (checkBoxSetDateTime.Enabled && checkBoxSetDateTime.Checked)
            {
                int year = int.Parse(textBoxYear.Text);
                int month = int.Parse(textBoxMonth.Text);
                int day = int.Parse(textBoxDay.Text);
                int hour = int.Parse(textBoxHour.Text);
                int minute = int.Parse(textBoxMinute.Text);
                int second = int.Parse(textBoxSecond.Text);

                DateTime dateTime = new DateTime(year, month, day, hour, minute, second);

                Telescope.Starbook.Response response = Telescope.starbook.SetTime(dateTime);

                if (response != Telescope.Starbook.Response.OK)
                {
                    MessageBox.Show(string.Format("Cannot set date & time: {0}", response));
                }
            }

            if (checkBoxSetGuideRate.Enabled && checkBoxSetGuideRate.Checked)
            {
                Telescope.Starbook.Response response = Telescope.starbook.SetSpeed(comboBoxGuideRate.SelectedIndex);

                if (response != Telescope.Starbook.Response.OK)
                {
                    MessageBox.Show(string.Format("Cannot set guide rate: {0}", response));
                }
            }

            Telescope.guideRate = comboBoxGuideRate.SelectedIndex;

            Telescope.guideRates[0] = double.Parse(textBoxGuideRate0.Text);
            Telescope.guideRates[1] = double.Parse(textBoxGuideRate1.Text);
            Telescope.guideRates[2] = double.Parse(textBoxGuideRate2.Text);
            Telescope.guideRates[3] = double.Parse(textBoxGuideRate3.Text);
            Telescope.guideRates[4] = double.Parse(textBoxGuideRate4.Text);
            Telescope.guideRates[5] = double.Parse(textBoxGuideRate5.Text);
            Telescope.guideRates[6] = double.Parse(textBoxGuideRate6.Text);
            Telescope.guideRates[7] = double.Parse(textBoxGuideRate7.Text);
            Telescope.guideRates[8] = double.Parse(textBoxGuideRate8.Text);

            Telescope.traceLogger.Enabled = chkTraceLogger.Checked;
        }

        private void cmdCancel_Click(object sender, EventArgs e) // Cancel button event handler
        {
            Close();
        }

        private void BrowseToAscom(object sender, EventArgs e) // Click on ASCOM logo event handler
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

        private void InitUI()
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

        private void buttonCheck_Click(object sender, EventArgs e)
        {
            IPAddress ipAddress = Telescope.starbook.IPAddress;

            Telescope.starbook.IPAddress = IPAddress.Parse(string.Format("{0}.{1}.{2}.{3}",
                textBoxIPAddress1.Text, textBoxIPAddress2.Text, textBoxIPAddress3.Text, textBoxIPAddress4.Text));

            string version = Telescope.starbook.Version; bool connected = false;

            if (string.IsNullOrEmpty(version))
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

                Telescope.Starbook.Place place = Telescope.starbook.GetPlace();
                DateTime dateTime = Telescope.starbook.GetTime();

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

                labelFirmwareVersion.Text = string.Format("Firmware Version: {0}", version);
            }

            textBoxLatitudeDegree.ReadOnly = !connected;
            textBoxLatitudeMinute.ReadOnly = !connected;
            comboBoxLatitudeDirection.Enabled = connected;

            textBoxLongitudeDegree.ReadOnly = !connected;
            textBoxLongitudeMinute.ReadOnly = !connected;
            comboBoxLongitudeDirection.Enabled = connected;

            textBoxTimezone.ReadOnly = !connected;

            textBoxYear.ReadOnly = !connected;
            textBoxMonth.ReadOnly = !connected;
            textBoxDay.ReadOnly = !connected;
            textBoxHour.ReadOnly = !connected;
            textBoxMinute.ReadOnly = !connected;
            textBoxSecond.ReadOnly = !connected;

            checkBoxSetLocation.Enabled = connected;
            checkBoxSetDateTime.Enabled = connected;

            checkBoxSetGuideRate.Enabled = connected;

            Telescope.starbook.IPAddress = ipAddress;
        }

        private void checkBoxSetLocation_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBoxSetDateTime_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxSyncSystemTime.Enabled = checkBoxSetDateTime.Checked;
        }

        private void checkBoxSyncSystemTime_CheckedChanged(object sender, EventArgs e)
        {
            timer.Enabled = checkBoxSyncSystemTime.Checked;
        }

        private void timer_Tick(object sender, EventArgs e)
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
}