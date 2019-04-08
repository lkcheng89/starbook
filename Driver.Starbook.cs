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

using ASCOM;
using ASCOM.Astrometry;
using ASCOM.Astrometry.AstroUtils;
using ASCOM.Utilities;
using ASCOM.DeviceInterface;
using System.Threading;

namespace ASCOM.Starbook
{
    public partial class Telescope : ITelescopeV3
    {
        [ComVisible(false)]
        public class Starbook
        {
            #region Inner Enum & Struct

            [ComVisible(false)]
            public enum State
            {
                Init, Guide, Scope, Chart, User, Unknown
            }

            [ComVisible(false)]
            public enum Response
            {
                OK, ErrorIllegalState, ErrorFormat, ErrorBelowHorizon, ErrorUnknown
            }

            [ComVisible(false)]
            public enum Direction
            {
                Unknown, Positive, Negative, North, South, East, West
            }

            [ComVisible(false)]
            public struct HMS
            {
                public HMS(int hour, int minute, int second)
                    : this()
                {
                    this.Hour = hour;
                    this.Minute = minute;
                    this.Second = second;
                }

                public HMS(double value)
                    : this()
                {
                    this.Hour = (int)Math.Floor(value); value -= this.Hour; value *= 60;
                    this.Minute = (int)Math.Floor(value); value -= this.Minute; value *= 60;
                    this.Second = (int)Math.Floor(value);
                }

                public HMS(string s)
                    : this()
                {
                    Match match = Regex.Match(s, @"(?<Hour>\d+)\+(?<Minute>\d+(\.\d+)?)");

                    if (match.Success)
                    {
                        this.Hour = int.Parse(match.Groups["Hour"].Value);

                        double minute = double.Parse(match.Groups["Minute"].Value);

                        this.Minute = (int)Math.Floor(minute);
                        this.Second = (int)Math.Floor((minute - this.Minute) * 60 + 0.5);
                    }
                    else
                    {
                        this.Hour = 0;
                        this.Minute = 0;
                        this.Second = 0;
                    }
                }

                public int Hour { get; set; }
                public int Minute { get; set; }
                public int Second { get; set; }

                public double Value
                {
                    get
                    {
                        return this.Hour + this.Minute / 60.0 + this.Second / 3600.0;
                    }
                }

                public override string ToString()
                {
                    return string.Format("{0:00}+{1:00.0}", this.Hour, this.Minute + this.Second / 60.0);
                }
            }

            [ComVisible(false)]
            public struct DMS
            {
                public DMS(int degree, int minute, int second, Direction direction = Direction.Unknown)
                    : this()
                {
                    this.Degree = degree;
                    this.Minute = minute;
                    this.Second = second;

                    this.Direction = direction;
                }

                public DMS(double value, Direction direction = Direction.Unknown)
                    : this()
                {
                    bool negative;

                    if (value < 0)
                    {
                        negative = true; value = -value;
                    }
                    else
                    {
                        negative = false;
                    }

                    this.Degree = (int)Math.Floor(value); value -= this.Degree; value *= 60;
                    this.Minute = (int)Math.Floor(value); value -= this.Minute; value *= 60;
                    this.Second = (int)Math.Floor(value + 0.5);

                    if (direction == Direction.Unknown)
                    {
                        direction = negative ? Direction.Negative : Direction.Positive;
                    }

                    this.Direction = direction;
                }

                public DMS(string s)
                    : this()
                {
                    Match match = Regex.Match(s, @"(?<Position>\w)?(?<Negative>-)?(?<Degree>\d+)\+(?<Minute>\d+)");

                    if (match.Success)
                    {
                        this.Degree = int.Parse(match.Groups["Degree"].Value);
                        this.Minute = int.Parse(match.Groups["Minute"].Value);
                        this.Second = 0;

                        Group position = match.Groups["Position"];

                        if (position.Success)
                        {
                            switch (position.Value)
                            {
                                case "N":
                                    this.Direction = Direction.North; break;
                                case "S":
                                    this.Direction = Direction.South; break;
                                case "E":
                                    this.Direction = Direction.East; break;
                                case "W":
                                    this.Direction = Direction.West; break;
                                default:
                                    this.Direction = Direction.Unknown; break;
                            }
                        }
                        else
                        {
                            Group negative = match.Groups["Negative"];

                            if (negative.Success)
                            {
                                this.Direction = Direction.Negative;
                            }
                            else
                            {
                                this.Direction = Direction.Positive;
                            }
                        }
                    }
                    else
                    {
                        this.Degree = 0;
                        this.Minute = 0;
                        this.Second = 0;

                        this.Direction = Direction.Unknown;
                    }
                }

