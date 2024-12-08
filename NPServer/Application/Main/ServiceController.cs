using NPServer.Core.Interfaces.Network;
using NPServer.Core.Interfaces.Session;
using NPServer.Core.Network.Firewall;
using NPServer.Core.Session;
using NPServer.Infrastructure.Configuration;
using NPServer.Infrastructure.Logging;
using NPServer.Infrastructure.Services;
using NPServer.Infrastructure.Configuration.Default;
using NPServer.Core.Pooling;
using NPServer.Core.Interfaces.Pooling;

namespace NPServer.Application.Main
{
    /// <summary>
    /// Lớp ServiceRegistry chịu trách nhiệm đăng ký các dịch vụ trong hệ thống.
    /// </summary>
    internal static class ServiceController
    {
        private static readonly NetworkConfig _networkConfig = ConfigManager.Instance.GetConfig<NetworkConfig>();
        private static readonly BufferConfig _bufferConfig = ConfigManager.Instance.GetConfig<BufferConfig>();

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
            // Application

            Singleton.Register<RequestLimiter>(() =>
            new RequestLimiter(_networkConfig.RateLimit, _networkConfig.ConnectionLockoutDuration));

            // Core
            Singleton.Register<ISessionManager, SessionManager>();

            Singleton.Register<IPacketPool, PacketPool>(() => new PacketPool(10));

            Singleton.Register<IConnLimiter, ConnLimiter>(() =>
            new ConnLimiter(_networkConfig.MaxConnections));

            Singleton.Register<IMultiSizeBufferPool, MultiSizeBufferPool>(() =>
            new MultiSizeBufferPool(_bufferConfig.BufferAllocations, _bufferConfig.TotalBuffers));

            Singleton.Register<IRequestLimiter, RequestLimiter>(() =>
            new RequestLimiter(_networkConfig.RateLimit, _networkConfig.ConnectionLockoutDuration));
        }
    }
}