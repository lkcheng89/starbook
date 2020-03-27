//tabs=4
// --------------------------------------------------------------------------------
// TODO fill in this information for your driver, then remove this line!
//
// ASCOM Driver for Vixen Starbook
//
// Description:	Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam 
//				nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam 
//				erat, sed diam voluptua. At vero eos et accusam et justo duo 
//				dolores et ea rebum. Stet clita kasd gubergren, no sea takimata 
//				sanctus est Lorem ipsum dolor sit amet.
//
// Implements:	ASCOM Telescope interface version: <To be completed by driver developer>
// Author:		(XXX) Your N. Here <your@email.here>
//
// Edit Log:
//
// Date			Who	Vers	Description
// -----------	---	-----	-------------------------------------------------------
// dd-mmm-yyyy	XXX	6.0.0	Initial edit, created from ASCOM driver template
// --------------------------------------------------------------------------------
//


// This is used to define code in the template that is specific to one class implementation
// unused code canbe deleted and this definition removed.
#define Telescope

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using ASCOM;
using ASCOM.Astrometry;
using ASCOM.Astrometry.AstroUtils;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;

namespace ASCOM.Starbook
{
    //
    // Your driver's DeviceID is ASCOM.Starbook.Telescope
    //
    // The Guid attribute sets the CLSID for ASCOM.Starbook.Telescope
    // The ClassInterface/None addribute prevents an empty interface called
    // _Starbook from being created and used as the [default] interface
    //
    // TODO Replace the not implemented exceptions with code to implement the function or
    // throw the appropriate ASCOM exception.
    //

    /// <summary>
    /// ASCOM Telescope Driver for Starbook.
    /// </summary>
    [Guid("f83d3c07-d114-4a3d-a15c-09295d08d638")]
    [ClassInterface(ClassInterfaceType.None)]
    public partial class Telescope : ITelescopeV3
    {
        /// <summary>
        /// ASCOM DeviceID (COM ProgID) for this driver.
        /// The DeviceID is used by ASCOM applications to load the driver at runtime.
        /// </summary>
        internal static string driverID = "ASCOM.Starbook.Telescope";
        // TODO Change the descriptive string for your driver then remove this line
        /// <summary>
        /// Driver description that displays in the ASCOM Chooser.
        /// </summary>
        private static string driverDescription = "Starbook";

        internal static string ipAddressProfileName = "IPAddress"; // Constants used for Profile persistence
        internal static string ipAddressDefault = "169.254.1.1";
        internal static string slewSettleTimeProfileName = "SlewSettleTime";
        internal static string slewSettleTimeDefault = "0";
        internal static string guideRateProfileName = "GuideRate";
        internal static string guideRateDefault = "0";
        internal static string guideRatesProfileName = "GuideRates";
        internal static string guideRatesDefault = "0.75,0.75,18,37,75,150,300,500,1000";
        internal static string traceLoggerProfileName = "TraceLogger";
        internal static string traceLoggerDefault = "False";

        internal static Starbook starbook = new Starbook(); // Variables to hold the currrent device configuration
        internal static short slewSettleTime;
        internal static int guideRate;
        internal static double[] guideRates;

        /// <summary>
        /// Private variable to hold the connected state
        /// </summary>
        private bool connectedState;

        /// <summary>
        /// Private variable to hold an ASCOM Utilities object
        /// </summary>
        private Util utilities;

        /// <summary>
        /// Private variable to hold an ASCOM AstroUtilities object to provide the Range method
        /// </summary>
        private AstroUtils astroUtilities;

        /// <summary>
        /// Private variable to hold an ASCOM Astrometry object to provide the Conversion method
        /// </summary>
        private ASCOM.Astrometry.Transform.Transform transform;

        /// <summary>
        /// Variable to hold the trace logger object (creates a diagnostic log file with information that you specify)
        /// </summary>
        internal static TraceLogger traceLogger;

        private double parkAzimuth;
        private double parkElevation;

        private Starbook.Place placeCache;
        private bool placeCached;

        private bool pulseGuiding;

        private double targetDeclination;
        private double targetRightAscension;

        /// <summary>
        /// Initializes a new instance of the <see cref="Starbook"/> class.
        /// Must be public for COM registration.
        /// </summary>
        public Telescope()
        {
            traceLogger = new TraceLogger("", "Starbook");
            ReadProfile(); // Read device configuration from the ASCOM Profile store

            traceLogger.LogMessage("Telescope", "Starting initialisation");

            connectedState = false; // Initialise connected to false
            utilities = new Util(); //Initialise util object
            astroUtilities = new AstroUtils(); // Initialise astro utilities object

            transform = new Astrometry.Transform.Transform();  // Initialise astrometry transform object
            transform.SiteElevation = 0;
            transform.SiteTemperature = 20;

            //TODO: Implement your additional construction here

            parkAzimuth = double.NaN;
            parkElevation = double.NaN;

            placeCached = false;

            pulseGuiding = false;

            targetRightAscension = double.NaN;
            targetDeclination = double.NaN;

            traceLogger.LogMessage("Telescope", "Completed initialisation");
        }


        //
        // PUBLIC COM INTERFACE ITelescopeV3 IMPLEMENTATION
        //

        #region Common properties and methods.

        /// <summary>
        /// Displays the Setup Dialog form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public void SetupDialog()
        {
            // consider only showing the setup dialog if not connected
            // or call a different dialog if connected
            if (IsConnected)
                System.Windows.Forms.MessageBox.Show("Already connected, just press OK");

            using (SetupDialogForm F = new SetupDialogForm())
            {
                var result = F.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    WriteProfile(); // Persist device configuration values to the ASCOM Profile store
                }
            }
        }

        public ArrayList SupportedActions
        {
            get
            {
                ArrayList actions = new ArrayList();

                foreach (string action in actions)
                {
                    LogMessage("SupportedActions_get", action);
                }

                return actions;
            }
        }

