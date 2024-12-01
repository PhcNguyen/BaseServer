using NServer.Application.Handlers;
using NServer.Core.Interfaces.Network;
using NServer.Core.Interfaces.Session;
using NServer.Core.Network.BufferPool;
using NServer.Core.Network.Firewall;
using NServer.Core.Packets.Queue;
using NServer.Core.Session;
using NServer.Infrastructure.Configuration;
using NServer.Infrastructure.Services;

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
        public static void Register()
        {
            Singleton.GetInstance<PacketOutgoing>();
            Singleton.GetInstance<PacketIncoming>();

            Singleton.GetInstance<MultiSizeBuffer>();
            Singleton.GetInstance<CommandDispatcher>();
            Singleton.GetInstance<RequestLimiter>(() =>
            new RequestLimiter(Setting.RateLimit, Setting.ConnectionLockoutDuration));

            Singleton.Register<IConnLimiter, ConnLimiter>();
            Singleton.Register<ISessionManager, SessionManager>(); 
        }
    }
}