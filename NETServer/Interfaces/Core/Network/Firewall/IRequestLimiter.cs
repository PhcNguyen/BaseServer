namespace NETServer.Interfaces.Core.Network.Firewall
{
    /// <summary>
    /// Interface xử lý giới hạn số lượng yêu cầu của mỗi địa chỉ IP trong một khoảng thời gian nhất định.
    /// </summary>
    internal interface IRequestLimiter
    {
        /// <summary>
        /// Kiểm tra xem địa chỉ IP có được phép gửi yêu cầu hay không, dựa trên số lượng yêu cầu và tình trạng khóa.
        /// </summary>
        /// <param name="ipAddress">Địa chỉ IP cần kiểm tra.</param>
        /// <returns>True nếu yêu cầu được phép, False nếu không.</returns>
        bool IsAllowed(string ipAddress);

        /// <summary>
        /// Phương thức làm sạch các IP không còn yêu cầu trong danh sách.
        /// </summary>
        /// <param name="cancellationToken">Token hủy để kiểm soát việc hủy bỏ phương thức bất đồng bộ.</param>
        /// <returns>Task đại diện cho công việc làm sạch yêu cầu bất đồng bộ.</returns>
        Task ClearInactiveRequests(CancellationToken cancellationToken);
    }
}
