namespace NETServer.Application.Handlers;

// byte là kiểu số nguyên không dấu 8 bit (1 byte).
// Phạm vi giá trị: từ 0 đến 255.

internal enum Command : byte
{
    PING,
    PONG,
    SET_KEY,
    GET_KEY
}

