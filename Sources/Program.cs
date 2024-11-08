using NETServer.Application.Network;
using NETServer.Logging;

namespace NETServer;

class Program
{
    private static void Main(string[] args)
    {
        // Initialize the server
        var server = new NetworkHost();

        // Start the server
        server.StartListener();

        Console.WriteLine("Press Enter to stop the server.");
        Console.ReadLine();

        // Stop the server
        server.StopListener();
    }
}
