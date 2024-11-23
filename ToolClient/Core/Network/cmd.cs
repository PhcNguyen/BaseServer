using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolClient.Core.Network
{
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
