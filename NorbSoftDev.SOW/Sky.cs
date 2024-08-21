//http://yaddb.blogspot.com/2013/01/how-to-calculate-sunrise-and-sunset.html
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
namespace NorbSoftDev.SOW
{
    public class Sky
    {
        public enum CalcMode
        {
            Sunset = 0, Sunrise = 1
        }


        // calculate the sunset or rise, enter date without time. 
        // mode determines wether to calc sunrise or set.
        // public static double Calculate( DateTime date, double longitude, double latitude, CalcMode mode )
        // {
        //     // add 12 hours to the date to determine the middle of the day
        //     date = date.AddHours( 12 );

        //     // equation of time (in minutes)
        //     var x = date.DayOfYear * 2 * Math.PI / 365; // fractional year in radians

        //     var eqtime = 229.18 * ( 0.000075 + 0.001868 * Math.Cos( x ) - 0.032077 * Math.Sin( x ) - 0.014615 * Math.Cos( 2 * x ) - 0.040849 * Math.Sin( 2 * x ) );

        //     // declination (in degrees)
        //     var declin = 0.006918 - 0.399912 * Math.Cos( x ) + 0.070257 * Math.Sin( x ) - 0.006758 * Math.Cos( 2 * x );
        //     declin = declin + 0.000907 * Math.Sin( 2 * x ) - 0.002697 * Math.Cos( 3 * x ) + 0.00148 * Math.Sin( 3 * x );
        //     declin = declin * 180 / Math.PI;

        //     // solar azimuth angle for sunrise and sunset corrected for atmospheric refraction (in degrees),
        //     x = Math.PI / 180;
        //     var hars = Math.Cos( x * 90.833 ) / ( Math.Cos( x * latitude ) * Math.Cos( x * declin ) );

        //     hars = hars - Math.Tan( x * latitude ) * Math.Tan( x * declin );
        //     hars = Math.Acos( hars ) / x;

        //     // get the local timezone in order to determine the daylighttime
        //     TimeZone localZone = TimeZone.CurrentTimeZone;

        //     // Create a DaylightTime object for the specified year.
        //     //disable, no daylith savings in 1860s!
        //     DaylightTime daylight = localZone.GetDaylightChanges( date.Year );

        //     double offset = localZone.IsDaylightSavingTime( date ) ? (double)daylight.Delta.Hours : 0;
        //     //double offset = 0;

        //     if ( mode == CalcMode.Sunset )
        //         x = 720 + 4 * ( longitude + hars ) - eqtime;
        //     else
        //         x = 720 + 4 * ( longitude - hars ) - eqtime;

        //     x = ( x / 60 ) + offset;

        //     return x;

        // }


        public static TimeSpan SunCalc( DateTime date, double latitude, CalcMode mode )
        {
            return TimeSpan.FromMinutes( Sky.SunCalc( date.Date.DayOfYear, latitude, mode ) );
        }




        /// <summary>
        /// Calculates sunrise or sunset in local time at a given latitude and day of year
        /// </summary>
        /// <returns>The minutes after midnight</returns>
        public static double SunCalc( int dayOfYear, double latitude, CalcMode mode )
        {

            // equation of time (in minutes)
            double x = dayOfYear * 2 * Math.PI / 365; // fractional year in radians

            double eqtime = 229.18 * ( 0.000075 + 0.001868 * Math.Cos( x ) - 0.032077 * Math.Sin( x ) - 0.014615 * Math.Cos( 2 * x ) - 0.040849 * Math.Sin( 2 * x ) );

            // declination (in degrees)
            double declin = 0.006918 - 0.399912 * Math.Cos( x ) + 0.070257 * Math.Sin( x ) - 0.006758 * Math.Cos( 2 * x );
            declin = declin + 0.000907 * Math.Sin( 2 * x ) - 0.002697 * Math.Cos( 3 * x ) + 0.00148 * Math.Sin( 3 * x );
            declin = declin * 180 / Math.PI;

            // solar azimuth angle for sunrise and sunset corrected for atmospheric refraction (in degrees),
            x = Math.PI / 180;

            double hars = Math.Cos( x * 90.833 ) / ( Math.Cos( x * latitude ) * Math.Cos( x * declin ) );

            hars = hars - Math.Tan( x * latitude ) * Math.Tan( x * declin );
            //limit to a resonable range, which includes artic circle
            hars = Math.Min(1, Math.Max(-1, hars));
            hars = Math.Acos( hars ) / x;


            double ret;
            double longitude = 0;
            if ( mode == CalcMode.Sunset )
                ret = 720 + 4 * ( longitude + hars ) - eqtime;
            else
                ret = 720 + 4 * ( longitude - hars ) - eqtime;

            // ret = ( ret / 60 );

            return ret;
        }

