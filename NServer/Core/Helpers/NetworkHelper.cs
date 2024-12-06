using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace NPServer.Core.Helpers;

/// <summary>
/// Provides helper methods for working with network information such as IP addresses.
/// </summary>
public static class NetworkHelper
{
    /// <summary>
    /// Retrieves the public IP address of the local machine by using DNS.
    /// </summary>
    /// <returns>The public IP address or "N/A" if an error occurs.</returns>
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

    /// <summary>
    /// Retrieves the local IP address (IPv4) of the local machine.
    /// </summary>
    /// <returns>The local IPv4 address as a string.</returns>
    public static string GetLocalIP() => ExtractLocalIP(AddressFamily.InterNetwork);

    /// <summary>
    /// Retrieves all local IP addresses of the machine.
    /// </summary>
    /// <returns>An array of local IP addresses as strings.</returns>
    public static string[] GetAllLocalIPs() => ExtractAllLocalIPs();

    /// <summary>
    /// Retrieves the IP address of the client from a TcpClient instance.
    /// </summary>
    /// <param name="client">The TcpClient instance.</param>
    /// <returns>The client's IP address as a string.</returns>
    public static string GetClientIP(TcpClient client) => ExtractClientIP(client.Client);

    /// <summary>
    /// Retrieves the IP address of the client from a Socket instance.
    /// </summary>
    /// <param name="socket">The Socket instance.</param>
    /// <returns>The client's IP address as a string.</returns>
    public static string GetClientIP(Socket socket) => ExtractClientIP(socket);

    /// <summary>
    /// Parses a string representation of an IP address and returns its IPAddress instance.
    /// </summary>
    /// <param name="ipAddress">The string representing the IP address.</param>
    /// <returns>The parsed IPAddress instance.</returns>
    /// <exception cref="ArgumentException">Thrown if the provided IP address is not valid.</exception>
    public static IPAddress ParseIPAddress(string ipAddress)
    {
        if (!IPAddress.TryParse(ipAddress, out var parsedIPAddress))
        {
            throw new ArgumentException("The provided IP address is not valid.", nameof(ipAddress));
        }
        return parsedIPAddress;
    }

    // Private helpers

    /// <summary>
    /// Extracts the local IP address of the machine for a given address family.
    /// </summary>
    /// <param name="family">The address family (IPv4 or IPv6).</param>
    /// <returns>The local IP address as a string.</returns>
    private static string ExtractLocalIP(AddressFamily family)
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        return host.AddressList.FirstOrDefault(ip => ip.AddressFamily == family)?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Extracts all local IP addresses of the machine.
    /// </summary>
    /// <returns>An array of local IP addresses as strings.</returns>
    private static string[] ExtractAllLocalIPs()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        return host.AddressList.Select(ip => ip.ToString()).ToArray();
    }

    /// <summary>
    /// Extracts the IP address of the client from a Socket.
    /// </summary>
    /// <param name="socket">The Socket instance representing the client.</param>
    /// <returns>The client's IP address as a string, or an empty string if unavailable.</returns>
    private static string ExtractClientIP(Socket socket)
    {
        if (socket.RemoteEndPoint is IPEndPoint remoteEndPoint)
        {
            return remoteEndPoint.Address.ToString();
        }
        return string.Empty;
    }
}