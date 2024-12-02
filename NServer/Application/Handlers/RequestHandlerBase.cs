using NServer.Application.Handlers.Enums;
using NServer.Application.Handlers.Packets;
using NServer.Core.Database;
using NServer.Core.Database.Postgre;
using NServer.Core.Interfaces.Packets;
using NServer.Infrastructure.Logging;
using System;

namespace NServer.Application.Handlers
{
    internal abstract class RequestHandlerBase
    {
        protected static readonly SqlExecutor SqlExecutor = new(new NpgsqlFactory());

        protected static IPacket HandleRequestError<TClass>(string message, Exception? ex = null) where TClass : class
        {
            if (ex != null)
                NLog.Instance.Error<TClass>(ex.Message);
            return PacketUtils.Response(Command.ERROR, message);
        }
    }
}