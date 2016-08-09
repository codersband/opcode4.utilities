using System;
using System.Security.Cryptography;
using System.Text;

namespace opcode4.utilities
{
    public static class HashUtils
    {
        public static string MD5Hash(string s)
        {
            return string.IsNullOrEmpty(s) ? "" : Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(s)));
        }

        #region ELF hash functions
        public static long ElfHash(string s)
        {
            var res = 0;

            if (string.IsNullOrEmpty(s))
                return res;

            var s1 = s.ToLower();
            try
            {
                for (var i = 0; i < s1.Length; i++)
                {
                    var b = s1[i];
                    res = (res << 4) + b;
                    var x = res & 0xf000000;
                    if (x != 0) res = res ^ (x >> 24);
                    res = res & (~x);
                }
            }
            catch { }

            return res;
        }
        public static string ElfHashS(string s)
        {
            var r = ElfHash(s);
            return r <= 0 ? "" : r.ToString();
        }
        public static string HashPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return "";
            
            const string nums = "0123456789";

            var sb = new StringBuilder();
            for (var i = 0; i < phone.Length; i++)
            {
                if (nums.IndexOf(phone[i]) >= 0)
                    sb.Append(phone[i]);
            }

            return ElfHashS(sb.ToString());
        }
        public static string HashRestrictedLength(string s, int len)
        {
            if (string.IsNullOrEmpty(s))
                return "";

            var r = s.Replace(" ", "").Replace("-", "");
            return ElfHashS(len > 0 && r.Length > len ? r.Substring(0, len) : r); 
        }
        public static string HashDateTime(DateTime? dt)
        {
            return dt == null ? "" : ElfHashS(((DateTime) dt).ToString("yyyyMMddHHmm"));
        }
        #endregion ELF hash functions
    }
}