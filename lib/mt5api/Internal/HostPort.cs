using System;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
	public class HostAndPort
	{
        public static KeyValuePair<string, int> Parse(string ip)
        {
            int port = 443;
            string host;

            if (ip.StartsWith("["))
            {
                // IPv6 format: [address]:port
                int endBracket = ip.IndexOf(']');
                if (endBracket == -1)
                    throw new FormatException("Invalid IPv6 format");

                host = ip.Substring(1, endBracket - 1); // exclude brackets

                if (endBracket + 1 < ip.Length && ip[endBracket + 1] == ':')
                {
                    string strPort = ip.Substring(endBracket + 2);
                    port = int.Parse(strPort);
                }
            }
            else
            {
                // IPv4 or hostname
                int lastColon = ip.LastIndexOf(':');
                if (lastColon != -1 && ip.IndexOf(':') == lastColon)
                {
                    host = ip.Substring(0, lastColon);
                    port = int.Parse(ip.Substring(lastColon + 1));
                }
                else
                {
                    host = ip;
                }
            }

            return new KeyValuePair<string, int>(host.Trim(), port);
        }
    }
}
