using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace NETServer.Infrastructure.Helper
{
    internal class Validator
    {
        /// <summary>
        /// Kiểm tra định dạng email có hợp lệ không.
        /// </summary>
        /// <param name="email">Email cần kiểm tra.</param>
        /// <returns>True nếu hợp lệ, ngược lại là false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmailValid(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            int atIndex = email.IndexOf('@');
            if (atIndex <= 0 || atIndex == email.Length - 1)
                return false;

            string localPart = email[..atIndex];
            string domainPart = email[(atIndex + 1)..];

            return IsValidLocalPart(localPart) && IsValidDomainPart(domainPart);
        }

        /// <summary>
        /// Kiểm tra mật khẩu có hợp lệ hay không.
        /// </summary>
        /// <param name="password">Mật khẩu cần kiểm tra.</param>
        /// <returns>True nếu mật khẩu hợp lệ, ngược lại là False.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPasswordValid(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            char[] specialChars = "!@#$%^&*(),.?\"{}|<>".ToCharArray();

            // Kiểm tra các tiêu chí
            bool hasLowerCase = password.Any(char.IsLower);                    // Có chữ thường
            bool hasUpperCase = password.Any(char.IsUpper);                    // Có chữ hoa
            bool hasDigit = password.Any(char.IsDigit);                        // Có chữ số
            bool hasSpecialChar = password.Any(c => specialChars.Contains(c)); // Có ký tự đặc biệt

            return hasLowerCase && hasUpperCase && hasDigit && hasSpecialChar;
        }

        /// <summary>
        /// Kiểm tra số điện thoại có hợp lệ không.
        /// </summary>
        /// <param name="phoneNumber">Số điện thoại cần kiểm tra.</param>
        /// <returns>True nếu hợp lệ, ngược lại là false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPhoneNumberValid(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return false;

            phoneNumber = phoneNumber.Replace("-", "")
                                     .Replace(" ", "")
                                     .Replace("(", "")
                                     .Replace(")", "")
                                     .Replace("+", "");

            return phoneNumber.All(char.IsDigit) && (phoneNumber.Length == 10 || phoneNumber.Length >= 11);
        }

        /// <summary>
        /// Kiểm tra dữ liệu có hợp lệ không.
        /// </summary>
        /// <param name="data">Mảng byte cần kiểm tra.</param>
        /// <returns>True nếu hợp lệ, ngược lại là false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDataValid(byte[] data)
        {
            if (data == null || data.Length == 0 || data.Length > 1024 * 1024)
                return false;

            return true;
        }

        /// <summary>
        /// Kiểm tra địa chỉ IP có hợp lệ không.
        /// </summary>
        /// <param name="ipAddress">Chuỗi địa chỉ IP cần kiểm tra.</param>
        /// <returns>True nếu hợp lệ, ngược lại là false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsIpAddressValid(string ipAddress)
        {
            return !string.IsNullOrEmpty(ipAddress) && IPAddress.TryParse(ipAddress, out _);
        }

        /// <summary>
        /// Kiểm tra định dạng ngày tháng có hợp lệ không.
        /// </summary>
        /// <param name="dateTime">Chuỗi ngày tháng cần kiểm tra.</param>
        /// <returns>True nếu hợp lệ, ngược lại là false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDateTimeValid(string dateTime)
        {
            return !string.IsNullOrEmpty(dateTime) && DateTime.TryParse(dateTime, out _);
        }

        /// <summary>
        /// Kiểm tra phần local-part của email có hợp lệ không.
        /// </summary>
        /// <param name="localPart">Phần local-part của email.</param>
        /// <returns>True nếu hợp lệ, ngược lại là false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsValidLocalPart(string localPart)
        {
            if (string.IsNullOrEmpty(localPart) || localPart.Length > 64 ||
                localPart.StartsWith('.') || localPart.EndsWith('.') || localPart.Contains(".."))
                return false;

            return localPart.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_');
        }

        /// <summary>
        /// Kiểm tra phần domain-part của email có hợp lệ không.
        /// </summary>
        /// <param name="domainPart">Phần domain-part của email.</param>
        /// <returns>True nếu hợp lệ, ngược lại là false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsValidDomainPart(string domainPart)
        {
            if (string.IsNullOrEmpty(domainPart) || domainPart.Length > 255)
                return false;

            string[] labels = domainPart.Split('.');
            if (labels.Length < 2)
                return false;

            return labels.All(label =>
                !string.IsNullOrEmpty(label) && label.Length <= 63 &&
                !label.StartsWith('-') && !label.EndsWith('-') &&
                label.All(c => char.IsLetterOrDigit(c) || c == '-'));
        }
    }
}
