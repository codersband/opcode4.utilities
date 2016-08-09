using System;
using System.Text.RegularExpressions;

namespace opcode4.utilities
{
    public static class MobileUtils
    {
        public const string ISRAEL_COUNTRY_PREFIX = "972";

        public static string FixIMEI(string IMEI)
        {
            var resIMEI = IMEI.Trim();
            if (resIMEI.IndexOf("IMEI") == 0)
                resIMEI = resIMEI.Substring(4);

            if (resIMEI.IndexOf("-") >= 0)
                resIMEI = resIMEI.Replace("-", "");

            return resIMEI.Trim();
        }

        public static String FixMSISDN(string phone)
        {
            var mobile = phone;
            if (mobile.StartsWith("+"))
            {
                var r = new Regex("[\\D]+");
                mobile = r.Replace(mobile, "");
            }

            return mobile;
        }

        public static string RemoveIsraelCountryPrefix(string phone)
        {
            if (phone.IndexOf('+' + ISRAEL_COUNTRY_PREFIX) == 0)
                return "0" + phone.Substring(4);

            if (phone.IndexOf(ISRAEL_COUNTRY_PREFIX) == 0)
                return "0" + phone.Substring(3);

            return phone;
        }

        public static string AddIsraelCountryPrefix(string phone)
        {
            if (phone.StartsWith('+' + ISRAEL_COUNTRY_PREFIX))
                return phone.Substring(1);

            if (!phone.StartsWith(ISRAEL_COUNTRY_PREFIX) && phone.Length == 10)
                return ISRAEL_COUNTRY_PREFIX + phone.Substring(1);

            return FixMSISDN(phone);
        }

        public static string ConvertToViewFormat(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return phone;

            var p = RemoveIsraelCountryPrefix(phone);
            return p.Substring(0, 3) + "-" + p.Substring(3);
        }
    }
}
