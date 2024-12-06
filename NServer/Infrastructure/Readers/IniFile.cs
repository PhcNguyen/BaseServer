using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace NPServer.Infrastructure.Readers
{
    /// <summary>
    /// Đại diện cho một trình xử lý tệp INI để đọc và ghi các tệp cấu hình INI.
    /// </summary>
    public class IniFile
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// Khởi tạo một đối tượng <see cref="IniFile"/> mới với đường dẫn tệp đã chỉ định.
        /// </summary>
        /// <param name="target">Đường dẫn đến tệp INI.</param>
        public IniFile(string target)
        {
            FileName = target;
            FileExists = File.Exists(target);
        }

        /// <summary>
        /// Lấy hoặc đặt đường dẫn đến tệp INI.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Lấy giá trị chỉ ra liệu tệp INI có tồn tại hay không.
        /// </summary>
        private readonly bool FileExists;

        /// <summary>
        /// Đọc giá trị chuỗi từ tệp INI.
        /// </summary>
        /// <param name="section">Phần trong tệp INI.</param>
        /// <param name="key">Khóa trong phần.</param>
        /// <param name="default">Giá trị mặc định trả về nếu khóa không tồn tại.</param>
        /// <returns>Giá trị chuỗi từ tệp INI, hoặc giá trị mặc định nếu khóa không tồn tại.</returns>
        public string ReadString(string section, string key, string @default)
        {
            if (!FileExists)
                return @default;
            StringBuilder builder = new StringBuilder(255);
            _ = GetPrivateProfileString(section, key, @default, builder, 255, FileName);

            return builder.ToString();
        }

        /// <summary>
        /// Đọc giá trị số nguyên từ tệp INI.
        /// </summary>
        /// <param name="section">Phần trong tệp INI.</param>
        /// <param name="key">Khóa trong phần.</param>
        /// <param name="default">Giá trị mặc định trả về nếu khóa không tồn tại.</param>
        /// <returns>Giá trị số nguyên từ tệp INI, hoặc giá trị mặc định nếu khóa không tồn tại.</returns>
        public int ReadInteger(string section, string key, int @default)
        {
            if (!FileExists)
                return @default;
            return Convert.ToInt32(ReadString(section, key, @default.ToString()));
        }

        /// <summary>
        /// Đọc giá trị boolean từ tệp INI.
        /// </summary>
        /// <param name="section">Phần trong tệp INI.</param>
        /// <param name="key">Khóa trong phần.</param>
        /// <param name="default">Giá trị mặc định trả về nếu khóa không tồn tại.</param>
        /// <returns>Giá trị boolean từ tệp INI, hoặc giá trị mặc định nếu khóa không tồn tại.</returns>
        public bool ReadBool(string section, string key, bool @default)
        {
            if (!FileExists)
                return @default;
            return Convert.ToBoolean(ReadString(section, key, Convert.ToString(@default)));
        }

        /// <summary>
        /// Ghi giá trị chuỗi vào tệp INI.
        /// </summary>
        /// <param name="section">Phần trong tệp INI.</param>
        /// <param name="key">Khóa trong phần.</param>
        /// <param name="value">Giá trị cần ghi.</param>
        public void WriteString(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, FileName);
        }

        /// <summary>
        /// Ghi giá trị số nguyên vào tệp INI.
        /// </summary>
        /// <param name="section">Phần trong tệp INI.</param>
        /// <param name="key">Khóa trong phần.</param>
        /// <param name="value">Giá trị cần ghi.</param>
        public void WriteInteger(string section, string key, int value)
        {
            WriteString(section, key, value.ToString());
        }

        /// <summary>
        /// Ghi giá trị boolean vào tệp INI.
        /// </summary>
        /// <param name="section">Phần trong tệp INI.</param>
        /// <param name="key">Khóa trong phần.</param>
        /// <param name="value">Giá trị cần ghi.</param>
        public void WriteBool(string section, string key, bool value)
        {
            WriteString(section, key, Convert.ToString(value));
        }
    }
}