using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;

namespace opcode4.utilities
{
    public static class CommonUtils
    {
        public static object LockObject = new Object();

        private static readonly string[] HexSymbols = { "%0D", "%0d", "%0A", "%0a", "%3C", "%3c", "%3E", "%3e", "%09", "%25" };
        private static readonly string[] CharSymbols = { "\r", "\r", "\n", "\n", "<", "<", ">", ">", " ", "%" };

        public static string CRLF = "\r\n";

        public static long Value2Long(object obj)
        {
            return obj == null || (obj is string && string.IsNullOrEmpty((string)obj)) ? 0 : Convert.ToInt64(obj);
        }
        public static ulong Value2uLong(object obj)
        {
            return obj == null || (obj is string && string.IsNullOrEmpty((string)obj)) ? 0 : Convert.ToUInt64(obj);
        }
        public static int Value2Int(object obj)
        {
            return obj == null || (obj is string && string.IsNullOrEmpty((string)obj)) ? 0 : Convert.ToInt32(obj);
        }
        public static string Value2Str(object obj)
        {
            return obj == null ? "" : Convert.ToString(obj);
        }
        public static string[] Value2StrArray(object obj)
        {
            var source = obj as IEnumerable;
            return source != null ? source.Cast<object>().Select(x => x.ToString()).ToArray() : new string[0];
        }
        public static string Value2StrDef(object obj, string DefValue)
        {
            var s = Value2Str(obj);
            return string.IsNullOrEmpty(s) ? DefValue : s;
        }
        public static DateTime Value2DateTime(object obj)
        {
            return obj == null || (obj is string && string.IsNullOrEmpty((string)obj)) ? DateTime.MinValue : Convert.ToDateTime(obj);
        }

        public static bool Value2Bool(object obj)
        {
            return !(obj == null || (obj is string && string.IsNullOrEmpty((string)obj))) && Convert.ToInt32(obj) == 1;
        }

        public static string DecodeBase64(string s)
        {
            return Encoding.UTF8.GetString(Base64.DecodeBase64(s));
        }

        public static string DecodeBase64URL(string s)
        {
            //            s = s.Replace('!', '+');
            //            s = s.Replace('-', '/');
            //            s = s.Replace('_', '=');

            return Encoding.UTF8.GetString(Base64.DecodeBase64URL(s));//Convert.FromBase64String(s)
        }
        public static string EncodeBase64URL(string s)
        {
            return Base64.EncodeBase64URL(Encoding.UTF8.GetBytes(s));
        }

        public static string EncodeBase64(string s)
        {
            return Base64.EncodeBase64(Encoding.UTF8.GetBytes(s));
        }

        public static Encoding GetEncoding(byte[] arr)
        {
            if (arr.Length >= 3 && arr[0] == 0xef && arr[1] == 0xbb && arr[2] == 0xbf) //UTF-8
                return Encoding.UTF8;
            if (arr.Length >= 4 && arr[0] == 0x00 && arr[1] == 0x00 && arr[2] == 0xfe && arr[3] == 0xff) //UTF-32 BE
                return Encoding.UTF32;
            if (arr.Length >= 4 && arr[0] == 0xff && arr[1] == 0xfe && arr[2] == 0x00 && arr[3] == 0x00) //UTF-32 BE
                return Encoding.UTF32;
            if (arr.Length >= 2 && arr[0] == 0xfe && arr[1] == 0xff) //BE
                return Encoding.Unicode;
            if (arr.Length >= 2 && arr[0] == 0xff && arr[1] == 0xfe) //LE
                return Encoding.Unicode;

            return Encoding.ASCII;
        }

        public static string CreateUID()
        {
            return Guid.NewGuid().ToString("N").ToUpper();
        }

        public static string Date2YYYYMMDD(DateTime d)
        {
            return d.ToString("yyyyMMddHHmmss");
        }

