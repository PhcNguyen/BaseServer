using System.Net.Sockets;
using System.Net;

namespace NServer.Infrastructure.Helper
{
    internal class IPAddressHelper
    {
        public static async Task<string> GetPublicIPAsync()
        {
            using HttpClient client = new();
            try
            {
                return await client.GetStringAsync("https://api.ipify.org");
            }
            catch
            {
                return "N/A";
            }
        }

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
