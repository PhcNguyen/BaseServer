namespace NETServer.Interfaces.Core.Network.Firewall
{
    /// <summary>
    /// Lớp xử lý giới hạn số lượng kết nối đồng thời từ mỗi địa chỉ IP.
    /// </summary>
    internal interface IConnLimiter
    {
        /// <summary>
        /// Kiểm tra xem kết nối từ địa chỉ IP có được phép hay không, dựa trên số lượng kết nối hiện tại.
        /// </summary>
        /// <param name="ipAddress">Địa chỉ IP cần kiểm tra.</param>
        /// <returns>True nếu kết nối được phép, False nếu không.</returns>
        bool IsConnectionAllowed(string ipAddress);

        /// <summary>
        /// Phương thức gọi khi kết nối bị đóng từ một địa chỉ IP.
        /// </summary>
        /// <param name="ipAddress">Địa chỉ IP cần cập nhật sau khi kết nối đóng.</param>
        bool ConnectionClosed(string ipAddress);
    }
}
