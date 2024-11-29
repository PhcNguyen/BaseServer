using NServer.Core.Packets.Enums;
using NServer.Infrastructure.Services;

namespace NServer.Core.Interfaces.Packets
{
    internal partial interface IPacket
    {
        UniqueId Id { get; }

        void Reset();
        string ToJson();
        byte[] ToByteArray();

        void SetId(UniqueId id);
    }
}
