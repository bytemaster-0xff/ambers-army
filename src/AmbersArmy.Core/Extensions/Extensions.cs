using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AmbersArmy
{
    public static class Extensions
    {
        private const string JSON_DATE_FORMAT = "yyyy-MM-ddTHH\\:mm\\:ss.ffffZ";

        public static DateTime? ToNullableDateTime(this string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return null;
            }
            else
            {
                DateTime result;
                if (DateTime.TryParseExact(value, JSON_DATE_FORMAT, new CultureInfo("en-US"), DateTimeStyles.None, out result))
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        public static bool IsValidJSONDate(this string date)
        {
            if (date.Length != 25)
            {
                return false;
            }

            return Regex.IsMatch(date, @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{4}Z");
        }

        public static DateTime ToDateTime(this string value)
        {
            return DateTime.ParseExact(value, JSON_DATE_FORMAT, new CultureInfo("en-US"), DateTimeStyles.None);
        }

        public static String ToJSONString(this DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString(JSON_DATE_FORMAT);
        }
    }
}