        public string Action(string actionName, string actionParameters)
        {
            LogMessage("Action", "{0} {1} not implemented", actionName, actionParameters);
            throw new ASCOM.ActionNotImplementedException("Action " + actionName + " is not implemented by this driver");
        }

        public void CommandBlind(string command, bool raw)
        {
            CheckConnected("CommandBlind");
            Telescope.starbook.Command(command);
        }

        public bool CommandBool(string command, bool raw)
        {
            CheckConnected("CommandBool");
            Telescope.starbook.Command(command); return false;
        }

        public string CommandString(string command, bool raw)
        {
            CheckConnected("CommandString");
            return Telescope.starbook.Command(command);
        }

        public void Dispose()
        {
            // Clean up the tracelogger and util objects
            traceLogger.Enabled = false;
            traceLogger.Dispose();
            traceLogger = null;
            utilities.Dispose();
            utilities = null;
            astroUtilities.Dispose();
            astroUtilities = null;
            transform.Dispose();
            transform = null;
        }

        public bool Connected
        {
            get
            {
                LogMessage("Connected_get", IsConnected.ToString());
                return IsConnected;
            }
            set
            {
                LogMessage("Connected_set", value.ToString());

                if (value == IsConnected)
                {
                    return;
                }

                if (value)
                {
                    connectedState = true;
                    LogMessage("Connected_set", "Connecting to {0}", starbook.IPAddress);
                    // TODO connect to the device
                    string version = starbook.Version;
                    if (string.IsNullOrEmpty(version))
                    {
                        connectedState = false;
                    }
                    else
                    {
                        starbook.Start();
                        starbook.Stop();

                        starbook.SetSpeed(guideRate);
                    }
                }
                else
                {
                    connectedState = false;
                    LogMessage("Connected_set", "Disconnecting from {0}", starbook.IPAddress);
                    // TODO disconnect from the device
                }
            }
        }

        public string Description
        {
            // TODO customise this device description
            get
            {
                LogMessage("Description_get", driverDescription);
                return driverDescription;
            }
        }

