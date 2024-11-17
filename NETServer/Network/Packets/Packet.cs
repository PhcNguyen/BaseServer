using NETServer.Infrastructure.Interfaces;
using NETServer.Infrastructure.Configuration;
using System.Runtime.CompilerServices;

namespace NETServer.Network.Packets
{
    /// <summary>
    /// Gói tin cơ bản, kế thừa từ PacketBase.
    /// </summary>
    internal class Packet : PacketBase, IPacket
    {
        /// <summary>
        /// Constructor để tạo Packet với Command và Payload.
        /// </summary>
        public Packet(byte? version = null, byte? flags = null, short? command = null, byte[]? payload = null)
        {
            Version = version ?? Setting.VERSION;

            Flags = flags.HasValue && Enum.IsDefined(typeof(PacketFlags), flags.Value)
                    ? (PacketFlags)flags.Value
                    : PacketFlags.NONE;

            Command = command;

            Payload = payload?.Length > 0
                    ? new Memory<byte>(payload)
                    : Memory<byte>.Empty;
        }

        /// <summary>
        /// Phương thức để đặt lại gói tin về trạng thái ban đầu.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            Version = Setting.VERSION;
            Flags = PacketFlags.NONE;
            Command = short.MaxValue;
            Payload = Memory<byte>.Empty;
        }

        /// <summary>
        /// Set Command mới cho gói tin.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVersion(byte version) => Version = version;

        /// <summary>
        /// Phương thức để thêm cờ trạng thái
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFlag(PacketFlags flag) => Flags |= flag;

        /// <summary>
        /// Phương thức để loại bỏ cờ trạng thái
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveFlag(PacketFlags flag) => Flags &= ~flag;

        /// <summary>
        /// Kiểm tra xem flag có tồn tại hay không.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasFlag(PacketFlags flag) => (Flags & flag) == flag;

        /// <summary>
        /// Set Command mới cho gói tin.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCommand(short command) => Command = command;

        /// <summary>
        /// Set Payload mới cho gói tin.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPayload(byte[] newPayload)
        {
            ArgumentNullException.ThrowIfNull(newPayload, nameof(newPayload));

            if (newPayload.Length == 0)
                throw new ArgumentException($"Payload cannot be empty. Current Command: " +
                    Command, nameof(newPayload));

            Payload = new Memory<byte>(newPayload);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValid()
        {
            return Command == null && Payload.Length > 0;
        }
    }
}

