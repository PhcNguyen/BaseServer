using NETServer.Application.NetSocketServer;
using NETServer.Logging;

namespace NETServer;

class Program
{
    private static void Main(string[] args)
    {
        // Initialize the server
        var server = new NetSocketServer();

        // Start the server
        server.StartListener();

        Console.WriteLine("Press Enter to stop the server.");
        Console.ReadLine();

        // Stop the server
        server.StopListener();

        Console.ReadLine();
    }
}

