using NPServer.Infrastructure.Random;

namespace NPServer.Core.Services
{
    /// <summary>
    /// Đại diện cho một ID phiên duy nhất.
    /// </summary>
    /// <remarks>
    /// Khởi tạo một thể hiện mới của lớp <see cref="UniqueId"/> với giá trị được chỉ định.
    /// </remarks>
    /// <param name="value">Giá trị của ID.</param>
    public readonly struct UniqueId(uint value) : System.IEquatable<UniqueId>, System.IComparable<UniqueId>
    {
        private const string Alphabet = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const int Base = 36;

        private static readonly byte[] CharToValue = CreateCharToValueMap();
        private readonly uint _value = value;

        /// <summary>
        /// ID Default
        /// </summary>
        public static readonly UniqueId Empty = new(0);

        /// <summary>
        /// Tạo bảng ánh xạ ký tự sang giá trị số để tăng tốc độ tra cứu.
        /// </summary>
        /// <returns>Bảng ánh xạ ký tự sang giá trị số.</returns>
        private static byte[] CreateCharToValueMap()
        {
            var map = new byte[128]; // ASCII table size
            for (int i = 0; i < map.Length; i++) map[i] = byte.MaxValue;

            for (byte i = 0; i < Alphabet.Length; i++)
            {
                map[Alphabet[i]] = i;
            }

            return map;
        }

        /// <summary>
        /// Tạo ID mới từ các yếu tố ngẫu nhiên và hệ thống.
        /// </summary>
        /// <returns>ID gọn nhẹ mới.</returns>
        public static UniqueId NewId()
        {
            // Sinh giá trị ngẫu nhiên mạnh (4 byte)
            var buffer = new byte[4];
            ThreadLocalRandom.Fill(buffer);
            uint randomValue = System.BitConverter.ToUInt32(buffer, 0);

            // Lấy timestamp hiện tại (Unix Time)
            uint timestamp = (uint)(System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() & 0xFFFFFFFF);

            // Kết hợp giá trị ngẫu nhiên và timestamp
            uint uniqueValue = randomValue ^ (timestamp << 5 | timestamp >> 27);

            return new UniqueId(uniqueValue);
        }

        /// <summary>
        /// Chuyển đổi ID thành chuỗi Base36.
        /// </summary>
        /// <returns>Chuỗi đại diện ID.</returns>
        public override string ToString()
        {
            System.Span<char> buffer = stackalloc char[13];
            int index = buffer.Length;
            uint mvalue = _value;

            while (mvalue > 0)
            {
                buffer[--index] = Alphabet[(int)(mvalue % Base)];
                mvalue /= Base;
            }

            while (index > buffer.Length - 7) // Đảm bảo độ dài tối thiểu 7 ký tự
            {
                buffer[--index] = '0';
            }

            return new string(buffer[index..]);
        }

        /// <summary>
        /// Chuyển đổi chuỗi Base36 thành <see cref="UniqueId"/>.
        /// </summary>
        /// <param name="input">Chuỗi cần chuyển đổi.</param>
        /// <returns>Đối tượng <see cref="UniqueId"/> từ chuỗi đã cho.</returns>
        /// <exception cref="ArgumentNullException">Ném ra nếu chuỗi nhập vào rỗng hoặc chỉ chứa khoảng trắng.</exception>
        /// <exception cref="ArgumentException">Ném ra nếu chuỗi nhập vào dài quá.</exception>
        /// <exception cref="FormatException">Ném ra nếu chuỗi nhập vào chứa ký tự không hợp lệ.</exception>
        public static UniqueId Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new System.ArgumentNullException(nameof(input));

            if (input.Length > 13)
                throw new System.ArgumentException("Input is too long to be a valid UniqueId.", nameof(input));

            uint value = 0;
            foreach (char c in input.ToUpperInvariant())
            {
                if (c > 127 || CharToValue[c] == byte.MaxValue)
                    throw new System.FormatException($"Invalid character '{c}' in input string.");
                value = value * Base + CharToValue[c];
            }

            return new UniqueId(value);
        }

        /// <summary>
        /// Xác định xem thể hiện hiện tại và đối tượng đã chỉ định có bằng nhau hay không.
        /// </summary>
        /// <param name="obj">Đối tượng để so sánh với thể hiện hiện tại.</param>
        /// <returns>true nếu đối tượng hiện tại bằng đối tượng đã chỉ định; ngược lại, false.</returns>
        public override bool Equals(object? obj) => obj is UniqueId other && Equals(other);

        /// <summary>
        /// Xác định xem thể hiện hiện tại và <see cref="UniqueId"/> đã chỉ định có bằng nhau hay không.
        /// </summary>
        /// <param name="other">Đối tượng <see cref="UniqueId"/> để so sánh với thể hiện hiện tại.</param>
        /// <returns>true nếu đối tượng hiện tại bằng <see cref="UniqueId"/> đã chỉ định; ngược lại, false.</returns>
        public bool Equals(UniqueId other) => _value == other._value;

        /// <summary>
        /// Trả về mã băm cho thể hiện hiện tại.
        /// </summary>
        /// <returns>Mã băm 32-bit có dấu cho thể hiện hiện tại.</returns>
        public override int GetHashCode() => _value.GetHashCode();

        /// <summary>
        /// So sánh thể hiện hiện tại với một <see cref="UniqueId"/> khác và trả về một số nguyên cho biết thứ tự tương đối của các đối tượng được so sánh.
        /// </summary>
        /// <param name="other">Đối tượng <see cref="UniqueId"/> để so sánh.</param>
        /// <returns>Một số nguyên cho biết thứ tự tương đối của các đối tượng được so sánh.</returns>
        public int CompareTo(UniqueId other) => _value.CompareTo(other._value);

        /// <summary>
        /// Xác định xem hai đối tượng <see cref="UniqueId"/> có bằng nhau hay không.
        /// </summary>
        /// <param name="left">Đối tượng đầu tiên để so sánh.</param>
        /// <param name="right">Đối tượng thứ hai để so sánh.</param>
        /// <returns>true nếu các đối tượng bằng nhau; ngược lại, false.</returns>
        public static bool operator ==(UniqueId left, UniqueId right) => left.Equals(right);

        /// <summary>
        /// Xác định xem hai đối tượng <see cref="UniqueId"/> có khác nhau hay không.
        /// </summary>
        /// <param name="left">Đối tượng đầu tiên để so sánh.</param>
        /// <param name="right">Đối tượng thứ hai để so sánh.</param>
        /// <returns>true nếu các đối tượng khác nhau; ngược lại, false.</returns>
        public static bool operator !=(UniqueId left, UniqueId right) => !(left == right);
    }
}