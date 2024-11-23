﻿namespace NServer.Application.Handler
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class CommandAttribute(Cmd command) : Attribute
    {
        // Gán giá trị cho thuộc tính Command
        public Cmd Command { get; } = command;  
    }

    internal enum Cmd : short
    {
        NONE,
        PING,
        PONG,
        SET_KEY,
        GET_KEY,

        REGISTER,
        LOGIN,
        LOGOUT,
        UPDATE_PASSWORD,

        SUCCESS = 100,
        ERROR = 101,
        INVALID_COMMAND = 102,
        TIMEOUT = 103,

        CLOSE = 104, // Lệnh đóng kết nối
    }
}
