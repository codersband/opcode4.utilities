using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace opcode4.utilities
{
    public class IPInfo
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

    }
}
