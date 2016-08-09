using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;

namespace opcode4.utilities
{
    public class ECfgParamNotDefined : Exception
    {
        public ECfgParamNotDefined(string paramName)
            : base(string.Format("Parameter {0} does not defined in .config file", paramName)) { }
        public ECfgParamNotDefined(string paramName, string cfgFile)
            : base(string.Format("Parameter {0} does not defined in config file {1}", paramName, cfgFile)) { }
    }

    public class MultiValueParameter
    {
        private readonly List<string> _lst = new List<string>();

        public MultiValueParameter(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            
            var arr = value.Split(',');
            foreach (string s in arr)
            {
                _lst.Add(s);
            }
        }

        public string ReadString(int index)
        {
            return _lst[index];
        }

        public long ReadLong(int index)
        {
            return Int64.Parse(_lst[index]);
        }

        public long ReadInt(int index)
        {
            return Int32.Parse(_lst[index]);
        }

        public string[] ToArray()
        {
            return _lst.ToArray();
        }

        public int Count
        {
            get { return _lst.Count; }
        }
    }

    public static class ConfigUtils
    {
        public static string ConnectionString
        {
            get { return ReadString("CS"); }
        }

       #region Read methods
        public static string ReadString(string appKey)
        {
            var value = ConfigurationManager.AppSettings[appKey];
            if (value == null)
                throw new ECfgParamNotDefined(appKey);

            return value;
        }
        public static string ReadStringDef(string appKey, string defValue)
        {
            var value = ConfigurationManager.AppSettings[appKey];
            return value ?? defValue;
        }

        public static int ReadInt(string appKey)
        {
            return Int32.Parse(ReadString(appKey));
        }
        public static int ReadIntDef(string appKey, int defValue)
        {
            string value = ConfigurationManager.AppSettings[appKey];
            
            return value == null ? defValue : Int32.Parse(value);
        }

        public static long ReadLong(string appKey)
        {
            return Int64.Parse(ReadString(appKey));
        } 
        
        public static long ReadLongDef(string appKey, long defValue)
        {
            string value = ConfigurationManager.AppSettings[appKey];

            return value == null ? defValue : Int64.Parse(value);
        }

        public static ulong ReadULongDef(string appKey, ulong defValue)
        {
            string value = ConfigurationManager.AppSettings[appKey];

            return value == null ? defValue : UInt64.Parse(value);
        }
        
        public static bool ReadBool(string appKey)
        {
            return (ReadInt(appKey) != 0);
        }

        public static bool SafeReadBool(string appKey)
        {
            try
            {
                return (ReadInt(appKey) != 0);
            }
            catch
            { return false; }
        }

        public static MultiValueParameter ReadMultiValueParameter(string appKey)
        {
            var s = ReadString(appKey);
            return new MultiValueParameter(s);
        }
        #endregion Read methods

        public static string ServerID
        {
            get
            {
                var s = ReadString("ServerID");
                return string.IsNullOrEmpty(s) ? Dns.GetHostName() : s;
            }
        }

        public static bool IsDebugMode { get { return SafeReadBool("IS_DEBUG_MODE"); } } 
    }
}