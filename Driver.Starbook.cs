#define Telescope

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
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
    public partial class Telescope : ITelescopeV3
    {
        [ComVisible(false)]
        public class Starbook
        {
            WebClientTimeout web;

            public Starbook()
            {
                this.web = new WebClientTimeout();
                this.web.Timeout = 5000;

                this.IPAddress = IPAddress.Parse("169.254.1.1");
            }

            /// <summary>
            /// IP address of the starbook controller (Default is 169.254.1.1)
            /// </summary>
            /// 
            public IPAddress IPAddress { get; set; }

            /// <summary>
            /// Get firmware version of the starbook controller.
            /// </summary>
            /// <returns>Firmware version which is formatted as [MAJORVERSION].[MINORVERSION]B[BUILDNUMBER]</returns>
            /// 
            public string Version
            {
                get
                {
                    Dictionary<string, string> dictionary = this.HandshakeDictionary("VERSION");

                    if (!dictionary.TryGetValue("VERSION", out string version))
                    {
                        version = string.Empty;
                    }

                    return version;
                }
            }

            /// <summary>
            /// Get status of the mount.
            /// </summary>
            /// <param name="status">
            /// Status which contains the following fields:
            /// [*] RA: Right Ascension which is formatted as [HH]+[MM.M]
            /// [*] DEC: Declination which is formatted as [S][DDD]+[MM]
            /// [*] STATE: State of starbook
            ///     [+] INIT (Power Up)
            ///     [+] GUIDE (?)
            ///     [+] SCOPE (User Presses OK or START)
            ///     [+] CHART (User Presses CHART)
            ///     [+] USER (User Presses MENU)
            /// [*] GOTO: Slewing (1) or Not Slewing (0)
            /// </param>
            /// <returns>Response string: OK or ERROR:%</returns>
            /// 
            public Response GetStatus(out Status status)
            {
                status = new Status(); bool statusRA = false, statusDEC = false, statusState = false, statusGoto = false;

                foreach (KeyValuePair<string, string> item in this.HandshakeDictionary("GETSTATUS"))
                {
                    switch (item.Key)
                    {
                        case "RA":
                        {
                            if (HMS.TryParse(item.Value, out HMS ra))
                            {
                                status.RA = ra; statusRA = true;
                            }
                            break;
                        }
                        case "DEC":
                        {
                            if (DMS.TryParse(item.Value, out DMS dec))
                            {
                                status.Dec = dec; statusDEC = true;
                            }
                            break;
                        }
                        case "STATE":
                        {
                            if (this.TryParseState(item.Value, out State state))
                            {
                                status.State = state; statusState = true;
                            }
                            break;
                        }
                        case "GOTO":
                        {
                            if (item.Value == "1")
                            {
                                status.Goto = true; statusGoto = true;
                            }
                            else if (item.Value == "0")
                            {
                                status.Goto = false; statusGoto = true;
                            }
                            break;
                        }
                    }
                }

                return (statusRA && statusDEC && statusState && statusGoto) ? Response.OK : Response.ErrorUnknown;
            }

            /// <summary>
            /// Get time of the mount.
            /// </summary>
            /// <param name="time">Time which is formatted as [YYYY]+[MM]+[DD]+[HH]+[MM]+[SS] (Local Time)</param>
            /// <returns>Response string: OK or ERROR:%</returns>
            /// 
            public Response GetTime(out DateTime time)
            {
                time = DateTime.MinValue; Dictionary<string, string> dictionary = this.HandshakeDictionary("GETTIME");

                if (!dictionary.TryGetValue("TIME", out string s) || !DateTime.TryParseExact(s, "yyyy+MM+dd+HH+mm+ss", null, DateTimeStyles.None, out time))
                {
                    return Response.ErrorUnknown;
                }

                return Response.OK;
            }

            /// <summary>
            /// Set time of the mount. [*NOTE*] This command is valid in INIT state only.
            /// </summary>
            /// <param name="time">Time which is formatted as [YYYY]+[MM]+[DD]+[HH]+[MM]+[SS] (Local Time)</param>
            /// <returns>Response string: OK or ERROR:%</returns>
            /// 
            public Response SetTime(DateTime time)
            {
                return this.Handshake(string.Format("SETTIME?TIME={0:yyyy+MM+dd+HH+mm+ss}", time));
            }

            /// <summary>
            /// Get place of the mount.
            /// </summary>
            /// <param name="place">
            /// Place contains the following fields:
            /// [*] LATITUDE: Latitude which is formatted as [N|S][DDD]+[MM]
            /// [*] LONGITUDE: Longitude which is formatted as [E|W][DDD]+[MM]
            /// [*] TIMEZONE: Timezone which is ranged from [-12] to [14]
            /// </param>
            /// <returns>Response string: OK or ERROR:%</returns>
            /// 
            public Response GetPlace(out Place place)
            {
                place = new Place(); bool placeLatitude = false, placeLongitude = false, placeTimezone = false;

                foreach (KeyValuePair<string, string> item in this.HandshakeDictionary("GETPLACE"))
                {
                    switch (item.Key)
                    {
                        case "LATITUDE":
                        {
                            if (DMS.TryParse(item.Value, out DMS latitude))
                            {
                                place.Latitude = latitude; placeLatitude = true;
                            }
                            break;
                        }
                        case "LONGITUDE":
                        {
                            if (DMS.TryParse(item.Value, out DMS longitude))
                            {
                                place.Longitude = longitude; placeLongitude = true;
                            }
                            break;
                        }
                        case "TIMEZONE":
                        {
                            if (int.TryParse(item.Value, out int timezone))
                            {
                                place.Timezone = timezone; placeTimezone = true;
                            }
                            break;
                        }
                    }
                }

                return (placeLatitude && placeLongitude && placeTimezone) ? Response.OK : Response.ErrorUnknown;
            }

            /// <summary>
            /// Set place of the mount. [*NOTE*] This command is valid in INIT state only.
            /// </summary>
            /// <param name="place">Place structure which is described above</param>
            /// <returns>Response string: OK or ERROR:%</returns>
            /// 
            public Response SetPlace(Place place)
            {
                return this.Handshake(string.Format("SETPLACE?LATITUDE={0}&LONGITUDE={1}&TIMEZONE={2:00}", place.Latitude, place.Longitude, place.Timezone));
            }

            /// <summary>
            /// Move the mount to home position. [*NOTE*] This command is invalid in INIT state.
            /// </summary>
            /// <returns>Response string: OK or ERROR:%</returns>
            /// 
            public Response Home()
            {
                return this.Handshake("GOHOME?HOME=0");
            }

            /// <summary>
            /// Move the mount in one of four directions continuously. [*NOTE*] This command is invalid in INIT state.
            /// </summary>
            /// <param name="direction">Direction to move (NORTH, SOUTH, WEST, EAST)</param>
            /// <returns>Response string: OK or ERROR:%</returns>
            /// 
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

            /// <summary>
            /// Stop moving the mount. [*NOTE*] This command is invalid in INIT state.
            /// </summary>
            /// <returns>Response string: OK or ERROR:%</returns>
            /// 
            public Response NoMove()
            {
                return this.Handshake("MOVE?NORTH=0&SOUTH=0&EAST=0&WEST=0");
            }

            /// <summary>
            /// Move the mount to specitfied celestial coordinate. [*NOTE*] This command is invalid in INIT state.
            /// </summary>
            /// <param name="ra">Right Ascension which is formatted as [HH]+[MM.M]</param>
            /// <param name="dec">Declination which is formatted as [S][DDD]+[MM]</param>
            /// <returns>Response string: OK or ERROR:%</returns>
            /// 
            public Response Goto(HMS ra, DMS dec)
            {
                return this.Handshake(string.Format("GOTORADEC?RA={0}&DEC={1}", ra, dec));
            }

            /// <summary>
            /// Stop moving the mount. This forces to stop the mount in MOVE/GOTO state, but NoMove() is also required when the mount
            /// is in GOTO state. [*NOTE*] This command is invalid in INIT state.
            /// </summary>
            /// <returns>Response string: OK or ERROR:%</returns>
            /// 
            public Response Stop()
            {
                return this.Handshake("STOP");
            }

            /// <summary>
            /// Hibernate the mount. Use Start() to restart the mount.
            /// </summary>
            /// <returns>Response string: OK or ERROR:%</returns>
            /// 
            public Response Reset()
            {
                return this.Handshake("RESET?RESET");
            }

            /// <summary>
            /// Start/Restart the mount to let it transit from INIT to SCOPE state.
            /// </summary>
            /// <returns>Response string: OK or ERROR:%</returns>
            /// 
            public Response Start()
            {
                return this.Handshake("START");
            }

            /// <summary>
            /// Align/Sync the mount with the specified celesctial coordinate. [*NOTE*] This command is invalid in INIT state.
            /// </summary>
            /// <param name="ra">Right Ascension which is formatted as [HH]+[MM.M]</param>
            /// <param name="dec">Declination which is formatted as [S][DDD]+[MM]</param>
            /// <returns>Response string: OK or ERROR:%</returns>
            public Response Align(HMS ra, DMS dec)
            {
                return this.Handshake(string.Format("ALIGN?RA={0}&DEC={1}", ra, dec));
            }

            /// <summary>
            /// Set the speed of mount. This affects the behaviour of MOVE()
            /// </summary>
            /// <param name="speed">Speed which is ranged from [0] to [8]</param>
            /// <returns>Response string: OK or ERROR:%</returns>
            public Response SetSpeed(int speed)
            {
                return this.Handshake(string.Format("SETSPEED?SPEED={0}", speed));
            }

            /// <summary>
            /// Get granularity of encoded value of the mount.
            /// </summary>
            /// <returns>Granularity of encoded value</returns>
            /// 
            public int GetRound()
            {
                int round;

                if (!int.TryParse(this.HandshakeString("GETROUND"), out round))
                {
                    round = 0;
                }

                return round;
            }

            /// <summary>
            /// Get encoded value of X/Y coordinate of the mount.
            /// </summary>
            /// <returns>Encoded value of X/Y coordinate</returns>
            /// 
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

            /// <summary>
            /// Save setting of the mount.
            /// </summary>
            /// <returns>Response string: OK or ERROR:%</returns>
            /// 
            public Response Save()
            {
                return this.Handshake("SAVESETTING");
            }

            /// <summary>
            /// Get screenshot of starbook controller.
            /// </summary>
            /// <param name="width">Width of sceenshot (Default is 160 pixels)</param>
            /// <param name="height">Height of sceenshot (Default is 160 pixels)</param>
            /// <param name="color">Color or monochrome (Default is monochrome)</param>
            /// <returns>Screenshot as Bitmap object</returns>
            /// 
            public Bitmap Screenshot(int width = 160, int height = 160, bool color = false)
            {
                Bitmap screenshot = new Bitmap(width, height);

                byte[] bytes = this.HandshakeBytes("GETSCREEN.BIN", false);
                int byteCount = (width * height * 12 + 7) / 8;

                if (bytes.Length == byteCount)
                {
                    byte[][] screenshotBytes = new byte[height][];
                    int screenshotBytesStride = width * 3;

                    for (int y = 0; y < height; y++)
                    {
                        screenshotBytes[y] = new byte[screenshotBytesStride];
                    }

                    if (color)
                    {
                        for (int index = 0, x = 0, y = 0, z = 0; index < bytes.Length - 2; )
                        {
                            byte byte1 = bytes[index++];
                            byte byte2 = bytes[index++];
                            byte byte3 = bytes[index++];

                            screenshotBytes[y][z++] = (byte)((byte2 & 0x0F) << 4);
                            screenshotBytes[y][z++] = (byte)((byte1 & 0xF0) << 0);
                            screenshotBytes[y][z++] = (byte)((byte1 & 0x0F) << 4); if (++x >= width) { if (++y >= height) break; x = z = 0; }

                            screenshotBytes[y][z++] = (byte)((byte3 & 0xF0) << 0);
                            screenshotBytes[y][z++] = (byte)((byte3 & 0x0F) << 4);
                            screenshotBytes[y][z++] = (byte)((byte2 & 0xF0) << 0); if (++x >= width) { if (++y >= height) break; x = z = 0; }
                        }
                    }
                    else
                    {
                        for (int index = 0, x = 0, y = 0, z = 0; index < bytes.Length; index++)
                        {
                            byte byte1 = (byte)((bytes[index] & 0x0F) << 4);
                            byte byte2 = (byte)((bytes[index] & 0xF0) << 0);

                            screenshotBytes[y][z++] = byte1;
                            screenshotBytes[y][z++] = byte1;
                            screenshotBytes[y][z++] = byte1; if (++x >= width) { if (++y >= height) break; x = z = 0; }

                            screenshotBytes[y][z++] = byte2;
                            screenshotBytes[y][z++] = byte2;
                            screenshotBytes[y][z++] = byte2; if (++x >= width) { if (++y >= height) break; x = z = 0; }
                        }
                    }

                    BitmapData screenshotData = screenshot.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                    if (screenshotData != null)
                    {
                        IntPtr screenshotDataScan = screenshotData.Scan0;
                        int screenshotDataStride = screenshotData.Stride;

                        for (int y = 0; y < height; y++)
                        {
                            Marshal.Copy(screenshotBytes[y], 0, screenshotDataScan, screenshotBytesStride);
                            screenshotDataScan = IntPtr.Add(screenshotDataScan, screenshotDataStride);
                        }

                        screenshot.UnlockBits(screenshotData);
                        screenshot.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    }
                }

                return screenshot;
            }

            /// <summary>
            /// Issue a low-level command defined by starbook controller.
            /// </summary>
            /// <param name="command">Command string</param>
            /// <returns>Response string</returns>
            /// 
            public string Command(string command)
            {
                return this.HandshakeString(command);
            }

            #region Private Methods

            private Response Handshake(string command)
            {
                return ParseResponse(this.HandshakeString(command));
            }

            private string HandshakeString(string command)
            {
                string strinq;
#if true

                try
                {
                    strinq = this.web.DownloadString(string.Format("http://{0}/{1}", this.IPAddress, command));
                }
                catch
                {
                    strinq = string.Empty;
                }
#else
                Dictionary<string, string> handshakes = new Dictionary<string, string>();

                handshakes["VERSION"] = "VERSION=2.7B50";
                handshakes["GETSTATUS"] = "RA=0+0.0&DEC=0+0&GOTO=0&STATE=INIT";
                handshakes["GETPLACE"] = "LATITUDE=N23+30&LONGITUDE=E120+00&TIMEZONE=8";
                handshakes["GETTIME"] = string.Format("TIME={0:yyyy+MM+dd+HH+mm+ss}", DateTime.Now);
                handshakes["GETROUND"] = "4320000";
                handshakes["GETXY"] = "X=0&Y=0";
                handshakes["SETPLACE"] = "OK";
                handshakes["SETTIME"] = "OK";
                handshakes["SETSPEED"] = "OK";
                handshakes["SAVESETTING"] = "OK";
                handshakes["START"] = "OK";
                handshakes["RESET"] = "OK";
                handshakes["GOHOME"] = "OK";
                handshakes["MOVE"] = "OK";
                handshakes["GOTORADEC"] = "OK";
                handshakes["ALIGN"] = "OK";

                int index = command.IndexOf('?');

                if (index >= 0)
                {
                    command = command.Substring(0, index);
                }

                if (handshakes.TryGetValue(command, out strinq))
                {
                    strinq = string.Format("<!--{0}-->", strinq);
                }
                else
                {
                    strinq = string.Empty;
                }
#endif

                Match match = Regex.Match(strinq, @"<!--(?<String>.*)-->");

                if (match.Success)
                {
                    strinq = match.Groups["String"].Value;
                }
                else
                {
                    strinq = string.Empty;
                }

                return strinq;
            }

            private Dictionary<string, string> HandshakeDictionary(string command, Dictionary<string, string> dictionary = null)
            {
                if (dictionary == null)
                {
                    dictionary = new Dictionary<string, string>();
                }
                else
                {
                    dictionary.Clear();
                }

                MatchCollection matches = Regex.Matches(this.HandshakeString(command), @"(?<Name>[^&=]+)=(?<Value>[^&]+)");

                foreach (Match natch in matches)
                {
                    string name = natch.Groups["Name"].Value.ToUpper();
                    string value = natch.Groups["Value"].Value;

                    dictionary[name] = value;
                }

                return dictionary;
            }

            private byte[] HandshakeBytes(string command, bool web = true)
            {
                byte[] bytes;

                try
                {
                    if (web)
                    {
                        bytes = this.web.DownloadData(string.Format("http://{0}/{1}", this.IPAddress, command));
                    }
                    else
                    {
                        using (TcpClient tcpClient = new TcpClient())
                        {
                            tcpClient.Connect(new IPEndPoint(this.IPAddress, 80));

                            using (NetworkStream networkStream = tcpClient.GetStream())
                            {
                                byte[] request = Encoding.UTF8.GetBytes(string.Format("GET /{0} HTTP/1.1\r\n\r\n", command));

                                networkStream.Write(request, 0, request.Length);
                                networkStream.Flush();

                                using (MemoryStream memoryStream = new MemoryStream())
                                {
                                    byte[] packet = new byte[8192];

                                    while (true)
                                    {
                                        int read = networkStream.Read(packet, 0, packet.Length);

                                        if (read <= 0)
                                        {
                                            break;
                                        }

                                        memoryStream.Write(packet, 0, read);
                                    }

                                    bytes = memoryStream.ToArray();
                                }
                            }
                        }
                    }
                }
                catch
                {
                    bytes = new byte[0];
                }

                return bytes;
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

            private bool TryParseState(string s, out State state)
            {
                switch (s)
                {
                    case "INIT":
                        state = State.Init; return true;
                    case "GUIDE":
                        state = State.Guide; return true;
                    case "SCOPE":
                        state = State.Scope; return true;
                    case "CHART":
                        state = State.Chart; return true;
                    case "USER":
                        state = State.User; return true;
                    default:
                        state = State.Unknown; return false;
                }
            }

            #endregion

            #region Inner Enum & Struct & Class

            [ComVisible(false)]
            public enum State
            {
                Unknown, Init, Guide, Scope, Chart, User
            }

            [ComVisible(false)]
            public enum Response
            {
                OK, ErrorIllegalState, ErrorFormat, ErrorBelowHorizon, ErrorUnknown
            }

            [ComVisible(false)]
            public enum Direction
            {
                Unknown, North, South, East, West, Positive, Negative
            }

            [ComVisible(false)]
            public struct HMS
            {
                public HMS(int hour, int minute, double second) : this()
                {
                    this.Hour = hour;
                    this.Minute = minute;
                    this.Second = second;
                }

                public static bool FromValue(double value, out HMS hms)
                {
                    hms = new HMS();

                    if (double.IsNaN(value) || value < 0 || 24 <= value)
                    {
                        return false;
                    }

                    hms.Hour = (int)Math.Floor(value); value -= hms.Hour; value *= 60;
                    hms.Minute = (int)Math.Floor(value); value -= hms.Minute; value *= 60;
                    hms.Second = value;

                    return true;
                }

                public static bool TryParse(string s, out HMS hms)
                {
                    hms = new HMS(); Match match = Regex.Match(s, @"(?<Hour>\d+)\+(?<Minute>\d+(\.\d+)?)");

                    if (!match.Success || !int.TryParse(match.Groups["Hour"].Value, out int hour) || hour < 0 || 23 < hour ||
                                          !double.TryParse(match.Groups["Minute"].Value, out double minute) || minute < 0 || 60 <= minute)
                    {
                        return false;
                    }

                    hms.Hour = hour;
                    hms.Minute = (int)Math.Floor(minute);
                    hms.Second = (minute - hms.Minute) * 60;

                    return true;
                }

                public int Hour { get; private set; }
                public int Minute { get; private set; }
                public double Second { get; private set; }

                public double Value
                {
                    get
                    {
                        return this.Hour + this.Minute / 60.0 + this.Second / 3600.0;
                    }
                }

                public override string ToString()
                {
                    return string.Format("{0:00}+{1:00.0}", this.Hour, this.Minute + this.Second / 60.0 + 0.05);
                }
            }

            [ComVisible(false)]
            public struct DMS
            {
                public DMS(Direction direction, int degree, int minute, double second) : this()
                {
                    this.Direction = direction;

                    this.Degree = degree;
                    this.Minute = minute;
                    this.Second = second;
                }

                public static bool FromValue(double value, out DMS dms)
                {
                    return FromValue(value, out dms, Direction.Positive, Direction.Negative);
                }

                public static bool FromValue(double value, out DMS dms, Direction positive, Direction negative)
                {
                    dms = new DMS();

                    double minValue = -90;
                    double maxValue =  90;

                    if (positive == Direction.East && negative == Direction.West)
                    {
                        minValue = -180;
                        maxValue =  180;
                    }

                    if (double.IsNaN(value) || value < minValue || maxValue < value)
                    {
                        return false;
                    }

                    if (value >= 0)
                    {
                        dms.Direction = positive;
                    }
                    else
                    {
                        dms.Direction = negative; value = -value;
                    }

                    dms.Degree = (int)Math.Floor(value); value -= dms.Degree; value *= 60;
                    dms.Minute = (int)Math.Floor(value); value -= dms.Minute; value *= 60;
                    dms.Second = value;

                    return true;
                }

                public static bool TryParse(string s, out DMS dms)
                {
                    dms = new DMS(); Match match = Regex.Match(s, @"(?<Direction>[NSEW\-])?(?<Degree>\d+)\+(?<Minute>\d+(\.\d+)?)");

                    if (!match.Success)
                    {
                        return false;
                    }

                    Direction direction;
                    double minValue = -90;
                    double maxValue =  90;

                    Group group = match.Groups["Direction"];

                    if (group.Success)
                    {
                        switch (group.Value)
                        {
                            case "N":
                                direction = Direction.North; break;
                            case "S":
                                direction = Direction.South; break;
                            case "E":
                                direction = Direction.East; minValue = -180; maxValue = 180; break;
                            case "W":
                                direction = Direction.West; minValue = -180; maxValue = 180; break;
                            case "-":
                            default:
                                direction = Direction.Negative; break;
                        }
                    }
                    else
                    {
                        direction = Direction.Positive;
                    }

                    if (!int.TryParse(match.Groups["Degree"].Value, out int degree) || !double.TryParse(match.Groups["Minute"].Value, out double minute))
                    {
                        return false;
                    }

                    double value = degree + minute / 60;

                    if (value < minValue || maxValue < value)
                    {
                        return false;
                    }

                    dms.Direction = direction;

                    dms.Degree = degree;
                    dms.Minute = (int)Math.Floor(minute);
                    dms.Second = (minute - dms.Minute) * 60;

                    return true;
                }

                public Direction Direction { get; private set; }

                public int Degree { get; private set; }
                public int Minute { get; private set; }
                public double Second { get; private set; }

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
                        case Direction.North:
                            direction = "N"; break;
                        case Direction.South:
                            direction = "S"; break;
                        case Direction.East:
                            direction = "E"; break;
                        case Direction.West:
                            direction = "W"; break;
                        case Direction.Positive:
                        default:
                            direction = string.Empty; break;
                        case Direction.Negative:
                            direction = "-"; break;
                    }

                    return string.Format("{0}{1:000}+{2:00}", direction, this.Degree, this.Minute);
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

            [ComVisible(false)]
            private class WebClientTimeout : WebClient
            {
                public WebClientTimeout()
                {
                    this.Timeout = 0;
                }

                public int Timeout { get; set; }

                protected override WebRequest GetWebRequest(Uri uri)
                {
                    WebRequest webRequest = base.GetWebRequest(uri);

                    if (this.Timeout > 0)
                    {
                        webRequest.Timeout = this.Timeout;
                    }

                    return webRequest;
                }
            }

            #endregion
        }
    }
}
