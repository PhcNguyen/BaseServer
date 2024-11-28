using Base.Core.Session;
using Base.Core.Interfaces.Packets;
using Base.Core.Interfaces.Session;
using Base.Infrastructure.Services;
using Base.Core.Packets.Queue;

namespace Base.Application.Main
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
            Singleton.Register<IPacketOutgoing, PacketOutgoing>();
            Singleton.Register<IPacketIncoming, PacketIncoming>();
            Singleton.Register<ISessionManager, SessionManager>();
        }
    }
}
