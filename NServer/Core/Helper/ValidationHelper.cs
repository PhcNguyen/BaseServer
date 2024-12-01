using System;
using System.Linq;
using System.Net;

namespace NServer.Core.Helper
{
    public static class ValidationHelper
    {
        public static bool ValidateInput(string[] parts, int expectedLength)
        {
            return parts.Length == expectedLength;
        }

        /// <summary>
        /// Kiểm tra định dạng email có hợp lệ không.
        /// </summary>
        /// <param name="email">Email cần kiểm tra.</param>
        /// <returns>True nếu hợp lệ, ngược lại là false.</returns>
        public static bool IsEmailValid(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            var emailSpan = email.AsSpan();
            int atIndex = emailSpan.IndexOf('@');
            if (atIndex <= 0 || atIndex == emailSpan.Length - 1)
                return false;

            var localPart = emailSpan[..atIndex];
            var domainPart = emailSpan[(atIndex + 1)..];

            return IsValidLocalPart(localPart) && IsValidDomainPart(domainPart);
        }

        /// <summary>
        /// Kiểm tra mật khẩu có hợp lệ hay không.
        /// </summary>
        /// <param name="password">Mật khẩu cần kiểm tra.</param>
        /// <returns>True nếu mật khẩu hợp lệ, ngược lại là False.</returns>
        public static bool IsPasswordValid(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            bool hasLowerCase = false, hasUpperCase = false, hasDigit = false, hasSpecialChar = false;
            char[] specialChars = "!@#$%^&*(),.?\"{}|<>".ToCharArray();

            foreach (var c in password)
            {
                switch (c)
                {
                    case var _ when char.IsLower(c): hasLowerCase = true; break;
                    case var _ when char.IsUpper(c): hasUpperCase = true; break;
                    case var _ when char.IsDigit(c): hasDigit = true; break;
                    case var _ when specialChars.Contains(c): hasSpecialChar = true; break;
                }

                if (hasLowerCase && hasUpperCase && hasDigit && hasSpecialChar)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Kiểm tra số điện thoại có hợp lệ không.
        /// </summary>
        /// <param name="phoneNumber">Số điện thoại cần kiểm tra.</param>
        /// <returns>True nếu hợp lệ, ngược lại là false.</returns>
        public static bool IsPhoneNumberValid(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return false;

            int digitCount = 0;

            foreach (var c in phoneNumber)
            {
                if (char.IsDigit(c))
                {
                    digitCount++;
                }
                else if (!(c is '-' or ' ' or '(' or ')' or '+'))
                {
                    return false;
                }
            }

            return digitCount == 10 || digitCount >= 11;
        }

        /// <summary>
        /// Kiểm tra dữ liệu có hợp lệ không.
        /// </summary>
        /// <param name="data">Mảng byte cần kiểm tra.</param>
        /// <returns>True nếu hợp lệ, ngược lại là false.</returns>
        public static bool IsDataValid(byte[] data) => data is { Length: > 0 and <= 1024 * 1024 };

        /// <summary>
        /// Kiểm tra địa chỉ IP có hợp lệ không.
        /// </summary>
        /// <param name="ipAddress">Chuỗi địa chỉ IP cần kiểm tra.</param>
        /// <returns>True nếu hợp lệ, ngược lại là false.</returns>
        public static bool IsIpAddressValid(string ipAddress)
        {
            return !string.IsNullOrEmpty(ipAddress) && IPAddress.TryParse(ipAddress, out _);
        }

        /// <summary>
        /// Kiểm tra định dạng ngày tháng có hợp lệ không.
        /// </summary>
        /// <param name="dateTime">Chuỗi ngày tháng cần kiểm tra.</param>
        /// <returns>True nếu hợp lệ, ngược lại là false.</returns>
        public static bool IsDateTimeValid(string dateTime)
        {
            return !string.IsNullOrEmpty(dateTime) && DateTime.TryParse(dateTime, out _);
        }

        /// <summary>
        /// Kiểm tra phần local-part của email có hợp lệ không.
        /// </summary>
        /// <param name="localPart">Phần local-part của email.</param>
        /// <returns>True nếu hợp lệ, ngược lại là false.</returns>
        private static bool IsValidLocalPart(ReadOnlySpan<char> localPart)
        {
            if (localPart.IsEmpty || localPart.Length > 64 ||
                localPart[0] == '.' || localPart[^1] == '.' || localPart.ToString().Contains(".."))
                return false;

            foreach (var c in localPart)
            {
                if (!(char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_'))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Kiểm tra phần domain-part của email có hợp lệ không.
        /// </summary>
        /// <param name="domainPart">Phần domain-part của email.</param>
        /// <returns>True nếu hợp lệ, ngược lại là false.</returns>
        private static bool IsValidDomainPart(ReadOnlySpan<char> domainPart)
        {
            if (domainPart.IsEmpty || domainPart.Length > 255)
                return false;

            int labelStart = 0;

            for (int i = 0; i <= domainPart.Length; i++)
            {
                // Tìm dấu chấm hoặc kết thúc chuỗi
                if (i == domainPart.Length || domainPart[i] == '.')
                {
                    var label = domainPart[labelStart..i];

                    // Kiểm tra các điều kiện của nhãn
                    if (label.IsEmpty || label.Length > 63 ||
                        label[0] == '-' || label[^1] == '-' ||
                        !IsLabelValid(label))
                        return false;

                    labelStart = i + 1; // Bắt đầu nhãn mới
                }
            }

            return true;
        }

        private static bool IsLabelValid(ReadOnlySpan<char> label)
        {
            foreach (var c in label)
            {
                if (!(char.IsLetterOrDigit(c) || c == '-'))
                    return false;
            }
            return true;
        }
    }
}