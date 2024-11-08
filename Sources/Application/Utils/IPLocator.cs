
using System.Net;
using System.Net.Sockets;

namespace NETServer.Application.Utils;

internal class IPLocator
{
    public static string Public()
    {
        using (HttpClient client = new HttpClient())
        {
            try { return $"{client.GetStringAsync("https://api.ipify.org").Result}"; }
            catch (Exception) { return "N/A"; }
        }
    }

    public static string Local()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        var localIP = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

        return localIP?.ToString() ?? "N/A";
    }
}
