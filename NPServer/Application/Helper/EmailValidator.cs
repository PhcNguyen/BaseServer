using System;

namespace NPServer.Application.Helper
{
    public static class EmailValidator
    {
        /// <summary>
        /// Kiểm tra xem địa chỉ email có hợp lệ không.
        /// </summary>
        /// <param name="email">Địa chỉ email cần kiểm tra.</param>
        /// <returns>Trả về true nếu hợp lệ, ngược lại là false.</returns>
        public static bool IsEmailValid(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            var emailSpan = email.AsSpan();
            int atIndex = emailSpan.IndexOf('@');

            // Kiểm tra nếu không có dấu '@' hoặc vị trí không hợp lệ
            if (atIndex <= 0 || atIndex == emailSpan.Length - 1)
                return false;

            var localPart = emailSpan[..atIndex];
            var domainPart = emailSpan[(atIndex + 1)..];

            return IsValidLocalPart(localPart) && IsValidDomainPart(domainPart);
        }

        private static bool IsValidLocalPart(ReadOnlySpan<char> localPart)
        {
            if (localPart.IsEmpty || localPart.Length > 64 ||
                localPart[0] == '.' || localPart[^1] == '.' || ContainsDoubleDot(localPart))
                return false;

            foreach (var c in localPart)
            {
                if (!(char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_'))
                    return false;
            }
            return true;
        }

        private static bool ContainsDoubleDot(ReadOnlySpan<char> span)
        {
            for (int i = 0; i < span.Length - 1; i++)
            {
                if (span[i] == '.' && span[i + 1] == '.')
                    return true;
            }
            return false;
        }

        private static bool IsValidDomainPart(ReadOnlySpan<char> domainPart)
        {
            if (domainPart.IsEmpty || domainPart.Length > 255)
                return false;

            int labelStart = 0;

            for (int i = 0; i <= domainPart.Length; i++)
            {
                // Tìm dấu '.' hoặc kết thúc chuỗi
                if (i == domainPart.Length || domainPart[i] == '.')
                {
                    var label = domainPart.Slice(labelStart, i - labelStart);

                    // Kiểm tra độ dài và ký tự hợp lệ của nhãn
                    if (label.IsEmpty || label.Length > 63 ||
                        label[0] == '-' || label[^1] == '-' || !IsLabelValid(label))
                        return false;

                    labelStart = i + 1;
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