                public int Degree { get; set; }
                public int Minute { get; set; }
                public int Second { get; set; }

                public Direction Direction { get; set; }

                public double Value
                {
                    get
                    {
                        double value = this.Degree + this.Minute / 60.0 + this.Second / 3600.0;

                        if (this.Direction == Direction.Negative || this.Direction == Direction.South || this.Direction == Direction.West)
                        {
                            value = -value;
                        }

                        return value;
                    }
                }

                public override string ToString()
                {
                    string direction;

                    switch (this.Direction)
                    {
                        case Direction.Positive:
                        default:
                            direction = string.Empty; break;
                        case Direction.Negative:
                            direction = "-"; break;
                        case Direction.North:
                            direction = "N"; break;
                        case Direction.South:
                            direction = "S"; break;
                        case Direction.East:
                            direction = "E"; break;
                        case Direction.West:
                            direction = "W"; break;
                    }

                    return string.Format("{0}{1:00}+{2:00}", direction, this.Degree, this.Minute);
                }
            }

            [ComVisible(false)]
            public struct Status
            {
                public HMS RA { get; set; }
                public DMS Dec { get; set; }
                public State State { get; set; }
                public bool Goto { get; set; }
            }

            [ComVisible(false)]
            public struct Place
            {
                public DMS Latitude { get; set; }
                public DMS Longitude { get; set; }
                public int Timezone { get; set; }
            }

            [ComVisible(false)]
            public struct XY
            {
                public int X { get; set; }
                public int Y { get; set; }
            }

            #endregion

            WebClient web;

            public Starbook(IPAddress ipAddress)
            {
                this.IPAddress = ipAddress;

                this.web = new WebClient();
            }

            public Starbook(string ipAddress)
                : this(IPAddress.Parse(ipAddress))
            {

            }

            public IPAddress IPAddress { get; set; }

            public string Version
            {
                get
                {
                    string version;

                    Dictionary<string, string> dictionary = this.HandshakeDictionary("VERSION");

                    if (!dictionary.TryGetValue("version", out version))
                    {
                        version = string.Empty;
                    }

                    return version;
                }
            }

            public Status GetStatus()
            {
                Status status = new Status();

                Dictionary<string, string> dictionary = this.HandshakeDictionary("GETSTATUS");

                foreach (KeyValuePair<string, string> item in dictionary)
                {
                    switch (item.Key)
                    {
                        case "RA":
                            status.RA = new HMS(item.Value); break;
                        case "DEC":
                            status.Dec = new DMS(item.Value); break;
                        case "STATE":
                            status.State = this.ParseState(item.Value); break;
                        case "GOTO":
                            status.Goto = item.Value == "1"; break;
                    }
                }

                return status;
            }

            public DateTime GetTime()
            {
                string timeString; DateTime time;

                Dictionary<string, string> dictionary = this.HandshakeDictionary("GETTIME");

                if (!dictionary.TryGetValue("time", out timeString) || !DateTime.TryParseExact(timeString, "yyyy+MM+dd+HH+mm+ss", null, DateTimeStyles.None, out time))
                {
                    time = DateTime.MinValue;
                }

                return time;
            }

            public Response SetTime(DateTime time)
            {
                return this.Handshake(string.Format("SETTIME?TIME={0:yyyy+MM+dd+HH+mm+ss}", time));
            }

            public Place GetPlace()
            {
                Place place = new Place();

                foreach (KeyValuePair<string, string> item in this.HandshakeDictionary("GETPLACE"))
                {
                    switch (item.Key)
                    {
                        case "latitude":
                            place.Latitude = new DMS(item.Value); break;
                        case "longitude":
                            place.Longitude = new DMS(item.Value); break;
                        case "timezone":
                            place.Timezone = int.Parse(item.Value); break;
                    }
                }

                return place;
            }

            public Response SetPlace(Place place)
            {
                return this.Handshake(string.Format("SETPLACE?LATITUDE={0}&LONGITUDE={1}&TIMEZONE={2:00}", place.Latitude, place.Longitude, place.Timezone));
            }

