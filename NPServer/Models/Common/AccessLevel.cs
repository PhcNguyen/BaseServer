namespace NPServer.Models.Common
{
    public enum AccessLevel : byte
    {
        /// <summary>
        /// Người dùng chưa đăng nhập hoặc đăng ký.
        /// </summary>
        Guests = 0,

        /// <summary>
        /// Người dùng đã đăng ký tiêu chuẩn với quyền truy cập cơ bản.
        /// </summary>
        User = 1,

        /// <summary>
        /// Người dùng có quyền hạn tăng cao, có thể quản lý nội dung,
        /// quản lý người dùng hoặc truy cập các tính năng cụ thể.
        /// </summary>
        Supervisor = 2,

        /// <summary>
        /// Cấp độ truy cập cao nhất, có quyền kiểm soát hoàn toàn hệ thống,
        /// quản lý người dùng, quản trị nội dung và cấu hình hệ thống.
        /// </summary>
        Admin = 3
    }
}