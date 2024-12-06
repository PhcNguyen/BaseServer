using System.Collections.Generic;

namespace NPServer.Application.Helper
{
    /// <summary>
    /// Cung cấp các phương thức để kiểm tra tính hợp lệ của số điện thoại.
    /// </summary>
    public static class PhoneNumberValidator
    {
        // Tập hợp các ký tự đặc biệt hợp lệ
        private static readonly HashSet<char> ValidSpecialChars = ['-', ' ', '(', ')', '+'];

        /// <summary>
        /// Kiểm tra xem số điện thoại có hợp lệ hay không.
        /// </summary>
        /// <param name="phoneNumber">Số điện thoại cần kiểm tra.</param>
        /// <returns>Trả về true nếu số điện thoại hợp lệ, ngược lại trả về false.</returns>
        /// <remarks>
        /// Một số điện thoại hợp lệ phải đáp ứng các yêu cầu sau:
        /// 1. Chứa từ 10 đến 11 chữ số.
        /// 2. Có thể chứa các ký tự đặc biệt như '-', ' ', '(', ')', và '+' để phân tách số.
        /// 3. Chỉ chứa các ký tự số hoặc các ký tự đặc biệt đã liệt kê.
        /// </remarks>
        public static bool IsPhoneNumberValid(string phoneNumber)
        {
            // Kiểm tra nếu số điện thoại là null hoặc rỗng
            if (string.IsNullOrEmpty(phoneNumber))
                return false;

            int digitCount = 0;

            // Duyệt qua từng ký tự trong số điện thoại
            foreach (var c in phoneNumber)
            {
                if (char.IsDigit(c))
                {
                    // Nếu là chữ số, tăng biến đếm
                    digitCount++;
                }
                else if (!ValidSpecialChars.Contains(c))
                {
                    // Nếu ký tự không hợp lệ và không phải là ký tự đặc biệt, trả về false ngay lập tức
                    return false;
                }
            }

            // Trả về true nếu số điện thoại có từ 10 đến 11 chữ số
            return digitCount >= 10 && digitCount <= 11;
        }
    }
}