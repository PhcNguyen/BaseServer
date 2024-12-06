using NPServer.Database.Postgre;
using NPServer.Core.Interfaces.Packets;
using NPServer.Core.Packets.Utilities;
using NPServer.Infrastructure.Logging;
using System;
using NPServer.Database;

namespace NPServer.Application.Handlers
{
    internal abstract class RequestHandlerBase
    {
        protected static readonly SqlExecutor SqlExecutor = new(new NpgsqlFactory());

        protected static IPacket HandleRequestError<TClass>(string message, Exception? ex = null) where TClass : class
        {
            if (ex != null)
                NPLog.Instance.Error<TClass>(ex.Message);
            return PacketExtensions.ToResponsePacket((short)Command.ERROR, message);
        }
    }
}