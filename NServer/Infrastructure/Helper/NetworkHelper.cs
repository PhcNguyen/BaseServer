using System.Net;
using System.Linq;
using System.Net.Sockets;
using NServer.Infrastructure.Logging;
using System;

namespace NServer.Infrastructure.Helper
{
    internal class NetworkHelper
    {
        public static string GetPublicIPFromDns()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                var ip = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                return ip?.ToString() ?? "N/A";
            }
            catch
            {
                return "N/A";
            }
        }

        public static string GetLocalIP() => ExtractLocalIP(AddressFamily.InterNetwork);
        public static string[] GetAllLocalIPs() => ExtractAllLocalIPs();
        public static string GetClientIP(TcpClient client) => ExtractClientIP(client.Client);
        public static string GetClientIP(Socket socket) => ExtractClientIP(socket);

        public static IPAddress ParseIPAddress(string ipAddress)
        {
            if (!IPAddress.TryParse(ipAddress, out var parsedIPAddress))
            {
                NLog.Instance.Error($"Invalid IP address format: {ipAddress}");
                throw new ArgumentException("The provided IP address is not valid.", nameof(ipAddress));
            }
            return parsedIPAddress;
        }

        // Private helpers
        private static string ExtractLocalIP(AddressFamily family)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList.FirstOrDefault(ip => ip.AddressFamily == family)?.ToString() ?? string.Empty;
        }

        private static string[] ExtractAllLocalIPs()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList.Select(ip => ip.ToString()).ToArray();
        }

        private static string ExtractClientIP(Socket socket)
        {
            if (socket.RemoteEndPoint is IPEndPoint remoteEndPoint)
            {
                return remoteEndPoint.Address.ToString();
            }
            return string.Empty;
        }
    }
}