        public static DateTime ParseYYYYMMDD(string sDate)
        {
            int h = 0; int n = 0; int s = 0;
            if (sDate.Length < 8)
                throw new Exception("Invalid date string");

            int y = Convert.ToInt32(sDate.Substring(0, 4));
            int m = Convert.ToInt32(sDate.Substring(4, 2));
            int d = Convert.ToInt32(sDate.Substring(6, 2));

            if (sDate.Length >= 12)
            {
                h = Convert.ToInt32(sDate.Substring(8, 2));
                n = Convert.ToInt32(sDate.Substring(10, 2));
            }

            if (sDate.Length >= 14)
                s = Convert.ToInt32(sDate.Substring(12, 2));

            return new DateTime(y, m, d, h, n, s);
        }

        public static DateTime ParseYYYYMMDDTZ(string sDate)
        {
            return ParseYYYYMMDD(sDate.Replace("T", "").Replace("Z", ""));
        }

        public static DateTime ParseYYYYMMDD(string sDate, DateTime def)
        {
            DateTime res = def;
            try
            {
                res = ParseYYYYMMDDTZ(sDate);
            }
            catch
            { }

            return res;
        }

        public static DateTime? Obj2DateTime(object obj)
        {
            if (obj == null)
                return null;

            if (obj is string)
            {
                var s = Convert.ToString(obj).Trim();
                if (s.Equals(""))
                    return null;

                return ParseYYYYMMDD(s);
            }

            return Convert.ToDateTime(obj);
        }

        public static string PartialHexDecode(string s)
        {
            if (string.IsNullOrEmpty(s) || s.IndexOf('%') == -1)
                return s;

            var res = s;
            for (var i = 0; i < HexSymbols.Length; i++)
            {
                if (res.IndexOf(HexSymbols[i]) != -1)
                    res = res.Replace(HexSymbols[i], CharSymbols[i]);
            }

            return res;
        }
        public static string DateTime2YYYYMMDDTHHmmSSZ(DateTime dt)
        {
            var sb = new StringBuilder();
            sb.Append(string.Format("{0:yyyyMMdd}", dt));
            sb.Append("T");
            sb.Append(string.Format("{0:HHmmss}", dt));
            //sb.Append("Z");

            return sb.ToString();
        }

        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }

        public static double ConvertToUnixTimestamp(DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        public static DateTime ConvertFromUnixTimestampMs(double timestamp)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddMilliseconds(timestamp);
        }

        public static double ConvertToUnixTimestampMs(DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            return Math.Floor(diff.TotalMilliseconds);
        }


        public static string GetPairValue(string s, char Delimeter)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentException("Utils.GetPairValue: Empty or not defined key=value string");

            var p = s.IndexOf(Delimeter);
            if (p < 0)
                throw new Exception(String.Format("Utils.GetPairValue: Invalid key=value string. Delimeter [{0}] was not found in [{1}]", Delimeter, s));

            if (p == 0)
                throw new Exception(String.Format("Utils.GetPairValue: Invalid key=value string. Delimeter [{0}] was found in 0 position in [{1}]", Delimeter, s));

