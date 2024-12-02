namespace NServer.Application.Handlers.Enums
{
    public enum Command : short
    {
        NONE = 0,

        // Hệ thống cơ bản
        PING,

        PONG,
        HEARTBEAT,
        CLOSE,

        // Quản lý khóa
        SET_KEY,

        GET_KEY,
        DELETE_KEY,

        // Quản lý người dùng
        REGISTER,

        LOGIN,
        LOGOUT,
        UPDATE_PASSWORD,
        VIEW_PROFILE,
        UPDATE_PROFILE,
        DELETE_ACCOUNT,

        // Quản lý hệ thống
        SHUTDOWN,

        RESTART,
        STATUS,

        // Kết quả xử lý
        SUCCESS = 100,

        ERROR = 101,
        INVALID_COMMAND = 102,
        TIMEOUT = 103,
    }
}