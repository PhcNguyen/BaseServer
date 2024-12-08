using NPServer.Core.Communication.Metadata;
using NPServer.Core.Interfaces.Communication;
using System;

namespace NPServer.Core.Communication.Base
{
    public partial class AbstractPacket : IAbstractPacket
    {
        private const int _headerSize = PacketMetadata.HEADERSIZE;

        /// <summary>
        /// Type để xác định loại gói tin.
        /// </summary>
        public PacketType Type { get; protected set; } = PacketType.NONE;

        /// <summary>
        /// Cờ trạng thái của gói tin.
        /// </summary>
        public PacketFlags Flags { get; protected set; } = PacketFlags.NONE;

        /// <summary>
        /// Command để xác định loại gói tin.
        /// </summary>
        public short Cmd { get; protected set; } = 0;

        /// <summary>
        /// Phương thức để thiết lập loại gói tin.
        /// </summary>
        public void SetType(PacketType type) => Type = type;

        /// <summary>
        /// Phương thức để thêm cờ trạng thái.
        /// </summary>
        public void EnableFlag(PacketFlags flag)
        {
            if (!HasFlag(flag))
                Flags |= flag;
        }

        /// <summary>
        /// Phương thức để loại bỏ cờ trạng thái.
        /// </summary>
        public void DisableFlag(PacketFlags flag) => Flags &= ~flag;

        /// <summary>
        /// Kiểm tra xem flag có tồn tại hay không.
        /// </summary>
        public bool HasFlag(PacketFlags flag) => (Flags & flag) == flag;

        /// <summary>
        /// Thiết lập Command mới cho gói tin.
        /// </summary>
        public void SetCmd(short cmd) => Cmd = cmd;

        /// <summary>
        /// Thiết lập giá trị lệnh từ một đối tượng enum bất kỳ.
        /// </summary>
        /// <param name="command">Đối tượng enum cần thiết lập.</param>
        public void SetCmd(object command)
        {
            if (command is Enum enumCommand)
            {
                Cmd = Convert.ToInt16(enumCommand); // Chuyển từ enum sang giá trị số (short)
            }
            else
            {
                // Ném ngoại lệ nếu không phải là enum hợp lệ
                throw new ArgumentException("Command must be an enum type.", nameof(command));
            }
        }
    }
}