using NServer.Application.Handlers.Packets;
using NServer.Core.Database;
using NServer.Core.Database.Postgre;
using NServer.Core.Interfaces.Packets;
using NServer.Core.Interfaces.Session;
using NServer.Infrastructure.Helper;
using NServer.Infrastructure.Logging;
using NServer.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace NServer.Application.Handlers.Base
{
    internal abstract class CommandHandlerBase
    {
        protected static readonly SqlExecutor SqlExecutor = new(new NpgsqlFactory());
        private static readonly ISessionManager _sessionManager = Singleton.GetInstanceOfInterface<ISessionManager>();

        // Helper Methods
        protected static string[]? ParseInput(byte[] data, int expectedParts)
        {
            var input = ConverterHelper.ToString(data).Split(';');
            return input.Length == expectedParts ? input : null;
        }

        protected static Task<IPacket> HandleError(string message, Exception? ex = null)
        {
            if (ex != null)
                NLog.Instance.Error<CommandHandlerBase>(ex.Message);
            return Task.FromResult(PacketUtils.Response(Command.ERROR, message));
        }

        protected static bool Authenticator(UniqueId id)
        {
            if (_sessionManager.TryGetSession(id, out ISessionClient? session))
            {
                if (session != null)
                {
                    session.Authenticator = true;
                    return true;
                }
            }

            return false;
        }
    }
}