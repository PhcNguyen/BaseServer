using NServer.Core.Services;

namespace NServer.Core.Interfaces.Packets
{
    public partial interface IPacket
    {
        UniqueId Id { get; }

        void Reset();

        string ToJson();

        byte[] ToByteArray();

        void SetId(UniqueId id);
    }
}