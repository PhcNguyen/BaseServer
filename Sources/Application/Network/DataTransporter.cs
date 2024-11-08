using System.Text;
using NETServer.Logging;

namespace NETServer.Application.Network
{
    internal class DataTransporter
    {
        private readonly Stream _stream;  // Stream dùng để nhận và gửi dữ liệu

        public DataTransporter(Stream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream), "Stream cannot be null.");
        }

        public async Task<string> ReceiveDataAsync()
        {
            try
            {
                if (!_stream.CanRead)
                {
                    NLog.Error("Stream is not readable.");
                    return string.Empty;
                }

                var buffer = new byte[1024];
                int bytesRead;
                var data = new StringBuilder();

                while ((bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    data.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                    // Có thể tùy chỉnh điều kiện dừng (chẳng hạn khi nhận được một ký tự đặc biệt hoặc hết dữ liệu)
                    if (data.ToString().Contains("\n"))
                    {
                        break;
                    }
                }

                // Trả về dữ liệu nhận được
                return data.ToString();
            }
            catch (Exception ex)
            {
                NLog.Error(ex);
                throw;  // Ném lại ngoại lệ để xử lý ở tầng cao hơn nếu cần thiết
            }
        }

        public async Task SendDataAsync(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                {
                    NLog.Warning("Attempted to send empty data.");
                    return;
                }

                if (!_stream.CanWrite)
                {
                    NLog.Error("Stream is not writable.");
                    return;
                }

                // Chuyển chuỗi thành mảng byte
                byte[] buffer = Encoding.UTF8.GetBytes(data);

                // Gửi dữ liệu đến client
                await _stream.WriteAsync(buffer, 0, buffer.Length);
                await _stream.FlushAsync();  // Đảm bảo dữ liệu được gửi ngay lập tức

                NLog.Info($"Sent data: {data}");
            }
            catch (Exception ex)
            {
                NLog.Error(ex);
                throw;  // Ném lại ngoại lệ nếu cần xử lý ở tầng cao hơn
            }
        }

        // Bạn có thể thêm các phương thức hỗ trợ, ví dụ như đọc dữ liệu theo một định dạng nhất định.
    }
}
