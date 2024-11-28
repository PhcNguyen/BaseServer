using Base.Core.Packets.Enums;
using Base.Infrastructure.Services;

namespace Base.Core.Interfaces.Packets
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
