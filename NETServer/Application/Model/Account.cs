namespace NETServer.Application.Model
{
    /// <summary>
    /// Đại diện cho tài khoản người dùng trong hệ thống.
    /// </summary>
    public class Account
    {
        /// <summary>
        /// ID của tài khoản, tự động tăng.
        /// </summary>
        public int Id { get; set; } 

        /// <summary>
        /// Địa chỉ email, duy nhất.
        /// </summary>
        public string? Email { get; set; } 

        /// <summary>
        /// Mật khẩu.
        /// </summary>
        public string? Password { get; set; } 

        /// <summary>
        /// Trạng thái khóa tài khoản, mặc định là không khóa.
        /// </summary>
        public bool Ban { get; set; } = false; 

        /// <summary>
        /// Quyền hạn, mặc định là quyền người dùng bình thường.
        /// </summary>
        public bool Role { get; set; } = false;

        /// <summary>
        /// Trạng thái trực tuyến, mặc định là không trực tuyến.
        /// </summary>
        public bool Active { get; set; } = false;

        /// <summary>
        /// Thời gian đăng nhập lần cuối.
        /// </summary>
        public DateTime LastLogin { get; set; } = DateTime.Now; 

        /// <summary>
        /// Thời gian tạo bản ghi.
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
