using NETServer.Application.Handlers;

namespace NETServer.Application.Helper
{
    internal class ValidationHelper
    {
        public static bool IsValidEmail(string email)
        {
            // Kiểm tra email hợp lệ
            return true;
        }

        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            // Kiểm tra số điện thoại hợp lệ
            return true;
        }

        public static bool IsValidCommand(Command command)
        {
            // Kiểm tra command có nằm trong enum Command và hợp lệ
            return Enum.IsDefined(typeof(Command), command);
        }

        public static bool IsValidData(byte[] data)
        {
            // Kiểm tra kích thước và nội dung của data
            return data.Length > 0 && data.Length <= 1024 * 1024;
        }
    }
}

