//using NPServer.Application.Helper;
//using NPServer.Commands;
//using NPServer.Models.Models;
//using System;
//using System.Threading.Tasks;

//namespace NPServer.Application.Implementations;

///// <summary>
///// Lớp xử lý các yêu cầu xác thực của khách hàng.
///// <para>
///// Lớp này chịu trách nhiệm xử lý các yêu cầu đăng ký, đăng nhập và cập nhật mật khẩu từ phía khách hàng.
///// </para>
///// </summary>
//internal class Authentication
//{
//    /// <summary>
//    /// Phương thức đăng ký người dùng mới.
//    /// </summary>
//    /// <param name="packet">Dữ liệu đăng ký bao gồm email và mật khẩu.</param>
//    /// <returns>Gói tin phản hồi kết quả đăng ký.</returns>
//    [Command(Command.Register, AccessLevel.Guests)]
//    public static async Task<Packet> Register(Packet packet)
//    {
//        byte[] data = packet.PayloadData.ToArray();

//        string[]? input = DataHelper.ParseInput(data, 2);

//        if (input == null)
//        {
//            packet.ResetForPool();
//            packet.SetCmd(Command.Error);
//            packet.SetPayload("Invalid registration data format.");

//            return packet;
//        }

//        var (email, password) = (input[0].Trim(), input[1].Trim());

//        if (!EmailHelper.IsEmailValid(email) || !PasswordHelper.IsPasswordValid(password))
//        {
//            packet.ResetForPool();
//            packet.SetCmd(Command.Error);
//            packet.SetPayload("Invalid email or weak password.");

//            return packet;
//        }

//        try
//        {
//            var hashedPassword = Pbkdf2Cyptography.GenerateHash(password);
//            bool success = await SqlExecutor.ExecuteAsync(SqlCommand.INSERT_ACCOUNT, email, hashedPassword);

//            packet.ResetForPool();
//            packet.SetCmd(success ? Command.SUCCESS : Command.ERROR);
//            packet.SetPayload(success ? "Registration successful." : "Registration failed.");

//            return packet;
//        }
//        catch (Exception ex) when (ex.Message.Contains("duplicate key"))
//        {
//            packet.ResetForPool();
//            packet.SetCmd(Command.ERROR);
//            packet.SetPayload("This email is already registered.");

//            return packet;
//        }
//        catch (Exception ex)
//        {
//            return HandleRequestError<Authentication>("Registration error.", ex);
//        }
//    }

//    /// <summary>
//    /// Phương thức đăng nhập người dùng.
//    /// </summary>
//    /// <param name="packet">Dữ liệu đăng nhập bao gồm email và mật khẩu.</param>
//    /// <returns>Gói tin phản hồi kết quả đăng nhập.</returns>
//    [Command(Command.Login, AccessLevel.Guests)]
//    public static async Task<IPacket> Login(IPacket packet)
//    {
//        byte[] data = packet.PayloadData.ToArray();

//        string[]? input = DataHelper.ParseInput(data, 2);

//        if (input == null)
//        {
//            packet.ResetForPool();
//            packet.SetCmd(Command.Error);
//            packet.SetPayload("Invalid login data format.");

//            return packet;
//        }

//        var (email, password) = (input[0].Trim(), input[1].Trim());

//        if (!EmailHelper.IsEmailValid(email))
//        {
//            packet.ResetForPool();
//            packet.SetCmd(Command.Error);
//            packet.SetPayload("Invalid email or password.");

//            return packet;
//        }

//        try
//        {
//            string hashedPassword = await SqlExecutor.ExecuteScalarAsync<string>(SqlCommand.SELECT_ACCOUNT_PASSWORD, email);
//            if (string.IsNullOrEmpty(hashedPassword))
//            {
//                packet.ResetForPool();
//                packet.SetCmd(Command.Error);
//                packet.SetPayload("Invalid email or password.");

//                return packet;
//            }

//            DateTime? lastLogin = await SqlExecutor.ExecuteScalarAsync<DateTime?>(SqlCommand.SELECT_LAST_LOGIN, email);
//            if (lastLogin.HasValue && (DateTime.UtcNow - lastLogin.Value).TotalSeconds < 20)
//            {
//                packet.ResetForPool();
//                packet.SetCmd(Command.Error);
//                packet.SetPayload("Please wait 20 seconds before trying again.");

