using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
        public readonly string[] Items;

        public MultiValueParameter(string delimitedString)
        {
            if (string.IsNullOrEmpty(delimitedString))
            {
                Items = null;
                return;
            }

            Items = delimitedString.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public string ReadString(int index)
        {
            if (Items == null)
                throw new Exception("MultiValueParameter: Items object was not assigned");
            return Items[index];
        }

        public bool IsEmpty => (Items == null || Items.Length == 0);

        public long ReadLong(int index)
        {
            return Int64.Parse(ReadString(index));
        }

        public long ReadInt(int index)
        {
            return Int32.Parse(ReadString(index));
        }

        public int Count => Items.Length;
    }

    public class MultiValueParameter<T>
    {
        private readonly List<T> _lst = new List<T>();

        public MultiValueParameter(string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            var arr = value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (typeof(T) == typeof(ulong))
            {
                _lst = arr.Select(s => (T)(object)ulong.Parse(s)).ToList();
            }
            else if (typeof(T) == typeof(long))
            {
                _lst = arr.Select(s => (T)(object)long.Parse(s)).ToList();
            }
            else if (typeof(T) == typeof(double))
            {
                _lst = arr.Select(s => (T)(object)double.Parse(s)).ToList();
            }
            else if (typeof(T) == typeof(int))
            {
                _lst = arr.Select(s => (T)(object)int.Parse(s)).ToList();
            }
            else if (typeof(T) == typeof(uint))
            {
                _lst = arr.Select(s => (T)(object)uint.Parse(s)).ToList();
            }
            else if (typeof(T) == typeof(float))
            {
                _lst = arr.Select(s => (T)(object)float.Parse(s)).ToList();
            }
            else if (typeof(T) == typeof(short))
            {
                _lst = arr.Select(s => (T)(object)short.Parse(s)).ToList();
            }
            else if (typeof(T) == typeof(ushort))
            {
                _lst = arr.Select(s => (T)(object)ushort.Parse(s)).ToList();
            }
            else
            {
                _lst = arr.Select(s => (T)(object)s).ToList();
            }
        }

        public T ReadItem(int index)
        {
            return _lst[index];
        }

        public T[] ToArray()
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

        public static MultiValueParameter ReadSaveMultiValueParameter(string appKey)
        {
            try
            {
                return new MultiValueParameter(ReadString(appKey));
            }
            catch
            { return null; }

        }

        public static MultiValueParameter<T> ReadMultiValueParameter<T>(string appKey)
        {
            var s = ReadStringDef(appKey, "");
            return new MultiValueParameter<T>(s);
        }

        public static MultiValueParameter<T> ReadSaveMultiValueParameter<T>(string appKey)
        {
            try
            {
                return new MultiValueParameter<T>(ReadString(appKey));
            }
            catch
            { return null; }

        }
        #endregion Read methods

        public static string ServerId
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