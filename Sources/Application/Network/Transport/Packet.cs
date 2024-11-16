namespace NETServer.Application.Network.Transport
{
    public class Packet(byte[] command, byte[] payload)
    {
        public byte[] Command { get; } = command ?? throw new ArgumentNullException(nameof(command));
        public byte[] Payload { get; private set; } = payload ?? throw new ArgumentNullException(nameof(payload));

        public int Length => Command.Length + Payload.Length;

        public void UpdatePayload(byte[] newPayload)
        {
            Payload = newPayload ?? throw new ArgumentNullException(nameof(newPayload));
        }

        // Phương thức giúp đóng gói gói tin thành một mảng byte để truyền qua mạng
        public byte[] ToByteArray()
        {
            byte[] lengthBytes = BitConverter.GetBytes(Length); // 4 byte cho chiều dài gói tin
            byte[] packet = new byte[Length + 4]; // Tổng cộng 4 byte chiều dài + dữ liệu gói tin

            Buffer.BlockCopy(lengthBytes, 0, packet, 0, 4); // Copy độ dài vào
            Buffer.BlockCopy(Command, 0, packet, 4, Command.Length); // Copy command vào
            Buffer.BlockCopy(Payload, 0, packet, 4 + Command.Length, Payload.Length); // Copy payload vào

            return packet;
        }

        // Phương thức để tạo một gói tin từ một mảng byte nhận được
        public static Packet FromByteArray(byte[] data)
        {
            if (data == null || data.Length < 4)
                throw new ArgumentException("Invalid data for packet.");

            int length = BitConverter.ToInt32(data, 0); // Lấy độ dài gói tin
            byte[] command = new byte[1]; // command là 1 byte
            byte[] payload = new byte[length - 1];

            command[0] = data[4]; // Command bắt đầu từ byte thứ 5
            Buffer.BlockCopy(data, 5, payload, 0, payload.Length); // Payload bắt đầu từ byte thứ 6

            return new Packet(command, payload);
        }
    }
}
