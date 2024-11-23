using System;
using System.Threading.Tasks;

using NServer.Core.Packet;
using NServer.Core.Database;
using NServer.Core.Security;
using NServer.Core.Database.Postgre;
using NServer.Infrastructure.Helper;
using NServer.Infrastructure.Logging;
using NServer.Interfaces.Core.Network;

namespace NServer.Application.Handler.Client
{
    internal class Authentication
    {
        private static readonly SqlExecutor _sqlExecutor = new(new NpgsqlFactory());

        [Command(Cmd.REGISTER)]
        public static async Task Register(ISession session, byte[] data)
        {
            Packets packet = new();
            string result = ConverterHelper.ToString(data);
            string[] parts = result.Split('|');

            if (parts.Length != 2)
            {
                packet.SetCommand((short)Cmd.ERROR);
                packet.SetPayload("Invalid registration data format.");

                session.Send(packet.ToByteArray());
                return;
            }

            string email = parts[0];
            string password = parts[1];

            if (!ValidatorHelper.IsEmailValid(email))
            {
                packet.SetCommand((short)Cmd.ERROR);
                packet.SetPayload("Invalid email format.");

                session.Send(packet.ToByteArray());
                return;
            }

            if (!ValidatorHelper.IsPasswordValid(password))
            {
                packet.SetCommand((short)Cmd.ERROR);
                packet.SetPayload("Password does not meet the required criteria.");

                session.Send(packet.ToByteArray());
                return;
            }

            int accountCount = await _sqlExecutor.ExecuteScalarAsync<int>(SqlCommand.SELECT_ACCOUNT_COUNT, email);
            if (accountCount > 0)
            {
                packet.SetCommand((short)Cmd.ERROR);
                packet.SetPayload("This email was used.");

                session.Send(packet.ToByteArray());
                return;
            }

            try
            {
                if (await _sqlExecutor.ExecuteAsync(SqlCommand.INSERT_ACCOUNT, email, PBKDF2.HashPassword(password)))
                {
                    packet.SetCommand((short)Cmd.SUCCESS);
                    packet.SetPayload("Registration successful.");

                    session.Send(packet.ToByteArray());
                }
                else
                {
                    packet.SetCommand((short)Cmd.ERROR);
                    packet.SetPayload("Registration failed. Please try again.");

                    session.Send(packet.ToByteArray());
                }
            }
            catch (Exception ex)
            {
                NLog.Error($"Registration failed for {email}: {ex.Message}");

                packet.SetCommand((short)Cmd.ERROR);
                packet.SetPayload("An error occurred during registration.");

                session.Send(packet.ToByteArray());
            }
        }

        [Command(Cmd.LOGIN)]
        public static async Task Login(ISession session, byte[] data)
        {
            Packets packet = new();
            string result = ConverterHelper.ToString(data);
            string[] parts = result.Split('|');

            if (parts.Length != 2)
            {
                packet.SetCommand((short)Cmd.ERROR);
                packet.SetPayload("Invalid login data format.");

                session.Send(packet.ToByteArray());
                return;
            }

            string email = parts[0].Trim();
            string password = parts[1].Trim();

            try
            {
                // Lấy mật khẩu đã hash từ cơ sở dữ liệu
                string hashedPassword = await _sqlExecutor.ExecuteScalarAsync<string>(
                    SqlCommand.SELECT_ACCOUNT_PASSWORD,
                    email
                );

                if (string.IsNullOrEmpty(hashedPassword))
                {
                    // Tài khoản không tồn tại
                    packet.SetCommand((short)Cmd.ERROR);
                    packet.SetPayload("Account not found.");

                    session.Send(packet.ToByteArray());
                    return;
                }

                // Xác thực mật khẩu
                if (PBKDF2.VerifyPassword(hashedPassword, password))
                {
                    // Đăng nhập thành công
                    packet.SetCommand((short)Cmd.SUCCESS);
                    packet.SetPayload("Login successful.");

                    session.Send(packet.ToByteArray());
                }
                else
                {
                    // Sai mật khẩu
                    packet.SetCommand((short)Cmd.ERROR);
                    packet.SetPayload("Incorrect password.");

                    session.Send(packet.ToByteArray());
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                NLog.Error($"Login failed for {email}: {ex.Message}");

                // Phản hồi chung chung để tránh tiết lộ thông tin nhạy cảm
                packet.SetCommand((short)Cmd.ERROR);
                packet.SetPayload("An error occurred during login. Please try again later.");

                session.Send(packet.ToByteArray());
            }
        }


        [Command(Cmd.UPDATE_PASSWORD)]
        public static async Task UpdatePassword(ISession session, byte[] data)
        {
            Packets packet = new();
            string result = ConverterHelper.ToString(data);
            string[] parts = result.Split('|');

            if (parts.Length != 3)
            {
                packet.SetCommand((short)Cmd.ERROR);
                packet.SetPayload("Invalid data format. Please provide email, current password, and new password.");
                session.Send(packet.ToByteArray());
                return;
            }

            string email = parts[0];
            string currentPassword = parts[1];
            string newPassword = parts[2];

            try
            {
                // Lấy mật khẩu hiện tại từ cơ sở dữ liệu
                string? storedPasswordHash = await _sqlExecutor.ExecuteScalarAsync<string?>(
                    SqlCommand.SELECT_ACCOUNT_PASSWORD, email
                );

                if (storedPasswordHash == null)
                {
                    packet.SetCommand((short)Cmd.ERROR);
                    packet.SetPayload("Account not found.");
                    session.Send(packet.ToByteArray());
                    return;
                }

                // Kiểm tra mật khẩu hiện tại của người dùng
                if (!PBKDF2.VerifyPassword(currentPassword, storedPasswordHash))
                {
                    packet.SetCommand((short)Cmd.ERROR);
                    packet.SetPayload("Current password is incorrect.");
                    session.Send(packet.ToByteArray());
                    return;
                }

                // Mã hóa mật khẩu mới
                string newPasswordHash = PBKDF2.HashPassword(newPassword);

                // Cập nhật mật khẩu mới vào cơ sở dữ liệu
                bool updateSuccess = await _sqlExecutor.ExecuteAsync(
                    SqlCommand.UPDATE_ACCOUNT_PASSWORD, email, newPasswordHash
                );

                if (updateSuccess)
                {
                    packet.SetCommand((short)Cmd.SUCCESS);
                    packet.SetPayload("Password changed successfully.");
                }
                else
                {
                    packet.SetCommand((short)Cmd.ERROR);
                    packet.SetPayload("Failed to change password. Please try again.");
                }

                session.Send(packet.ToByteArray());
            }
            catch (Exception ex)
            {
                NLog.Error($"Password change failed for {email}: {ex.Message}");

                packet.SetCommand((short)Cmd.ERROR);
                packet.SetPayload("An error occurred while changing the password.");
                session.Send(packet.ToByteArray());
            }
        }
    }
}