            public Response Home()
            {
                return this.Handshake("GOHOME?HOME=0");
            }

            public Response Move(Direction direction)
            {
                switch (direction)
                {
                    case Direction.North:
                        return this.Handshake("MOVE?NORTH=1&SOUTH=0&EAST=0&WEST=0");
                    case Direction.South:
                        return this.Handshake("MOVE?NORTH=0&SOUTH=1&EAST=0&WEST=0");
                    case Direction.East:
                        return this.Handshake("MOVE?NORTH=0&SOUTH=0&EAST=1&WEST=0");
                    case Direction.West:
                        return this.Handshake("MOVE?NORTH=0&SOUTH=0&EAST=0&WEST=1");
                    default:
                        return Response.ErrorFormat;
                }
            }

            public Response NoMove()
            {
                return this.Handshake("MOVE?NORTH=0&SOUTH=0&EAST=0&WEST=0");
            }

            public Response Goto(HMS ra, DMS dec)
            {
                return this.Handshake(string.Format("GOTORADEC?RA={0}&DEC={1}", ra, dec));
            }

            // Stop the mount (e.g. during Move/Goto)

            public Response Stop()
            {
                return this.Handshake("STOP");
            }

            // Hibernate the Mount. Use Start() to Restart.

            public Response Reset()
            {
                return this.Handshake("RESET?RESET");
            }

            // Set/Reset the StarBook in SCOPE Mode

            public Response Start()
            {
                return this.Handshake("START");
            }

            public Response Align(HMS ra, DMS dec)
            {
                return this.Handshake(string.Format("ALIGN?RA={0}&DEC={1}", ra, dec));
            }

            public Response SetSpeed(int speed)
            {
                return this.Handshake(string.Format("SETSPEED?SPEED={0}", speed));
            }

            public int GetRound()
            {
                int round;

                if (!int.TryParse(this.HandshakeString("GETROUND"), out round))
                {
                    round = 0;
                }

                return round;
            }

            public XY GetXY()
            {
                XY xy = new XY();

                foreach (KeyValuePair<string, string> item in this.HandshakeDictionary("GETXY"))
                {
                    switch (item.Key)
                    {
                        case "X":
                            xy.X = int.Parse(item.Value); break;
                        case "Y":
                            xy.Y = int.Parse(item.Value); break;
                    }
                }

                return xy;
            }

            public string Command(string command)
            {
                return this.HandshakeString(command);
            }

            private Response Handshake(string command)
            {
                return ParseResponse(this.HandshakeString(command));
            }

            private string HandshakeString(string command)
            {
                string response;

                try
                {
                    response = this.web.DownloadString(string.Format("http://{0}/{1}", this.IPAddress, command));
                }
                catch
                {
                    response = string.Empty;
                }

                Match match = Regex.Match(response, @"<!--(?<Response>.*)-->");

                if (match.Success)
                {
                    response = match.Groups["Response"].Value;
                }
                else
                {
                    response = string.Empty;
                }

                return response;
            }

            private Dictionary<string, string> HandshakeDictionary(string command)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>();

                MatchCollection matches = Regex.Matches(this.HandshakeString(command), @"(?<Name>[^&=]+)=(?<Value>[^&]+)");

                foreach (Match natch in matches)
                {
                    string name = natch.Groups["Name"].Value;
                    string value = natch.Groups["Value"].Value;

                    dictionary[name] = value;
                }

                return dictionary;
            }

            private Response ParseResponse(string response)
            {
                switch (response)
                {
                    case "OK":
                        return Response.OK;
                    case "ERROR:ILLEGAL STATE":
                        return Response.ErrorIllegalState;
                    case "ERROR:FORMAT":
                        return Response.ErrorFormat;
                    case "ERROR:BELOW HORIZONE":
                        return Response.ErrorBelowHorizon;
                    default:
                        return Response.ErrorUnknown;
                }
            }

            private State ParseState(string state)
            {
                switch (state)
                {
                    case "INIT":
                        return State.Init;
                    case "GUIDE":
                        return State.Guide;
                    case "SCOPE":
                        return State.Scope;
                    case "CHART":
                        return State.Chart;
                    case "USER":
                        return State.User;
                    default:
                        return State.Unknown;
                }
            }
        }
    }
}
