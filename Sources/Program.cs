using NETServer.Application.Network;

namespace NETServer;

class Program
{
    private static void Main(string[] args)
    {
        // Initialize the server
        var server = new NETServer.Application.Network.ServerEngine();

        // Start the server
        server.StartServer();

        Console.WriteLine("Press Enter to stop the server.");
        Console.ReadLine();

        // Stop the server
        server.StopServer();

        Console.ReadLine();
    }
}

