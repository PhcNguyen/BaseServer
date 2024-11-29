using System;
using System.Threading.Tasks;

using NServer.Core.Packets;
using NServer.Core.Database;
using NServer.Core.Security;
using NServer.Core.Packets.Utils;
using NServer.Core.Database.Postgre;
using NServer.Infrastructure.Helper;
using NServer.Infrastructure.Logging;

namespace NServer.Application.Handlers.Client
{
    internal class Authentication
    {
        private static readonly SqlExecutor _sqlExecutor = new(new NpgsqlFactory());

        [Command(Cmd.PING)]
        public static async Task<Packet> Ping(byte[] data) =>
            await Task.FromResult(Utils.Response(Cmd.PONG, "Server is alive."));

        [Command(Cmd.CLOSE)]
        public static async Task<Packet> Close(byte[] data) =>
            await Task.FromResult(Utils.EmptyPacket);

        [Command(Cmd.REGISTER)]
        public static async Task<Packet> Register(byte[] data)
        {
            string[] input = ConverterHelper.ToString(data).Split('|');

            if (!ValidationHelper.ValidateInput(input, 2))
                return Utils.Response(Cmd.ERROR, "Invalid registration data format.");

            string email = input[0].Trim();
            string password = input[1].Trim();

            if (!ValidationHelper.IsEmailValid(email) || !ValidationHelper.IsPasswordValid(password))
                return Utils.Response(Cmd.ERROR, "Invalid email or weak password.");

            try
            {
                bool success = await _sqlExecutor.ExecuteAsync(SqlCommand.INSERT_ACCOUNT, email, PBKDF2.HashPassword(password));
                return success
                    ? Utils.Response(Cmd.SUCCESS, "Registration successful.")
                    : Utils.Response(Cmd.ERROR, "Registration failed.");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("duplicate key")) // PostgreSQL báo lỗi trùng lặp
                    return Utils.Response(Cmd.ERROR, "This email is already registered.");

                NLog.Instance.Error($"Registration failed: {ex.Message}");
                return Utils.Response(Cmd.ERROR, "An error occurred during registration.");
            }
        }

        [Command(Cmd.LOGIN)]
        public static async Task<Packet> Login(byte[] data)
        {
            string[] input = ConverterHelper.ToString(data).Split('|');

            if (!ValidationHelper.ValidateInput(input, 2))
                return Utils.Response(Cmd.ERROR, "Invalid login data format.");

            string email = input[0].Trim();
            string password = input[1].Trim();

            if (!ValidationHelper.IsEmailValid(email))
                return Utils.Response(Cmd.ERROR, "Invalid email or password.");

            try
            {
                string hashedPassword = await _sqlExecutor.ExecuteScalarAsync<string>(SqlCommand.SELECT_ACCOUNT_PASSWORD, email);

                if (string.IsNullOrEmpty(hashedPassword))
                    return Utils.Response(Cmd.ERROR, "Invalid email or password.");

                DateTime? lastLogin = await _sqlExecutor.ExecuteScalarAsync<DateTime?>(SqlCommand.SELECT_LAST_LOGIN, email);

                if (lastLogin.HasValue && (DateTime.UtcNow - lastLogin.Value).TotalSeconds < 20)
                    return Utils.Response(Cmd.ERROR, "You must wait 20 seconds before trying again.");

                if (!PBKDF2.VerifyPassword(hashedPassword, password))
                {
                    // Tăng cường bảo mật: Chặn brute-force
                    await _sqlExecutor.ExecuteAsync(SqlCommand.UPDATE_LAST_LOGIN, email); // Cập nhật lần đăng nhập sai
                    return Utils.Response(Cmd.ERROR, "Invalid email or password.");
                }

                return Utils.Response(Cmd.SUCCESS, "Login successful.");
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Login failed for {email}: {ex.Message}");
                return Utils.Response(Cmd.ERROR, "An error occurred during login.");
            }
        }

        [Command(Cmd.UPDATE_PASSWORD)]
        public static async Task<Packet> UpdatePassword(byte[] data)
        {
            string[] input = ConverterHelper.ToString(data).Split('|');

            if (!ValidationHelper.ValidateInput(input, 3))
                return Utils.Response(Cmd.ERROR, "Invalid data format.");

            string email = input[0].Trim();
            string currentPassword = input[1].Trim();
            string newPassword = input[2].Trim();

            if (!ValidationHelper.IsPasswordValid(newPassword))
                return Utils.Response(Cmd.ERROR, "New password is too weak.");

            try
            {
                string storedPasswordHash = await _sqlExecutor.ExecuteScalarAsync<string>(SqlCommand.SELECT_ACCOUNT_PASSWORD, email);

                if (!PBKDF2.VerifyPassword(currentPassword, storedPasswordHash))
                    return Utils.Response(Cmd.ERROR, "Incorrect current password.");

                bool updateSuccess = await _sqlExecutor.ExecuteAsync(
                    SqlCommand.UPDATE_ACCOUNT_PASSWORD, email, PBKDF2.HashPassword(newPassword));

                return updateSuccess
                    ? Utils.Response(Cmd.SUCCESS, "Password updated successfully.")
                    : Utils.Response(Cmd.ERROR, "Failed to update password.");
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Password update failed for {email}: {ex.Message}");
                return Utils.Response(Cmd.ERROR, "An error occurred during password update.");
            }
        }
    }
}