        public const string formatDate = "MM/dd/yyyy";
        public const string formatTime = "H:mm:ss";
        public const string formatShortTime = "H:mm";

        public const string formatTimeSpan = @"hh\:mm\:ss";
        public const string formatDateTime = formatDate + " " + formatTime;
        public const string formatDateTimePretty = "h:mm tt, MMMM d, yyyy";
        public const string formatTimePretty = "h:mm tt";

        public static string[] dateTimeFormats = new string[] {
            formatDateTime, formatDateTimePretty
        };

        public static DateTime ParseDateTime(string str)
        {
            //return DateTime.ParseExact(str, Sky.formatDateTime, CultureInfo.InvariantCulture);
        
            DateTime t = new DateTime();
            try { 
                try {
                   t = DateTime.ParseExact(str, formatTime, CultureInfo.InvariantCulture);
                } catch (System.FormatException e) {
                    t = DateTime.ParseExact(str, formatShortTime, CultureInfo.InvariantCulture);
                }
            } catch (System.FormatException ex) {
                Log.Error(str, "Unable to Parse DateTime \""+str+"\"");
                Log.Exception(str, ex);
                throw;
            }
            return t;
        }

        public static DateTime ParseDate(string str)
        {
            try {
                return DateTime.ParseExact(str, Sky.formatDate, CultureInfo.InvariantCulture);
            } catch (System.FormatException ex) {
                Log.Error(str, "Unable to Parse Date \""+str+"\"");
                Log.Exception(str, ex);
                throw;
            }
        }

        static Regex invalidTimespanRegex = new Regex("[^0-9:.]");
        public static TimeSpan ParseTimeSpan(string str)
        {
            //The builtin Timespan and Dattime parsing either throw or incorrectly parse values over 24:00:00

            //fail if invalid characters with nothing (usually PM or similar)
            if(invalidTimespanRegex.IsMatch(str)) {
                throw new FormatException("Unable to parse TimeSpan \"" + str + "\". Expected format \"Hours:Minutes:Seconds\" or \"Date.Hours:Minutes:Seconds\"");

            }
            //Regex rgx = 
            //str = rgx.Replace(str, "");

            int days = 0;
            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            string[] dots = str.Split('.');

            string time = null;

            bool success;

            switch (dots.Length)
            {
                case 1:
                    time = str;
                    success = true;
                    break;
                case 2:
                    success = int.TryParse(dots[0], out days);
                    time = dots[1];
                    break;
                default:
                    Log.Error(str, "Too many '.' chars in TimeSpan \"" + str + "\", only one allowed");
                    success = false;
                    break;
            }

            if (!success)
            {
                throw new FormatException("Unable to parse TimeSpan \"" + str + "\". Expected format \"Hours:Minutes:Seconds\" or \"Date.Hours:Minutes:Seconds\"");
            }

            string[] colons = time.Split(':');

  

            switch (colons.Length)
            {
                case 1:
                    success = int.TryParse(colons[0], out minutes);
                    break;
                case 2:
                    success = int.TryParse(colons[0], out hours)
                        && int.TryParse(colons[1], out minutes);
                    break;

                case 3:
                    success = int.TryParse(colons[0], out hours)
                        && int.TryParse(colons[1], out minutes)
                        && int.TryParse(colons[2], out seconds);
                    break;

                case 4:
                    success = int.TryParse(colons[0], out days)
                        && int.TryParse(colons[1], out hours)
                        && int.TryParse(colons[2], out minutes)
                        && int.TryParse(colons[3], out seconds);
                    break;

                default:
                    success = false;
                    break;
            }



            if (!success)
            {
                throw new FormatException("Unable to parse TimeSpan \"" + str + "\". Expected format \"Hours:Minutes:Seconds\" or \"Date.Hours:Minutes:Seconds\"");
            }

            seconds += days * 24 * 60 * 60 + hours * 60 * 60 + minutes * 60;

            TimeSpan timeSpan = new TimeSpan(0, 0, seconds);
            return timeSpan;

            //Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            //TimeSpan timeSpan = TimeSpan.Parse(str);
            //return timeSpan;

            //// this one should work but throws
            //TimeSpan timeSpan = TimeSpan.ParseExact(str, Sky.formatTimeSpan, CultureInfo.InvariantCulture);
            //return timeSpan;

            //// workaround:
            //DateTime t = ParseDateTime(str);

            //TimeSpan timeSpan = t - t.Date;
            //return timeSpan;

        }

