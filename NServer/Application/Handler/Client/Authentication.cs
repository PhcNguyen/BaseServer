using System;
using System.Threading.Tasks;

using NServer.Core.Packets;
using NServer.Core.Database;
using NServer.Core.Security;
using NServer.Core.Database.Postgre;
using NServer.Infrastructure.Helper;
using NServer.Infrastructure.Logging;

namespace NServer.Application.Handler.Client
{
    internal class Authentication
    {
        private static readonly SqlExecutor _sqlExecutor = new(new NpgsqlFactory());

        public static Packet Response(Cmd command, string message)
        {
            var packet = new Packet();
            packet.SetCommand((short)command);
            packet.SetPayload(message);
            return packet;
        }

        [Command(Cmd.REGISTER)]
        public static async Task<Packet> Register(byte[] data)
        {
            var result = ConverterHelper.ToString(data);
            var parts = result.Split('|');

            if (parts.Length != 2)
                return Response(Cmd.ERROR, "Invalid registration data format.");

            string email = parts[0];
            string password = parts[1];

            if (!ValidatorHelper.IsEmailValid(email))
                return Response(Cmd.ERROR, "Invalid email format.");

            if (!ValidatorHelper.IsPasswordValid(password))
                return Response(Cmd.ERROR, "Password does not meet the required criteria.");

            int accountCount = await _sqlExecutor.ExecuteScalarAsync<int>(SqlCommand.SELECT_ACCOUNT_COUNT, email);
            if (accountCount > 0)
                return Response(Cmd.ERROR, "This email was used.");

            try
            {
                bool success = await _sqlExecutor.ExecuteAsync(SqlCommand.INSERT_ACCOUNT, email, PBKDF2.HashPassword(password));
                return success
                    ? Response(Cmd.SUCCESS, "Registration successful.")
                    : Response(Cmd.ERROR, "Registration failed. Please try again.");
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Registration failed for {email}: {ex.Message}");
                return Response(Cmd.ERROR, "An error occurred during registration.");
            }
        }

        [Command(Cmd.LOGIN)]
        public static async Task<Packet> Login(byte[] data)
        {
            var result = ConverterHelper.ToString(data);
            var parts = result.Split('|');

            if (parts.Length != 2)
                return Response(Cmd.ERROR, "Invalid login data format.");

            string email = parts[0].Trim();
            string password = parts[1].Trim();

            try
            {
                string hashedPassword = await _sqlExecutor.ExecuteScalarAsync<string>(SqlCommand.SELECT_ACCOUNT_PASSWORD, email);
                if (string.IsNullOrEmpty(hashedPassword))
                    return Response(Cmd.ERROR, "Account not found.");

                bool isPasswordValid = PBKDF2.VerifyPassword(hashedPassword, password);
                var responseMessage = isPasswordValid ? "Login successful." : "Incorrect password.";
                var responseCmd = isPasswordValid ? Cmd.SUCCESS : Cmd.ERROR;

                return Response(responseCmd, responseMessage);
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Login failed for {email}: {ex.Message}");
                return Response(Cmd.ERROR, "An error occurred during login. Please try again later.");
            }
        }

        [Command(Cmd.UPDATE_PASSWORD)]
        public static async Task<Packet> UpdatePassword(byte[] data)
        {
            var result = ConverterHelper.ToString(data);
            var parts = result.Split('|');

            if (parts.Length != 3)
                return Response(Cmd.ERROR, "Invalid data format. Please provide email, current password, and new password.");

            string email = parts[0];
            string currentPassword = parts[1];
            string newPassword = parts[2];

            try
            {
                string? storedPasswordHash = await _sqlExecutor.ExecuteScalarAsync<string?>(SqlCommand.SELECT_ACCOUNT_PASSWORD, email);

                if (storedPasswordHash == null)
                    return Response(Cmd.ERROR, "Account not found.");

                if (!PBKDF2.VerifyPassword(currentPassword, storedPasswordHash))
                    return Response(Cmd.ERROR, "Current password is incorrect.");

                string newPasswordHash = PBKDF2.HashPassword(newPassword);

                bool updateSuccess = await _sqlExecutor.ExecuteAsync(SqlCommand.UPDATE_ACCOUNT_PASSWORD, email, newPasswordHash);

                return updateSuccess
                    ? Response(Cmd.SUCCESS, "Password changed successfully.")
                    : Response(Cmd.ERROR, "Failed to change password. Please try again.");
            }
            catch (Exception ex)
            {
                NLog.Instance.Error($"Password change failed for {email}: {ex.Message}");
                return Response(Cmd.ERROR, "An error occurred while changing the password.");
            }
        }
    }
}