using NServer.Core.Packet;
using NServer.Core.Packet.Utils;
using NServer.Infrastructure.Logging;
using NServer.Infrastructure.Services;

namespace NServer.Core.Network.SocketAsync
{
    internal class PacketProcessor(Guid sessionId)
    {
        private readonly PacketContainer _packetContainer = Singleton.GetInstance<PacketContainer>();
        private readonly Guid _sessionId = sessionId;

        public void ProcessPacket(byte[] data)
        {
            if (data.Length < 8)
            {
                return;
            }

            try
            {
                Packets packet = PacketExtensions.FromByteArray(data);
                if (packet == null || !packet.IsValid())
                {
                    // NLog.Warning($"{_sessionId} - Invalid packet received. Ignoring.");
                    return;
                }

                _packetContainer.AddPacket(_sessionId, packet);
            }
            catch (Exception ex)
            {
                NLog.Error($"{_sessionId} - Error processing packet: {ex.Message}");
            }
        }
    }
}
