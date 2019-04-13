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
        /// Variable to hold the trace logger object (creates a diagnostic log file with information that you specify)
        /// </summary>
        internal static TraceLogger traceLogger;

        private double parkDeclination;
        private double parkRightAscension;

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

            //TODO: Implement your additional construction here

            parkRightAscension = double.NaN;
            parkDeclination = double.NaN;

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
                    traceLogger.LogMessage("SupportedActions Get", action);
                }
                return actions;
            }
        }

        public string Action(string actionName, string actionParameters)
        {
            LogMessage("", "Action {0}, parameters {1} not implemented", actionName, actionParameters);
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
        }

        public bool Connected
        {
            get
            {
                LogMessage("Connected", "Get {0}", IsConnected);
                return IsConnected;
            }
            set
            {
                traceLogger.LogMessage("Connected", "Set {0}", value);
                if (value == IsConnected)
                    return;

                if (value)
                {
                    connectedState = true;
                    LogMessage("Connected Set", "Connecting to {0}", starbook.IPAddress);
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
                    LogMessage("Connected Set", "Disconnecting from {0}", starbook.IPAddress);
                    // TODO disconnect from the device
                }
            }
        }

        public string Description
        {
            // TODO customise this device description
            get
            {
                traceLogger.LogMessage("Description Get", driverDescription);
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
                traceLogger.LogMessage("DriverInfo Get", driverInfo);
                return driverInfo;
            }
        }

        public string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                traceLogger.LogMessage("DriverVersion Get", driverVersion);
                return driverVersion;
            }
        }

        public short InterfaceVersion
        {
            // set by the driver wizard
            get
            {
                LogMessage("InterfaceVersion Get", "3");
                return Convert.ToInt16("3");
            }
        }

        public string Name
        {
            get
            {
                string name = "Starbook";
                traceLogger.LogMessage("Name Get", name);
                return name;
            }
        }

        #endregion

        #region ITelescope Implementation
        public void AbortSlew()
        {
            Starbook.Response response = starbook.Stop();
            traceLogger.LogMessage("AbortSlew", response.ToString());
        }

        public AlignmentModes AlignmentMode
        {
            get
            {
                AlignmentModes mode = AlignmentModes.algGermanPolar;
                traceLogger.LogMessage("AlignmentMode Get", mode.ToString());
                return mode;
            }
        }

        public double Altitude
        {
            get
            {
                traceLogger.LogMessage("Altitude", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Altitude", false);
            }
        }

        public double ApertureArea
        {
            get
            {
                traceLogger.LogMessage("ApertureArea Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("ApertureArea", false);
            }
        }

        public double ApertureDiameter
        {
            get
            {
                traceLogger.LogMessage("ApertureDiameter Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("ApertureDiameter", false);
            }
        }

        public bool AtHome
        {
            get
            {
                traceLogger.LogMessage("AtHome", "Get - " + false.ToString());
                return false;
            }
        }

        public bool AtPark
        {
            get
            {
                Telescope.Starbook.Status status = Telescope.starbook.GetStatus();
                double rightAscension = status.RA.Value;
                double declination = status.Dec.Value;
                bool atPark = (rightAscension == parkRightAscension && declination == parkDeclination);
                traceLogger.LogMessage("AtPark", "Get - " + atPark.ToString());
                return atPark;
            }
        }

        public IAxisRates AxisRates(TelescopeAxes Axis)
        {
            traceLogger.LogMessage("AxisRates", "Get - " + Axis.ToString());
            return new AxisRates(Axis);
        }

        public double Azimuth
        {
            get
            {
                traceLogger.LogMessage("Azimuth Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Azimuth", false);
            }
        }

        public bool CanFindHome
        {
            get
            {
                traceLogger.LogMessage("CanFindHome", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanMoveAxis(TelescopeAxes Axis)
        {
            traceLogger.LogMessage("CanMoveAxis", "Get - " + Axis.ToString());
            switch (Axis)
            {
                case TelescopeAxes.axisPrimary: return true;
                case TelescopeAxes.axisSecondary: return true;
                case TelescopeAxes.axisTertiary: return false;
                default: throw new InvalidValueException("CanMoveAxis", Axis.ToString(), "0 to 2");
            }
        }

        public bool CanPark
        {
            get
            {
                traceLogger.LogMessage("CanPark", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanPulseGuide
        {
            get
            {
                traceLogger.LogMessage("CanPulseGuide", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSetDeclinationRate
        {
            get
            {
                traceLogger.LogMessage("CanSetDeclinationRate", "Get - " + false.ToString());
                return true;
            }
        }

        public bool CanSetGuideRates
        {
            get
            {
                traceLogger.LogMessage("CanSetGuideRates", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSetPark
        {
            get
            {
                traceLogger.LogMessage("CanSetPark", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSetPierSide
        {
            get
            {
                traceLogger.LogMessage("CanSetPierSide", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSetRightAscensionRate
        {
            get
            {
                traceLogger.LogMessage("CanSetRightAscensionRate", "Get - " + false.ToString());
                return true;
            }
        }

        public bool CanSetTracking
        {
            get
            {
                traceLogger.LogMessage("CanSetTracking", "Get - " + true.ToString());
                return false;
            }
        }

        public bool CanSlew
        {
            get
            {
                traceLogger.LogMessage("CanSlew", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSlewAltAz
        {
            get
            {
                traceLogger.LogMessage("CanSlewAltAz", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSlewAltAzAsync
        {
            get
            {
                traceLogger.LogMessage("CanSlewAltAzAsync", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSlewAsync
        {
            get
            {
                traceLogger.LogMessage("CanSlewAsync", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSync
        {
            get
            {
                traceLogger.LogMessage("CanSync", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSyncAltAz
        {
            get
            {
                traceLogger.LogMessage("CanSyncAltAz", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanUnpark
        {
            get
            {
                traceLogger.LogMessage("CanUnpark", "Get - " + true.ToString());
                return true;
            }
        }

        public double Declination
        {
            get
            {
                double declination = starbook.GetStatus().Dec.Value;
                traceLogger.LogMessage("Declination", "Get - " + declination.ToString());
                return declination;
            }
        }

        public double DeclinationRate
        {
            get
            {
                double declination = 0.0;
                traceLogger.LogMessage("DeclinationRate", "Get - " + declination.ToString());
                return declination;
            }
            set
            {
                traceLogger.LogMessage("DeclinationRate Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("DeclinationRate", true);
            }
        }

        public PierSide DestinationSideOfPier(double RightAscension, double Declination)
        {
            traceLogger.LogMessage("DestinationSideOfPier Get", "Not implemented");
            throw new ASCOM.PropertyNotImplementedException("DestinationSideOfPier", false);
        }

        public bool DoesRefraction
        {
            get
            {
                traceLogger.LogMessage("DoesRefraction Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("DoesRefraction", false);
            }
            set
            {
                traceLogger.LogMessage("DoesRefraction Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("DoesRefraction", true);
            }
        }

        public EquatorialCoordinateType EquatorialSystem
        {
            get
            {
                EquatorialCoordinateType equatorialSystem = EquatorialCoordinateType.equTopocentric;
                traceLogger.LogMessage("DeclinationRate", "Get - " + equatorialSystem.ToString());
                return equatorialSystem;
            }
        }

        public void FindHome()
        {
            Starbook.Response response = starbook.Home();
            traceLogger.LogMessage("FindHome", response.ToString());
        }

        public double FocalLength
        {
            get
            {
                traceLogger.LogMessage("FocalLength Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("FocalLength", false);
            }
        }

        public static double GuideRate(int guideRate)
        {
            return Telescope.guideRates[guideRate] * (15.041 / 3600);
        }

        public static int GuideRate(double guideRate)
        {
            guideRate /= (15.041 / 3600);

            double guideRateDifference = double.MaxValue;
            int guideRateIndex = 0;

            for (int index = 0; index <= 8; index++)
            {
                double difference = Math.Abs(guideRate - Telescope.guideRates[index]);

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
                traceLogger.LogMessage("GuideRateDeclination Get", guideRate.ToString());
                return guideRate;
            }
            set
            {
                Telescope.Starbook.Response response = Telescope.starbook.SetSpeed(Telescope.guideRate = GuideRate(value));
                traceLogger.LogMessage("GuideRateDeclination Set", string.Format("{0}: {1}", value, response));
            }
        }

        public double GuideRateRightAscension
        {
            get
            {
                double guideRate = GuideRate(Telescope.guideRate);
                traceLogger.LogMessage("GuideRateRightAscension Get", guideRate.ToString());
                return guideRate;
            }
            set
            {
                Telescope.Starbook.Response response = Telescope.starbook.SetSpeed(Telescope.guideRate = GuideRate(value));
                traceLogger.LogMessage("GuideRateRightAscension Set", string.Format("{0}: {1}", value, response));
            }
        }

        public bool IsPulseGuiding
        {
            get
            {
                traceLogger.LogMessage("IsPulseGuiding Get", pulseGuiding.ToString());
                return pulseGuiding;
            }
        }

        public void MoveAxis(TelescopeAxes Axis, double Rate)
        {
            switch (Axis)
            {
                case TelescopeAxes.axisPrimary:
                    if (Rate == 0)
                    {
                        Telescope.starbook.NoMove();
                        Telescope.starbook.SetSpeed(Telescope.guideRate);
                    }
                    else
                    {
                        Telescope.starbook.SetSpeed(GuideRate(Math.Abs(Rate)));
                        Telescope.starbook.Move(Rate > 0 ? Telescope.Starbook.Direction.East : Telescope.Starbook.Direction.West);
                    }
                    break;
                case TelescopeAxes.axisSecondary:
                    if (Rate == 0)
                    {
                        Telescope.starbook.NoMove();
                        Telescope.starbook.SetSpeed(Telescope.guideRate);
                    }
                    else
                    {
                        Telescope.starbook.SetSpeed(GuideRate(Math.Abs(Rate)));
                        Telescope.starbook.Move(Rate > 0 ? Telescope.Starbook.Direction.North : Telescope.Starbook.Direction.South);
                    }
                    break;
                case TelescopeAxes.axisTertiary:
                    traceLogger.LogMessage("MoveAxis", "Not implemented: axisTertiary");
                    throw new ASCOM.MethodNotImplementedException("MoveAxis");
            }
        }

        public void Park()
        {
            if (double.IsNaN(parkRightAscension) || double.IsNaN(parkDeclination))
            {
                throw new ASCOM.InvalidOperationException("Park: RightAscension/Declination is not set by SetPark.");
            }

            Telescope.Starbook.HMS rightAscension = new Starbook.HMS(parkRightAscension);
            Telescope.Starbook.DMS declination = new Starbook.DMS(parkDeclination);
            Telescope.Starbook.Response response = Telescope.starbook.Goto(rightAscension, declination);
            traceLogger.LogMessage("Park", response.ToString());
        }

        public void PulseGuide(GuideDirections Direction, int Duration)
        {
            Starbook.Response response;

            switch (Direction)
            {
                case GuideDirections.guideNorth:
                    response = starbook.Move(Starbook.Direction.North); break;
                case GuideDirections.guideSouth:
                    response = starbook.Move(Starbook.Direction.South); break;
                case GuideDirections.guideEast:
                    response = starbook.Move(Starbook.Direction.East); break;
                case GuideDirections.guideWest:
                    response = starbook.Move(Starbook.Direction.West); break;
                default:
                    response = Starbook.Response.ErrorUnknown; break;
            }

            if (response == Starbook.Response.OK)
            {
                pulseGuiding = true;

                if (Duration > 0)
                {
                    Thread.Sleep(Duration);
                }

                starbook.NoMove();
                
                pulseGuiding = false;
            }

            traceLogger.LogMessage("PulseGuide", response.ToString());
        }

        public double RightAscension
        {
            get
            {
                double rightAscension = starbook.GetStatus().RA.Value;
                traceLogger.LogMessage("RightAscension", "Get - " + rightAscension.ToString());
                return rightAscension;
            }
        }

        public double RightAscensionRate
        {
            get
            {
                double rightAscensionRate = 0.0;
                traceLogger.LogMessage("RightAscensionRate", "Get - " + rightAscensionRate.ToString());
                return rightAscensionRate;
            }
            set
            {
                traceLogger.LogMessage("RightAscensionRate Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("RightAscensionRate", true);
            }
        }

        public void SetPark()
        {
            Telescope.Starbook.Status status = Telescope.starbook.GetStatus();
            parkRightAscension = status.RA.Value;
            parkDeclination = status.Dec.Value;
            traceLogger.LogMessage("SetPark", string.Format("{0} {1}", parkRightAscension, parkDeclination));
        }

        public PierSide SideOfPier
        {
            get
            {
                traceLogger.LogMessage("SideOfPier Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SideOfPier", false);
            }
            set
            {
                traceLogger.LogMessage("SideOfPier Set", "Not implemented");
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

                traceLogger.LogMessage("SiderealTime", "Get - " + siderealTime.ToString());
                return siderealTime;
            }
        }

        public double SiteElevation
        {
            get
            {
                double siteElevation = 0;
                traceLogger.LogMessage("SiteElevation Get", siteElevation.ToString());
                return siteElevation;
            }
            set
            {
                traceLogger.LogMessage("SiteElevation Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteElevation", true);
            }
        }

        public double SiteLatitude
        {
            get
            {
                double latitude = starbook.GetPlace().Latitude.Value;
                traceLogger.LogMessage("SiteLatitude Get", latitude.ToString());
                return latitude;
            }
            set
            {
                Starbook.Place place = starbook.GetPlace();
                place.Latitude = new Starbook.DMS(Math.Abs(value), value >= 0 ? Starbook.Direction.North : Starbook.Direction.South);
                Starbook.Response response = starbook.SetPlace(place);
                traceLogger.LogMessage("SiteLatitude Set", string.Format("{0}: {1}", value, response));
            }
        }

        public double SiteLongitude
        {
            get
            {
                double longitude = starbook.GetPlace().Longitude.Value;
                traceLogger.LogMessage("SiteLongitude Get", longitude.ToString());
                return longitude;
            }
            set
            {
                Starbook.Place place = starbook.GetPlace();
                place.Longitude = new Starbook.DMS(Math.Abs(value), value >= 0 ? Starbook.Direction.East : Starbook.Direction.West);
                Starbook.Response response = starbook.SetPlace(place);
                traceLogger.LogMessage("SiteLongitude Set", string.Format("{0}: {1}", value, response));
            }
        }

        public short SlewSettleTime
        {
            get
            {
                traceLogger.LogMessage("SlewSettleTime Get", slewSettleTime.ToString());
                return slewSettleTime;
            }
            set
            {
                traceLogger.LogMessage("SlewSettleTime Set", value.ToString());
                slewSettleTime = value;
            }
        }

        public void SlewToAltAz(double Azimuth, double Altitude)
        {
            traceLogger.LogMessage("SlewToAltAz", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToAltAz");
        }

        public void SlewToAltAzAsync(double Azimuth, double Altitude)
        {
            traceLogger.LogMessage("SlewToAltAzAsync", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToAltAzAsync");
        }

        public void SlewToCoordinates(double RightAscension, double Declination)
        {
            Starbook.HMS rightAscension = new Starbook.HMS(targetRightAscension = RightAscension);
            Starbook.DMS declination = new Starbook.DMS(targetDeclination = Declination);
            Starbook.Response response = starbook.Goto(rightAscension, declination);

            if (slewSettleTime > 0)
            {
                Thread.Sleep(slewSettleTime * 1000);
            }

            if (response == Starbook.Response.OK)
            {
                while (true)
                {
                    Starbook.Status status = starbook.GetStatus();

                    if (!status.Goto)
                    {
                        break;
                    }

                    Thread.Sleep(100);
                }
            }

            traceLogger.LogMessage("SlewToCoordinates", response.ToString());
        }

        public void SlewToCoordinatesAsync(double RightAscension, double Declination)
        {
            Starbook.HMS rightAscension = new Starbook.HMS(targetRightAscension = RightAscension);
            Starbook.DMS declination = new Starbook.DMS(targetDeclination = Declination);
            Starbook.Response response = starbook.Goto(rightAscension, declination);
            traceLogger.LogMessage("SlewToCoordinatesAsync", response.ToString());
        }

        public void SlewToTarget()
        {
            if (double.IsNaN(targetRightAscension) || double.IsNaN(targetDeclination))
            {
                throw new ASCOM.InvalidOperationException("SlewToTarget: Target RightAscension/Declination is not set.");
            }

            Starbook.HMS rightAscension = new Starbook.HMS(targetRightAscension);
            Starbook.DMS declination = new Starbook.DMS(targetDeclination);
            Starbook.Response response = starbook.Goto(rightAscension, declination);

            if (slewSettleTime > 0)
            {
                Thread.Sleep(slewSettleTime * 1000);
            }

            if (response == Starbook.Response.OK)
            {
                while (true)
                {
                    Starbook.Status status = starbook.GetStatus();

                    if (!status.Goto)
                    {
                        break;
                    }

                    Thread.Sleep(100);
                }
            }

            traceLogger.LogMessage("SlewToTarget", response.ToString());
        }

        public void SlewToTargetAsync()
        {
            if (double.IsNaN(targetRightAscension) || double.IsNaN(targetDeclination))
            {
                throw new ASCOM.InvalidOperationException("SlewToTargetAsync: Target RightAscension/Declination is not set.");
            }

            Starbook.HMS rightAscension = new Starbook.HMS(targetRightAscension);
            Starbook.DMS declination = new Starbook.DMS(targetDeclination);
            Starbook.Response response = starbook.Goto(rightAscension, declination);
            traceLogger.LogMessage("SlewToTargetAsync", response.ToString());
        }

        public bool Slewing
        {
            get
            {
                bool qoto = starbook.GetStatus().Goto;
                traceLogger.LogMessage("Slewing Get", qoto.ToString());
                return qoto;
            }
        }

        public void SyncToAltAz(double Azimuth, double Altitude)
        {
            traceLogger.LogMessage("SyncToAltAz", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SyncToAltAz");
        }

        public void SyncToCoordinates(double RightAscension, double Declination)
        {
            Starbook.HMS rightAscension = new Starbook.HMS(targetRightAscension = RightAscension);
            Starbook.DMS declination = new Starbook.DMS(targetDeclination = Declination);
            Starbook.Response response = starbook.Align(rightAscension, declination);
            traceLogger.LogMessage("SyncToCoordinates", response.ToString());
        }

        public void SyncToTarget()
        {
            if (double.IsNaN(targetRightAscension) || double.IsNaN(targetDeclination))
            {
                throw new ASCOM.InvalidOperationException("SyncToTarget: Target RightAscension/Declination is not set.");
            }

            Starbook.HMS rightAscension = new Starbook.HMS(targetRightAscension);
            Starbook.DMS declination = new Starbook.DMS(targetDeclination);
            Starbook.Response response = starbook.Align(rightAscension, declination);
            traceLogger.LogMessage("SyncToTarget", response.ToString());
        }

        public double TargetDeclination
        {
            get
            {
                traceLogger.LogMessage("TargetDeclination Get", double.IsNaN(targetDeclination) ? "NaN" : targetDeclination.ToString());
                return targetDeclination;
            }
            set
            {
                traceLogger.LogMessage("TargetDeclination Set", value.ToString());
                targetDeclination = value;
            }
        }

        public double TargetRightAscension
        {
            get
            {
                traceLogger.LogMessage("TargetRightAscension Get", double.IsNaN(targetRightAscension) ? "NaN" : targetRightAscension.ToString());
                return targetRightAscension;
            }
            set
            {
                traceLogger.LogMessage("TargetRightAscension Set", value.ToString());
                targetRightAscension = value;
            }
        }

        public bool Tracking
        {
            get
            {
                Starbook.State state = starbook.GetStatus().State;
                bool tracking = (state == Starbook.State.Guide || state == Starbook.State.Scope || state == Starbook.State.Chart || state == Starbook.State.User);
                traceLogger.LogMessage("Tracking", "Get - " + tracking.ToString());
                return tracking;
            }
            set
            {
                Starbook.Response response = starbook.Start();
                traceLogger.LogMessage("Tracking Set", response.ToString());
            }
        }

        public DriveRates TrackingRate
        {
            get
            {
                DriveRates trackingRate = DriveRates.driveSidereal;
                traceLogger.LogMessage("TrackingRate Get", trackingRate.ToString());
                return trackingRate;
            }
            set
            {
                traceLogger.LogMessage("TrackingRate Set", value.ToString());
                if (value != DriveRates.driveSidereal)
                {
                    throw new ASCOM.MethodNotImplementedException("TrackingRate");
                }
            }
        }

        public ITrackingRates TrackingRates
        {
            get
            {
                ITrackingRates trackingRates = new TrackingRates();
                traceLogger.LogMessage("TrackingRates", "Get - ");
                foreach (DriveRates driveRate in trackingRates)
                {
                    traceLogger.LogMessage("TrackingRates", "Get - " + driveRate.ToString());
                }
                return trackingRates;
            }
        }

        public DateTime UTCDate
        {
            get
            {
                DateTime utcDate = starbook.GetTime().ToUniversalTime();
                traceLogger.LogMessage("TrackingRates", "Get - " + String.Format("MM/dd/yy HH:mm:ss", utcDate));
                return utcDate;
            }
            set
            {
                Starbook.Response response = starbook.SetTime(value.ToLocalTime());
                traceLogger.LogMessage("UTCDate Set", response.ToString());
            }
        }

        public void Unpark()
        {
            traceLogger.LogMessage("Unpark", "");
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

                if (traceLogger.Enabled)
                {
                    traceLogger.LogFilePath = "D:\\";
                }
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