            return s.Substring(p + 1).Trim();
        }

        public static bool CompareByteArrays(Byte[] arr1, Byte[] arr2)
        {
            if (arr1 == null)
                throw new ArgumentException("CompareByteArrays: First array is not assigned");
            if (arr2 == null)
                throw new ArgumentException("CompareByteArrays: Second array is not assigned");

            if (arr1.Length != arr2.Length)
                return false;

            for (var i = 0; i < arr1.Length; i++)
            {
                if (arr1[i] != arr2[i])
                    return false;
            }

            return true;
        }

        public static string CheckClosingBackSlash(string dir)
        {
            return dir[dir.Length - 1] == '\\' ? dir : String.Format("{0}\\", dir);
        }

        public static void Stream2File(Stream st, string filename)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException("Utils.Stream2File: Filename is empty or not defined");

            var fi = new FileInfo(filename);
            if (fi.Directory == null)
                throw new Exception(String.Format("Utils.Stream2File: Directory object does not exists. Invalid or incomplete path [{0}]", filename));

            if (!fi.Directory.Exists)
                fi.Directory.Create();

            var fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            try
            {
                var oldPos = st.Position;
                st.Position = 0;
                const int Len = 256;
                var buffer = new Byte[Len];
                var bytesRead = st.Read(buffer, 0, Len);
                while (bytesRead > 0)
                {
                    fs.Write(buffer, 0, bytesRead);
                    bytesRead = st.Read(buffer, 0, Len);
                }

                st.Position = oldPos;
                fs.Flush();
            }
            finally
            {
                fs.Close();
            }
        }

        public static void StreamCopy(Stream readStream, Stream writeStream)
        {
            readStream.Position = 0;
            const int Len = 256;
            var buffer = new Byte[Len];
            var bytesRead = readStream.Read(buffer, 0, Len);
            while (bytesRead > 0)
            {
                writeStream.Write(buffer, 0, bytesRead);
                bytesRead = readStream.Read(buffer, 0, Len);
            }
        }

        public static string LoadFromFile(string fileName)
        {
            var res = "";
            if (File.Exists(fileName))
                res = File.ReadAllText(fileName);
            //                using (var st = fi.OpenRead())
            //                {
            //                    var arr = new Byte[st.Length];
            //                    st.Read(arr, 0, arr.Length);
            //                    var enc = GetEncoding(arr);
            //                    res = enc.GetString(arr);
            //                }

            return res;
        }

        public static T IntToEnum<T>(int number)
        {
            return (T)Enum.ToObject(typeof(T), number);
        }

        public static bool IsBitWiseEquals(int bitwise, int mask)
        {
            return ((bitwise & mask) == mask);
        }


        public static string IncludePathBackslash(string dir)
        {
            return dir[dir.Length - 1] == '\\' ? dir : dir + "\\";
        }
        public static string ExcludePathBackslash(string dir)
        {
            return dir[dir.Length - 1] == '\\' ? dir.Substring(0, dir.Length - 1) : dir;
        }


        public static byte ComputeAdditionChecksum(byte[] data)
        {
            var longSum = data.Sum(x => (long)x);
            return unchecked((byte)longSum);
        }


        /// <summary>
        /// Convert an object to a byte array
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;

            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Convert a byte array to an Object
        /// </summary>
        /// <param name="arrBytes"></param>
        /// <returns></returns>
        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);

                return binForm.Deserialize(memStream);
            }
        }

        public static string GetFullFileNameWithoutExtension(string path)
        {
            if (path != null)
            {
                int i;
                return (i = path.LastIndexOf('.')) == -1 ? path : path.Substring(0, i);
            }
            return null;
        }

        public static void CopyPropertyValues(object source, object destination, List<string> ignoreProperties = null)
        {
            var destProperties = destination.GetType().GetProperties();
            if (ignoreProperties != null)
                destProperties = destProperties.Where(p => !ignoreProperties.Contains(p.Name)).ToArray();

            foreach (var sourceProperty in source.GetType().GetProperties())
            {
                foreach (var destProperty in destProperties)
                {
                    if (destProperty.GetSetMethod() != null)

                        if (destProperty.Name == sourceProperty.Name && destProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                        {
                            destProperty.SetValue(destination, sourceProperty.GetValue(source, new object[] { }), new object[] { });
                            break;
                        }
                }
            }
        }

        public static string NormalizePhoneNumber(string phoneNumber, bool toLocalFormat = false)
        {
            // var phone = "+972 50-2495650";
            if (string.IsNullOrEmpty(phoneNumber))
                return string.Empty;

            phoneNumber = phoneNumber.Trim();

            var phone = new Regex("[\\D]+").Replace(phoneNumber, "");
            if (phoneNumber.StartsWith("+"))
                phone = $"+{phone}";

            if (!toLocalFormat) return phone;

            if (phoneNumber.StartsWith("+972"))
            {
                phone = new Regex("^" + "\\+972").Replace(phone, "0");
            }

            return phone;
        }

    }
}