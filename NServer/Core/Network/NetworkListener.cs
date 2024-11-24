using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;

using NServer.Infrastructure.Logging;
using NServer.Infrastructure.Configuration;

namespace NServer.Core.Network
{
    internal class NetworkListener: IAsyncDisposable
    {
        private readonly Socket _listenerSocket;
        private readonly int _maxConnections = Setting.MaxConnections;

        public NetworkListener()
        {
            _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            ConfigureSocket(_listenerSocket);
        }

        private static void ConfigureSocket(Socket socket)
        {
            socket.Blocking = Setting.Blocking;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, Setting.KeepAlive);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, Setting.ReuseAddress);
        }

        private static IPAddress ParseIPAddress(string ipAddress)
        {
            try
            {
                return IPAddress.Parse(ipAddress);
            }
            catch (FormatException ex)
            {
                NLog.Instance.Error($"Invalid IP address format: {ipAddress}. Error: {ex.Message}");
                throw new ArgumentException("The provided IP address is not valid.", nameof(ipAddress), ex);
            }
        }

        public void StartListening(string? ipAddress, int port)
        {
            if (_listenerSocket.IsBound)
            {
                NLog.Instance.Warning("Socket is already bound. StartListening cannot be called multiple times.");
                throw new InvalidOperationException("Socket is already bound.");
            }

            if (port < 0 || port > 65535)
            {
                NLog.Instance.Error($"Invalid port number: {port}");
                throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 0 and 65535.");
            }

            try
            {
                IPAddress parsedIPAddress = string.IsNullOrEmpty(ipAddress) ? IPAddress.Any : ParseIPAddress(ipAddress);
                var localEndPoint = new IPEndPoint(parsedIPAddress, port);

                _listenerSocket.Bind(localEndPoint);
                _listenerSocket.Listen(_maxConnections);

                if (port == 0)
                {
                    var selectedPort = ((IPEndPoint)_listenerSocket.LocalEndPoint!).Port;
                    NLog.Instance.Info($"Listening on dynamically selected port: {selectedPort}");
                    return;
                }

                NLog.Instance.Info($"Listening on {localEndPoint}");
            }
            catch (FormatException ex)
            {
                NLog.Instance.Error($"Invalid IP address format: {ipAddress}. Error: {ex.Message}");
                throw new ArgumentException("Invalid IP address format.", nameof(ipAddress), ex);
            }
            catch (SocketException ex)
            {
                NLog.Instance.Error($"Socket error during binding or listening: {ex.Message}");
                throw new InvalidOperationException("Failed to start listening on the specified endpoint.", ex);
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Unexpected error while starting to listen: {ex.Message}");
                throw;
            }
        }

        public async Task<Socket?> AcceptClientAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested) 
                { 
                    NLog.Instance.Info("AcceptClientAsync operation was cancelled."); 
                    return null; 
                }

                var acceptSocket = await Task.Factory.FromAsync(
                    _listenerSocket.BeginAccept,
                    _listenerSocket.EndAccept,
                    null).ConfigureAwait(false);

                ConfigureSocket(acceptSocket);

                return acceptSocket;
            }
            catch (SocketException ex)
            {
                NLog.Instance.Error($"Socket error (Code {ex.SocketErrorCode}): {ex.Message}");
                return null;
            }
            catch (ObjectDisposedException)
            {
                NLog.Instance.Warning("Socket was closed during Accept operation.");
                return null;
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Unexpected error: {ex.Message}");
                return null;
            }
        }

        public void StopListening()
        {
            try
            {
                if (_listenerSocket.Connected)
                    _listenerSocket.Shutdown(SocketShutdown.Both);

                _listenerSocket.Close();
                NLog.Instance.Info("Socket stopped.");
            }
            catch (SocketException ex)
            {
                NLog.Instance.Error($"Error stopping socket: {ex.Message}");
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Unexpected error while stopping socket: {ex.Message}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            StopListening();
            NLog.Instance.Info("Socket resources disposed.");
            await Task.CompletedTask;
        }
    }
}