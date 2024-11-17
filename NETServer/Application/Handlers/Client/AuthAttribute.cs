using NETServer.Database;
using NETServer.Infrastructure.Services;
using NETServer.Infrastructure.Logging;
using NETServer.Infrastructure.Interfaces;
using NETServer.Infrastructure.Helper;
using NETServer.Application.Handlers;

namespace NETServer.Application.Handlers.Client
{
    internal class AuthAttribute
    {
        [Command(Cmd.REGISTER)]
        public static async Task Register(IClientSession session, byte[] data)
        {
            string result = ByteConverter.ToString(data);

            // Giả sử dữ liệu được phân tách bằng dấu "|"
            string email = result.Split('|')[0];
            string password = result.Split('|')[1];

            // Kiểm tra tính hợp lệ của email 
            if (!Validator.IsEmailValid(email))
            {
                await session.Transport.SendAsync((short)Cmd.ERROR, "Invalid email format.");
                return;
            }

            // Kiểm tra tính hợp lệ của password
            if (!Validator.IsPasswordValid(password))
            {
                await session.Transport.SendAsync((short)Cmd.ERROR, "Password does not meet the required criteria.");
                return;
            }

            // Nếu email và password hợp lệ, tiếp tục với việc lưu vào cơ sở dữ liệu
            try
            {
                // Thực thi câu lệnh SQL để lưu thông tin người dùng vào cơ sở dữ liệu
                string query = "INSERT INTO account (email, password) VALUES (@params0, @params1)";

                if (await PostgreManager.ExecuteAsync(query, email, password))
                {
                    await session.Transport.SendAsync((short)Cmd.SUCCESS, "Registration successful.");
                }
                else
                {
                    await session.Transport.SendAsync((short)Cmd.ERROR, "Registration failed. Please try again.");
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi khi kết nối hoặc thực thi lệnh SQL
                NLog.Error($"Registration failed for {email}: {ex.Message}");
                await session.Transport.SendAsync((short)Cmd.ERROR, "An error occurred during registration.");
            }
        }

        [Command(Cmd.LOGIN)]
        public void Login(IClientSession session, byte[] data)
        {

        }


        public void ChangePassword(IClientSession session, byte[] data) { }
    }
}
