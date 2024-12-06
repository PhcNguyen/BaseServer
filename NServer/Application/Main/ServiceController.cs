using NPServer.Application.Handlers;
using NPServer.Application.Handlers.Packets.Queue;
using NPServer.Core.Interfaces.Network;
using NPServer.Core.Interfaces.Pooling;
using NPServer.Core.Interfaces.Session;
using NPServer.Core.Network.Firewall;
using NPServer.Core.Pooling;
using NPServer.Core.Services;
using NPServer.Core.Session;
using NPServer.Infrastructure.Configuration;
using NPServer.Infrastructure.Logging;

namespace NPServer.Application.Main
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
            Singleton.Register<PacketInserver>();
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