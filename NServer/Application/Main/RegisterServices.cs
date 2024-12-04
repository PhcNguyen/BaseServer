using NServer.Application.Handlers;
using NServer.Application.Handlers.Packets.Queue;
using NServer.Core.Session;
using NServer.Core.BufferPool;
using NServer.Core.Network.Firewall;
using NServer.Core.Interfaces.Session;
using NServer.Core.Interfaces.Network;
using NServer.Core.Interfaces.BufferPool;
using NServer.Infrastructure.Configuration;
using NServer.Core.Services;

namespace NServer.Application.Main
{
    /// <summary>
    /// Lớp ServiceRegistry chịu trách nhiệm đăng ký các dịch vụ trong hệ thống.
    /// </summary>
    internal static class ServiceRegistry
    {
        /// <summary>
        /// Đăng ký các instance của dịch vụ vào Singleton.
        /// </summary>
        public static void RegisterServices()
        {
            Singleton.Register<PacketOutgoing>();
            Singleton.Register<PacketIncoming>();
            Singleton.Register<CommandDispatcher>();

            Singleton.Register<RequestLimiter>(() =>
            new RequestLimiter(Setting.RateLimit, Setting.ConnectionLockoutDuration));

            Singleton.Register<IMultiSizeBuffer, MultiSizeBuffer>(() =>
            new MultiSizeBuffer(Setting.BufferAllocations, Setting.TotalBuffers));

            Singleton.Register<IConnLimiter, ConnLimiter>();
            Singleton.Register<ISessionManager, SessionManager>();
        }
    }
}