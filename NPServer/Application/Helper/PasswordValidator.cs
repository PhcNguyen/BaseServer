using System.Collections.Generic;

namespace NPServer.Application.Helper
{
    /// <summary>
    /// Cung cấp các phương thức để kiểm tra độ mạnh của mật khẩu.
    /// </summary>
    public static class PasswordValidator
    {
        private static readonly HashSet<char> SpecialChars = new("!@#$%^&*(),.?\"{}|<>");

        /// <summary>
        /// Kiểm tra xem mật khẩu có đáp ứng các yêu cầu độ mạnh hay không.
        /// </summary>
        /// <param name="password">Mật khẩu cần kiểm tra.</param>
        /// <returns>Trả về true nếu mật khẩu hợp lệ, ngược lại trả về false.</returns>
        /// <remarks>
        /// Một mật khẩu hợp lệ phải đáp ứng các điều kiện sau:
        /// 1. Độ dài ít nhất 8 ký tự.
        /// 2. Có ít nhất một ký tự chữ thường.
        /// 3. Có ít nhất một ký tự chữ hoa.
        /// 4. Có ít nhất một chữ số.
        /// 5. Có ít nhất một ký tự đặc biệt từ tập ký tự đã định nghĩa.
        /// </remarks>
        public static bool IsPasswordValid(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            bool hasLowerCase = false, hasUpperCase = false, hasDigit = false, hasSpecialChar = false;

            foreach (var c in password)
            {
                if (char.IsLower(c)) hasLowerCase = true;
                else if (char.IsUpper(c)) hasUpperCase = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else if (SpecialChars.Contains(c)) hasSpecialChar = true;
            }

            return hasLowerCase && hasUpperCase && hasDigit && hasSpecialChar;
        }
    }
}