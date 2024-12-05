using NServer.Application.Handlers;
using NServer.Core.Session;
using NServer.Core.Network.Firewall;
using NServer.Core.Interfaces.Session;
using NServer.Core.Interfaces.Network;
using NServer.Core.Interfaces.Pooling;
using NServer.Infrastructure.Configuration;
using NServer.Core.Services;
using NServer.Infrastructure.Logging;
using NServer.Core.Pooling;
using NServer.Application.Handlers.Packets.Queue;

namespace NServer.Application.Main
{
    /// <summary>
    /// Lớp ServiceRegistry chịu trách nhiệm đăng ký các dịch vụ trong hệ thống.
    /// </summary>
    internal static class ServiceController
    {
        /// <summary>
        /// Đăng ký các instance của dịch vụ vào Singleton.
        /// </summary>
        public static void Register()
        {
            // Application
            Singleton.Register<PacketOutgoing>();
            Singleton.Register<PacketIncoming>();
            Singleton.Register<CommandDispatcher>();


            // Core
            Singleton.Register<ISessionManager, SessionManager>();

            Singleton.Register<IPacketPool, PacketPool>(() => new PacketPool(10));

            Singleton.Register<IConnLimiter, ConnLimiter>(() =>
            new ConnLimiter(Setting.MaxConnections));

            Singleton.Register<IMultiSizeBufferPool, MultiSizeBufferPool>(() =>
            new MultiSizeBufferPool(Setting.BufferAllocations, Setting.TotalBuffers));

            Singleton.Register<IRequestLimiter, RequestLimiter>(() =>
            new RequestLimiter(Setting.RateLimit, Setting.ConnectionLockoutDuration));
        }

        public static void Initialization()
        {
            Singleton.GetInstanceOfInterface<IMultiSizeBufferPool>().AllocateBuffers();
            NLog.Instance.DefaultInitialization();
        }
    }
}