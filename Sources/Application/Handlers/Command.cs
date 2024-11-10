using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NETServer.Application.Handlers;

// byte là kiểu số nguyên không dấu 8 bit (1 byte).
// Phạm vi giá trị: từ 0 đến 255.

internal enum Command : byte
{
    SET_KEY,
    GET_KEY
}

