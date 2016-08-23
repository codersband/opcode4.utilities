using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace opcode4.utilities
{
    public static class IPInfo
    {
        public static bool IsIsraelLocation(string ip)
        {
            bool isIsrael = false;
            IPAddress address;

            if (IPAddress.TryParse(ip, out address))
                try
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        //http://api.hostip.info/?ip=62.90.149.56
                        var url = String.Format("http://api.hostip.info/?ip={0}", ip);

                        var request = WebRequest.Create(url);
                        request.Method = "GET";
                        request.ContentType = "text/html; charset=UTF-8";
                        request.Timeout = 90000;
                        request.ContentLength = 0;
                        using (var response = request.GetResponse())
                        {
                            using (var rs = response.GetResponseStream())
                            {
                                using (var rdr = new StreamReader(rs))
                                {
                                    var resp = rdr.ReadToEnd().Trim();
                                    if (!string.IsNullOrEmpty(resp))
                                    {
                                        isIsrael = "ISRAEL".Equals(XmlUtils.GetNode(resp, "countryName").ToUpper());
                                    }

                                    rdr.Close();
                                }
                            }

                            response.Close();
                        }
                    }
                }
                catch (Exception)
                {
                }

            return isIsrael;
        }

        public static IPAddress LocalIPAddress()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                return null;

            var host = Dns.GetHostEntry(Dns.GetHostName());

            return host
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }

        public static bool IsLocalIP(IPAddress ip)
        {
            return IsInRange(ip, IPAddress.Parse("10.0.0.0"), IPAddress.Parse("10.255.255.255"))
                || IsInRange(ip, IPAddress.Parse("172.16.0.0"), IPAddress.Parse("172.31.255.255"))
                || IsInRange(ip, IPAddress.Parse("192.168.0.0"), IPAddress.Parse("192.168.255.255"))
                || IsInRange(ip, IPAddress.Parse("127.0.0.0"), IPAddress.Parse("127.255.255.255"));
        }

        public static IPAddress PrefferedLocalIPAddress()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                return null;

            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("192.168.10.1", 65530);
                    var endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint?.Address;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool IsWifiRequest(IPAddress ip, int port)
        {
            var result = false;

            // "IP:[212.143.244.194,212.143.244.201,192.168.10.1],PORT:[12, 440-443],INVERT:[True]";
            var ranges = ConfigUtils.ReadString("WIFI_CRITERION");

            if (string.IsNullOrEmpty(ranges))
                return false;

            const RegexOptions regOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnoreCase;
            if (port < 1 || port > 65535)
                return false;

            var portRanges = Regex.Match(ranges, ".*?PORT:\\[(.*?)\\].*?", regOptions).Groups[1].Value;
            if (!string.IsNullOrEmpty(portRanges))
            {

                var blocks = portRanges.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var block in blocks)
                {
                    if (block.IndexOf('-') > 0)
                    {
                        var range = block.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                        if (range.Length < 2)
                            continue;
                        if (port >= Int32.Parse(range[0].Trim()) && port <= Int32.Parse(range[1].Trim()))
                            return true;
                    }
                    else if (String.CompareOrdinal(block, port.ToString()) == 0)
                        return true;
                }
            }
            if (ip != null)
            {
                var ipRanges = Regex.Match(ranges, ".*?IP:\\[(.*?)\\].*?", regOptions).Groups[1].Value;
                if (!string.IsNullOrEmpty(ipRanges))
                {
                    var ipBlocks = ipRanges.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var ipBlock in ipBlocks)
                    {
                        if (ipBlock.IndexOf('-') > 0)
                        {
                            var range = ipBlock.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                            if (range.Length < 2)
                                continue;
                            if (ip.IsInRange(IPAddress.Parse(range[0].Trim()), IPAddress.Parse(range[1].Trim())))
                                result = true;
                        }
                        else if (String.CompareOrdinal(ip.ToString(), ipBlock.Trim()) == 0)
                            result = true;
                    }
                }
            }
            var invert = Regex.Match(ranges, ".*?INVERT:\\[(.*?)\\].*?", regOptions).Groups[1].Value;
            try
            {
                if (Boolean.Parse(invert))
                    result = !result;
            }
            catch (Exception) { }

            var igPort = false;
            var ignorePort443 = Regex.Match(ranges, ".*?Ignore443:\\[(.*?)\\].*?", regOptions).Groups[1].Value;
            if (!string.IsNullOrEmpty(ignorePort443) && String.Compare(ignorePort443, "true", StringComparison.OrdinalIgnoreCase) == 0)
                igPort = true;

            if (result && !igPort)
            {
                if (port != 443)
                    throw new Exception($"Unsecure WIFI requests do not allowed, ip={ip} port={port}");
            }

            return result;

        }

        /// <summary>
        /// Check is the IP in the allowed ip-ranges 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="ranges">IP:[212.143.244.194,212.143.244.201,192.168.10.1-192.168.10.2],INVERT:[False]</param>
        /// <returns></returns>
        public static bool IsInRange(this IPAddress ip, string ranges)
        {
            if (string.IsNullOrEmpty(ranges))
                return false;

            var result = false;

            const RegexOptions regOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnoreCase;

            if (ip != null)
            {
                var ipRanges = Regex.Match(ranges, ".*?IP:\\[(.*?)\\].*?", regOptions).Groups[1].Value;
                if (!string.IsNullOrEmpty(ipRanges))
                {
                    var ipBlocks = ipRanges.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var ipBlock in ipBlocks)
                    {
                        if (ipBlock.IndexOf('-') > 0)
                        {
                            var range = ipBlock.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                            if (range.Length < 2)
                                continue;
                            if (ip.IsInRange(IPAddress.Parse(range[0].Trim()), IPAddress.Parse(range[1].Trim())))
                                result = true;
                        }
                        else if (String.CompareOrdinal(ip.ToString(), ipBlock.Trim()) == 0)
                            result = true;
                    }
                }
            }

            try
            {
                var invert = Regex.Match(ranges, ".*?INVERT:\\[(.*?)\\].*?", regOptions).Groups[1].Value;
                if (Boolean.Parse(invert))
                    result = !result;
            }
            catch (Exception) { }


            return result;

        }

        public static bool IsInRange(this IPAddress source, IPAddress start, IPAddress end)
        {
            var sInt = IPToUint(source);
            return sInt >= IPToUint(start) && sInt <= IPToUint(end);
        }

        private static UInt32 IPToUint(IPAddress source)
        {
            var s = source.ToString().Split(new[] { '.' });
            return (Convert.ToUInt32(s[0]) << 24) | (Convert.ToUInt32(s[1]) << 16) | (Convert.ToUInt32(s[2]) << 8) | (Convert.ToUInt32(s[3]));
        }
    }
}
