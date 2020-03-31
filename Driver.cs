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
        private bool parking;

        private Starbook.Place placeCache;
        private bool placeCached;

        private bool movingAxis;
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

            parkAzimuth = 270;
            parkElevation = 0;
            parking = false;

            placeCached = false;

            movingAxis = false;
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
            if (this.parking)
            {
                LogMessage("AbortSlew", "InvalidOperationException: AtPark");
                throw new ASCOM.InvalidOperationException("AbortSlew: AtPark, Starbook.Unpark() must be called first.");
            }

            Starbook.Response response = starbook.Stop();

            if (response != Starbook.Response.OK)
            {
                LogMessage("AbortSlew", "InvalidOperationException: Starbook.Stop()={0}", response);
                throw new ASCOM.InvalidOperationException("AbortSlew: Starbook.Stop() is not working.");
            }

            LogMessage("AbortSlew", "OK");
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
                LogMessage("Altitude_get", "PropertyNotImplementedException");
                throw new ASCOM.PropertyNotImplementedException("Altitude", false);
            }
        }

        public double ApertureArea
        {
            get
            {
                LogMessage("ApertureArea_get", "PropertyNotImplementedException");
                throw new ASCOM.PropertyNotImplementedException("ApertureArea", false);
            }
        }

        public double ApertureDiameter
        {
            get
            {
                LogMessage("ApertureDiameter_get", "PropertyNotImplementedException");
                throw new ASCOM.PropertyNotImplementedException("ApertureDiameter", false);
            }
        }

        public bool AtHome
        {
            get
            {
                Starbook.Response response = starbook.GetXY(out Starbook.XY xy);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("AtHome_get", "InvalidOperationException: Starbook.GetXY()={0}", response);
                    throw new ASCOM.InvalidOperationException("AtHome_get: Starbook.GetXY() is not working.");
                }

                // TODO: This doesn't work!!

                bool atHome = xy.X == 0 && xy.Y == 0;
                LogMessage("AtHome_get", "{0}: X={1},Y={2}", atHome, xy.X, xy.Y);
                return atHome;
            }
        }

        public bool AtPark
        {
            get
            {
                bool atPark = parking;
                LogMessage("AtPark_get", atPark.ToString());
                return atPark;
            }
        }

        public IAxisRates AxisRates(TelescopeAxes Axis)
        {
            IAxisRates axisRates = new AxisRates(Axis);

            foreach (IRate axisRate in axisRates)
            {
                LogMessage("AxisRates", "{0}-{1}: Axis={2}", axisRate.Minimum, axisRate.Maximum, Axis);
            }

            return axisRates;
        }

        public double Azimuth
        {
            get
            {
                LogMessage("Azimuth_get", "PropertyNotImplementedException");
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
                {
                    canMoveAxis = true; break;
                }
                case TelescopeAxes.axisSecondary:
                {
                    canMoveAxis = true; break;
                }
                case TelescopeAxes.axisTertiary:
                {
                    canMoveAxis = false; break;
                }
                default:
                {
                    LogMessage("CanMoveAxis", "InvalidValueException: Axis={0}", Axis);
                    throw new InvalidValueException("CanMoveAxis", "Axis", "Primary, Secondary, Tertiary");
                }
            }

            LogMessage("CanMoveAxis", "{0}: Axis={1}", canMoveAxis, Axis);
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

                if (response != Starbook.Response.OK)
                {
                    LogMessage("Declination_get", "InvalidOperationException: Starbook.GetStatus()={0}", response);
                    throw new ASCOM.InvalidOperationException("Declination_get: Starbook.GetStatus() is not working.");
                }

                double declination = status.Dec.Value;
                LogMessage("Declination_get", "{0}: Status.Dec={1}", declination, status.Dec);
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
                LogMessage("DeclinationRate_set", "PropertyNotImplementedException");
                throw new ASCOM.PropertyNotImplementedException("DeclinationRate", true);
            }
        }

        public PierSide DestinationSideOfPier(double RightAscension, double Declination)
        {
            LogMessage("DestinationSideOfPier", "MethodNotImplementedException");
            throw new ASCOM.MethodNotImplementedException("DestinationSideOfPier");
        }

        public bool DoesRefraction
        {
            get
            {
                LogMessage("DoesRefraction_get", "PropertyNotImplementedException");
                throw new ASCOM.PropertyNotImplementedException("DoesRefraction", false);
            }
            set
            {
                LogMessage("DoesRefraction_set", "PropertyNotImplementedException");
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
            if (this.parking)
            {
                LogMessage("FindHome", "InvalidOperationException: AtPark");
                throw new ASCOM.InvalidOperationException("FindHome: AtPark, Starbook.Unpark() must be called first.");
            }

            Starbook.Response response = starbook.Home();

            if (response != Starbook.Response.OK)
            {
                LogMessage("FindHome", "InvalidOperationException: Starbook.Home()={0}", response);
                throw new ASCOM.InvalidOperationException("FindHome: Starbook.Home() is not working.");
            }

            LogMessage("FindHome", "OK");
        }

        public double FocalLength
        {
            get
            {
                LogMessage("FocalLength_get", "PropertyNotImplementedException");
                throw new ASCOM.PropertyNotImplementedException("FocalLength", false);
            }
        }

        public static double GuideRate(int guideRate)
        {
            return guideRates[guideRate] * (15.041 / 3600);
        }

        public static int GuideRate(double guideRate)
        {
            double guideRateDifference = double.MaxValue;
            int guideRateIndex = 0;

            for (int index = 0; index <= 8; index++)
            {
                double difference = Math.Abs(guideRate - GuideRate(index));

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
                LogMessage("GuideRateDeclination_get", "{0}: GuideRate={1}", guideRate, Telescope.guideRate);
                return guideRate;
            }
            set
            {
                Starbook.Response response = starbook.SetSpeed(Telescope.guideRate = GuideRate(value));

                if (response != Starbook.Response.OK)
                {
                    LogMessage("GuideRateDeclination_set", "InvalidOperationException: {0}, Starbook.SetSpeed({1})={2}", value, Telescope.guideRate, response);
                    throw new ASCOM.InvalidOperationException("GuideRateDeclination_set: Starbook.SetSpeed() is not working.");
                }

                LogMessage("GuideRateDeclination_set", "OK: {0}, GuideRate={1}", value, Telescope.guideRate);
            }
        }

        public double GuideRateRightAscension
        {
            get
            {
                double guideRate = GuideRate(Telescope.guideRate);
                LogMessage("GuideRateRightAscension_get", "{0}: GuideRate={1}", guideRate, Telescope.guideRate);
                return guideRate;
            }
            set
            {
                Starbook.Response response = starbook.SetSpeed(guideRate = GuideRate(value));

                if (response != Starbook.Response.OK)
                {
                    LogMessage("GuideRateRightAscension_set", "InvalidOperationException: {0}, Starbook.SetSpeed({1})={2}", value, Telescope.guideRate, response);
                    throw new ASCOM.InvalidOperationException("GuideRateRightAscension_set: Starbook.SetSpeed() is not working.");
                }

                LogMessage("GuideRateRightAscension_set", "OK: {0}, GuideRate={1}", value, Telescope.guideRate);
            }
        }

        public bool IsPulseGuiding
        {
            get
            {
                bool isPulseGuiding = pulseGuiding;
                LogMessage("IsPulseGuiding_get", isPulseGuiding.ToString());
                return isPulseGuiding;
            }
        }

        public void MoveAxis(TelescopeAxes Axis, double Rate)
        {
            switch (Axis)
            {
                case TelescopeAxes.axisPrimary:
                case TelescopeAxes.axisSecondary:
                {
                    if (this.parking)
                    {
                        LogMessage("MoveAxis", "InvalidOperationException: AtPark; Axis={0},Rate={1}", Axis, Rate);
                        throw new ASCOM.InvalidOperationException("MoveAxis: AtPark, Starbook.Unpark() must be called first.");
                    }

                    if (Rate == 0)
                    {
                        Starbook.Response response = starbook.NoMove();

                        if (response != Starbook.Response.OK)
                        {
                            LogMessage("MoveAxis", "InvalidOperationException: Starbook.NoMove()={0}", response);
                            throw new ASCOM.InvalidOperationException("MoveAxis: Starbook.NoMove() is not working.");
                        }

                        movingAxis = false;

                        response = starbook.SetSpeed(Telescope.guideRate);

                        if (response != Starbook.Response.OK)
                        {
                            LogMessage("MoveAxis", "InvalidOperationException: Starbook.SetSpeed({0})={1}", guideRate, response);
                            throw new ASCOM.InvalidOperationException("MoveAxis: Starbook.SetSpeed() is not working.");
                        }
                    }
                    else
                    {
                        double minRate = GuideRate(0);
                        double maxRate = GuideRate(8);

                        if (Math.Abs(Rate) < minRate || maxRate < Math.Abs(Rate))
                        {
                            LogMessage("MoveAxis", "InvalidValueException: Rate={0}", Rate);
                            throw new ASCOM.InvalidValueException("MoveAxis", "Rate", string.Format("{0} to {1} or {2} to {3}", minRate, maxRate, -maxRate, -minRate));
                        }

                        int guideRate = GuideRate(Math.Abs(Rate));

                        Starbook.Response response = starbook.SetSpeed(guideRate);

                        if (response != Starbook.Response.OK)
                        {
                            LogMessage("MoveAxis", "InvalidOperationException: Rate={0}, Starbook.SetSpeed({1})={2}", Rate, guideRate, response);
                            throw new ASCOM.InvalidOperationException("MoveAxis: Starbook.SetSpeed() is not working.");
                        }

                        Starbook.Direction direction;

                        if (Axis == TelescopeAxes.axisPrimary)
                        {
                            direction = Rate > 0 ? Starbook.Direction.East : Starbook.Direction.West;
                        }
                        else/* if (Axis == TelescopeAxes.axisSecondary)*/
                        {
                            direction = Rate > 0 ? Starbook.Direction.North : Starbook.Direction.South;
                        }

                        response = starbook.Move(direction);

                        if (response != Starbook.Response.OK)
                        {
                            LogMessage("MoveAxis", "InvalidOperationException: Axis={0}, Starbook.Move({1})={2}", Axis, direction, response);
                            throw new ASCOM.InvalidOperationException("MoveAxis: Starbook.Move() is not working.");
                        }

                        movingAxis = true;
                    }

                    break;
                }
                case TelescopeAxes.axisTertiary:
                {
                    LogMessage("MoveAxis", "InvalidValueException: Axis={0}", Axis);
                    throw new ASCOM.InvalidValueException("MoveAxis", "Axis", "Primary, Secondary");
                }
            }
        }

        public void Park()
        {
            if (parking)
            {
                LogMessage("Park", "OK: Skipped");
            }
            else
            {
                Starbook.Response response;

                if (!placeCached)
                {
                    response = starbook.GetPlace(out placeCache);

                    if (response != Starbook.Response.OK)
                    {
                        LogMessage("Park", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                        throw new ASCOM.InvalidOperationException("Park: Starbook.GetPlace() is not working.");
                    }
                    
                    placeCached = true;
                }

                response = starbook.GetTime(out DateTime time);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("Park", "InvalidOperationException: Starbook.GetTime()={0}", response);
                    throw new ASCOM.InvalidOperationException("SetPark: Starbook.GetTime() is not working.");
                }

                transform.SiteLatitude = placeCache.Latitude.Value;
                transform.SiteLongitude = placeCache.Longitude.Value;
                transform.JulianDateUTC = utilities.DateUTCToJulian(time.AddHours(-placeCache.Timezone));
                transform.SetAzimuthElevation(parkAzimuth, parkElevation);

                if (!Starbook.HMS.FromValue(transform.RAJ2000, out Starbook.HMS rightAscension))
                {
                    LogMessage("Park", "InvalidOperationException: Cannot determine rightAscension={0}", transform.RAJ2000);
                    throw new ASCOM.InvalidOperationException("Park: Cannot determine rightAscension.");
                }

                if (!Starbook.DMS.FromValue(transform.DecJ2000, out Starbook.DMS declination))
                {
                    LogMessage("Park", "InvalidOperationException: Cannot determine declination={0}", transform.DecJ2000);
                    throw new ASCOM.InvalidOperationException("Park: Cannot determine declination.");
                }

                response = starbook.Goto(rightAscension, declination);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("Park", "InvalidOperationException: Starbook.Goto({0},{1})={2}", rightAscension, declination, response);
                    throw new ASCOM.InvalidOperationException("Park: Starbook.Goto() is not working.");
                }

                LogMessage("Park", "OK: Altitude={0},Azimuth={1},RightAscension={2},Declination={3}", parkElevation, parkAzimuth, rightAscension, declination);

                parking = true;
            }
        }

        public void PulseGuide(GuideDirections Direction, int Duration)
        {
            if (this.parking)
            {
                LogMessage("PulseGuide", "InvalidOperationException: AtPark");
                throw new ASCOM.InvalidOperationException("PulseGuide: AtPark, Starbook.Unpark() must be called first.");
            }

            Starbook.Direction direction;

            switch (Direction)
            {
                case GuideDirections.guideNorth:
                {
                    direction = Starbook.Direction.North; break;
                }
                case GuideDirections.guideSouth:
                {
                    direction = Starbook.Direction.South; break;
                }
                case GuideDirections.guideEast:
                {
                    direction = Starbook.Direction.East; break;
                }
                case GuideDirections.guideWest:
                {
                    direction = Starbook.Direction.West; break;
                }
                default:
                {
                    LogMessage("PulseGuide", "InvalidValueException: Direction={0}", Direction);
                    throw new ASCOM.InvalidValueException("PulseGuide", "Direction", "North, South, East, West");
                }
            }

            Starbook.Response response = starbook.Move(direction);

            if (response != Starbook.Response.OK)
            {
                LogMessage("PulseGuide", "InvalidOperationException: Starbook.Move({0})={1}", direction, response);
                throw new ASCOM.InvalidOperationException("PulseGuide: Starbook.Move() is not working.");
            }

            pulseGuiding = true;

            if (Duration > 0)
            {
                Thread.Sleep(Duration);
            }

            response = starbook.NoMove();

            pulseGuiding = false;

            if (response != Starbook.Response.OK)
            {
                LogMessage("PulseGuide", "InvalidOperationException: Starbook.NoMove()={0}", response);
                throw new ASCOM.InvalidOperationException("PulseGuide: Starbook.NoMove() is not working.");
            }

            LogMessage("PulseGuide", "OK: Direction={0},Duration={1}", Direction, Duration);
        }

        public double RightAscension
        {
            get
            {
                Starbook.Response response = starbook.GetStatus(out Starbook.Status status);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("RightAscension_get", "InvalidOperationException: Starbook.GetStatus()={0}", response);
                    throw new ASCOM.InvalidOperationException("RightAscension_get: Starbook.GetStatus() is not working.");
                }

                double rightAscension = status.RA.Value;
                LogMessage("RightAscension_get", "{0}: Status.RA={1}", rightAscension, status.RA);
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
                LogMessage("RightAscensionRate_set", "PropertyNotImplementedException");
                throw new ASCOM.PropertyNotImplementedException("RightAscensionRate", true);
            }
        }

        public void SetPark()
        {
            Starbook.Response response = starbook.GetStatus(out Starbook.Status status);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SetPark", "InvalidOperationException: Starbook.GetStatus()={0}", response);
                throw new ASCOM.InvalidOperationException("SetPark: Starbook.GetStatus() is not working.");
            }

            if (!placeCached)
            {
                response = starbook.GetPlace(out placeCache);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("SetPark", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                    throw new ASCOM.InvalidOperationException("SetPark: Starbook.GetPlace() is not working.");
                }

                placeCached = true;
            }

            response = starbook.GetTime(out DateTime time);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SetPark", "InvalidOperationException: Starbook.GetTime()={0}", response);
                throw new ASCOM.InvalidOperationException("SetPark: Starbook.GetTime() is not working.");
            }

            transform.SiteLatitude = placeCache.Latitude.Value;
            transform.SiteLongitude = placeCache.Longitude.Value;
            transform.JulianDateUTC = utilities.DateUTCToJulian(time.AddHours(-placeCache.Timezone));
            transform.SetJ2000(status.RA.Value, status.Dec.Value);

            parkAzimuth = transform.AzimuthTopocentric;
            parkElevation = transform.ElevationTopocentric;

            LogMessage("SetPark", "OK: Altitude={0},Azimuth={1},RightAscension={2},Declination={3}", parkElevation, parkAzimuth, status.RA, status.Dec);
        }

        public PierSide SideOfPier
        {
            get
            {
                LogMessage("SideOfPier_get", "PropertyNotImplementedException");
                throw new ASCOM.PropertyNotImplementedException("SideOfPier", false);
            }
            set
            {
                LogMessage("SideOfPier_set", "PropertyNotImplementedException");
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
                LogMessage("SiteElevation_set", "PropertyNotImplementedException");
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

                    if (response != Starbook.Response.OK)
                    {
                        LogMessage("SiteLatitude_get", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                        throw new ASCOM.InvalidOperationException("SiteLatitude_get: Starbook.GetPlace() is not working.");
                    }

                    placeCached = true;
                }

                double latitude = placeCache.Latitude.Value;
                LogMessage("SiteLatitude_get", "{0}: Place.Latitude={1}", latitude, placeCache.Latitude);
                return latitude;
            }
            set
            {
                if (!Starbook.DMS.FromValue(value, out Starbook.DMS latitude, Starbook.Direction.North, Starbook.Direction.South))
                {
                    LogMessage("SiteLatitude_set", "InvalidValueException: {0}", value);
                    throw new ASCOM.InvalidValueException("SiteLatitude", "SiteLatitude", "-90 to 90");
                }

                Starbook.Response response;

                if (!placeCached)
                {
                    response = starbook.GetPlace(out placeCache);

                    if (response != Starbook.Response.OK)
                    {
                        LogMessage("SiteLatitude_set", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                        throw new ASCOM.InvalidOperationException("SiteLatitude_set: Starbook.GetPlace() is not working.");
                    }

                    placeCached = true;
                }

                Starbook.Place place = placeCache;
                place.Latitude = latitude;
                response = starbook.SetPlace(place);

                if (response == Starbook.Response.OK)
                {
                    LogMessage("SiteLatitude_set", "OK: {0}", value);

                    this.placeCache = place;
                    this.placeCached = true;
                }
                else
                {
                    LogMessage("SiteLatitude_set", "FAIL: {0}, Starbook.SetPlace({1} {2} {3})={4}", value, place.Latitude, place.Longitude, place.Timezone, response);
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

                    if (response != Starbook.Response.OK)
                    {
                        LogMessage("SiteLongitude_get", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                        throw new ASCOM.InvalidOperationException("SiteLongitude_get: Starbook.GetPlace() is not working.");
                    }

                    placeCached = true;
                }

                double longitude = placeCache.Longitude.Value;
                LogMessage("SiteLongitude_get", "{0}: Place.Longitude={1}", longitude, placeCache.Longitude);
                return longitude;
            }
            set
            {
                if (!Starbook.DMS.FromValue(value, out Starbook.DMS longitude, Starbook.Direction.East, Starbook.Direction.West))
                {
                    LogMessage("SiteLongitude_set", "InvalidValueException: {0}", value);
                    throw new ASCOM.InvalidValueException("SiteLongitude", "SiteLongitude", "-180 to 180");
                }

                Starbook.Response response;

                if (!placeCached)
                {
                    response = starbook.GetPlace(out placeCache);

                    if (response != Starbook.Response.OK)
                    {
                        LogMessage("SiteLongitude_set", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                        throw new ASCOM.InvalidOperationException("SiteLongitude_set: Starbook.GetPlace() is not working.");
                    }

                    placeCached = true;
                }

                Starbook.Place place = placeCache;
                place.Longitude = longitude;
                response = starbook.SetPlace(place);

                if (response == Starbook.Response.OK)
                {
                    LogMessage("SiteLongitude_set", "OK: {0}", value);

                    this.placeCache = place;
                    this.placeCached = true;
                }
                else
                {
                    LogMessage("SiteLongitude_set", "FAIL: {0}, Starbook.SetPlace({1} {2} {3})={4}", value, place.Latitude, place.Longitude, place.Timezone, response);
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
                if (value < 0)
                {
                    LogMessage("SlewSettleTime_set", "InvalidValueException: {0}", value);
                    throw new ASCOM.InvalidValueException("SlewSettleTime_set", "SlewSettleTime", "0 to 32767");
                }

                slewSettleTime = value;
                LogMessage("SlewSettleTime_set", "OK: {0}", value);
            }
        }

        public void SlewToAltAz(double Azimuth, double Altitude)
        {
            LogMessage("SlewToAltAz", "MethodNotImplementedException");
            throw new ASCOM.MethodNotImplementedException("SlewToAltAz");
        }

        public void SlewToAltAzAsync(double Azimuth, double Altitude)
        {
            LogMessage("SlewToAltAzAsync", "MethodNotImplementedException");
            throw new ASCOM.MethodNotImplementedException("SlewToAltAzAsync");
        }

        public void SlewToCoordinates(double RightAscension, double Declination)
        {
            if (this.parking)
            {
                LogMessage("SlewToCoordinates", "InvalidOperationException: AtPark");
                throw new ASCOM.InvalidOperationException("SlewToCoordinates: AtPark, Starbook.Unpark() must be called first.");
            }

            if (!Starbook.HMS.FromValue(RightAscension, out Starbook.HMS rightAscension))
            {
                LogMessage("SlewToCoordinates", "InvalidValueException: RightAscension={0}", RightAscension);
                throw new ASCOM.InvalidValueException("SlewToCoordinates", "RightAscension", "0 to 24");
            }

            if (!Starbook.DMS.FromValue(Declination, out Starbook.DMS declination))
            {
                LogMessage("SlewToCoordinates", "InvalidValueException: Declination={0}", Declination);
                throw new ASCOM.InvalidValueException("SlewToCoordinates", "Declination", "-90 to 90");
            }

            targetRightAscension = RightAscension;
            targetDeclination = Declination;

            Starbook.Response response = starbook.Goto(rightAscension, declination);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SlewToCoordinates", "InvalidOperationException: Starbook.Goto({0},{1})={2}", rightAscension, declination, response);
                throw new ASCOM.InvalidOperationException("SlewToCoordinates: Starbook.Goto() is not working.");
            }
            
            if (slewSettleTime > 0)
            {
                Thread.Sleep(slewSettleTime * 1000);
            }

            while (true)
            {
                response = starbook.GetStatus(out Starbook.Status status);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("SlewToCoordinates", "InvalidOperationException: Starbook.GetStatus()={0}", response);
                    throw new ASCOM.InvalidOperationException("SlewToCoordinates: Starbook.GetStatus() is not working.");
                }

                if (!status.Goto)
                {
                    break;
                }

                Thread.Sleep(100);
            }

            LogMessage("SlewToCoordinates", "OK: RightAscension={0},Declination={1}", RightAscension, Declination);
        }

        public void SlewToCoordinatesAsync(double RightAscension, double Declination)
        {
            if (this.parking)
            {
                LogMessage("SlewToCoordinatesAsync", "InvalidOperationException: AtPark");
                throw new ASCOM.InvalidOperationException("SlewToCoordinatesAsync: AtPark, Starbook.Unpark() must be called first.");
            }

            if (!Starbook.HMS.FromValue(RightAscension, out Starbook.HMS rightAscension))
            {
                LogMessage("SlewToCoordinatesAsync", "InvalidValueException: RightAscension={0}", RightAscension);
                throw new ASCOM.InvalidValueException("SlewToCoordinatesAsync", "RightAscension", "0 to 24");
            }

            if (!Starbook.DMS.FromValue(Declination, out Starbook.DMS declination))
            {
                LogMessage("SlewToCoordinatesAsync", "InvalidValueException: Declination={0}", Declination);
                throw new ASCOM.InvalidValueException("SlewToCoordinatesAsync", "Declination", "-90 to 90");
            }

            targetRightAscension = RightAscension;
            targetDeclination = Declination;

            Starbook.Response response = starbook.Goto(rightAscension, declination);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SlewToCoordinatesAsync", "InvalidOperationException: Starbook.Goto({0},{1})={2}", rightAscension, declination, response);
                throw new ASCOM.InvalidOperationException("SlewToCoordinatesAsync: Starbook.Goto() is not working.");
            }

            LogMessage("SlewToCoordinatesAsync", "OK: RightAscension={0},Declination={1}", RightAscension, Declination);
        }

        public void SlewToTarget()
        {
            if (this.parking)
            {
                LogMessage("SlewToTarget", "InvalidOperationException: AtPark");
                throw new ASCOM.InvalidOperationException("SlewToTarget: AtPark, Starbook.Unpark() must be called first.");
            }

            if (!Starbook.HMS.FromValue(targetRightAscension, out Starbook.HMS rightAscension))
            {
                LogMessage("SlewToTarget", "InvalidValueException: TargetRightAscension={0}", targetRightAscension);
                throw new ASCOM.InvalidValueException("SlewToTarget", "TargetRightAscension",  "0 to 24");
            }

            if (!Starbook.DMS.FromValue(targetDeclination, out Starbook.DMS declination))
            {
                LogMessage("SlewToTarget", "InvalidValueException: TargetDeclination={0}", targetDeclination);
                throw new ASCOM.InvalidValueException("SlewToTarget", "TargetDeclination", "-90 to 90");
            }

            Starbook.Response response = starbook.Goto(rightAscension, declination);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SlewToTarget", "InvalidOperationException: Starbook.Goto({0},{1})={2}", rightAscension, declination, response);
                throw new ASCOM.InvalidOperationException("SlewToTarget: Starbook.Goto() is not working.");
            }

            if (slewSettleTime > 0)
            {
                Thread.Sleep(slewSettleTime * 1000);
            }

            while (true)
            {
                response = starbook.GetStatus(out Starbook.Status status);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("SlewToTarget", "InvalidOperationException: Starbook.GetStatus()={0}", response);
                    throw new ASCOM.InvalidOperationException("SlewToTarget: Starbook.GetStatus() is not working.");
                }

                if (!status.Goto)
                {
                    break;
                }

                Thread.Sleep(100);
            }

            LogMessage("SlewToTarget", "OK: TargetRightAscension={0},TargetDeclination={1}", targetRightAscension, targetDeclination);
        }

        public void SlewToTargetAsync()
        {
            if (this.parking)
            {
                LogMessage("SlewToTargetAsync", "InvalidOperationException: AtPark");
                throw new ASCOM.InvalidOperationException("SlewToTargetAsync: AtPark, Starbook.Unpark() must be called first.");
            }

            if (!Starbook.HMS.FromValue(targetRightAscension, out Starbook.HMS rightAscension))
            {
                LogMessage("SlewToTargetAsync", "InvalidValueException: TargetRightAscension={0}", targetRightAscension);
                throw new ASCOM.InvalidValueException("SlewToTargetAsync", "TargetRightAscension", "0 to 24");
            }

            if (!Starbook.DMS.FromValue(targetDeclination, out Starbook.DMS declination))
            {
                LogMessage("SlewToTargetAsync", "InvalidValueException: TargetDeclination={0}", targetDeclination);
                throw new ASCOM.InvalidValueException("SlewToTargetAsync", "TargetDeclination", "-90 to 90");
            }

            Starbook.Response response = starbook.Goto(rightAscension, declination);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SlewToTargetAsync", "InvalidOperationException: Starbook.Goto({0},{1})={2}", rightAscension, declination, response);
                throw new ASCOM.InvalidOperationException("SlewToTargetAsync: Starbook.Goto() is not working.");
            }

            LogMessage("SlewToTarget", "OK: TargetRightAscension={0},TargetDeclination={1}", targetRightAscension, targetDeclination);
        }

        public bool Slewing
        {
            get
            {
                bool slewing = this.movingAxis;

                if (!slewing)
                {
                    Starbook.Response response = starbook.GetStatus(out Telescope.Starbook.Status status);

                    if (response != Starbook.Response.OK)
                    {
                        LogMessage("Slewing_get", "InvalidOperationException: Starbook.GetStatus()={0}", response);
                        throw new ASCOM.InvalidOperationException("Slewing_get: Starbook.GetStatus() is not working.");
                    }

                    slewing = status.Goto;
                }

                LogMessage("Slewing_get", slewing.ToString());
                return slewing;
            }
        }

        public void SyncToAltAz(double Azimuth, double Altitude)
        {
            LogMessage("SyncToAltAz", "MethodNotImplementedException");
            throw new ASCOM.MethodNotImplementedException("SyncToAltAz");
        }

        public void SyncToCoordinates(double RightAscension, double Declination)
        {
            if (this.parking)
            {
                LogMessage("SyncToCoordinates", "InvalidOperationException: AtPark");
                throw new ASCOM.InvalidOperationException("SyncToCoordinates: AtPark, Starbook.Unpark() must be called first.");
            }

            if (!Starbook.HMS.FromValue(RightAscension, out Starbook.HMS rightAscension))
            {
                LogMessage("SyncToCoordinates", "InvalidValueException: RightAscension={0}", RightAscension);
                throw new ASCOM.InvalidValueException("SyncToCoordinates", "RightAscension", "0 to 24");
            }

            if (!Starbook.DMS.FromValue(Declination, out Starbook.DMS declination))
            {
                LogMessage("SyncToCoordinates", "InvalidValueException: Declination={0}", Declination);
                throw new ASCOM.InvalidValueException("SyncToCoordinate", "Declination", "-90 to 90");
            }

            targetRightAscension = RightAscension;
            targetDeclination = Declination;

            Starbook.Response response = starbook.Align(rightAscension, declination);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SyncToCoordinates", "InvalidOperationException: Starbook.Align({0},{1})={2}", rightAscension, declination, response);
                throw new ASCOM.InvalidOperationException("SyncToCoordinates: Starbook.Align() is not working.");
            }

            LogMessage("SyncToCoordinates", "OK: RightAscension={0},Declination={1}", RightAscension, Declination);
        }

        public void SyncToTarget()
        {
            if (this.parking)
            {
                LogMessage("SyncToTarget", "InvalidOperationException: AtPark");
                throw new ASCOM.InvalidOperationException("SyncToTarget: AtPark, Starbook.Unpark() must be called first.");
            }

            if (!Starbook.HMS.FromValue(targetRightAscension, out Starbook.HMS rightAscension))
            {
                LogMessage("SyncToTarget", "InvalidValueException: TargetRightAscension={0}", targetRightAscension);
                throw new ASCOM.InvalidValueException("SyncToTarget", "TargetRightAscension", "0 to 24");
            }

            if (!Starbook.DMS.FromValue(targetDeclination, out Starbook.DMS declination))
            {
                LogMessage("SyncToTarget", "InvalidValueException: TargetDeclination={0}", targetDeclination);
                throw new ASCOM.InvalidValueException("SyncToTarget", "TargetDeclination", "-90 to 90");
            }

            Starbook.Response response = starbook.Align(rightAscension, declination);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SyncToTarget", "InvalidOperationException: Starbook.Align({0},{1})={2}", rightAscension, declination, response);
                throw new ASCOM.InvalidOperationException("SyncToTarget: Starbook.Align() is not working.");
            }

            LogMessage("SyncToTarget", "OK: TargetRightAscension={0},TargetDeclination={1}", targetRightAscension, targetDeclination);
        }

        public double TargetDeclination
        {
            get
            {
                if (double.IsNaN(targetDeclination))
                {
                    LogMessage("SyncToTarget", "InvalidOperationException: TargetDeclination=NaN");
                    throw new ASCOM.InvalidOperationException("TargetDeclination_get: TargetDeclination is not set yet.");
                }

                LogMessage("TargetDeclination_get", targetDeclination.ToString());
                return targetDeclination;
            }
            set
            {
                if (value < -90 || 90 < value)
                {
                    LogMessage("TargetDeclination_set", "InvalidValueException: {0}", value);
                    throw new ASCOM.InvalidValueException("TargetDeclination_set", "TargetDeclination", "-90 to 90");
                }

                targetDeclination = value;
                LogMessage("TargetDeclination_set", "OK: {0}", value);
            }
        }

        public double TargetRightAscension
        {
            get
            {
                if (double.IsNaN(targetRightAscension))
                {
                    LogMessage("SyncToTarget", "InvalidOperationException: TargetRightAscension=NaN");
                    throw new ASCOM.InvalidOperationException("TargetRightAscension_get: TargetRightAscension is not set yet.");
                }

                LogMessage("TargetRightAscension_get", targetRightAscension.ToString());
                return targetRightAscension;
            }
            set
            {
                if (value < 0 || 24 < value)
                {
                    LogMessage("TargetRightAscension_set", "InvalidValueException: {0}", value);
                    throw new ASCOM.InvalidValueException("TargetRightAscension_set", "TargetRightAscension", "0 to 24");
                }

                targetRightAscension = value;
                LogMessage("TargetRightAscension_set", "OK: {0}", value);
            }
        }

        public bool Tracking
        {
            get
            {
                Starbook.Response response = starbook.GetStatus(out Starbook.Status status);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("Tracking_get", "InvalidOperationException: Starbook.GetStatus()={0}", response);
                    throw new ASCOM.InvalidOperationException("Tracking_get: Starbook.GetStatus() is not working.");
                }

                bool tracking = (status.State == Starbook.State.Guide || status.State == Starbook.State.Scope || status.State == Starbook.State.Chart || status.State == Starbook.State.User);
                LogMessage("Tracking_get", "{0}: Status.State={1}", tracking, status.State);
                return tracking;
            }
            set
            {
                if (value)
                {
                    Starbook.Response response = starbook.Start();

                    if (response != Starbook.Response.OK)
                    {
                        LogMessage("Tracking_set", "InvalidOperationException: Starbook.Start()={0}", response);
                        throw new ASCOM.InvalidOperationException("Tracking_set: Starbook.Start() is not working.");
                    }

                    LogMessage("Tracking_set", "OK: {0}", value);
                }
                else
                {
                    LogMessage("Tracking_set", "FAIL: {0}, Cannot stop tracking.", value);
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
                if (value != DriveRates.driveSidereal)
                {
                    LogMessage("TrackingRate", "InvalidValueException: {0}", value);
                    throw new ASCOM.InvalidValueException("TrackingRate", "TrackingRate", "Sidereal");
                }

                LogMessage("TrackingRate_set", "OK: {0}", value);
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

                if (response != Starbook.Response.OK)
                {
                    LogMessage("UTCDate_get", "InvalidOperationException: Starbook.GetTime()={0}", response);
                    throw new ASCOM.InvalidOperationException("UTCDate_get: Starbook.GetTime() is not working.");
                }

                if (!placeCached)
                {
                    response = starbook.GetPlace(out placeCache);

                    if (response != Starbook.Response.OK)
                    {
                        LogMessage("UTCDate_get", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                        throw new ASCOM.InvalidOperationException("UTCDate_get: Starbook.GetPlace() is not working.");
                    }

                    placeCached = true;
                }

                DateTime utcDate = time.AddHours(-placeCache.Timezone);
                LogMessage("UTCDate_get", "{0:yyyy/MM/dd HH:mm:ss}", utcDate);
                return utcDate;
            }
            set
            {
                Starbook.Response response;

                if (!placeCached)
                {
                    response = starbook.GetPlace(out placeCache);

                    if (response != Starbook.Response.OK)
                    {
                        LogMessage("UTCDate_set", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                        throw new ASCOM.InvalidOperationException("UTCDate_set: Starbook.GetPlace() is not working.");
                    }

                    placeCached = true;
                }

                DateTime time = value.AddHours(placeCache.Timezone);
                response = starbook.SetTime(time);

                if (response == Starbook.Response.OK)
                {
                    LogMessage("UTCDate_set", "OK: {0:yyyy/MM/dd HH:mm:ss}", value);
                }
                else
                {
                    LogMessage("UTCDate_set", "FAIL: {0:yyyy/MM/dd HH:mm:ss}, Starbook.SetTime({1:yyyy+MM+dd+HH+mm+ss})={1}", value, time);
                }
            }
        }

        public void Unpark()
        {
            if (parking)
            {
                LogMessage("Unpark", "OK"); parking = false;
            }
            else
            {
                LogMessage("Unpark", "OK: Skipped");
            }
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
