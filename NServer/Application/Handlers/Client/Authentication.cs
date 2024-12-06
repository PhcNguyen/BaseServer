using NPServer.Application.Helper;
using NPServer.Core.Database;
using NPServer.Core.Handlers;
using NPServer.Core.Interfaces.Packets;
using NPServer.Database;
using NPServer.Infrastructure.Security;
using System;
using System.Threading.Tasks;

namespace NPServer.Application.Handlers.Client
{
    /// <summary>
    /// Lớp xử lý các yêu cầu xác thực của khách hàng.
    /// <para>
    /// Lớp này chịu trách nhiệm xử lý các yêu cầu đăng ký, đăng nhập và cập nhật mật khẩu từ phía khách hàng.
    /// </para>
    /// </summary>
    internal class Authentication : RequestHandlerBase
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

            string[]? input = DataValidator.ParseInput(data, 2);

            if (input == null)
            {
                packet.Reset();
                packet.SetCmd(Command.ERROR);
                packet.SetPayload("Invalid registration data format.");

                return packet;
            }

            var (email, password) = (input[0].Trim(), input[1].Trim());

            if (!EmailValidator.IsEmailValid(email) || !PasswordValidator.IsPasswordValid(password))
            {
                packet.Reset();
                packet.SetCmd(Command.ERROR);
                packet.SetPayload("Invalid email or weak password.");

                return packet;
            }

            try
            {
                var hashedPassword = Pbkdf2Cyptography.GenerateHash(password);
                bool success = await SqlExecutor.ExecuteAsync(SqlCommand.INSERT_ACCOUNT, email, hashedPassword);

                packet.Reset();
                packet.SetCmd(success ? Command.SUCCESS : Command.ERROR);
                packet.SetPayload(success ? "Registration successful." : "Registration failed.");

                return packet;
            }
            catch (Exception ex) when (ex.Message.Contains("duplicate key"))
            {
                packet.Reset();
                packet.SetCmd(Command.ERROR);
                packet.SetPayload("This email is already registered.");

                return packet;
            }
            catch (Exception ex)
            {
                return HandleRequestError<Authentication>("Registration error.", ex);
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

            string[]? input = DataValidator.ParseInput(data, 2);

            if (input == null)
            {
                packet.Reset();
                packet.SetCmd(Command.ERROR);
                packet.SetPayload("Invalid login data format.");

                return packet;
            }

            var (email, password) = (input[0].Trim(), input[1].Trim());

            if (!EmailValidator.IsEmailValid(email))
            {
                packet.Reset();
                packet.SetCmd(Command.ERROR);
                packet.SetPayload("Invalid email or password.");

                return packet;
            }

            try
            {
                string hashedPassword = await SqlExecutor.ExecuteScalarAsync<string>(SqlCommand.SELECT_ACCOUNT_PASSWORD, email);
                if (string.IsNullOrEmpty(hashedPassword))
                {
                    packet.Reset();
                    packet.SetCmd(Command.ERROR);
                    packet.SetPayload("Invalid email or password.");

                    return packet;
                }

                DateTime? lastLogin = await SqlExecutor.ExecuteScalarAsync<DateTime?>(SqlCommand.SELECT_LAST_LOGIN, email);
                if (lastLogin.HasValue && (DateTime.UtcNow - lastLogin.Value).TotalSeconds < 20)
                {
                    packet.Reset();
                    packet.SetCmd(Command.ERROR);
                    packet.SetPayload("Please wait 20 seconds before trying again.");

                    return packet;
                }

                if (!Pbkdf2Cyptography.ValidatePassword(hashedPassword, password))
                {
                    await SqlExecutor.ExecuteAsync(SqlCommand.UPDATE_LAST_LOGIN, email); // Log failed attempt

                    packet.Reset();
                    packet.SetCmd(Command.ERROR);
                    packet.SetPayload("Invalid email or password.");

                    return packet;
                }

                await SqlExecutor.ExecuteAsync(SqlCommand.UPDATE_ACCOUNT_ACTIVE, true, email);

                packet.Reset();
                packet.SetCmd(Command.SUCCESS);
                packet.SetPayload("Login successful.");

                return packet;
            }
            catch (Exception ex)
            {
                return HandleRequestError<Authentication>("Login error.", ex);
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

            string[]? input = DataValidator.ParseInput(data, 1);
            if (input == null)
            {
                packet.Reset();
                packet.SetCmd(Command.ERROR);
                packet.SetPayload("Invalid logout data format.");

                return packet;
            }

            var email = input[0].Trim();

            try
            {
                await SqlExecutor.ExecuteAsync(SqlCommand.UPDATE_ACCOUNT_ACTIVE, false, email);

                packet.Reset();
                packet.SetCmd(Command.SUCCESS);
                packet.SetPayload("Logout successful.");

                return packet;
            }
            catch (Exception ex)
            {
                return HandleRequestError<Authentication>("Logout error.", ex);
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

            string[]? input = DataValidator.ParseInput(data, 3);
            if (input == null)
            {
                packet.Reset();
                packet.SetCmd(Command.ERROR);
                packet.SetPayload("Invalid data format.");

                return packet;
            }

            var (email, currentPassword, newPassword) = (input[0].Trim(), input[1].Trim(), input[2].Trim());

            if (!PasswordValidator.IsPasswordValid(newPassword))
            {
                packet.Reset();
                packet.SetCmd(Command.ERROR);
                packet.SetPayload("New password is too weak.");

                return packet;
            }

            try
            {
                var storedPasswordHash = await SqlExecutor.ExecuteScalarAsync<string>(SqlCommand.SELECT_ACCOUNT_PASSWORD, email);

                if (!Pbkdf2Cyptography.ValidatePassword(currentPassword, storedPasswordHash))
                {
                    packet.Reset();
                    packet.SetCmd(Command.ERROR);
                    packet.SetPayload("Incorrect current password.");

                    return packet;
                }

                var hashedNewPassword = Pbkdf2Cyptography.GenerateHash(newPassword);
                bool updateSuccess = await SqlExecutor.ExecuteAsync(SqlCommand.UPDATE_ACCOUNT_PASSWORD, email, hashedNewPassword);

                packet.Reset();
                packet.SetCmd(updateSuccess ? Command.SUCCESS : Command.ERROR);
                packet.SetPayload(updateSuccess ? "Password updated successfully." : "Failed to update password.");

                return packet;
            }
            catch (Exception ex)
            {
                return HandleRequestError<Authentication>("Update password failed.", ex);
            }
        }
    }
}