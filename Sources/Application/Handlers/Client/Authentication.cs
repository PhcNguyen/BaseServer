using NETServer.Application.Helper;
using NETServer.Infrastructure.Services;
using NETServer.Infrastructure.Interfaces;
using NETServer.Database;
using NETServer.Infrastructure.Logging;

namespace NETServer.Application.Handlers.Client
{
    internal class Authentication
    {
        public async Task Register(IClientSession session, byte[] data)
        {
            string result = ByteConverter.ToString(data);

            // Giả sử dữ liệu được phân tách bằng dấu "|"
            string username = result.Split('|')[0];
            string password = result.Split('|')[1];

            // Kiểm tra tính hợp lệ của email (username)
            if (!Validator.IsEmailValid(username))
            {
                await session.Transport.SendAsync("Invalid email format.");
                return;
            }

            // Kiểm tra tính hợp lệ của password
            if (!Validator.IsPasswordValid(password))
            {
                await session.Transport.SendAsync("Password does not meet the required criteria.");
                return;
            }

            // Nếu username và password hợp lệ, tiếp tục với việc lưu vào cơ sở dữ liệu
            try
            {
                // Thực thi câu lệnh SQL để lưu thông tin người dùng vào cơ sở dữ liệu
                var commandText = "INSERT INTO users (username, password) VALUES (@Username, @Password)";

                if (await PostgreManager.ExecuteAsync(commandText, username, password))
                {
                    await session.Transport.SendAsync("Registration successful.");
                }
                else
                {
                    await session.Transport.SendAsync("Registration failed. Please try again.");
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi khi kết nối hoặc thực thi lệnh SQL
                NLog.Error($"Registration failed for {username}: {ex.Message}");
                await session.Transport.SendAsync("An error occurred during registration.");
            }
        }
        public void Login(IClientSession session, byte[] data) { }
        public void ChangePassword(IClientSession session, byte[] data) { }
    }
}
