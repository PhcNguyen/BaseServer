using FreeSql.DataAnnotations;
using NPServer.Common.Models;

namespace NPServer.Database
{
    /// <summary>
    /// Lớp đại diện cho bảng "user" trong cơ sở dữ liệu.
    /// </summary>
    /// <remarks>
    /// Hàm khởi tạo cho lớp DbUser.
    /// </remarks>
    /// <param name="username">Tên người dùng.</param>
    /// <param name="password">Mật khẩu.</param>
    /// <param name="authority">Quyền hạn của người dùng.</param>
    [Table(Name = "user")]
    public class DbUser(string username, string password, Authoritys authority)
    {
        /// <summary>
        /// ID của người dùng, là khóa chính và tự tăng.
        /// </summary>
        [Column(IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }

        /// <summary>
        /// Tên người dùng.
        /// </summary>
        public string Username { get; set; } = username;

        /// <summary>
        /// Mật khẩu của người dùng.
        /// </summary>
        public string Password { get; set; } = password;

        /// <summary>
        /// Quyền hạn của người dùng.
        /// </summary>
        public Authoritys Authority { get; set; } = authority;
    }
}