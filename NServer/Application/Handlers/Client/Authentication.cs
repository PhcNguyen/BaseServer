using NServer.Application.Handlers.Base;
using NServer.Application.Handlers.Packets;
using NServer.Core.Database;
using NServer.Core.Handlers;
using NServer.Core.Helper;
using NServer.Core.Interfaces.Packets;
using NServer.Infrastructure.Security;
using System;
using System.Threading.Tasks;

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
        [CommandAttribute<Command>(Command.REGISTER)]
        public static async Task<IPacket> Register(IPacket packet)
        {
            byte[] data = packet.Payload.ToArray();

            string[]? input = ParseInput(data, 2);

            if (input == null)
                return PacketUtils.Response(Command.ERROR, "Invalid registration data format.");

            var (email, password) = (input[0].Trim(), input[1].Trim());

            if (!ValidationHelper.IsEmailValid(email) || !ValidationHelper.IsPasswordValid(password))
                return PacketUtils.Response(Command.ERROR, "Invalid email or weak password.");

            try
            {
                var hashedPassword = Pbkdf2Cyptography.GenerateHash(password);
                bool success = await SqlExecutor.ExecuteAsync(SqlCommand.INSERT_ACCOUNT, email, hashedPassword);

                return success
                    ? PacketUtils.Response(Command.SUCCESS, "Registration successful.")
                    : PacketUtils.Response(Command.ERROR, "Registration failed.");
            }
            catch (Exception ex) when (ex.Message.Contains("duplicate key"))
            {
                return PacketUtils.Response(Command.ERROR, "This email is already registered.");
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
        [CommandAttribute<Command>(Command.LOGIN)]
        public static async Task<IPacket> Login(IPacket packet)
        {
            byte[] data = packet.Payload.ToArray();

            string[]? input = ParseInput(data, 2);

            if (input == null)
                return PacketUtils.Response(Command.ERROR, "Invalid login data format.");

            var (email, password) = (input[0].Trim(), input[1].Trim());

            if (!ValidationHelper.IsEmailValid(email))
                return PacketUtils.Response(Command.ERROR, "Invalid email or password.");

            try
            {
                string hashedPassword = await SqlExecutor.ExecuteScalarAsync<string>(SqlCommand.SELECT_ACCOUNT_PASSWORD, email);
                if (string.IsNullOrEmpty(hashedPassword))
                    return PacketUtils.Response(Command.ERROR, "Invalid email or password.");

                DateTime? lastLogin = await SqlExecutor.ExecuteScalarAsync<DateTime?>(SqlCommand.SELECT_LAST_LOGIN, email);
                if (lastLogin.HasValue && (DateTime.UtcNow - lastLogin.Value).TotalSeconds < 20)
                    return PacketUtils.Response(Command.ERROR, "Please wait 20 seconds before trying again.");

                if (!Pbkdf2Cyptography.ValidatePassword(hashedPassword, password))
                {
                    await SqlExecutor.ExecuteAsync(SqlCommand.UPDATE_LAST_LOGIN, email); // Log failed attempt
                    return PacketUtils.Response(Command.ERROR, "Invalid email or password.");
                }

                if (!Authenticator(packet.Id))
                {
                    return PacketUtils.Response(Command.SUCCESS, "Login faild.");
                }

                await SqlExecutor.ExecuteAsync(SqlCommand.UPDATE_ACCOUNT_ACTIVE, true, email);

                return PacketUtils.Response(Command.SUCCESS, "Login successful.");
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
        [CommandAttribute<Command>(Command.LOGOUT)]
        public static async Task<IPacket> Logout(IPacket packet)
        {
            byte[] data = packet.Payload.ToArray();

            string[]? input = ParseInput(data, 1);
            if (input == null)
                return PacketUtils.Response(Command.ERROR, "Invalid logout data format.");

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
        [CommandAttribute<Command>(Command.UPDATE_PASSWORD)]
        public static async Task<IPacket> UpdatePassword(IPacket packet)
        {
            byte[] data = packet.Payload.ToArray();

            string[]? input = ParseInput(data, 3);
            if (input == null)
                return PacketUtils.Response(Command.ERROR, "Invalid data format.");

            var (email, currentPassword, newPassword) = (input[0].Trim(), input[1].Trim(), input[2].Trim());

            if (!ValidationHelper.IsPasswordValid(newPassword))
                return PacketUtils.Response(Command.ERROR, "New password is too weak.");

            try
            {
                var storedPasswordHash = await SqlExecutor.ExecuteScalarAsync<string>(SqlCommand.SELECT_ACCOUNT_PASSWORD, email);

                if (!Pbkdf2Cyptography.ValidatePassword(currentPassword, storedPasswordHash))
                    return PacketUtils.Response(Command.ERROR, "Incorrect current password.");

                var hashedNewPassword = Pbkdf2Cyptography.GenerateHash(newPassword);
                bool updateSuccess = await SqlExecutor.ExecuteAsync(SqlCommand.UPDATE_ACCOUNT_PASSWORD, email, hashedNewPassword);

                return updateSuccess
                    ? PacketUtils.Response(Command.SUCCESS, "Password updated successfully.")
                    : PacketUtils.Response(Command.ERROR, "Failed to update password.");
            }
            catch (Exception ex)
            {
                return await HandleError("Password update error.", ex);
            }
        }
    }
}