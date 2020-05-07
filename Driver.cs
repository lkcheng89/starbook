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
        internal static IPAddress ipAddressDefault = new IPAddress(new byte[] { 169, 254, 1, 1 });
        internal static string slewSettleTimeProfileName = "SlewSettleTime";
        internal static short slewSettleTimeDefault = 0;
        internal static string guideRateProfileName = "GuideRate";
        internal static int guideRateDefault = 0;
        internal static string guideRatesProfileName = "GuideRates";
        internal static double[] guideRatesDefault = new double[] { 0.75, 0.75, 18, 37, 75, 150, 300, 500, 1000 };
        internal static string j2000ProfileName = "J2000";
        internal static bool j2000Default = true;
        internal static string autoMeridianFlipProfileName = "AutoMeridianFlip";
        internal static int autoMeridianFlipDefault = 0;
        internal static string traceLoggerProfileName = "TraceLogger";
        internal static bool traceLoggerDefault = false;

        internal static Starbook starbook = new Starbook(ipAddressDefault); // Variables to hold the currrent device configuration
        internal static short slewSettleTime;
        internal static int guideRate;
        internal static double[] guideRates;
        internal static bool j2000;
        internal static int autoMeridianFlip;

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

        private double targetDeclination;
        private double targetRightAscension;

        private bool threadRunning;
        private Thread thread;
        private int poll;

        private object status;
        private object response;
        private Queue<Request> requests;
        private bool atHome;
        private bool movingAxis;
        private bool pulseGuiding;
        private bool tracking;

        private int round;

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

            targetRightAscension = double.NaN;
            targetDeclination = double.NaN;

            threadRunning = false;
            thread = null;
            poll = 100;

            status = null;
            response = null;
            requests = new Queue<Request>();
            atHome = true;
            movingAxis = false;
            pulseGuiding = false;
            tracking = true;

            round = 0;

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

            bool connected;

            lock (this)
            {
                connected = this.connectedState;
            }

            if (connected)
            {
                System.Windows.Forms.MessageBox.Show("Already connected, just press OK");
            }

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
                bool connected = IsConnected;
                LogMessage("Connected_get", "{0}", connected);
                return connected;
            }
            set
            {
                LogMessage("Connected_set", "{0}", value);

                lock (this)
                {
                    connectedState = value; status = null; response = null;
                }

                if (thread != null)
                {
                    threadRunning = false;

                    try
                    {
                        thread.Join();
                    }
                    catch
                    {

                    }
                }

                if (value)
                {
                    threadRunning = true;
                    thread = new Thread(ThreadEntry);
                    thread.Start();
                }
                else
                {
                    thread = null;
                }

                placeCached = false;
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
            CheckConnected("AbortSlew");

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
                LogMessage("AlignmentMode_get", "{0}", mode);
                return mode;
            }
        }

        public double Altitude
        {
            get
            {
                CheckConnected("Altitude_get");

                Starbook.Response response = GetStatus(out Starbook.Status status);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("Altitude_get", "InvalidOperationException: Starbook.GetStatus()={0}", response);
                    throw new ASCOM.InvalidOperationException("Altitude_get: Starbook.GetStatus() is not working.");
                }

                response = GetPlace(out Starbook.Place place);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("Altitude_get", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                    throw new ASCOM.InvalidOperationException("Altitude_get: Starbook.GetPlace() is not working.");
                }

                response = starbook.GetTime(out DateTime time);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("Altitude_get", "InvalidOperationException: Starbook.GetTime()={0}", response);
                    throw new ASCOM.InvalidOperationException("Altitude_get: Starbook.GetTime() is not working.");
                }

                transform.SiteLatitude = place.Latitude.Value;
                transform.SiteLongitude = place.Longitude.Value;
                transform.JulianDateUTC = utilities.DateUTCToJulian(time.AddHours(-place.Timezone));

                if (starbook.J2000)
                {
                    transform.SetJ2000(status.RA.Value, status.Dec.Value);
                }
                else
                {
                    transform.SetTopocentric(status.RA.Value, status.Dec.Value);
                }

                double altitude = transform.ElevationTopocentric;
                LogMessage("Altitude_get", "{0}", altitude);
                return altitude;
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
                CheckConnected("AtHome");

                bool atHome;

                lock (this)
                {
                    atHome = this.atHome;
                }

                LogMessage("AtHome_get", "{0}", atHome);
                return atHome;
            }
        }

        public bool AtPark
        {
            get
            {
                CheckConnected("AtPark_get");

                bool atPark = parking;
                LogMessage("AtPark_get", "{0}", atPark);
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
                CheckConnected("Azimuth_get");

                Starbook.Response response = GetStatus(out Starbook.Status status);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("Azimuth_get", "InvalidOperationException: Starbook.GetStatus()={0}", response);
                    throw new ASCOM.InvalidOperationException("Azimuth_get: Starbook.GetStatus() is not working.");
                }

                response = GetPlace(out Starbook.Place place);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("Azimuth_get", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                    throw new ASCOM.InvalidOperationException("Azimuth_get: Starbook.GetPlace() is not working.");
                }

                response = starbook.GetTime(out DateTime time);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("Azimuth_get", "InvalidOperationException: Starbook.GetTime()={0}", response);
                    throw new ASCOM.InvalidOperationException("Azimuth_get: Starbook.GetTime() is not working.");
                }

                transform.SiteLatitude = place.Latitude.Value;
                transform.SiteLongitude = place.Longitude.Value;
                transform.JulianDateUTC = utilities.DateUTCToJulian(time.AddHours(-place.Timezone));

                if (starbook.J2000)
                {
                    transform.SetJ2000(status.RA.Value, status.Dec.Value);
                }
                else
                {
                    transform.SetTopocentric(status.RA.Value, status.Dec.Value);
                }

                double azimuth = transform.AzimuthTopocentric;
                LogMessage("Azimuth_get", "{0}", azimuth);
                return azimuth;
            }
        }

        public bool CanFindHome
        {
            get
            {
                LogMessage("CanFindHome_get", "{0}", true);
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
                LogMessage("CanPark_get", "{0}", true);
                return true;
            }
        }

        public bool CanPulseGuide
        {
            get
            {
                LogMessage("CanPulseGuide_get", "{0}", true);
                return true;
            }
        }

        public bool CanSetDeclinationRate
        {
            get
            {
                LogMessage("CanSetDeclinationRate_get", "{0}", false);
                return false;
            }
        }

        public bool CanSetGuideRates
        {
            get
            {
                LogMessage("CanSetGuideRates_get", "{0}", true);
                return true;
            }
        }

        public bool CanSetPark
        {
            get
            {
                LogMessage("CanSetPark_get", "{0}", true);
                return true;
            }
        }

        public bool CanSetPierSide
        {
            get
            {
                LogMessage("CanSetPierSide_get", "{0}", false);
                return false;
            }
        }

        public bool CanSetRightAscensionRate
        {
            get
            {
                LogMessage("CanSetRightAscensionRate_get", "{0}", false);
                return false;
            }
        }

        public bool CanSetTracking
        {
            get
            {
                LogMessage("CanSetTracking_get", "{0}", true);
                return true;
            }
        }

        public bool CanSlew
        {
            get
            {
                LogMessage("CanSlew_get", "{0}", true);
                return true;
            }
        }

        public bool CanSlewAltAz
        {
            get
            {
                LogMessage("CanSlewAltAz_get", "{0}", true);
                return true;
            }
        }

        public bool CanSlewAltAzAsync
        {
            get
            {
                LogMessage("CanSlewAltAzAsync_get", "{0}", true);
                return true;
            }
        }

        public bool CanSlewAsync
        {
            get
            {
                LogMessage("CanSlewAsync_get", "{0}", true);
                return true;
            }
        }

        public bool CanSync
        {
            get
            {
                LogMessage("CanSync_get", "{0}", true);
                return true;
            }
        }

        public bool CanSyncAltAz
        {
            get
            {
                LogMessage("CanSyncAltAz_get", "{0}", true);
                return true;
            }
        }

        public bool CanUnpark
        {
            get
            {
                LogMessage("CanUnpark_get", "{0}", true);
                return true;
            }
        }

        public double Declination
        {
            get
            {
                CheckConnected("Declination_get");

                Starbook.Response response = GetStatus(out Starbook.Status status);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("Declination_get", "InvalidOperationException: Starbook.GetStatus()={0}", response);
                    throw new ASCOM.InvalidOperationException("Declination_get: Starbook.GetStatus() is not working.");
                }

                double declination;

                if (j2000 == starbook.J2000)
                {
                    declination = status.Dec.Value;
                    LogMessage("Declination_get", "{0}: Status.Dec={1}", declination, status.Dec);
                }
                else
                {
                    response = GetPlace(out Starbook.Place place);

                    if (response != Starbook.Response.OK)
                    {
                        LogMessage("Declination_get", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                        throw new ASCOM.InvalidOperationException("Declination_get: Starbook.GetPlace() is not working.");
                    }

                    response = starbook.GetTime(out DateTime time);

                    if (response != Starbook.Response.OK)
                    {
                        LogMessage("Declination_get", "InvalidOperationException: Starbook.GetTime()={0}", response);
                        throw new ASCOM.InvalidOperationException("Declination_get: Starbook.GetTime() is not working.");
                    }

                    transform.SiteLatitude = place.Latitude.Value;
                    transform.SiteLongitude = place.Longitude.Value;
                    transform.JulianDateUTC = utilities.DateUTCToJulian(time.AddHours(-place.Timezone));

                    double statusDecValue = status.Dec.Value;

                    if (starbook.J2000)
                    {
                        transform.SetJ2000(status.RA.Value, statusDecValue);
                        declination = transform.DECTopocentric;
                    }
                    else
                    {
                        transform.SetTopocentric(status.RA.Value, statusDecValue);
                        declination = transform.DecJ2000;
                    }

                    LogMessage("Declination_get", "{0}=>{1}: Status.Dec={2}", statusDecValue, declination, status.Dec);
                }

                return declination;
            }
        }

        public double DeclinationRate
        {
            get
            {
                double declination = 0.0;
                LogMessage("DeclinationRate_get", "{0}", declination);
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
                CheckConnected("EquatorialSystem_get");

                EquatorialCoordinateType equatorialSystem = j2000 ? EquatorialCoordinateType.equJ2000 : EquatorialCoordinateType.equTopocentric;
                LogMessage("EquatorialSystem_get", "{0}", equatorialSystem);
                return equatorialSystem;
            }
        }

        public void FindHome()
        {
            CheckConnected("FindHome");

            if (this.parking)
            {
                LogMessage("FindHome", "InvalidOperationException: AtPark");
                throw new ASCOM.InvalidOperationException("FindHome: AtPark, Starbook.Unpark() must be called first.");
            }

            Starbook.Response response = Home();

            if (response != Starbook.Response.OK)
            {
                LogMessage("FindHome", "InvalidOperationException: Starbook.Home()={0}", response);
                throw new ASCOM.InvalidOperationException("FindHome: Starbook.Home() is not working.");
            }

            while (true)
            {
                response = GetStatus(out Starbook.Status status);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("FindHome", "InvalidOperationException: Starbook.GetStatus()={0}", response);
                    throw new ASCOM.InvalidOperationException("FindHome: Starbook.GetStatus() is not working.");
                }

                if (!status.Goto)
                {
                    break;
                }

                Thread.Sleep(100);
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
            return guideRates[guideRate] * (15.04106858 / 3600);
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
                CheckConnected("GuideRateDeclination_set");

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
                CheckConnected("GuideRateRightAscension_set");

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
                CheckConnected("IsPulseGuiding_get");

                bool isPulseGuiding;

                lock (this)
                {
                    isPulseGuiding = pulseGuiding;
                }

                LogMessage("IsPulseGuiding_get", "{0}", isPulseGuiding);
                return isPulseGuiding;
            }
        }

        public void MoveAxis(TelescopeAxes Axis, double Rate)
        {
            CheckConnected("MoveAxis");

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

                        lock (this)
                        {
                            movingAxis = false;
                        }

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
                            throw new ASCOM.InvalidValueException("MoveAxis", "Rate", string.Format(CultureInfo.InvariantCulture, "{0} to {1} or {2} to {3}", minRate, maxRate, -maxRate, -minRate));
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

                        lock (this)
                        {
                            movingAxis = true; atHome = false;
                        }
                    }

                    LogMessage("MoveAxis", "OK: Axis={0},Rate={1}", Axis, Rate);

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
            CheckConnected("Park");
            
            if (parking)
            {
                LogMessage("Park", "OK: Skipped");
            }
            else
            {
                Starbook.Response response = GetPlace(out Starbook.Place place);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("Park", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                    throw new ASCOM.InvalidOperationException("Park: Starbook.GetPlace() is not working.");
                }

                response = starbook.GetTime(out DateTime time);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("Park", "InvalidOperationException: Starbook.GetTime()={0}", response);
                    throw new ASCOM.InvalidOperationException("SetPark: Starbook.GetTime() is not working.");
                }

                transform.SiteLatitude = place.Latitude.Value;
                transform.SiteLongitude = place.Longitude.Value;
                transform.JulianDateUTC = utilities.DateUTCToJulian(time.AddHours(-place.Timezone));
                transform.SetAzimuthElevation(parkAzimuth, parkElevation);

                double parkRightAscension, parkDeclination;

                if (starbook.J2000)
                {
                    parkRightAscension = transform.RAJ2000;
                    parkDeclination = transform.DecJ2000;
                }
                else
                {
                    parkRightAscension = transform.RATopocentric;
                    parkDeclination = transform.DECTopocentric;
                }

                if (!Starbook.HMS.FromValue(parkRightAscension, out Starbook.HMS rightAscension))
                {
                    LogMessage("Park", "InvalidOperationException: Cannot determine rightAscension={0}", parkRightAscension);
                    throw new ASCOM.InvalidOperationException("Park: Cannot determine rightAscension.");
                }

                if (!Starbook.DMS.FromValue(parkDeclination, out Starbook.DMS declination))
                {
                    LogMessage("Park", "InvalidOperationException: Cannot determine declination={0}", parkDeclination);
                    throw new ASCOM.InvalidOperationException("Park: Cannot determine declination.");
                }

                response = Goto(rightAscension, declination);

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
            CheckConnected("PulseGuide");

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

            lock (this)
            {
                pulseGuiding = true; atHome = false;
            }

            if (Duration > 0)
            {
                Thread.Sleep(Duration);
            }

            response = starbook.NoMove();

            lock (this)
            {
                pulseGuiding = false;
            }

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
                CheckConnected("RightAscension_get");

                Starbook.Response response = GetStatus(out Starbook.Status status);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("RightAscension_get", "InvalidOperationException: Starbook.GetStatus()={0}", response);
                    throw new ASCOM.InvalidOperationException("RightAscension_get: Starbook.GetStatus() is not working.");
                }

                double rightAscension;

                if (j2000 == starbook.J2000)
                {
                    rightAscension = status.RA.Value;
                    LogMessage("RightAscension_get", "{0}: Status.RA={1}", rightAscension, status.RA);
                }
                else
                {
                    response = GetPlace(out Starbook.Place place);

                    if (response != Starbook.Response.OK)
                    {
                        LogMessage("RightAscension_get", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                        throw new ASCOM.InvalidOperationException("RightAscension_get: Starbook.GetPlace() is not working.");
                    }

                    response = starbook.GetTime(out DateTime time);

                    if (response != Starbook.Response.OK)
                    {
                        LogMessage("RightAscension_get", "InvalidOperationException: Starbook.GetTime()={0}", response);
                        throw new ASCOM.InvalidOperationException("RightAscension_get: Starbook.GetTime() is not working.");
                    }

                    transform.SiteLatitude = place.Latitude.Value;
                    transform.SiteLongitude = place.Longitude.Value;
                    transform.JulianDateUTC = utilities.DateUTCToJulian(time.AddHours(-place.Timezone));

                    double statusRAValue = status.RA.Value;

                    if (starbook.J2000)
                    {
                        transform.SetJ2000(statusRAValue, status.Dec.Value);
                        rightAscension = transform.RATopocentric;
                    }
                    else
                    {
                        transform.SetTopocentric(statusRAValue, status.Dec.Value);
                        rightAscension = transform.RAJ2000;
                    }

                    LogMessage("RightAscension_get", "{0}=>{1}: Status.RA={2}", statusRAValue, rightAscension, status.RA);
                }

                return rightAscension;
            }
        }

        public double RightAscensionRate
        {
            get
            {
                double rightAscensionRate = 0.0;
                LogMessage("RightAscensionRate_get", "{0}", rightAscensionRate);
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
            CheckConnected("SetPark");

            Starbook.Response response = GetStatus(out Starbook.Status status);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SetPark", "InvalidOperationException: Starbook.GetStatus()={0}", response);
                throw new ASCOM.InvalidOperationException("SetPark: Starbook.GetStatus() is not working.");
            }

            response = GetPlace(out Starbook.Place place);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SetPark", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                throw new ASCOM.InvalidOperationException("SetPark: Starbook.GetPlace() is not working.");
            }

            response = starbook.GetTime(out DateTime time);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SetPark", "InvalidOperationException: Starbook.GetTime()={0}", response);
                throw new ASCOM.InvalidOperationException("SetPark: Starbook.GetTime() is not working.");
            }

            transform.SiteLatitude = place.Latitude.Value;
            transform.SiteLongitude = place.Longitude.Value;
            transform.JulianDateUTC = utilities.DateUTCToJulian(time.AddHours(-place.Timezone));

            if (starbook.J2000)
            {
                transform.SetJ2000(status.RA.Value, status.Dec.Value);
            }
            else
            {
                transform.SetTopocentric(status.RA.Value, status.Dec.Value);
            }

            parkAzimuth = transform.AzimuthTopocentric;
            parkElevation = transform.ElevationTopocentric;

            LogMessage("SetPark", "OK: Altitude={0},Azimuth={1},RightAscension={2},Declination={3}", parkElevation, parkAzimuth, status.RA, status.Dec);
        }

        public PierSide SideOfPier
        {
            get
            {
                Starbook.Response response;

                if (round == 0)
                {
                    response = starbook.GetRound(out int rovnd);

                    if (response != Starbook.Response.OK)
                    {
                        LogMessage("SideOfPier", "InvalidOperationException: Starbook.GetRound()={0}", response);
                        throw new ASCOM.InvalidOperationException("SideOfPier: Starbook.GetRound() is not working.");
                    }

                    round = rovnd;
                }

                response = starbook.GetXY(out Starbook.XY xy);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("SideOfPier", "InvalidOperationException: Starbook.GetXY()={0}", response);
                    throw new ASCOM.InvalidOperationException("SideOfPier: Starbook.GetXY() is not working.");
                }

                double mechanicalDec = -360.0 * xy.Y / round;

                PierSide sideOfPier = Math.Abs(mechanicalDec) <= 90 ? PierSide.pierEast : PierSide.pierWest;
                LogMessage("SideOfPier_get", "{0}: MechanicalDec={1}", sideOfPier, mechanicalDec);
                return sideOfPier;
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

                LogMessage("SiderealTime_get", "{0}", siderealTime);
                return siderealTime;
            }
        }

        public double SiteElevation
        {
            get
            {
                double siteElevation = 0;
                LogMessage("SiteElevation_get", "{0}", siteElevation);
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
                CheckConnected("SiteLatitude_get");

                Starbook.Response response = GetPlace(out Starbook.Place place);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("SiteLatitude_get", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                    throw new ASCOM.InvalidOperationException("SiteLatitude_get: Starbook.GetPlace() is not working.");
                }

                double latitude = place.Latitude.Value;
                LogMessage("SiteLatitude_get", "{0}: Place.Latitude={1}", latitude, place.Latitude);
                return latitude;
            }
            set
            {
                CheckConnected("SiteLatitude_set");

                if (!Starbook.DMS.FromValue(value, out Starbook.DMS latitude, Starbook.Direction.North, Starbook.Direction.South))
                {
                    LogMessage("SiteLatitude_set", "InvalidValueException: {0}", value);
                    throw new ASCOM.InvalidValueException("SiteLatitude", "SiteLatitude", "-90 to 90");
                }

                Starbook.Response response = GetPlace(out Starbook.Place place);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("SiteLatitude_set", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                    throw new ASCOM.InvalidOperationException("SiteLatitude_set: Starbook.GetPlace() is not working.");
                }

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
                CheckConnected("SiteLongitude_get");

                Starbook.Response response = GetPlace(out Starbook.Place place);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("SiteLongitude_get", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                    throw new ASCOM.InvalidOperationException("SiteLongitude_get: Starbook.GetPlace() is not working.");
                }

                double longitude = place.Longitude.Value;
                LogMessage("SiteLongitude_get", "{0}: Place.Longitude={1}", longitude, place.Longitude);
                return longitude;
            }
            set
            {
                CheckConnected("SiteLongitude_set");

                if (!Starbook.DMS.FromValue(value, out Starbook.DMS longitude, Starbook.Direction.East, Starbook.Direction.West))
                {
                    LogMessage("SiteLongitude_set", "InvalidValueException: {0}", value);
                    throw new ASCOM.InvalidValueException("SiteLongitude", "SiteLongitude", "-180 to 180");
                }

                Starbook.Response response = GetPlace(out Starbook.Place place);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("SiteLongitude_set", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                    throw new ASCOM.InvalidOperationException("SiteLongitude_set: Starbook.GetPlace() is not working.");
                }

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
                LogMessage("SlewSettleTime_get", "{0}", slewSettleTime);
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
            CheckConnected("SlewToAltAz");

            if (this.parking)
            {
                LogMessage("SlewToAltAz", "InvalidOperationException: AtPark");
                throw new ASCOM.InvalidOperationException("SlewToAltAz: AtPark, Starbook.Unpark() must be called first.");
            }

            if (Azimuth < 0 || 360 < Azimuth)
            {
                LogMessage("SlewToAltAz", "InvalidValueException: Azimuth={0}", Azimuth);
                throw new ASCOM.InvalidValueException("SlewToAltAz", "Azimuth", "0 to 360");
            }

            if (Altitude < 0 || 90 < Altitude)
            {
                LogMessage("SlewToAltAz", "InvalidValueException: Altitude={0}", Altitude);
                throw new ASCOM.InvalidValueException("SlewToAltAz", "Altitude", "0 to 90");
            }

            Starbook.Response response = GetPlace(out Starbook.Place place);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SlewToAltAz", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                throw new ASCOM.InvalidOperationException("SlewToAltAz: Starbook.GetPlace() is not working.");
            }

            response = starbook.GetTime(out DateTime time);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SlewToAltAz", "InvalidOperationException: Starbook.GetTime()={0}", response);
                throw new ASCOM.InvalidOperationException("SlewToAltAz: Starbook.GetTime() is not working.");
            }

            transform.SiteLatitude = place.Latitude.Value;
            transform.SiteLongitude = place.Longitude.Value;
            transform.JulianDateUTC = utilities.DateUTCToJulian(time.AddHours(-place.Timezone));
            transform.SetAzimuthElevation(Azimuth, Altitude);

            double RightAscension, Declination;

            if (starbook.J2000)
            {
                RightAscension = transform.RAJ2000;
                Declination = transform.DecJ2000;
            }
            else
            {
                RightAscension = transform.RATopocentric;
                Declination = transform.DECTopocentric;
            }

            if (!Starbook.HMS.FromValue(RightAscension, out Starbook.HMS rightAscension))
            {
                LogMessage("SlewToAltAz", "InvalidValueException: RightAscension={0}", RightAscension);
                throw new ASCOM.InvalidValueException("SlewToAltAz", "RightAscension", "0 to 24");
            }

            if (!Starbook.DMS.FromValue(Declination, out Starbook.DMS declination))
            {
                LogMessage("SlewToAltAz", "InvalidValueException: Declination={0}", Declination);
                throw new ASCOM.InvalidValueException("SlewToAltAz", "Declination", "-90 to 90");
            }

            if (j2000)
            {
                targetRightAscension = transform.RAJ2000;
                targetDeclination = transform.DecJ2000;
            }
            else
            {
                targetRightAscension = transform.RATopocentric;
                targetDeclination = transform.DECTopocentric;
            }

            response = Goto(rightAscension, declination);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SlewToAltAz", "InvalidOperationException: Starbook.Goto({0},{1})={2}", rightAscension, declination, response);
                throw new ASCOM.InvalidOperationException("SlewToAltAz: Starbook.Goto() is not working.");
            }

            while (true)
            {
                response = GetStatus(out Starbook.Status status);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("SlewToAltAz", "InvalidOperationException: Starbook.GetStatus()={0}", response);
                    throw new ASCOM.InvalidOperationException("SlewToAltAz: Starbook.GetStatus() is not working.");
                }

                if (!status.Goto)
                {
                    break;
                }

                Thread.Sleep(100);
            }

            LogMessage("SlewToAltAz", "OK: Azimuth={0},Altitude={1}", Azimuth, Altitude);
        }

        public void SlewToAltAzAsync(double Azimuth, double Altitude)
        {
            CheckConnected("SlewToAltAzAsync");

            if (this.parking)
            {
                LogMessage("SlewToAltAzAsync", "InvalidOperationException: AtPark");
                throw new ASCOM.InvalidOperationException("SlewToAltAzAsync: AtPark, Starbook.Unpark() must be called first.");
            }

            if (Azimuth < 0 || 360 < Azimuth)
            {
                LogMessage("SlewToAltAzAsync", "InvalidValueException: Azimuth={0}", Azimuth);
                throw new ASCOM.InvalidValueException("SlewToAltAzAsync", "Azimuth", "0 to 360");
            }

            if (Altitude < 0 || 90 < Altitude)
            {
                LogMessage("SlewToAltAzAsync", "InvalidValueException: Altitude={0}", Altitude);
                throw new ASCOM.InvalidValueException("SlewToAltAzAsync", "Altitude", "0 to 90");
            }

            Starbook.Response response = GetPlace(out Starbook.Place place);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SlewToAltAzAsync", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                throw new ASCOM.InvalidOperationException("SlewToAltAzAsync: Starbook.GetPlace() is not working.");
            }

            response = starbook.GetTime(out DateTime time);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SlewToAltAzAsync", "InvalidOperationException: Starbook.GetTime()={0}", response);
                throw new ASCOM.InvalidOperationException("SlewToAltAzAsync: Starbook.GetTime() is not working.");
            }

            transform.SiteLatitude = place.Latitude.Value;
            transform.SiteLongitude = place.Longitude.Value;
            transform.JulianDateUTC = utilities.DateUTCToJulian(time.AddHours(-place.Timezone));
            transform.SetAzimuthElevation(Azimuth, Altitude);

            double RightAscension, Declination;

            if (starbook.J2000)
            {
                RightAscension = transform.RAJ2000;
                Declination = transform.DecJ2000;
            }
            else
            {
                RightAscension = transform.RATopocentric;
                Declination = transform.DECTopocentric;
            }

            if (!Starbook.HMS.FromValue(RightAscension, out Starbook.HMS rightAscension))
            {
                LogMessage("SlewToAltAzAsync", "InvalidValueException: RightAscension={0}", RightAscension);
                throw new ASCOM.InvalidValueException("SlewToAltAzAsync", "RightAscension", "0 to 24");
            }

            if (!Starbook.DMS.FromValue(Declination, out Starbook.DMS declination))
            {
                LogMessage("SlewToAltAzAsync", "InvalidValueException: Declination={0}", Declination);
                throw new ASCOM.InvalidValueException("SlewToAltAzAsync", "Declination", "-90 to 90");
            }

            if (j2000)
            {
                targetRightAscension = transform.RAJ2000;
                targetDeclination = transform.DecJ2000;
            }
            else
            {
                targetRightAscension = transform.RATopocentric;
                targetDeclination = transform.DECTopocentric;
            }

            response = Goto(rightAscension, declination);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SlewToAltAzAsync", "InvalidOperationException: Starbook.Goto({0},{1})={2}", rightAscension, declination, response);
                throw new ASCOM.InvalidOperationException("SlewToAltAzAsync: Starbook.Goto() is not working.");
            }

            LogMessage("SlewToAltAzAsync", "OK: Azimuth={0},Altitude={1}", Azimuth, Altitude);
        }

        public void SlewToCoordinates(double RightAscension, double Declination)
        {
            CheckConnected("SlewToCoordinates");

            if (this.parking)
            {
                LogMessage("SlewToCoordinates", "InvalidOperationException: AtPark");
                throw new ASCOM.InvalidOperationException("SlewToCoordinates: AtPark, Starbook.Unpark() must be called first.");
            }

            Starbook.Response response;
            Starbook.HMS rightAscension;
            Starbook.DMS declination;

            if (j2000 == starbook.J2000)
            {
                if (!Starbook.HMS.FromValue(RightAscension, out rightAscension))
                {
                    LogMessage("SlewToCoordinates", "InvalidValueException: RightAscension={0}", RightAscension);
                    throw new ASCOM.InvalidValueException("SlewToCoordinates", "RightAscension", "0 to 24");
                }

                if (!Starbook.DMS.FromValue(Declination, out declination))
                {
                    LogMessage("SlewToCoordinates", "InvalidValueException: Declination={0}", Declination);
                    throw new ASCOM.InvalidValueException("SlewToCoordinates", "Declination", "-90 to 90");
                }
            }
            else
            {
                response = GetPlace(out Starbook.Place place);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("SlewToCoordinates", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                    throw new ASCOM.InvalidOperationException("SlewToCoordinates: Starbook.GetPlace() is not working.");
                }

                response = starbook.GetTime(out DateTime time);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("SlewToCoordinates", "InvalidOperationException: Starbook.GetTime()={0}", response);
                    throw new ASCOM.InvalidOperationException("SlewToCoordinates: Starbook.GetTime() is not working.");
                }

                transform.SiteLatitude = place.Latitude.Value;
                transform.SiteLongitude = place.Longitude.Value;
                transform.JulianDateUTC = utilities.DateUTCToJulian(time.AddHours(-place.Timezone));

                double transformRightAscension, transformDeclination;

                if (starbook.J2000)
                {
                    transform.SetTopocentric(RightAscension, Declination);
                    transformRightAscension = transform.RAJ2000;
                    transformDeclination = transform.DecJ2000;
                }
                else
                {
                    transform.SetJ2000(RightAscension, Declination);
                    transformRightAscension = transform.RATopocentric;
                    transformDeclination = transform.DECTopocentric;
                }

                if (!Starbook.HMS.FromValue(transformRightAscension, out rightAscension))
                {
                    LogMessage("SlewToCoordinates", "InvalidValueException: RightAscension={0}=>{1}", RightAscension, transformRightAscension);
                    throw new ASCOM.InvalidValueException("SlewToCoordinates", "RightAscension", "0 to 24");
                }

                if (!Starbook.DMS.FromValue(transformDeclination, out declination))
                {
                    LogMessage("SlewToCoordinates", "InvalidValueException: Declination={0}=>{1}", Declination, transformDeclination);
                    throw new ASCOM.InvalidValueException("SlewToCoordinates", "Declination", "-90 to 90");
                }
            }

            targetRightAscension = RightAscension;
            targetDeclination = Declination;

            response = Goto(rightAscension, declination);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SlewToCoordinates", "InvalidOperationException: Starbook.Goto({0},{1})={2}", rightAscension, declination, response);
                throw new ASCOM.InvalidOperationException("SlewToCoordinates: Starbook.Goto() is not working.");
            }

            while (true)
            {
                response = GetStatus(out Starbook.Status status);

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
            CheckConnected("SlewToCoordinatesAsync");

            if (this.parking)
            {
                LogMessage("SlewToCoordinatesAsync", "InvalidOperationException: AtPark");
                throw new ASCOM.InvalidOperationException("SlewToCoordinatesAsync: AtPark, Starbook.Unpark() must be called first.");
            }

            Starbook.Response response;
            Starbook.HMS rightAscension;
            Starbook.DMS declination;

            if (j2000 == starbook.J2000)
            {
                if (!Starbook.HMS.FromValue(RightAscension, out rightAscension))
                {
                    LogMessage("SlewToCoordinatesAsync", "InvalidValueException: RightAscension={0}", RightAscension);
                    throw new ASCOM.InvalidValueException("SlewToCoordinatesAsync", "RightAscension", "0 to 24");
                }

                if (!Starbook.DMS.FromValue(Declination, out declination))
                {
                    LogMessage("SlewToCoordinatesAsync", "InvalidValueException: Declination={0}", Declination);
                    throw new ASCOM.InvalidValueException("SlewToCoordinatesAsync", "Declination", "-90 to 90");
                }
            }
            else
            {
                response = GetPlace(out Starbook.Place place);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("SlewToCoordinatesAsync", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                    throw new ASCOM.InvalidOperationException("SlewToCoordinatesAsync: Starbook.GetPlace() is not working.");
                }

                response = starbook.GetTime(out DateTime time);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("SlewToCoordinatesAsync", "InvalidOperationException: Starbook.GetTime()={0}", response);
                    throw new ASCOM.InvalidOperationException("SlewToCoordinatesAsync: Starbook.GetTime() is not working.");
                }

                transform.SiteLatitude = place.Latitude.Value;
                transform.SiteLongitude = place.Longitude.Value;
                transform.JulianDateUTC = utilities.DateUTCToJulian(time.AddHours(-place.Timezone));

                double transformRightAscension, transformDeclination;

                if (starbook.J2000)
                {
                    transform.SetTopocentric(RightAscension, Declination);
                    transformRightAscension = transform.RAJ2000;
                    transformDeclination = transform.DecJ2000;
                }
                else
                {
                    transform.SetJ2000(RightAscension, Declination);
                    transformRightAscension = transform.RATopocentric;
                    transformDeclination = transform.DECTopocentric;
                }

                if (!Starbook.HMS.FromValue(transformRightAscension, out rightAscension))
                {
                    LogMessage("SlewToCoordinatesAsync", "InvalidValueException: RightAscension={0}=>{1}", RightAscension, transformRightAscension);
                    throw new ASCOM.InvalidValueException("SlewToCoordinatesAsync", "RightAscension", "0 to 24");
                }

                if (!Starbook.DMS.FromValue(transformDeclination, out declination))
                {
                    LogMessage("SlewToCoordinatesAsync", "InvalidValueException: Declination={0}=>{1}", Declination, transformDeclination);
                    throw new ASCOM.InvalidValueException("SlewToCoordinatesAsync", "Declination", "-90 to 90");
                }
            }

            targetRightAscension = RightAscension;
            targetDeclination = Declination;

            response = Goto(rightAscension, declination);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SlewToCoordinatesAsync", "InvalidOperationException: Starbook.Goto({0},{1})={2}", rightAscension, declination, response);
                throw new ASCOM.InvalidOperationException("SlewToCoordinatesAsync: Starbook.Goto() is not working.");
            }

            LogMessage("SlewToCoordinatesAsync", "OK: RightAscension={0},Declination={1}", RightAscension, Declination);
        }

        public void SlewToTarget()
        {
            CheckConnected("SlewToTarget");

            if (this.parking)
            {
                LogMessage("SlewToTarget", "InvalidOperationException: AtPark");
                throw new ASCOM.InvalidOperationException("SlewToTarget: AtPark, Starbook.Unpark() must be called first.");
            }

            Starbook.Response response;
            Starbook.HMS rightAscension;
            Starbook.DMS declination;

            if (j2000 == starbook.J2000)
            {
                if (!Starbook.HMS.FromValue(targetRightAscension, out rightAscension))
                {
                    LogMessage("SlewToTarget", "InvalidValueException: TargetRightAscension={0}", targetRightAscension);
                    throw new ASCOM.InvalidValueException("SlewToTarget", "TargetRightAscension", "0 to 24");
                }

                if (!Starbook.DMS.FromValue(targetDeclination, out declination))
                {
                    LogMessage("SlewToTarget", "InvalidValueException: TargetDeclination={0}", targetDeclination);
                    throw new ASCOM.InvalidValueException("SlewToTarget", "TargetDeclination", "-90 to 90");
                }
            }
            else
            {
                response = GetPlace(out Starbook.Place place);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("SlewToTarget", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                    throw new ASCOM.InvalidOperationException("SlewToTarget: Starbook.GetPlace() is not working.");
                }

                response = starbook.GetTime(out DateTime time);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("SlewToTarget", "InvalidOperationException: Starbook.GetTime()={0}", response);
                    throw new ASCOM.InvalidOperationException("SlewToTarget: Starbook.GetTime() is not working.");
                }

                transform.SiteLatitude = place.Latitude.Value;
                transform.SiteLongitude = place.Longitude.Value;
                transform.JulianDateUTC = utilities.DateUTCToJulian(time.AddHours(-place.Timezone));

                double transformRightAscension, transformDeclination;

                if (starbook.J2000)
                {
                    transform.SetTopocentric(targetRightAscension, targetDeclination);
                    transformRightAscension = transform.RAJ2000;
                    transformDeclination = transform.DecJ2000;
                }
                else
                {
                    transform.SetJ2000(targetRightAscension, targetDeclination);
                    transformRightAscension = transform.RATopocentric;
                    transformDeclination = transform.DECTopocentric;
                }

                if (!Starbook.HMS.FromValue(transformRightAscension, out rightAscension))
                {
                    LogMessage("SlewToTarget", "InvalidValueException: RightAscension={0}=>{1}", targetRightAscension, transformRightAscension);
                    throw new ASCOM.InvalidValueException("SlewToTarget", "RightAscension", "0 to 24");
                }

                if (!Starbook.DMS.FromValue(transformDeclination, out declination))
                {
                    LogMessage("SlewToTarget", "InvalidValueException: Declination={0}=>{1}", targetDeclination, transformDeclination);
                    throw new ASCOM.InvalidValueException("SlewToTarget", "Declination", "-90 to 90");
                }
            }

            response = Goto(rightAscension, declination);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SlewToTarget", "InvalidOperationException: Starbook.Goto({0},{1})={2}", rightAscension, declination, response);
                throw new ASCOM.InvalidOperationException("SlewToTarget: Starbook.Goto() is not working.");
            }

            while (true)
            {
                response = GetStatus(out Starbook.Status status);

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
            CheckConnected("SlewToTargetAsync");

            if (this.parking)
            {
                LogMessage("SlewToTargetAsync", "InvalidOperationException: AtPark");
                throw new ASCOM.InvalidOperationException("SlewToTargetAsync: AtPark, Starbook.Unpark() must be called first.");
            }

            Starbook.Response response;
            Starbook.HMS rightAscension;
            Starbook.DMS declination;

            if (j2000 == starbook.J2000)
            {
                if (!Starbook.HMS.FromValue(targetRightAscension, out rightAscension))
                {
                    LogMessage("SlewToTargetAsync", "InvalidValueException: TargetRightAscension={0}", targetRightAscension);
                    throw new ASCOM.InvalidValueException("SlewToTargetAsync", "TargetRightAscension", "0 to 24");
                }

                if (!Starbook.DMS.FromValue(targetDeclination, out declination))
                {
                    LogMessage("SlewToTargetAsync", "InvalidValueException: TargetDeclination={0}", targetDeclination);
                    throw new ASCOM.InvalidValueException("SlewToTargetAsync", "TargetDeclination", "-90 to 90");
                }
            }
            else
            {
                response = GetPlace(out Starbook.Place place);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("SlewToTargetAsync", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                    throw new ASCOM.InvalidOperationException("SlewToTargetAsync: Starbook.GetPlace() is not working.");
                }

                response = starbook.GetTime(out DateTime time);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("SlewToTargetAsync", "InvalidOperationException: Starbook.GetTime()={0}", response);
                    throw new ASCOM.InvalidOperationException("SlewToTargetAsync: Starbook.GetTime() is not working.");
                }

                transform.SiteLatitude = place.Latitude.Value;
                transform.SiteLongitude = place.Longitude.Value;
                transform.JulianDateUTC = utilities.DateUTCToJulian(time.AddHours(-place.Timezone));

                double transformRightAscension, transformDeclination;

                if (starbook.J2000)
                {
                    transform.SetTopocentric(targetRightAscension, targetDeclination);
                    transformRightAscension = transform.RAJ2000;
                    transformDeclination = transform.DecJ2000;
                }
                else
                {
                    transform.SetJ2000(targetRightAscension, targetDeclination);
                    transformRightAscension = transform.RATopocentric;
                    transformDeclination = transform.DECTopocentric;
                }

                if (!Starbook.HMS.FromValue(transformRightAscension, out rightAscension))
                {
                    LogMessage("SlewToTargetAsync", "InvalidValueException: RightAscension={0}=>{1}", targetRightAscension, transformRightAscension);
                    throw new ASCOM.InvalidValueException("SlewToTargetAsync", "RightAscension", "0 to 24");
                }

                if (!Starbook.DMS.FromValue(transformDeclination, out declination))
                {
                    LogMessage("SlewToTargetAsync", "InvalidValueException: Declination={0}=>{1}", targetDeclination, transformDeclination);
                    throw new ASCOM.InvalidValueException("SlewToTargetAsync", "Declination", "-90 to 90");
                }
            }

            response = Goto(rightAscension, declination);

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
                CheckConnected("Slewing_get");

                bool slewing;

                lock (this)
                {
                    slewing = this.movingAxis || this.pulseGuiding;
                }

                if (!slewing)
                {
                    Starbook.Response response = GetStatus(out Starbook.Status status);

                    if (response != Starbook.Response.OK)
                    {
                        LogMessage("Slewing_get", "InvalidOperationException: Starbook.GetStatus()={0}", response);
                        throw new ASCOM.InvalidOperationException("Slewing_get: Starbook.GetStatus() is not working.");
                    }

                    slewing = status.Goto;
                }

                LogMessage("Slewing_get", "{0}", slewing);
                return slewing;
            }
        }

        public void SyncToAltAz(double Azimuth, double Altitude)
        {
            CheckConnected("SyncToAltAz");

            if (this.parking)
            {
                LogMessage("SyncToAltAz", "InvalidOperationException: AtPark");
                throw new ASCOM.InvalidOperationException("SyncToAltAz: AtPark, Starbook.Unpark() must be called first.");
            }

            if (Azimuth < 0 || 360 < Azimuth)
            {
                LogMessage("SyncToAltAz", "InvalidValueException: Azimuth={0}", Azimuth);
                throw new ASCOM.InvalidValueException("SyncToAltAz", "Azimuth", "0 to 360");
            }

            if (Altitude < 0 || 90 < Altitude)
            {
                LogMessage("SyncToAltAz", "InvalidValueException: Altitude={0}", Altitude);
                throw new ASCOM.InvalidValueException("SyncToAltAz", "Altitude", "0 to 90");
            }

            Starbook.Response response = GetPlace(out Starbook.Place place);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SyncToAltAz", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                throw new ASCOM.InvalidOperationException("SyncToAltAz: Starbook.GetPlace() is not working.");
            }

            response = starbook.GetTime(out DateTime time);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SyncToAltAz", "InvalidOperationException: Starbook.GetTime()={0}", response);
                throw new ASCOM.InvalidOperationException("SyncToAltAz: Starbook.GetTime() is not working.");
            }

            transform.SiteLatitude = place.Latitude.Value;
            transform.SiteLongitude = place.Longitude.Value;
            transform.JulianDateUTC = utilities.DateUTCToJulian(time.AddHours(-place.Timezone));
            transform.SetAzimuthElevation(Azimuth, Altitude);

            double RightAscension, Declination;

            if (starbook.J2000)
            {
                RightAscension = transform.RAJ2000;
                Declination = transform.DecJ2000;
            }
            else
            {
                RightAscension = transform.RATopocentric;
                Declination = transform.DECTopocentric;
            }

            if (!Starbook.HMS.FromValue(RightAscension, out Starbook.HMS rightAscension))
            {
                LogMessage("SyncToAltAz", "InvalidValueException: RightAscension={0}", RightAscension);
                throw new ASCOM.InvalidValueException("SyncToAltAz", "RightAscension", "0 to 24");
            }

            if (!Starbook.DMS.FromValue(Declination, out Starbook.DMS declination))
            {
                LogMessage("SyncToAltAz", "InvalidValueException: Declination={0}", Declination);
                throw new ASCOM.InvalidValueException("SyncToAltAz", "Declination", "-90 to 90");
            }

            if (j2000)
            {
                targetRightAscension = transform.RAJ2000;
                targetDeclination = transform.DecJ2000;
            }
            else
            {
                targetRightAscension = transform.RATopocentric;
                targetDeclination = transform.DECTopocentric;
            }

            response = Align(rightAscension, declination);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SyncToAltAz", "InvalidOperationException: Starbook.Align({0},{1})={2}", rightAscension, declination, response);
                throw new ASCOM.InvalidOperationException("SyncToAltAz: Starbook.Align() is not working.");
            }

            LogMessage("SyncToAltAz", "OK: Azimuth={0},Altitude={1}", Azimuth, Altitude);
        }

        public void SyncToCoordinates(double RightAscension, double Declination)
        {
            CheckConnected("SyncToCoordinates");

            if (this.parking)
            {
                LogMessage("SyncToCoordinates", "InvalidOperationException: AtPark");
                throw new ASCOM.InvalidOperationException("SyncToCoordinates: AtPark, Starbook.Unpark() must be called first.");
            }

            Starbook.Response response;
            Starbook.HMS rightAscension;
            Starbook.DMS declination;

            if (j2000 == starbook.J2000)
            {
                if (!Starbook.HMS.FromValue(RightAscension, out rightAscension))
                {
                    LogMessage("SyncToCoordinates", "InvalidValueException: RightAscension={0}", RightAscension);
                    throw new ASCOM.InvalidValueException("SyncToCoordinates", "RightAscension", "0 to 24");
                }

                if (!Starbook.DMS.FromValue(Declination, out declination))
                {
                    LogMessage("SyncToCoordinates", "InvalidValueException: Declination={0}", Declination);
                    throw new ASCOM.InvalidValueException("SyncToCoordinate", "Declination", "-90 to 90");
                }
            }
            else
            {
                response = GetPlace(out Starbook.Place place);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("SyncToCoordinates", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                    throw new ASCOM.InvalidOperationException("SyncToCoordinates: Starbook.GetPlace() is not working.");
                }

                response = starbook.GetTime(out DateTime time);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("SyncToCoordinates", "InvalidOperationException: Starbook.GetTime()={0}", response);
                    throw new ASCOM.InvalidOperationException("SyncToCoordinates: Starbook.GetTime() is not working.");
                }

                transform.SiteLatitude = place.Latitude.Value;
                transform.SiteLongitude = place.Longitude.Value;
                transform.JulianDateUTC = utilities.DateUTCToJulian(time.AddHours(-place.Timezone));

                double transformRightAscension, transformDeclination;

                if (starbook.J2000)
                {
                    transform.SetTopocentric(RightAscension, Declination);
                    transformRightAscension = transform.RAJ2000;
                    transformDeclination = transform.DecJ2000;
                }
                else
                {
                    transform.SetJ2000(RightAscension, Declination);
                    transformRightAscension = transform.RATopocentric;
                    transformDeclination = transform.DECTopocentric;
                }

                if (!Starbook.HMS.FromValue(transformRightAscension, out rightAscension))
                {
                    LogMessage("SyncToCoordinates", "InvalidValueException: RightAscension={0}=>{1}", RightAscension, transformRightAscension);
                    throw new ASCOM.InvalidValueException("SyncToCoordinates", "RightAscension", "0 to 24");
                }

                if (!Starbook.DMS.FromValue(transformDeclination, out declination))
                {
                    LogMessage("SyncToCoordinates", "InvalidValueException: Declination={0}=>{1}", Declination, transformDeclination);
                    throw new ASCOM.InvalidValueException("SyncToCoordinates", "Declination", "-90 to 90");
                }
            }

            targetRightAscension = RightAscension;
            targetDeclination = Declination;

            response = Align(rightAscension, declination);

            if (response != Starbook.Response.OK)
            {
                LogMessage("SyncToCoordinates", "InvalidOperationException: Starbook.Align({0},{1})={2}", rightAscension, declination, response);
                throw new ASCOM.InvalidOperationException("SyncToCoordinates: Starbook.Align() is not working.");
            }

            LogMessage("SyncToCoordinates", "OK: RightAscension={0},Declination={1}", RightAscension, Declination);
        }

        public void SyncToTarget()
        {
            CheckConnected("SyncToTarget");

            if (this.parking)
            {
                LogMessage("SyncToTarget", "InvalidOperationException: AtPark");
                throw new ASCOM.InvalidOperationException("SyncToTarget: AtPark, Starbook.Unpark() must be called first.");
            }

            Starbook.Response response;
            Starbook.HMS rightAscension;
            Starbook.DMS declination;

            if (j2000 == starbook.J2000)
            {
                if (!Starbook.HMS.FromValue(targetRightAscension, out rightAscension))
                {
                    LogMessage("SyncToTarget", "InvalidValueException: TargetRightAscension={0}", targetRightAscension);
                    throw new ASCOM.InvalidValueException("SyncToTarget", "TargetRightAscension", "0 to 24");
                }

                if (!Starbook.DMS.FromValue(targetDeclination, out declination))
                {
                    LogMessage("SyncToTarget", "InvalidValueException: TargetDeclination={0}", targetDeclination);
                    throw new ASCOM.InvalidValueException("SyncToTarget", "TargetDeclination", "-90 to 90");
                }
            }
            else
            {
                response = GetPlace(out Starbook.Place place);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("SyncToTarget", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                    throw new ASCOM.InvalidOperationException("SyncToTarget: Starbook.GetPlace() is not working.");
                }

                response = starbook.GetTime(out DateTime time);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("SyncToTarget", "InvalidOperationException: Starbook.GetTime()={0}", response);
                    throw new ASCOM.InvalidOperationException("SyncToTarget: Starbook.GetTime() is not working.");
                }

                transform.SiteLatitude = place.Latitude.Value;
                transform.SiteLongitude = place.Longitude.Value;
                transform.JulianDateUTC = utilities.DateUTCToJulian(time.AddHours(-place.Timezone));

                double transformRightAscension, transformDeclination;

                if (starbook.J2000)
                {
                    transform.SetTopocentric(targetRightAscension, targetDeclination);
                    transformRightAscension = transform.RAJ2000;
                    transformDeclination = transform.DecJ2000;
                }
                else
                {
                    transform.SetJ2000(targetRightAscension, targetDeclination);
                    transformRightAscension = transform.RATopocentric;
                    transformDeclination = transform.DECTopocentric;
                }

                if (!Starbook.HMS.FromValue(transformRightAscension, out rightAscension))
                {
                    LogMessage("SyncToTarget", "InvalidValueException: RightAscension={0}=>{1}", targetRightAscension, transformRightAscension);
                    throw new ASCOM.InvalidValueException("SyncToTarget", "RightAscension", "0 to 24");
                }

                if (!Starbook.DMS.FromValue(transformDeclination, out declination))
                {
                    LogMessage("SyncToTarget", "InvalidValueException: Declination={0}=>{1}", targetDeclination, transformDeclination);
                    throw new ASCOM.InvalidValueException("SyncToTarget", "Declination", "-90 to 90");
                }
            }

            response = Align(rightAscension, declination);

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

                LogMessage("TargetDeclination_get", "{0}", targetDeclination);
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

                LogMessage("TargetRightAscension_get", "{0}", targetRightAscension);
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
                CheckConnected("Tracking_get");

                bool tracking = this.tracking;

                if (tracking)
                {
                    Starbook.Response response = GetStatus(out Starbook.Status status);

                    if (response != Starbook.Response.OK)
                    {
                        LogMessage("Tracking_get", "InvalidOperationException: Starbook.GetStatus()={0}", response);
                        throw new ASCOM.InvalidOperationException("Tracking_get: Starbook.GetStatus() is not working.");
                    }

                    tracking = (status.State == Starbook.State.Guide || status.State == Starbook.State.Scope || status.State == Starbook.State.Chart || status.State == Starbook.State.User);
                    LogMessage("Tracking_get", "{0}: Status.State={1}", tracking, status.State);
                }
                else
                {
                    LogMessage("Tracking_get", "{0}", tracking);
                }

                return tracking;
            }
            set
            {
                CheckConnected("Tracking_set");

                if (value == tracking)
                {
                    LogMessage("Tracking_set", "OK: {0}, Ignored", value); return;
                }

                tracking = value;

                if (value)
                {
                    Starbook.Response response = starbook.Start();

                    if (response == Starbook.Response.OK)
                    {
                        LogMessage("Tracking_set", "OK: {0}", value);
                    }
                    else
                    {
                        LogMessage("Tracking_set", "FAIL: {0}, Starbook.Start()={1}", value, response);
                    }
                }
                else
                {
                    Starbook.Response response = starbook.Stop();

                    if (response == Starbook.Response.OK)
                    {
                        LogMessage("Tracking_set", "OK: {0}", value);
                    }
                    else
                    {
                        LogMessage("Tracking_set", "FAIL: {0}, Starbook.Stop()={1}", value, response);
                    }
                }
            }
        }

        public DriveRates TrackingRate
        {
            get
            {
                DriveRates trackingRate = DriveRates.driveSidereal;
                LogMessage("TrackingRate_get", "{0}", trackingRate);
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
                    LogMessage("TrackingRates_get", "{0}", driveRate);
                }

                return trackingRates;
            }
        }

        public DateTime UTCDate
        {
            get
            {
                CheckConnected("UTCDate_get");

                Starbook.Response response = starbook.GetTime(out DateTime time);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("UTCDate_get", "InvalidOperationException: Starbook.GetTime()={0}", response);
                    throw new ASCOM.InvalidOperationException("UTCDate_get: Starbook.GetTime() is not working.");
                }

                response = GetPlace(out Starbook.Place place);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("UTCDate_get", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                    throw new ASCOM.InvalidOperationException("UTCDate_get: Starbook.GetPlace() is not working.");
                }

                DateTime utcDate = time.AddHours(-place.Timezone);
                LogMessage("UTCDate_get", "{0:yyyy/MM/dd HH:mm:ss}", utcDate);
                return utcDate;
            }
            set
            {
                CheckConnected("UTCDate_set");

                Starbook.Response response = GetPlace(out Starbook.Place place);

                if (response != Starbook.Response.OK)
                {
                    LogMessage("UTCDate_set", "InvalidOperationException: Starbook.GetPlace()={0}", response);
                    throw new ASCOM.InvalidOperationException("UTCDate_set: Starbook.GetPlace() is not working.");
                }

                DateTime time = value.AddHours(place.Timezone);
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
            CheckConnected("Unpark");

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

                bool connectedState;

                lock (this)
                {
                    connectedState = this.connectedState;
                }

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
                LogMessage(message, "NotConnectedException");
                throw new ASCOM.NotConnectedException(message);
            }
        }

        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        internal void ReadProfile()
        {
            bool recovering = false;

            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Telescope";

                if (!IPAddress.TryParse(driverProfile.GetValue(driverID, ipAddressProfileName, string.Empty, string.Empty), out IPAddress ipAddress))
                {
                    ipAddress = ipAddressDefault; recovering = true;
                }

                starbook.IPAddress = ipAddress;

                if (!short.TryParse(driverProfile.GetValue(driverID, slewSettleTimeProfileName, string.Empty, string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out slewSettleTime))
                {
                    slewSettleTime = slewSettleTimeDefault; recovering = true;
                }

                if (!int.TryParse(driverProfile.GetValue(driverID, guideRateProfileName, string.Empty, string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out guideRate))
                {
                    guideRate = guideRateDefault; recovering = true;
                }

                guideRates = new double[9];

                string[] guideRateStrings = driverProfile.GetValue(driverID, guideRatesProfileName, string.Empty, string.Empty).Split(',');

                if (guideRateStrings.Length == 9)
                {
                    for (int index = 0; index < 9; index++)
                    {
                        if (guideRateStrings[index] == "NaN")
                        {
                            guideRates[index] = double.NaN;
                        }
                        else if (!double.TryParse(guideRateStrings[index], NumberStyles.Number, CultureInfo.InvariantCulture, out guideRates[index]))
                        {
                            Array.Copy(guideRatesDefault, guideRates, 9); recovering = true; break;
                        }
                    }
                }
                else
                {
                    Array.Copy(guideRatesDefault, guideRates, 9); recovering = true;
                }

                if (!bool.TryParse(driverProfile.GetValue(driverID, j2000ProfileName, string.Empty, string.Empty), out j2000))
                {
                    j2000 = j2000Default; recovering = true;
                }

                if (!int.TryParse(driverProfile.GetValue(driverID, autoMeridianFlipProfileName, string.Empty, string.Empty), out autoMeridianFlip))
                {
                    autoMeridianFlip = autoMeridianFlipDefault; recovering = true;
                }

                if (!bool.TryParse(driverProfile.GetValue(driverID, traceLoggerProfileName, string.Empty, string.Empty), out bool traceLoggerEnabled))
                {
                    traceLoggerEnabled = traceLoggerDefault; recovering = true;
                }

                traceLogger.Enabled = traceLoggerEnabled;
            }

            if (recovering)
            {
                WriteProfile();
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
                driverProfile.WriteValue(driverID, slewSettleTimeProfileName, slewSettleTime.ToString(CultureInfo.InvariantCulture));

                driverProfile.WriteValue(driverID, guideRateProfileName, guideRate.ToString(CultureInfo.InvariantCulture));

                string[] guideRateStrings = new string[9];

                for (int index = 0; index < 9; index++)
                {
                    if (double.IsNaN(guideRates[index]))
                    {
                        guideRateStrings[index] = "NaN";
                    }
                    else
                    {
                        guideRateStrings[index] = guideRates[index].ToString(CultureInfo.InvariantCulture);
                    }
                }

                driverProfile.WriteValue(driverID, guideRatesProfileName, string.Join(",", guideRateStrings));

                driverProfile.WriteValue(driverID, j2000ProfileName, j2000.ToString(CultureInfo.InvariantCulture));
                driverProfile.WriteValue(driverID, autoMeridianFlipProfileName, autoMeridianFlip.ToString(CultureInfo.InvariantCulture));
                driverProfile.WriteValue(driverID, traceLoggerProfileName, traceLogger.Enabled.ToString(CultureInfo.InvariantCulture));
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
            var msg = string.Format(CultureInfo.InvariantCulture, message, args);

            lock (traceLogger)
            {
                traceLogger.LogMessage(identifier, msg);
            }
        }
        #endregion

        #region Multi-threading features

        private class Request
        {
            public Request(string name)
            {
                this.Name = name;
                this.Parameters = new Dictionary<string, object>();
                this.Response = null;
            }

            public string Name { get; set; }
            public Dictionary<string, object> Parameters { get; private set; }
            public object Response { get; set; }
        }

        private void ThreadEntry()
        {
            int slewingState = 0; // 0: NotSlewing, 1: Slewing, 2: SlewingSettle
            DateTime slewingSettle = DateTime.MinValue;
            bool findingHome = false;
            Starbook.Response response;

            for (bool initializing = true; threadRunning; )
            {
                if (initializing)
                {
                    initializing = false;

                    do
                    {
                        if ((response = starbook.Start()) != Starbook.Response.OK)
                        {
                            LogMessage("Thread", "Starbook.Start()={0}", response);
                        }

                        /*if ((response = starbook.Stop()) != Starbook.Response.OK)
                        {
                            LogMessage("Thread", "Starbook.Stop()={0}", response); break;
                        }*/

                        if ((response = starbook.NoMove()) != Starbook.Response.OK)
                        {
                            LogMessage("Thread", "Starbook.NoMove()={0}", response); break;
                        }

                        if ((response = starbook.SetSpeed(guideRate)) != Starbook.Response.OK)
                        {
                            LogMessage("Thread", "Starbook.SetSpeed({0})={1}", guideRate, response); break;
                        }
                    }
                    while (false);

                    if (response != Starbook.Response.OK)
                    {
                        lock (this)
                        {
                            connectedState = false;
                        }

                        break;
                    }
                }
                else
                {
                    bool slewing = false; Request request = null;

                    lock (this.requests)
                    {
                        if (this.requests.Count > 0)
                        {
                            request = this.requests.Dequeue();
                        }
                    }

                    if (request != null)
                    {
                        switch (request.Name)
                        {
                            case "Align":
                            {
                                Starbook.HMS ra = (Starbook.HMS)request.Parameters["RA"];
                                Starbook.DMS dec = (Starbook.DMS)request.Parameters["Dec"];

                                response = starbook.Align(ra, dec);

                                if (response == Starbook.Response.OK)
                                {
                                    lock (this)
                                    {
                                        this.status = null; this.response = null;
                                    }
                                }

                                break;
                            }
                            case "Goto":
                            {
                                Starbook.HMS ra = (Starbook.HMS)request.Parameters["RA"];
                                Starbook.DMS dec = (Starbook.DMS)request.Parameters["Dec"];

                                response = starbook.Goto(ra, dec);

                                if (response == Starbook.Response.OK)
                                {
                                    slewing = true;

                                    lock (this)
                                    {
                                        this.status = null; this.response = null; this.atHome = false;
                                    }
                                }

                                break;
                            }
                            case "Home":
                            {
                                response = starbook.Home();

                                if (response == Starbook.Response.OK)
                                {
                                    slewing = true; findingHome = true;

                                    lock (this)
                                    {
                                        this.status = null; this.response = null; this.atHome = false;
                                    }
                                }

                                break;
                            }
                            default:
                            {
                                response = Starbook.Response.ErrorUnknown; break;
                            }
                        }

                        lock (request)
                        {
                            request.Response = response;
                        }
                    }

                    response = starbook.GetStatus(out Starbook.Status status);

                    if (response != Starbook.Response.OK)
                    {
                        lock (this)
                        {
                            connectedState = false; this.status = null; this.response = null;
                        }

                        LogMessage("Thread", "Starbook.GetStatus()={0}", response); break;
                    }

                    switch (slewingState)
                    {
                        case 0: // NotSlewing
                        {
                            if (slewing)
                            {
                                slewingState = 1; status.Goto = true;
                            }

                            break;
                        }
                        case 1: // Slewing
                        {
                            if (!status.Goto)
                            {
                                slewingState = 2; slewingSettle = DateTime.Now; status.Goto = true;
                            }

                            break;
                        }
                        case 2: // SlewingSettle
                        {
                            if (status.Goto)
                            {
                                slewingState = 1;
                            }
                            else
                            {
                                TimeSpan timeSpan = DateTime.Now - slewingSettle;

                                if (timeSpan.TotalSeconds >= slewSettleTime)
                                {
                                    slewingState = 0;

                                    if (findingHome)
                                    {
                                        lock (this)
                                        {
                                            this.atHome = true;
                                        }

                                        findingHome = false;
                                    }
                                }
                                else
                                {
                                    status.Goto = true;
                                }
                            }

                            break;
                        }
                    }

                    lock (this)
                    {
                        this.status = status; this.response = response;
                    }

                    Thread.Sleep(poll);
                }
            }

            lock (this.requests)
            {
                foreach (Request request in this.requests)
                {
                    lock (request)
                    {
                        request.Response = Starbook.Response.ErrorUnknown;
                    }
                }

                this.requests.Clear();
            }
        }

        private Starbook.Response GetPlace(out Starbook.Place place)
        {
            Starbook.Response response = Starbook.Response.OK;

            if (placeCached)
            {
                place = placeCache;
            }
            else
            {
                response = starbook.GetPlace(out place);

                if (response == Starbook.Response.OK)
                {
                    placeCache = place; placeCached = true;
                }  
            }

            return response;
        }

        private Starbook.Response GetStatus(out Starbook.Status status)
        {
            DateTime dateTime = DateTime.Now;

            while (true)
            {
                bool connected; object statusPolled, responsePolled;

                lock (this)
                {
                    connected = this.connectedState; statusPolled = this.status; responsePolled = this.response;
                }

                TimeSpan timeSpan = DateTime.Now - dateTime;

                if (timeSpan.TotalSeconds >= 60 || !connected)
                {
                    status = new Starbook.Status(); return Starbook.Response.ErrorUnknown;
                }

                if (statusPolled != null && responsePolled != null)
                {
                    status = (Starbook.Status)statusPolled; return (Starbook.Response)responsePolled;
                }

                Thread.Sleep(100);
            }
        }

        private Starbook.Response Align(Starbook.HMS ra, Starbook.DMS dec)
        {
            Request request = new Request("Align");
            request.Parameters.Add("RA", ra);
            request.Parameters.Add("Dec", dec);

            return Handshake(request);
        }

        private Starbook.Response Goto(Starbook.HMS ra, Starbook.DMS dec)
        {
            Request request = new Request("Goto");
            request.Parameters.Add("RA", ra);
            request.Parameters.Add("Dec", dec);

            return Handshake(request);
        }

        private Starbook.Response Home()
        {
            Request request = new Request("Home");

            return Handshake(request);
        }

        private Starbook.Response Handshake(Request request)
        {
            lock (this.requests)
            {
                this.requests.Enqueue(request);
            }

            DateTime dateTime = DateTime.Now;

            while (true)
            {
                lock (request)
                {
                    if (request.Response != null)
                    {
                        break;
                    }
                }

                bool connected;

                lock (this)
                {
                    connected = this.connectedState;
                }

                if (!connected)
                {
                    lock (request)
                    {
                        request.Response = Starbook.Response.ErrorUnknown;
                    }

                    break;
                }

                TimeSpan timeSpan = DateTime.Now - dateTime;

                if (timeSpan.TotalSeconds >= 60)
                {
                    lock (request)
                    {
                        request.Response = Starbook.Response.ErrorUnknown;
                    }

                    break;
                }

                Thread.Sleep(100);
            }

            return (Starbook.Response)request.Response;
        }

        #endregion
    }
}