        public string DriverInfo
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                // TODO customise this driver description
                string driverInfo = "ASCOM Driver for Vixen Starbook v" + String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                LogMessage("DriverInfo_get", driverInfo);
                return driverInfo;
            }
        }

        public string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                LogMessage("DriverVersion_get", driverVersion);
                return driverVersion;
            }
        }

        public short InterfaceVersion
        {
            // set by the driver wizard
            get
            {
                LogMessage("InterfaceVersion_get", "3");
                return Convert.ToInt16("3");
            }
        }

        public string Name
        {
            get
            {
                string name = "Starbook";
                LogMessage("Name_get", name);
                return name;
            }
        }

        #endregion

        #region ITelescope Implementation
        public void AbortSlew()
        {
            Starbook.Response response = starbook.Stop();
            LogMessage("AbortSlew", "Starbook.Stop() = {0}", response);
        }

        public AlignmentModes AlignmentMode
        {
            get
            {
                AlignmentModes mode = AlignmentModes.algGermanPolar;
                LogMessage("AlignmentMode_get", mode.ToString());
                return mode;
            }
        }

        public double Altitude
        {
            get
            {
                LogMessage("Altitude_get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Altitude", false);
            }
        }

        public double ApertureArea
        {
            get
            {
                LogMessage("ApertureArea_get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("ApertureArea", false);
            }
        }

        public double ApertureDiameter
        {
            get
            {
                LogMessage("ApertureDiameter_get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("ApertureDiameter", false);
            }
        }

        public bool AtHome
        {
            get
            {
                LogMessage("AtHome_get", false.ToString());
                return false;
            }
        }

        public bool AtPark
        {
            get
            {
                LogMessage("AtPark_get", false.ToString());
                return false;
            }
        }

        public IAxisRates AxisRates(TelescopeAxes Axis)
        {
            LogMessage("AxisRates_get", Axis.ToString());
            return new AxisRates(Axis);
        }

        public double Azimuth
        {
            get
            {
                LogMessage("Azimuth_get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Azimuth", false);
            }
        }

        public bool CanFindHome
        {
            get
            {
                LogMessage("CanFindHome_get", true.ToString());
                return true;
            }
        }

        public bool CanMoveAxis(TelescopeAxes Axis)
        {
            bool canMoveAxis = false;

            switch (Axis)
            {
                case TelescopeAxes.axisPrimary:
                    canMoveAxis = true;
                    break;
                case TelescopeAxes.axisSecondary:
                    canMoveAxis = true;
                    break;
                case TelescopeAxes.axisTertiary:
                    canMoveAxis = false;
                    break;
                default:
                    throw new InvalidValueException("CanMoveAxis", Axis.ToString(), "0 to 2");
            }

            LogMessage("CanMoveAxis_get", "{0} {1}", Axis, canMoveAxis);
            return canMoveAxis;
        }

        public bool CanPark
        {
            get
            {
                LogMessage("CanPark_get", true.ToString());
                return true;
            }
        }

        public bool CanPulseGuide
        {
            get
            {
                LogMessage("CanPulseGuide_get", true.ToString());
                return true;
            }
        }

        public bool CanSetDeclinationRate
        {
            get
            {
                LogMessage("CanSetDeclinationRate_get", false.ToString());
                return false;
            }
        }

        public bool CanSetGuideRates
        {
            get
            {
                LogMessage("CanSetGuideRates_get", true.ToString());
                return true;
            }
        }

        public bool CanSetPark
        {
            get
            {
                LogMessage("CanSetPark_get", true.ToString());
                return true;
            }
        }

        public bool CanSetPierSide
        {
            get
            {
                LogMessage("CanSetPierSide_get", false.ToString());
                return false;
            }
        }

        public bool CanSetRightAscensionRate
        {
            get
            {
                LogMessage("CanSetRightAscensionRate_get", false.ToString());
                return false;
            }
        }

        public bool CanSetTracking
        {
            get
            {
                LogMessage("CanSetTracking_get", true.ToString());
                return true;
            }
        }

        public bool CanSlew
        {
            get
            {
                LogMessage("CanSlew_get", true.ToString());
                return true;
            }
        }

        public bool CanSlewAltAz
        {
            get
            {
                LogMessage("CanSlewAltAz_get", false.ToString());
                return false;
            }
        }

        public bool CanSlewAltAzAsync
        {
            get
            {
                LogMessage("CanSlewAltAzAsync_get", false.ToString());
                return false;
            }
        }

        public bool CanSlewAsync
        {
            get
            {
                LogMessage("CanSlewAsync_get", true.ToString());
                return true;
            }
        }

        public bool CanSync
        {
            get
            {
                LogMessage("CanSync_get", true.ToString());
                return true;
            }
        }

        public bool CanSyncAltAz
        {
            get
            {
                LogMessage("CanSyncAltAz_get", false.ToString());
                return false;
            }
        }

        public bool CanUnpark
        {
            get
            {
                LogMessage("CanUnpark_get", true.ToString());
                return true;
            }
        }

        public double Declination
        {
            get
            {
                Starbook.Response response = starbook.GetStatus(out Telescope.Starbook.Status status);

                if (response == Starbook.Response.OK)
                {
                    LogMessage("Declination_get", "Starbook.GetStatus() = {0}, {1} {2} {3} {4}", response, status.RA, status.Dec, status.State, status.Goto);
                }
                else
                {
                    LogMessage("Declination_get", "Starbook.GetStatus() = {0}", response);
                    throw new ASCOM.InvalidOperationException("Declination_get: Status is not available.");
                }

                double declination = status.Dec.Value;
                LogMessage("Declination_get", declination.ToString());
                return declination;
            }
        }

        public double DeclinationRate
        {
            get
            {
                double declination = 0.0;
                LogMessage("DeclinationRate_get", declination.ToString());
                return declination;
            }
            set
            {
                LogMessage("DeclinationRate_set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("DeclinationRate", true);
            }
        }

        public PierSide DestinationSideOfPier(double RightAscension, double Declination)
        {
            LogMessage("DestinationSideOfPier_get", "Not implemented");
            throw new ASCOM.PropertyNotImplementedException("DestinationSideOfPier", false);
        }

        public bool DoesRefraction
        {
            get
            {
                LogMessage("DoesRefraction_get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("DoesRefraction", false);
            }
            set
            {
                LogMessage("DoesRefraction_set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("DoesRefraction", true);
            }
        }

        public EquatorialCoordinateType EquatorialSystem
        {
            get
            {
                EquatorialCoordinateType equatorialSystem = EquatorialCoordinateType.equJ2000;
                LogMessage("EquatorialSystem_get", equatorialSystem.ToString());
                return equatorialSystem;
            }
        }

        public void FindHome()
        {
            Starbook.Response response = starbook.Home();
            LogMessage("FindHome", "Starbook.Home() = {0}", response);
        }

        public double FocalLength
        {
            get
            {
                LogMessage("FocalLength_get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("FocalLength", false);
            }
        }

        public static double GuideRate(int guideRate)
        {
            return guideRates[guideRate] * (15.041 / 3600);
        }

        public static int GuideRate(double guideRate)
        {
            guideRate /= (15.041 / 3600);

            double guideRateDifference = double.MaxValue;
            int guideRateIndex = 0;

            for (int index = 0; index <= 8; index++)
            {
                double difference = Math.Abs(guideRate - guideRates[index]);

                if (guideRateDifference > difference)
                {
                    guideRateDifference = difference;
                    guideRateIndex = index;
                }
            }

            return guideRateIndex;
        }

        public double GuideRateDeclination
        {
            get
            {
                double guideRate = GuideRate(Telescope.guideRate);
                LogMessage("GuideRateDeclination_get", "{0} {1}", guideRate, Telescope.guideRate);
                return guideRate;
            }
            set
            {
                Starbook.Response response = starbook.SetSpeed(guideRate = GuideRate(value));
                LogMessage("GuideRateDeclination_set", "{0} Starbook.SetSpeed({1}) = {2}", value, guideRate, response);
            }
        }

        public double GuideRateRightAscension
        {
            get
            {
                double guideRate = GuideRate(Telescope.guideRate);
                LogMessage("GuideRateRightAscension_get", "{0} {1}", guideRate, Telescope.guideRate);
                return guideRate;
            }
            set
            {
                Starbook.Response response = starbook.SetSpeed(guideRate = GuideRate(value));
                LogMessage("GuideRateRightAscension_set", "{0} Starbook.SetSpeed({1}) = {2}", value, guideRate, response);
            }
        }

        public bool IsPulseGuiding
        {
            get
            {
                LogMessage("IsPulseGuiding_get", pulseGuiding.ToString());
                return pulseGuiding;
            }
        }

        public void MoveAxis(TelescopeAxes Axis, double Rate)
        {
            LogMessage("MoveAxis", "{0} {1}", Axis, Rate);

            switch (Axis)
            {
                case TelescopeAxes.axisPrimary:
                {
                    if (Rate == 0)
                    {
                        starbook.NoMove();
                        starbook.SetSpeed(guideRate);
                    }
                    else
                    {
                        starbook.SetSpeed(GuideRate(Math.Abs(Rate)));
                        starbook.Move(Rate > 0 ? Starbook.Direction.East : Starbook.Direction.West);
                    }
                    break;
                }
                case TelescopeAxes.axisSecondary:
                {
                    if (Rate == 0)
                    {
                        starbook.NoMove();
                        starbook.SetSpeed(guideRate);
                    }
                    else
                    {
                        starbook.SetSpeed(GuideRate(Math.Abs(Rate)));
                        starbook.Move(Rate > 0 ? Starbook.Direction.North : Starbook.Direction.South);
                    }
                    break;
                }
                case TelescopeAxes.axisTertiary:
                {
                    throw new ASCOM.InvalidValueException("MoveAxis", "Axis", "Primary, Secondary");
                }
            }
        }

        public void Park()
        {
            LogMessage("Park", "{0} {1}", parkAzimuth, parkElevation);

            if (double.IsNaN(parkAzimuth) || double.IsNaN(parkElevation))
            {
                throw new ASCOM.InvalidOperationException("Park: SetPark must be called first.");
            }

            Starbook.Response response;

            if (!placeCached)
            {
                response = starbook.GetPlace(out placeCache);

                if (response == Starbook.Response.OK)
                {
                    LogMessage("Park", "Starbook.GetPlace() = {0}, {1} {2} {3}", response, placeCache.Latitude, placeCache.Longitude, placeCache.Timezone);
                }
                else
                {
                    LogMessage("Park", "Starbook.GetPlace() = {0}", response);
                    throw new ASCOM.InvalidOperationException("Park: Place is not available.");
                }

                placeCached = true;
            }

            response = starbook.GetTime(out DateTime time);

            if (response == Starbook.Response.OK)
            {
                LogMessage("Park", "Starbook.GetTime() = {0}, {1:yyyy+MM+dd+HH+mm+ss}", response, time);
            }
            else
            {
                LogMessage("Park", "Starbook.GetTime() = {0}", response);
                throw new ASCOM.InvalidOperationException("SetPark: Time is not available.");
            }

            transform.SiteLatitude = placeCache.Latitude.Value;
            transform.SiteLongitude = placeCache.Longitude.Value;
            transform.JulianDateUTC = utilities.DateUTCToJulian(time.AddHours(-placeCache.Timezone));
            transform.SetAzimuthElevation(parkAzimuth, parkElevation);

            if (!Starbook.HMS.FromValue(transform.RAJ2000, out Starbook.HMS rightAscension))
            {
                throw new ASCOM.InvalidOperationException("Park: Cannot determine rightAscension.");
            }

            if (!Starbook.DMS.FromValue(transform.DecJ2000, out Starbook.DMS declination))
            {
                throw new ASCOM.InvalidOperationException("Park: Cannot determine declination.");
            }

            response = starbook.Goto(rightAscension, declination);
            LogMessage("Park", "Starbook.Goto({0}, {1}) = {2}", rightAscension, declination, response);

            if (response == Starbook.Response.OK)
            {
                throw new ASCOM.InvalidOperationException("Park: Goto is not working.");
            }
        }

        public void PulseGuide(GuideDirections Direction, int Duration)
        {
            LogMessage("PulseGuide", "{0} {1}", Direction, Duration);

            Starbook.Direction direction;

            switch (Direction)
            {
                case GuideDirections.guideNorth:
                    direction = Starbook.Direction.North; break;
                case GuideDirections.guideSouth:
                    direction = Starbook.Direction.South; break;
                case GuideDirections.guideEast:
                    direction = Starbook.Direction.East; break;
                case GuideDirections.guideWest:
                    direction = Starbook.Direction.West; break;
                default:
                    throw new ASCOM.InvalidValueException("PulseGuide", "Direction", "North, South, East, West");
            }

            Starbook.Response response = starbook.Move(direction);
            LogMessage("PulseGuide", "Starbook.Move({0}) = {1}", direction, response);

            if (response == Starbook.Response.OK)
            {
                pulseGuiding = true;

                if (Duration > 0)
                {
                    Thread.Sleep(Duration);
                }

                response = starbook.NoMove();
                LogMessage("PulseGuide", "Starbook.NoMove() = {0}", response);

                pulseGuiding = false;

                if (response != Starbook.Response.OK)
                {
                    throw new ASCOM.InvalidOperationException("PulseGuide: NoMove is not working.");
                }
            }
            else
            {
                throw new ASCOM.InvalidOperationException("PulseGuide: Move is not working.");
            }
        }

        public double RightAscension
        {
            get
            {
                Starbook.Response response = starbook.GetStatus(out Starbook.Status status);

                if (response == Starbook.Response.OK)
                {
                    LogMessage("RightAscension_get", "Starbook.GetStatus() = {0}, {1} {2} {3} {4}", response, status.RA, status.Dec, status.State, status.Goto);
                }
                else
                {
                    LogMessage("RightAscension_get", "Starbook.GetStatus() = {0}", response);
                    throw new ASCOM.InvalidOperationException("RightAscension_get: Status is not available.");
                }

                double rightAscension = status.RA.Value;
                LogMessage("RightAscension_get", rightAscension.ToString());
                return rightAscension;
            }
        }

        public double RightAscensionRate
        {
            get
            {
                double rightAscensionRate = 0.0;
                LogMessage("RightAscensionRate_get", rightAscensionRate.ToString());
                return rightAscensionRate;
            }
            set
            {
                LogMessage("RightAscensionRate_set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("RightAscensionRate", true);
            }
        }

        public void SetPark()
        {
            Starbook.Response response = starbook.GetStatus(out Starbook.Status status);

            if (response == Starbook.Response.OK)
            {
                LogMessage("SetPark", "Starbook.GetStatus() = {0}, {1} {2} {3} {4}", response, status.RA, status.Dec, status.State, status.Goto);
            }
            else
            {
                LogMessage("SetPark", "Starbook.GetStatus() = {0}", response);
                throw new ASCOM.InvalidOperationException("SetPark: Status is not available.");
            }

            if (!placeCached)
            {
                response = starbook.GetPlace(out placeCache);

                if (response == Starbook.Response.OK)
                {
                    LogMessage("SetPark", "Starbook.GetPlace() = {0}, {1} {2} {3}", response, placeCache.Latitude, placeCache.Longitude, placeCache.Timezone);
                }
                else
                {
                    LogMessage("SetPark", "Starbook.GetPlace() = {0}", response);
                    throw new ASCOM.InvalidOperationException("SetPark: Place is not available.");
                }

                placeCached = true;
            }

            response = starbook.GetTime(out DateTime time);

            if (response == Starbook.Response.OK)
            {
                LogMessage("SetPark", "Starbook.GetTime() = {0}, {1:yyyy+MM+dd+HH+mm+ss}", response, time);
            }
            else
            {
                LogMessage("SetPark", "Starbook.GetTime() = {0}", response);
                throw new ASCOM.InvalidOperationException("SetPark: Time is not available.");
            }

            transform.SiteLatitude = placeCache.Latitude.Value;
            transform.SiteLongitude = placeCache.Longitude.Value;
            transform.JulianDateUTC = utilities.DateUTCToJulian(time.AddHours(-placeCache.Timezone));
            transform.SetJ2000(status.RA.Value, status.Dec.Value);

            parkAzimuth = transform.AzimuthTopocentric;
            parkElevation = transform.ElevationTopocentric;

            LogMessage("SetPark", "{0} {1}", parkAzimuth, parkElevation);
        }

        public PierSide SideOfPier
        {
            get
            {
                LogMessage("SideOfPier_get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SideOfPier", false);
            }
            set
            {
                LogMessage("SideOfPier_set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SideOfPier", true);
            }
        }

        public double SiderealTime
        {
            get
            {
                // Now using NOVAS 3.1
                double siderealTime = 0.0;
                using (var novas = new ASCOM.Astrometry.NOVAS.NOVAS31())
                {
                    var jd = utilities.DateUTCToJulian(DateTime.UtcNow);
                    novas.SiderealTime(jd, 0, novas.DeltaT(jd),
                        ASCOM.Astrometry.GstType.GreenwichApparentSiderealTime,
                        ASCOM.Astrometry.Method.EquinoxBased,
                        ASCOM.Astrometry.Accuracy.Reduced, ref siderealTime);
                }

                // Allow for the longitude
                siderealTime += SiteLongitude / 360.0 * 24.0;

                // Reduce to the range 0 to 24 hours
                siderealTime = astroUtilities.ConditionRA(siderealTime);

                LogMessage("SiderealTime_get", siderealTime.ToString());
                return siderealTime;
            }
        }

        public double SiteElevation
        {
            get
            {
                double siteElevation = 0;
                LogMessage("SiteElevation_get", siteElevation.ToString());
                return siteElevation;
            }
            set
            {
                LogMessage("SiteElevation_set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteElevation", true);
            }
        }

        public double SiteLatitude
        {
            get
            {
                if (!placeCached)
                {
                    Starbook.Response response = starbook.GetPlace(out placeCache);

                    if (response == Starbook.Response.OK)
                    {
                        LogMessage("SiteLatitude_get", "Starbook.GetPlace() = {0}, {1} {2} {3}", response, placeCache.Latitude, placeCache.Longitude, placeCache.Timezone);
                    }
                    else
                    {
                        LogMessage("SiteLatitude_get", "Starbook.GetPlace() = {0}", response);
                        throw new ASCOM.InvalidOperationException("SiteLatitude_get: Place is not available.");
                    }

                    placeCached = true;
                }

                double latitude = placeCache.Latitude.Value;
                LogMessage("SiteLatitude_get", latitude.ToString());
                return latitude;
            }
            set
            {
                LogMessage("SiteLatitude_set", value.ToString());

                if (!Starbook.DMS.FromValue(value, out Starbook.DMS latitude, Starbook.Direction.North, Starbook.Direction.South))
                {
                    throw new ASCOM.InvalidValueException("SiteLatitude", "SiteLatitude", "-90 to 90");
                }

                Starbook.Response response;

                if (!placeCached)
                {
                    response = starbook.GetPlace(out placeCache);

                    if (response == Starbook.Response.OK)
                    {
                        LogMessage("SiteLatitude_set", "Starbook.GetPlace() = {0}, {1} {2} {3}", response, placeCache.Latitude, placeCache.Longitude, placeCache.Timezone);
                    }
                    else
                    {
                        LogMessage("SiteLatitude_set", "Starbook.GetPlace() = {0}", response);
                        throw new ASCOM.InvalidOperationException("SiteLatitude_set: Place is not available.");
                    }

                    placeCached = true;
                }

                Starbook.Place place = placeCache;
                place.Latitude = latitude;
                response = starbook.SetPlace(place);
                LogMessage("SiteLatitude_set", "Starbook.SetPlace({0} {1} {2}) = {3}", place.Latitude, place.Longitude, place.Timezone, response);

                if (response == Starbook.Response.OK)
                {
                    this.placeCache = place;
                    this.placeCached = true;
                }
            }
        }

        public double SiteLongitude
        {
            get
            {
                if (!placeCached)
                {
                    Starbook.Response response = starbook.GetPlace(out placeCache);

                    if (response == Starbook.Response.OK)
                    {
                        LogMessage("SiteLongitude_get", "Starbook.GetPlace() = {0}, {1} {2} {3}", response, placeCache.Latitude, placeCache.Longitude, placeCache.Timezone);
                    }
                    else
                    {
                        LogMessage("SiteLongitude_get", "Starbook.GetPlace() = {0}", response);
                        throw new ASCOM.InvalidOperationException("SiteLongitude_get: Place is not available.");
                    }

                    placeCached = true;
                }

                double longitude = placeCache.Longitude.Value;
                LogMessage("SiteLongitude_get", longitude.ToString());
                return longitude;
            }
            set
            {
                LogMessage("SiteLongitude_set", value.ToString());

                if (!Starbook.DMS.FromValue(value, out Starbook.DMS longitude, Starbook.Direction.East, Starbook.Direction.West))
                {
                    throw new ASCOM.InvalidValueException("SiteLongitude", "SiteLongitude", "-180 to 180");
                }

                Starbook.Response response;

                if (!placeCached)
                {
                    response = starbook.GetPlace(out placeCache);

                    if (response == Starbook.Response.OK)
                    {
                        LogMessage("SiteLongitude_set", "Starbook.GetPlace() = {0}, {1} {2} {3}", response, placeCache.Latitude, placeCache.Longitude, placeCache.Timezone);
                    }
                    else
                    {
                        LogMessage("SiteLongitude_set", "Starbook.GetPlace() = {0}", response);
                        throw new ASCOM.InvalidOperationException("SiteLongitude_set: Place is not available.");
                    }

                    placeCached = true;
                }

                Starbook.Place place = placeCache;
                place.Longitude = longitude;
                response = starbook.SetPlace(place);
                LogMessage("SiteLongitude_set", "Starbook.SetPlace({0} {1} {2}) = {3}", place.Latitude, place.Longitude, place.Timezone, response);

                if (response == Starbook.Response.OK)
                {
                    this.placeCache = place;
                    this.placeCached = true;
                }
            }
        }

        public short SlewSettleTime
        {
            get
            {
                LogMessage("SlewSettleTime_get", slewSettleTime.ToString());
                return slewSettleTime;
            }
            set
            {
                LogMessage("SlewSettleTime_set", value.ToString());
                slewSettleTime = value;
            }
        }

        public void SlewToAltAz(double Azimuth, double Altitude)
        {
            LogMessage("SlewToAltAz", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToAltAz");
        }

        public void SlewToAltAzAsync(double Azimuth, double Altitude)
        {
            LogMessage("SlewToAltAzAsync", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToAltAzAsync");
        }

        public void SlewToCoordinates(double RightAscension, double Declination)
        {
            LogMessage("SlewToCoordinates", "{0} {1}", RightAscension, Declination);

            if (Starbook.HMS.FromValue(RightAscension, out Starbook.HMS rightAscension))
            {
                targetRightAscension = RightAscension;
            }
            else
            {
                throw new ASCOM.InvalidValueException("SlewToCoordinates", "RightAscension", "0 to 24");
            }

            if (Starbook.DMS.FromValue(Declination, out Starbook.DMS declination))
            {
                targetDeclination = Declination;
            }
            else
            {
                throw new ASCOM.InvalidValueException("SlewToCoordinates", "Declination", "-90 to 90");
            }

            Starbook.Response response = starbook.Goto(rightAscension, declination);
            LogMessage("SlewToCoordinates", "Starbook.Goto({0}, {1}) = {2}", rightAscension, declination, response);

            if (response != Starbook.Response.OK)
            {
                throw new ASCOM.InvalidOperationException("SlewToCoordinates: Goto is not working.");
            }
            
            if (slewSettleTime > 0)
            {
                Thread.Sleep(slewSettleTime * 1000);
            }

            while (true)
            {
                response = starbook.GetStatus(out Starbook.Status status);

                if (response == Starbook.Response.OK)
                {
                    LogMessage("SlewToCoordinates", "Starbook.GetStatus() = {0}, {1} {2} {3} {4}", response, status.RA, status.Dec, status.State, status.Goto);
                }
                else
                {
                    LogMessage("SlewToCoordinates", "Starbook.GetStatus() = {0}", response);
                    throw new ASCOM.InvalidOperationException("SlewToCoordinates: Status is not available.");
                }

                if (!status.Goto)
                {
                    break;
                }

                Thread.Sleep(100);
            }
        }

        public void SlewToCoordinatesAsync(double RightAscension, double Declination)
        {
            LogMessage("SlewToCoordinatesAsync", "{0} {1}", RightAscension, Declination);

            if (Starbook.HMS.FromValue(RightAscension, out Starbook.HMS rightAscension))
            {
                targetRightAscension = RightAscension;
            }
            else
            {
                throw new ASCOM.InvalidValueException("SlewToCoordinatesAsync", "RightAscension", "0 to 24");
            }

            if (Starbook.DMS.FromValue(Declination, out Starbook.DMS declination))
            {
                targetDeclination = Declination;
            }
            else
            {
                throw new ASCOM.InvalidValueException("SlewToCoordinatesAsync", "Declination", "-90 to 90");
            }

            Starbook.Response response = starbook.Goto(rightAscension, declination);
            LogMessage("SlewToCoordinatesAsync", "Starbook.Goto({0}, {1}) = {2}", rightAscension, declination, response);
        }

        public void SlewToTarget()
        {
            LogMessage("SlewToTarget", "{0} {1}", targetRightAscension, targetRightAscension);

            if (!Starbook.HMS.FromValue(targetRightAscension, out Starbook.HMS rightAscension))
            {
                throw new ASCOM.InvalidValueException("SlewToTarget: Target RightAscension is not set correctly.");
            }

            if (!Starbook.DMS.FromValue(targetDeclination, out Starbook.DMS declination))
            {
                throw new ASCOM.InvalidValueException("SlewToTarget: Target Declination is not set correctly.");
            }

            Starbook.Response response = starbook.Goto(rightAscension, declination);
            LogMessage("SlewToTarget", "Starbook.Goto({0}, {1}) = {2}", rightAscension, declination, response);

            if (response != Starbook.Response.OK)
            {
                throw new ASCOM.InvalidOperationException("SlewToTarget: Goto is not working.");
            }

            if (slewSettleTime > 0)
            {
                Thread.Sleep(slewSettleTime * 1000);
            }

            while (true)
            {
                response = starbook.GetStatus(out Starbook.Status status);

                if (response == Starbook.Response.OK)
                {
                    LogMessage("SlewToTarget", "Starbook.GetStatus() = {0}, {1} {2} {3} {4}", response, status.RA, status.Dec, status.State, status.Goto);
                }
                else
                {
                    LogMessage("SlewToTarget", "Starbook.GetStatus() = {0}", response);
                    throw new ASCOM.InvalidOperationException("SlewToTarget: Status is not available.");
                }

                if (!status.Goto)
                {
                    break;
                }

                Thread.Sleep(100);
            }
        }

        public void SlewToTargetAsync()
        {
            LogMessage("SlewToTargetAsync", "{0} {1}", targetRightAscension, targetRightAscension);

            if (!Starbook.HMS.FromValue(targetRightAscension, out Starbook.HMS rightAscension))
            {
                throw new ASCOM.InvalidOperationException("SlewToTargetAsync: Target RightAscension is not set correctly.");
            }

            if (!Starbook.DMS.FromValue(targetDeclination, out Starbook.DMS declination))
            {
                throw new ASCOM.InvalidOperationException("SlewToTargetAsync: Target Declination is not set correctly.");
            }

            Starbook.Response response = starbook.Goto(rightAscension, declination);
            LogMessage("SlewToTargetAsync", "Starbook.Goto({0}, {1}) = {2}", rightAscension, declination, response);
        }

        public bool Slewing
        {
            get
            {
                Starbook.Response response = starbook.GetStatus(out Telescope.Starbook.Status status);

                if (response == Starbook.Response.OK)
                {
                    LogMessage("Slewing_get", "Starbook.GetStatus() = {0}, {1} {2} {3} {4}", response, status.RA, status.Dec, status.State, status.Goto);
                }
                else
                {
                    LogMessage("Slewing_get", "Starbook.GetStatus() = {0}", response);
                    throw new ASCOM.InvalidOperationException("Slewing_get: Status is not available.");
                }

                bool qoto = status.Goto;
                LogMessage("Slewing_get", qoto.ToString());
                return qoto;
            }
        }

        public void SyncToAltAz(double Azimuth, double Altitude)
        {
            LogMessage("SyncToAltAz", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SyncToAltAz");
        }

        public void SyncToCoordinates(double RightAscension, double Declination)
        {
            LogMessage("SyncToCoordinates", "{0} {1}", RightAscension, Declination);

            if (Starbook.HMS.FromValue(RightAscension, out Starbook.HMS rightAscension))
            {
                targetRightAscension = RightAscension;
            }
            else
            {
                throw new ASCOM.InvalidValueException("SyncToCoordinates", "RightAscension", "0 to 24");
            }

            if (Starbook.DMS.FromValue(Declination, out Starbook.DMS declination))
            {
                targetDeclination = Declination;
            }
            else
            {
                throw new ASCOM.InvalidValueException("SyncToCoordinate", "Declination", "-90 to 90");
            }

            Starbook.Response response = starbook.Align(rightAscension, declination);
            LogMessage("SyncToCoordinates", "Starbook.Align({0}, {1}) = {2}", rightAscension, declination, response);
        }

        public void SyncToTarget()
        {
            LogMessage("SyncToTarget", "{0} {1}", targetRightAscension, targetRightAscension);

            if (!Starbook.HMS.FromValue(targetRightAscension, out Starbook.HMS rightAscension))
            {
                throw new ASCOM.InvalidOperationException("SyncToTarget: Target RightAscension is not set correctly.");
            }

            if (!Starbook.DMS.FromValue(targetDeclination, out Starbook.DMS declination))
            {
                throw new ASCOM.InvalidOperationException("SyncToTarget: Target Declination is not set correctly.");
            }

            Starbook.Response response = starbook.Align(rightAscension, declination);
            LogMessage("SyncToTarget", "Starbook.Align({0}, {1}) = {2}", rightAscension, declination, response);
        }

        public double TargetDeclination
        {
            get
            {
                LogMessage("TargetDeclination_get", double.IsNaN(targetDeclination) ? "NaN" : targetDeclination.ToString());
                return targetDeclination;
            }
            set
            {
                LogMessage("TargetDeclination_set", value.ToString());
                targetDeclination = value;
            }
        }

        public double TargetRightAscension
        {
            get
            {
                LogMessage("TargetRightAscension_get", double.IsNaN(targetRightAscension) ? "NaN" : targetRightAscension.ToString());
                return targetRightAscension;
            }
            set
            {
                LogMessage("TargetRightAscension_set", value.ToString());
                targetRightAscension = value;
            }
        }

        public bool Tracking
        {
            get
            {
                Starbook.Response response = starbook.GetStatus(out Starbook.Status status);

                if (response == Starbook.Response.OK)
                {
                    LogMessage("Tracking_get", "Starbook.GetStatus() = {0}, {1} {2} {3} {4}", response, status.RA, status.Dec, status.State, status.Goto);
                }
                else
                {
                    LogMessage("Tracking_get", "Starbook.GetStatus() = {0}", response);
                    throw new ASCOM.InvalidOperationException("Tracking_get: Status is not available.");
                }

                bool tracking = (status.State == Starbook.State.Guide || status.State == Starbook.State.Scope || status.State == Starbook.State.Chart || status.State == Starbook.State.User);
                LogMessage("Tracking_get", tracking.ToString());
                return tracking;
            }
            set
            {
                LogMessage("Tracking_set", value.ToString());

                if (value)
                {
                    Starbook.Response response = starbook.Start();
                    LogMessage("Tracking_set", "Starbook.Start() = {0}", response);
                }
                else
                {
                    LogMessage("Tracking_set", "Cannot stop tracking.");
                }
            }
        }

        public DriveRates TrackingRate
        {
            get
            {
                DriveRates trackingRate = DriveRates.driveSidereal;
                LogMessage("TrackingRate_get", trackingRate.ToString());
                return trackingRate;
            }
            set
            {
                LogMessage("TrackingRate_set", value.ToString());

                if (value != DriveRates.driveSidereal)
                {
                    throw new ASCOM.InvalidValueException("TrackingRate", "TrackingRate", "Sidereal");
                }
            }
        }

        public ITrackingRates TrackingRates
        {
            get
            {
                ITrackingRates trackingRates = new TrackingRates();

                foreach (DriveRates driveRate in trackingRates)
                {
                    LogMessage("TrackingRates_get", driveRate.ToString());
                }

                return trackingRates;
            }
        }

        public DateTime UTCDate
        {
            get
            {
                Starbook.Response response = starbook.GetTime(out DateTime time);

                if (response == Starbook.Response.OK)
                {
                    LogMessage("UTCDate_get", "Starbook.GetTime() = {0}, {1:yyyy+MM+dd+HH+mm+ss}", response, time);
                }
                else
                {
                    LogMessage("UTCDate_get", "Starbook.GetTime() = {0}", response);
                    throw new ASCOM.InvalidOperationException("UTCDate_get: Time is not available.");
                }

                if (!placeCached)
                {
                    response = starbook.GetPlace(out placeCache);

                    if (response == Starbook.Response.OK)
                    {
                        LogMessage("UTCDate_get", "Starbook.GetPlace() = {0}, {1} {2} {3}", response, placeCache.Latitude, placeCache.Longitude, placeCache.Timezone);
                    }
                    else
                    {
                        LogMessage("UTCDate_get", "Starbook.GetPlace() = {0}", response);
                        throw new ASCOM.InvalidOperationException("UTCDate_get: Place is not available.");
                    }

                    placeCached = true;
                }

                DateTime utcDate = time.AddHours(-placeCache.Timezone);
                LogMessage("UTCDate_get", "{0:yyyy/MM/dd HH:mm:ss}", utcDate);
                return utcDate;
            }
            set
            {
                LogMessage("UTCDate_set", "{0:yyyy/MM/dd HH:mm:ss}", value);

                Starbook.Response response;

                if (!placeCached)
                {
                    response = starbook.GetPlace(out placeCache);

                    if (response == Starbook.Response.OK)
                    {
                        LogMessage("UTCDate_set", "Starbook.GetPlace() = {0}, {1} {2} {3}", response, placeCache.Latitude, placeCache.Longitude, placeCache.Timezone);
                    }
                    else
                    {
                        LogMessage("UTCDate_set", "Starbook.GetPlace() = {0}", response);
                        throw new ASCOM.InvalidOperationException("UTCDate_set: Place is not available.");
                    }

                    placeCached = true;
                }

                DateTime time = value.AddHours(placeCache.Timezone);
                response = starbook.SetTime(time);
                LogMessage("UTCDate_set", "Starbook.SetTime({0:yyyy+MM+dd+HH+mm+ss}) = {1}", time, response);
            }
        }

        public void Unpark()
        {
            LogMessage("Unpark", "");
        }

        #endregion

        #region Private properties and methods
        // here are some useful properties and methods that can be used as required
        // to help with driver development

        #region ASCOM Registration

        // Register or unregister driver for ASCOM. This is harmless if already
        // registered or unregistered. 
        //
        /// <summary>
        /// Register or unregister the driver with the ASCOM Platform.
        /// This is harmless if the driver is already registered/unregistered.
        /// </summary>
        /// <param name="bRegister">If <c>true</c>, registers the driver, otherwise unregisters it.</param>
        private static void RegUnregASCOM(bool bRegister)
        {
            using (var P = new ASCOM.Utilities.Profile())
            {
                P.DeviceType = "Telescope";
                if (bRegister)
                {
                    P.Register(driverID, driverDescription);
                }
                else
                {
                    P.Unregister(driverID);
                }
            }
        }

        /// <summary>
        /// This function registers the driver with the ASCOM Chooser and
        /// is called automatically whenever this class is registered for COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is successfully built.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During setup, when the installer registers the assembly for COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually register a driver with ASCOM.
        /// </remarks>
        [ComRegisterFunction]
        public static void RegisterASCOM(Type t)
        {
            RegUnregASCOM(true);
        }

        /// <summary>
        /// This function unregisters the driver from the ASCOM Chooser and
        /// is called automatically whenever this class is unregistered from COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is cleaned or prior to rebuilding.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During uninstall, when the installer unregisters the assembly from COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually unregister a driver from ASCOM.
        /// </remarks>
        [ComUnregisterFunction]
        public static void UnregisterASCOM(Type t)
        {
            RegUnregASCOM(false);
        }

        #endregion

        /// <summary>
        /// Returns true if there is a valid connection to the driver hardware
        /// </summary>
        private bool IsConnected
        {
            get
            {
                // TODO check that the driver hardware connection exists and is connected to the hardware
                return connectedState;
            }
        }

        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private void CheckConnected(string message)
        {
            if (!IsConnected)
            {
                throw new ASCOM.NotConnectedException(message);
            }
        }

        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        internal void ReadProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Telescope";
                starbook.IPAddress = IPAddress.Parse(driverProfile.GetValue(driverID, ipAddressProfileName, string.Empty, ipAddressDefault));
                slewSettleTime = short.Parse(driverProfile.GetValue(driverID, slewSettleTimeProfileName, string.Empty, slewSettleTimeDefault));

                guideRate = int.Parse(driverProfile.GetValue(driverID, guideRateProfileName, string.Empty, guideRateDefault));
                guideRates = new double[9];

                string[] guideRateStrings = driverProfile.GetValue(driverID, guideRatesProfileName, string.Empty, guideRatesDefault).Split(',');

                if (guideRateStrings.Length == 9)
                {
                    for (int index = 0; index < 9; index++)
                    {
                        if (guideRateStrings[index] == "NaN")
                        {
                            guideRates[index] = double.NaN;
                        }
                        else
                        {
                            guideRates[index] = double.Parse(guideRateStrings[index]);
                        }
                    }
                }

                traceLogger.Enabled = Convert.ToBoolean(driverProfile.GetValue(driverID, traceLoggerProfileName, string.Empty, traceLoggerDefault));
            }
        }

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        internal void WriteProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Telescope";
                driverProfile.WriteValue(driverID, ipAddressProfileName, starbook.IPAddress.ToString());
                driverProfile.WriteValue(driverID, slewSettleTimeProfileName, slewSettleTime.ToString());

                driverProfile.WriteValue(driverID, guideRateProfileName, guideRate.ToString());

                string[] guideRateStrings = new string[9];

                for (int index = 0; index < 9; index++)
                {
                    if (double.IsNaN(guideRates[index]))
                    {
                        guideRateStrings[index] = "NaN";
                    }
                    else
                    {
                        guideRateStrings[index] = guideRates[index].ToString();
                    }
                }

                driverProfile.WriteValue(driverID, guideRatesProfileName, string.Join(",", guideRateStrings));

                driverProfile.WriteValue(driverID, traceLoggerProfileName, traceLogger.Enabled.ToString());
            }
        }

        /// <summary>
        /// Log helper function that takes formatted strings and arguments
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        internal static void LogMessage(string identifier, string message, params object[] args)
        {
            var msg = string.Format(message, args);
            traceLogger.LogMessage(identifier, msg);
        }
        #endregion
    }
}
