using NPServer.Models.Database;

namespace NPServer.DatabaseAccess
{
    /// <summary>
    /// Giao diện chung cho các cài đặt lưu trữ <see cref="DBAccount"/>.
    /// </summary>
    public interface IDBManager
    {
        /// <summary>
        /// Lấy hoặc thiết lập giá trị chỉ ra liệu có xác minh tài khoản (mật khẩu và cờ) hay không.
        /// Thiết lập giá trị này là <c>false</c> để tắt xác minh mật khẩu và cờ cho các tài khoản.
        /// </summary>
        /// <value><c>true</c> nếu xác minh tài khoản được bật; ngược lại, <c>false</c>.</value>
        public bool VerifyAccounts { get => true; }

        /// <summary>
        /// Khởi tạo kết nối đến cơ sở dữ liệu.
        /// </summary>
        /// <returns><c>true</c> nếu khởi tạo thành công; ngược lại, <c>false</c>.</returns>
        public bool Initialize();

        /// <summary>
        /// Truy vấn một <see cref="DBAccount"/> từ cơ sở dữ liệu bằng email của nó.
        /// </summary>
        /// <param name="email">Email của tài khoản cần truy vấn.</param>
        /// <param name="account">Tài khoản <see cref="DBAccount"/> được lấy từ cơ sở dữ liệu, nếu tìm thấy.</param>
        /// <returns><c>true</c> nếu tài khoản được tìm thấy; ngược lại, <c>false</c>.</returns>
        public bool TryQueryAccountByEmail(string email, out DBAccount account);

        /// <summary>
        /// Truy vấn xem tên người chơi có bị trùng hay không.
        /// </summary>
        /// <param name="playerName">Tên người chơi cần kiểm tra.</param>
        /// <returns><c>true</c> nếu tên người chơi đã được sử dụng; ngược lại, <c>false</c>.</returns>
        public bool QueryIsPlayerNameTaken(string playerName);

        /// <summary>
        /// Chèn một <see cref="DBAccount"/> mới với tất cả dữ liệu của nó vào cơ sở dữ liệu.
        /// </summary>
        /// <param name="account">Tài khoản cần chèn vào cơ sở dữ liệu.</param>
        /// <returns><c>true</c> nếu tài khoản đã được chèn thành công; ngược lại, <c>false</c>.</returns>
        public bool InsertAccount(DBAccount account);

        /// <summary>
        /// Cập nhật <see cref="DBAccount"/> trong cơ sở dữ liệu.
        /// </summary>
        /// <param name="account">Tài khoản với dữ liệu cập nhật.</param>
        /// <returns><c>true</c> nếu tài khoản được cập nhật thành công; ngược lại, <c>false</c>.</returns>
        public bool UpdateAccount(DBAccount account);

        /// <summary>
        /// Tải dữ liệu trò chơi lâu dài từ cơ sở dữ liệu cho tài khoản <see cref="DBAccount"/> đã cho.
        /// </summary>
        /// <param name="account">Tài khoản cần tải dữ liệu trò chơi.</param>
        /// <returns><c>true</c> nếu dữ liệu được tải thành công; ngược lại, <c>false</c>.</returns>
        public bool LoadPlayerData(DBAccount account);

        /// <summary>
        /// Lưu dữ liệu trò chơi lâu dài vào cơ sở dữ liệu cho tài khoản <see cref="DBAccount"/> đã cho.
        /// </summary>
        /// <param name="account">Tài khoản cần lưu dữ liệu trò chơi.</param>
        /// <returns><c>true</c> nếu dữ liệu được lưu thành công; ngược lại, <c>false</c>.</returns>
        public bool SavePlayerData(DBAccount account);
    }
}