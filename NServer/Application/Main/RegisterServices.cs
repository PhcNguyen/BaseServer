﻿using NServer.Application.Handlers;
using NServer.Application.Handlers.Packets.Queue;
using NServer.Core.BufferPool;
using NServer.Core.Interfaces.Network;
using NServer.Core.Interfaces.Session;
using NServer.Core.Network.Firewall;
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
        public static void RegisterServices()
        {
            Singleton.GetInstance<PacketOutgoing>();
            Singleton.GetInstance<PacketIncoming>();
            Singleton.GetInstance<CommandDispatcher>();

            Singleton.GetInstance<MultiSizeBuffer>(() =>
            new MultiSizeBuffer(Setting.BufferAllocations, Setting.TotalBuffers));

            Singleton.GetInstance<RequestLimiter>(() =>
            new RequestLimiter(Setting.RateLimit, Setting.ConnectionLockoutDuration));

            Singleton.Register<IConnLimiter, ConnLimiter>();
            Singleton.Register<ISessionManager, SessionManager>();
        }
    }
}