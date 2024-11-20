using NETServer.Application.Handlers;
using NETServer.Infrastructure.Helper;
using NETServer.Infrastructure.Configuration;

using System.Runtime.CompilerServices;
using NETServer.Core.Network.Packet.Enums;

namespace NETServer.Core.Network.Packet
{
    internal partial class PacketBase
    {
        /// <summary>
        /// Phương thức để đặt lại gói tin về trạng thái ban đầu.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            Version = Setting.VERSION;
            Flags = PacketFlags.NONE;
            Command = (short)Cmd.NONE;
            Payload = Memory<byte>.Empty;
            Timestamp = DateTimeOffset.UtcNow;
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
        public void SetFlag(PacketFlags flag)
        {
            if (!Enum.IsDefined(typeof(PacketFlags), flag))
                throw new ArgumentOutOfRangeException(nameof(flag), $"Invalid flag value: {flag}");
            Flags |= flag;
        }

        /// <summary>
        /// Phương thức để loại bỏ cờ trạng thái
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveFlag(PacketFlags flag)
        {
            if (!Enum.IsDefined(typeof(PacketFlags), flag))
                throw new ArgumentOutOfRangeException(nameof(flag), $"Invalid flag value: {flag}");
            Flags &= ~flag;
        }

        /// <summary>
        /// Kiểm tra xem flag có tồn tại hay không.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasFlag(PacketFlags flag) => Flags.HasFlag(flag);

        /// <summary>
        /// Set Command mới cho gói tin.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCommand(short command)
        {
            if (!Enum.IsDefined(typeof(Cmd), command))
                throw new ArgumentOutOfRangeException(nameof(command), $"Invalid command value: {command}");
            Command = command;
        }

        // <summary>
        /// Set Command mới cho gói tin.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCommand(Cmd command) => Command = (short)command;

        /// <summary>
        /// Set Payload mới cho gói tin.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPayload(ReadOnlySpan<byte> newPayload)
        {
            if (newPayload.IsEmpty)
                throw new ArgumentException($"Payload cannot be empty. Current Command: {Command}", nameof(newPayload));

            Payload = new Memory<byte>(newPayload.ToArray());
        }

        /// <summary>
        /// Set Payload mới cho gói tin.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPayload(string newPayload)
        {
            ArgumentNullException.ThrowIfNull(newPayload, nameof(newPayload));
            SetPayload(ByteConverter.ToBytes(newPayload));
        }
    }
}
