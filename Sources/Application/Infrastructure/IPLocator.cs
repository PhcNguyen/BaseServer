using System.Net;
using System.Net.Sockets;

namespace NETServer.Application.Infrastructure;

internal class IPResolver
{
    // Lấy địa chỉ IP công cộng từ dịch vụ ipify
    public static async Task<string> PublicHttps()
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                return await client.GetStringAsync("https://api.ipify.org");
            }
            catch (Exception)
            {
                return "N/A";
            }
        }
    }

    public static string PublicDns()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ip = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            return ip?.ToString() ?? "N/A";
        }
        catch (Exception)
        {
            return "N/A";
        }
    }


    // Lấy địa chỉ IP cục bộ (IPv4) của máy chủ
    public static string Local()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        var localIP = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

        return localIP?.ToString() ?? "N/A";
    }

    // Lấy tất cả địa chỉ IP cục bộ (IPv4 và IPv6) của máy chủ
    public static string[] LocalAll()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        return host.AddressList.Select(ip => ip.ToString()).ToArray();
    }

    // Lấy tất cả địa chỉ IPv4 từ máy chủ
    public static string[] LocalIPv4()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        return host.AddressList
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                    .Select(ip => ip.ToString())
                    .ToArray();
    }

    // Lấy tất cả địa chỉ IPv6 từ máy chủ
    public static string[] LocalIPv6()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        return host.AddressList
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetworkV6)
                    .Select(ip => ip.ToString())
                    .ToArray();
    }
}