//                return packet;
//            }

//            if (!Pbkdf2Cyptography.ValidatePassword(hashedPassword, password))
//            {
//                await SqlExecutor.ExecuteAsync(SqlCommand.UPDATE_LAST_LOGIN, email); // Log failed attempt

//                packet.ResetForPool();
//                packet.SetCmd(Command.Error);
//                packet.SetPayload("Invalid email or password.");

//                return packet;
//            }

//            await SqlExecutor.ExecuteAsync(SqlCommand.UPDATE_ACCOUNT_ACTIVE, true, email);

//            packet.ResetForPool();
//            packet.SetCmd(Command.Error);
//            packet.SetPayload("Login successful.");

//            return packet;
//        }
//        catch (Exception ex)
//        {
//            return HandleRequestError<Authentication>("Login error.", ex);
//        }
//    }

//    /// <summary>
//    /// Phương thức đăng xuất người dùng.
//    /// </summary>
//    /// <param name="packet">Dữ liệu đăng xuất bao gồm email (nếu có yêu cầu).</param>
//    /// <returns>Gói tin phản hồi kết quả đăng xuất.</returns>
//    [Command(Command.Logout, AccessLevel.User)]
//    public static async Task<IPacket> Logout(IPacket packet)
//    {
//        byte[] data = packet.PayloadData.ToArray();

//        string[]? input = DataHelper.ParseInput(data, 1);
//        if (input == null)
//        {
//            packet.ResetForPool();
//            packet.SetCmd(Command.Error);
//            packet.SetPayload("Invalid logout data format.");

//            return packet;
//        }

//        var email = input[0].Trim();

//        try
//        {
//            await SqlExecutor.ExecuteAsync(SqlCommand.UPDATE_ACCOUNT_ACTIVE, false, email);

//            packet.ResetForPool();
//            packet.SetCmd(Command.Success);
//            packet.SetPayload("Logout successful.");

//            return packet;
//        }
//        catch (Exception ex)
//        {
//            return HandleRequestError<Authentication>("Logout error.", ex);
//        }
//    }

//    /// <summary>
//    /// Phương thức cập nhật mật khẩu người dùng.
//    /// </summary>
//    /// <param name="packet">Dữ liệu bao gồm email, mật khẩu hiện tại và mật khẩu mới.</param>
//    /// <returns>Gói tin phản hồi kết quả cập nhật mật khẩu.</returns>
//    [Command(Command.UpdatePassword, AccessLevel.User)]
//    public static async Task<IPacket> UpdatePassword(IPacket packet)
//    {
//        byte[] data = packet.PayloadData.ToArray();

//        string[]? input = DataHelper.ParseInput(data, 3);
//        if (input == null)
//        {
//            packet.ResetForPool();
//            packet.SetCmd(Command.Error);
//            packet.SetPayload("Invalid data format.");

//            return packet;
//        }

//        var (email, currentPassword, newPassword) = (input[0].Trim(), input[1].Trim(), input[2].Trim());

//        if (!PasswordHelper.IsPasswordValid(newPassword))
//        {
//            packet.ResetForPool();
//            packet.SetCmd(Command.Error);
//            packet.SetPayload("New password is too weak.");

//            return packet;
//        }

//        try
//        {
//            var storedPasswordHash = await SqlExecutor.ExecuteScalarAsync<string>(SqlCommand.SELECT_ACCOUNT_PASSWORD, email);

//            if (!Pbkdf2Cyptography.ValidatePassword(currentPassword, storedPasswordHash))
//            {
//                packet.ResetForPool();
//                packet.SetCmd(Command.Error);
//                packet.SetPayload("Incorrect current password.");

//                return packet;
//            }

//            var hashedNewPassword = Pbkdf2Cyptography.GenerateHash(newPassword);
//            bool updateSuccess = await SqlExecutor.ExecuteAsync(SqlCommand.UPDATE_ACCOUNT_PASSWORD, email, hashedNewPassword);

//            packet.ResetForPool();
//            packet.SetCmd(updateSuccess ? Command.Success : Command.Error);
//            packet.SetPayload(updateSuccess ? "Password updated successfully." : "Failed to update password.");

//            return packet;
//        }
//        catch (Exception ex)
//        {
//            return HandleRequestError<Authentication>("Update password failed.", ex);
//        }
//    }
//}