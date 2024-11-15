using NETServer.Application.Enums;

using System.Net;
using System.Net.Sockets;

namespace NETServer.Infrastructure.Services
{
    internal class ValidationService
    {
        public static void EnsureNotNull<T>(T arg, string paramName) where T : class
        {
            if (arg == null)
                throw new ArgumentNullException(paramName);
        }

        public static string GetClientAddress(TcpClient? tcpClient)
        {
            ArgumentNullException.ThrowIfNull(tcpClient);

            var clientEndPoint = tcpClient.Client.RemoteEndPoint as IPEndPoint
                                 ?? throw new InvalidOperationException("Client address is invalid.");

            return clientEndPoint.Address.ToString();
        }


        public static void ValidateStream(Stream? stream)
        {
            if (stream == null || !stream.CanRead || !stream.CanWrite)
                throw new InvalidOperationException("Stream is invalid or connection is closed.");
        }

        public static ValidationStatus ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return ValidationStatus.Invalid; // Email không hợp lệ nếu rỗng hoặc null
            }

            // Kiểm tra phần local-part và domain
            int atIndex = email.IndexOf('@');
            if (atIndex <= 0 || atIndex == email.Length - 1)
            {
                return ValidationStatus.Invalid; // Kiểm tra vị trí dấu '@'
            }

            string localPart = email[..atIndex];
            string domainPart = email[(atIndex + 1)..];

            if (!IsValidLocalPart(localPart) || !IsValidDomainPart(domainPart))
            {
                return ValidationStatus.Invalid; // Phần email không hợp lệ
            }

            return ValidationStatus.Valid;
        }

        public static ValidationStatus ValidatePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return ValidationStatus.Invalid;
            }

            // Loại bỏ các ký tự không cần thiết như dấu cách, dấu gạch ngang, dấu ngoặc
            phoneNumber = phoneNumber.Replace("-", "")
                                     .Replace(" ", "")
                                     .Replace("(", "")
                                     .Replace(")", "")
                                     .Replace("+", "");

            // Kiểm tra nếu tất cả là số
            foreach (char c in phoneNumber)
            {
                if (!char.IsDigit(c))
                {
                    return ValidationStatus.Invalid;
                }
            }

            // Kiểm tra độ dài của số điện thoại (ví dụ: 10 chữ số cho số điện thoại VN hoặc >= 11 cho quốc tế)
            if (phoneNumber.Length == 10 || phoneNumber.Length >= 11)
            {
                return ValidationStatus.Valid;
            }

            return ValidationStatus.Invalid; // Nếu không đủ độ dài
        }


        public static ValidationStatus ValidateCommand(Cmd command)
        {
            // Kiểm tra command có nằm trong enum Command và hợp lệ
            if (Enum.IsDefined(typeof(Cmd), command))
            {
                return ValidationStatus.Valid;
            }

            return ValidationStatus.Invalid;
        }

        public static ValidationStatus ValidateData(byte[] data)
        {
            if (data == null)
            {
                return ValidationStatus.Invalid; // Không hợp lệ nếu data là null
            }

            if (data.Length == 0)
            {
                return ValidationStatus.Empty; // Data rỗng
            }

            if (data.Length > 1024 * 1024) // Giới hạn 1MB
            {
                return ValidationStatus.TooLarge; // Data quá lớn
            }

            return ValidationStatus.Valid;
        }

        /// <summary>
        /// Kiểm tra một địa chỉ IP có hợp lệ không.
        /// </summary>
        public static ValidationStatus ValidateIpAddress(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                return ValidationStatus.Invalid;
            }

            if (IPAddress.TryParse(ipAddress, out _))
            {
                return ValidationStatus.Valid;
            }
            return ValidationStatus.Invalid;
        }

        /// <summary>
        /// Kiểm tra định dạng ngày tháng hợp lệ.
        /// </summary>
        public static ValidationStatus ValidateDateTime(string dateTime)
        {
            if (string.IsNullOrEmpty(dateTime))
            {
                return ValidationStatus.Invalid;
            }

            if (DateTime.TryParse(dateTime, out _))
            {
                return ValidationStatus.Valid;
            }

            return ValidationStatus.Invalid;
        }

        private static bool IsValidLocalPart(string localPart)
        {
            if (string.IsNullOrEmpty(localPart) || localPart.Length > 64 ||
                localPart.StartsWith('.') || localPart.EndsWith('.') || localPart.Contains(".."))
                return false;

            return localPart.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_');
        }

        private static bool IsValidDomainPart(string domainPart)
        {
            if (string.IsNullOrEmpty(domainPart) || domainPart.Length > 255)
                return false;

            string[] labels = domainPart.Split('.');
            if (labels.Length < 2)
                return false;

            foreach (string label in labels)
            {
                if (string.IsNullOrEmpty(label) || label.Length > 63 || label.StartsWith('-') || label.EndsWith('-'))
                    return false;

                if (label.Any(c => !char.IsLetterOrDigit(c) && c != '-'))
                    return false;
            }

            return true;
        }
    }
}