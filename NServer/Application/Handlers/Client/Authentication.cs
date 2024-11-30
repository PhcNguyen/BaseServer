using System;
using System.Threading.Tasks;

using NServer.Core.Database;
using NServer.Core.Security;
using NServer.Core.Interfaces.Packets;

using NServer.Infrastructure.Helper;
using NServer.Application.Handlers.Base;
using NServer.Application.Handlers.Packets;

namespace NServer.Application.Handlers.Client
{
    /// <summary>
    /// Lớp xử lý các yêu cầu xác thực của khách hàng.
    /// <para>
    /// Lớp này chịu trách nhiệm xử lý các yêu cầu đăng ký, đăng nhập và cập nhật mật khẩu từ phía khách hàng.
    /// </para>
    /// </summary>
    internal class Authentication : CommandHandlerBase
    {
        /// <summary>
        /// Phương thức đăng ký người dùng mới.
        /// </summary>
        /// <param name="packet">Dữ liệu đăng ký bao gồm email và mật khẩu.</param>
        /// <returns>Gói tin phản hồi kết quả đăng ký.</returns>
        [Command(Cmd.REGISTER)]
        public static async Task<IPacket> Register(IPacket packet)
        {
            byte[] data = packet.Payload.ToArray();

            string[]? input = ParseInput(data, 2);

            if (input == null)
                return PacketUtils.Response(Cmd.ERROR, "Invalid registration data format.");

            var (email, password) = (input[0].Trim(), input[1].Trim());

            if (!ValidationHelper.IsEmailValid(email) || !ValidationHelper.IsPasswordValid(password))
                return PacketUtils.Response(Cmd.ERROR, "Invalid email or weak password.");

            try
            {
                var hashedPassword = PBKDF2.HashPassword(password);
                bool success = await SqlExecutor.ExecuteAsync(SqlCommand.INSERT_ACCOUNT, email, hashedPassword);

                return success
                    ? PacketUtils.Response(Cmd.SUCCESS, "Registration successful.")
                    : PacketUtils.Response(Cmd.ERROR, "Registration failed.");
            }
            catch (Exception ex) when (ex.Message.Contains("duplicate key"))
            {
                return PacketUtils.Response(Cmd.ERROR, "This email is already registered.");
            }
            catch (Exception ex)
            {
                return await HandleError("Registration error.", ex);
            }
        }

        /// <summary>
        /// Phương thức đăng nhập người dùng.
        /// </summary>
        /// <param name="packet">Dữ liệu đăng nhập bao gồm email và mật khẩu.</param>
        /// <returns>Gói tin phản hồi kết quả đăng nhập.</returns>
        [Command(Cmd.LOGIN)]
        public static async Task<IPacket> Login(IPacket packet)
        {
            byte[] data = packet.Payload.ToArray();

            string[]? input = ParseInput(data, 2);

            if (input == null)
                return PacketUtils.Response(Cmd.ERROR, "Invalid login data format.");

            var (email, password) = (input[0].Trim(), input[1].Trim());

            if (!ValidationHelper.IsEmailValid(email))
                return PacketUtils.Response(Cmd.ERROR, "Invalid email or password.");

            try
            {
                string hashedPassword = await SqlExecutor.ExecuteScalarAsync<string>(SqlCommand.SELECT_ACCOUNT_PASSWORD, email);
                if (string.IsNullOrEmpty(hashedPassword))
                    return PacketUtils.Response(Cmd.ERROR, "Invalid email or password.");

                DateTime? lastLogin = await SqlExecutor.ExecuteScalarAsync<DateTime?>(SqlCommand.SELECT_LAST_LOGIN, email);
                if (lastLogin.HasValue && (DateTime.UtcNow - lastLogin.Value).TotalSeconds < 20)
                    return PacketUtils.Response(Cmd.ERROR, "Please wait 20 seconds before trying again.");

                if (!PBKDF2.VerifyPassword(hashedPassword, password))
                {
                    await SqlExecutor.ExecuteAsync(SqlCommand.UPDATE_LAST_LOGIN, email); // Log failed attempt
                    return PacketUtils.Response(Cmd.ERROR, "Invalid email or password.");
                }

                if (!Authenticator(packet.Id))
                {
                    return PacketUtils.Response(Cmd.SUCCESS, "Login faild.");
                }

                await SqlExecutor.ExecuteAsync(SqlCommand.UPDATE_ACCOUNT_ACTIVE, true, email);

                return PacketUtils.Response(Cmd.SUCCESS, "Login successful.");
            }
            catch (Exception ex)
            {
                return await HandleError("Login error.", ex);
            }
        }

        /// <summary>
        /// Phương thức đăng xuất người dùng.
        /// </summary>
        /// <param name="packet">Dữ liệu đăng xuất bao gồm email (nếu có yêu cầu).</param>
        /// <returns>Gói tin phản hồi kết quả đăng xuất.</returns>
        [Command(Cmd.LOGOUT)]
        public static async Task<IPacket> Logout(IPacket packet)
        {
            byte[] data = packet.Payload.ToArray();

            string[]? input = ParseInput(data, 1);
            if (input == null)
                return PacketUtils.Response(Cmd.ERROR, "Invalid logout data format.");

            var email = input[0].Trim();

            try
            {
                // Thực hiện các xử lý đăng xuất tại đây (ví dụ: cập nhật trạng thái người dùng)
                await SqlExecutor.ExecuteAsync(SqlCommand.UPDATE_ACCOUNT_ACTIVE, false, email);
                
                return PacketUtils.EmptyPacket;
            }
            catch (Exception ex)
            {
                return await HandleError("Logout error.", ex);
            }
        }

        /// <summary>
        /// Phương thức cập nhật mật khẩu người dùng.
        /// </summary>
        /// <param name="packet">Dữ liệu bao gồm email, mật khẩu hiện tại và mật khẩu mới.</param>
        /// <returns>Gói tin phản hồi kết quả cập nhật mật khẩu.</returns>
        [Command(Cmd.UPDATE_PASSWORD)]
        public static async Task<IPacket> UpdatePassword(IPacket packet)
        {
            byte[] data = packet.Payload.ToArray();

            string[]? input = ParseInput(data, 3);
            if (input == null)
                return PacketUtils.Response(Cmd.ERROR, "Invalid data format.");

            var (email, currentPassword, newPassword) = (input[0].Trim(), input[1].Trim(), input[2].Trim());

            if (!ValidationHelper.IsPasswordValid(newPassword))
                return PacketUtils.Response(Cmd.ERROR, "New password is too weak.");

            try
            {
                var storedPasswordHash = await SqlExecutor.ExecuteScalarAsync<string>(SqlCommand.SELECT_ACCOUNT_PASSWORD, email);

                if (!PBKDF2.VerifyPassword(currentPassword, storedPasswordHash))
                    return PacketUtils.Response(Cmd.ERROR, "Incorrect current password.");

                var hashedNewPassword = PBKDF2.HashPassword(newPassword);
                bool updateSuccess = await SqlExecutor.ExecuteAsync(SqlCommand.UPDATE_ACCOUNT_PASSWORD, email, hashedNewPassword);

                return updateSuccess
                    ? PacketUtils.Response(Cmd.SUCCESS, "Password updated successfully.")
                    : PacketUtils.Response(Cmd.ERROR, "Failed to update password.");
            }
            catch (Exception ex)
            {
                return await HandleError("Password update error.", ex);
            }
        }
    }
}