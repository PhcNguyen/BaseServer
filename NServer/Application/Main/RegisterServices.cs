using NServer.Core.Session;
using NServer.Core.Packets.Queue;
using NServer.Core.Network.Firewall;
using NServer.Core.Interfaces.Packets;
using NServer.Core.Interfaces.Session;
using NServer.Core.Interfaces.Network;

using NServer.Infrastructure.Services;

namespace NServer.Application.Main
{
    /// <summary>
    /// Lớp ServiceRegistry chịu trách nhiệm đăng ký các dịch vụ trong hệ thống.
    /// </summary>
    internal class ServiceRegistry
    {
        /// <summary>
        /// Đăng ký các instance của dịch vụ vào Singleton.
        /// </summary>
        public static void Register()
        {
            Singleton.Register<IConnLimiter, ConnLimiter>();

            Singleton.Register<IPacketOutgoing, PacketOutgoing>();
            Singleton.Register<IPacketIncoming, PacketIncoming>();
            Singleton.Register<ISessionManager, SessionManager>();
        }
    }
}