        public static bool TryParseTimeSpan(string str, out TimeSpan timeSpan)
        {
            // this one should work but throws
            //return TimeSpan.ParseExact(str, Sky.formatTimeSpan, CultureInfo.InvariantCulture);

            // workaround:
            //DateTime t;
            //if (DateTime.TryParseExact(str, formatTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out t)) {
            //    timeSpan = t - t.Date;
            //    return true;
            //}
            //timeSpan = new TimeSpan();
            //return false;

            try
            {
                timeSpan = ParseTimeSpan(str);
                return true;
            }
            catch (FormatException)
            {
                timeSpan = new TimeSpan();
                return false;
            }
        }


        // public static DateTime ParseTime(string str)
        // {
        //     return DateTime.ParseExact(str, Sky.formatTime, CultureInfo.InvariantCulture);
        // }

        // public static bool TryParseDateTime(string str, out DateTime result)
        // {
        //     return DateTime.TryParseExact(str, Sky.formatDateTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        // }

        public static string TimeToString(DateTime dateTime)
        {
            return dateTime.ToString(Sky.formatTime, CultureInfo.InvariantCulture);
        }

        public static string TimeToString(TimeSpan timeSpan)
        {
            // return timeSpan.ToString(formatTimeSpan, CultureInfo.InvariantCulture);
            return timeSpan.ToString();
        }


        internal static string ToCsv(TimeSpan timeSpan)
        {
            int hours = timeSpan.Days * 24 + timeSpan.Hours;
            string str = String.Format("{0:D2}:{1:D2}:{2:D2}",hours, timeSpan.Minutes, timeSpan.Seconds);
            return str;

            //return timeSpan.ToString(Sky.formatTimeSpan, CultureInfo.InvariantCulture);
        }

        public static string DateTimeToPrettyString(DateTime dateTime)
        {
            return dateTime.ToString(Sky.formatDateTimePretty, CultureInfo.InvariantCulture);
        }

        public static string TimeToPrettyString(DateTime dateTime)
        {
            return dateTime.ToString(Sky.formatTimePretty, CultureInfo.InvariantCulture);
        } 

        // // convert the double in hours to a time string HH:MM:SS
        // public static string HoursMinutes( double time )
        // {
        //     var h = Math.Floor( time );
        //     var min = Math.Round( 60.0 * SunHelper.Frac( time ) );
        //     if (min == 60){min=0;h++;}
        //     var str = h + ":";
        //     if ( min >= 10 ) str = str + min;
        //     else str = str + "0" + min;
        //     if ( str.LastIndexOf( 'N' ) > 0 ) str = "--:--";
        //     return str;
        // }

        // static double Frac( double X )
        // {
        //     X = X - Math.Floor( X );
        //     if ( X < 0 ) X = X + 1.0;
        //     return X;
        // }


    }
}