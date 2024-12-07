namespace NPServer.Commands
{
    public enum Command : short
    {
        None = 0,
        Default,

        // Hệ thống cơ bản
        Ping,
        Pong,
        Heartbeat,
        Close,

        // Quản lý khóa
        SetKey,
        GetKey,
        DeleteKey,

        // Quản lý người dùng
        Register,
        Login,
        Logout,
        UpdatePassword,
        ViewProfile,
        UpdateProfile,
        DeleteAccount,

        // Quản lý hệ thống
        Shutdown,
        Restart,
        Status,

        // Kết quả xử lý
        Success = 100,
        Error = 101,
        InvalidCommand = 102,
        Timeout = 103,
    }
}