using NPServer.Application.Handlers;
using NPServer.Application.Handlers.Packets.Queue;
using NPServer.Core.Interfaces.Network;
using NPServer.Core.Interfaces.Pooling;
using NPServer.Core.Interfaces.Session;
using NPServer.Core.Network.Firewall;
using NPServer.Core.Pooling;
using NPServer.Core.Session;
using NPServer.Infrastructure.Configuration;
using NPServer.Infrastructure.Logging;
using NPServer.Infrastructure.Configuration.Default;
using NPServer.Infrastructure.Services;

namespace NPServer.Application.Main
{
    /// <summary>
    /// Lớp ServiceRegistry chịu trách nhiệm đăng ký các dịch vụ trong hệ thống.
    /// </summary>
    internal static class ServiceController
    {
        public static void Initialization()
        {
            Singleton.GetInstanceOfInterface<IMultiSizeBufferPool>().AllocateBuffers();
            NPLog.Instance.DefaultInitialization();
        }

        /// <summary>
        /// Đăng ký các instance của dịch vụ vào Singleton.
        /// </summary>
        public static void RegisterSingleton()
        {
            // Config
            NetworkConfig networkConfig = ConfigManager.Instance.GetConfig<NetworkConfig>();
            BufferConfig bufferConfig = ConfigManager.Instance.GetConfig<BufferConfig>();
            SqlConfig sqlConfig = ConfigManager.Instance.GetConfig<SqlConfig>();

            // Application
            Singleton.Register<PacketOutgoing>();
            Singleton.Register<PacketIncoming>();
            Singleton.Register<PacketInserver>();
            Singleton.Register<CommandDispatcher>();

            Singleton.Register<RequestLimiter>(() =>
            new RequestLimiter(networkConfig.RateLimit, networkConfig.ConnectionLockoutDuration));

            // Core
            Singleton.Register<ISessionManager, SessionManager>();

            Singleton.Register<IPacketPool, PacketPool>(() => new PacketPool(10));

            Singleton.Register<IConnLimiter, ConnLimiter>(() =>
            new ConnLimiter(networkConfig.MaxConnections));

            Singleton.Register<IMultiSizeBufferPool, MultiSizeBufferPool>(() =>
            new MultiSizeBufferPool(bufferConfig.BufferAllocations, bufferConfig.TotalBuffers));

            Singleton.Register<IRequestLimiter, RequestLimiter>(() =>
            new RequestLimiter(networkConfig.RateLimit, networkConfig.ConnectionLockoutDuration));
        }
    }
}