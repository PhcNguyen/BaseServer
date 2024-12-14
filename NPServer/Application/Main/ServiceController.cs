using NPServer.Core.Helpers;
using NPServer.Core.Interfaces.Memory;
using NPServer.Core.Interfaces.Network;
using NPServer.Core.Interfaces.Session;
using NPServer.Core.Memory;
using NPServer.Core.Network.Firewall;
using NPServer.Core.Session;
using NPServer.Infrastructure.Config;
using NPServer.Infrastructure.Helper;
using NPServer.Infrastructure.Logging;
using NPServer.Infrastructure.Services;
using NPServer.Infrastructure.Settings;
using System;

namespace NPServer.Application.Main
{
    /// <summary>
    /// Lớp ServiceRegistry chịu trách nhiệm đăng ký các dịch vụ trong hệ thống.
    /// </summary>
    internal static class ServiceController
    {
        private static readonly BufferConfig _bufferConfig = ConfigManager.Instance.GetConfig<BufferConfig>();
        private static readonly NetworkConfig _networkConfig = ConfigManager.Instance.GetConfig<NetworkConfig>();

        public static readonly string VersionInfo =
            $"Version {AssemblyHelper.GetAssemblyInformationalVersion()} " +
            $"| {(System.Diagnostics.Debugger.IsAttached ? "Debug" : "Release")}";

        public static void Initialization()
        {
            NPLog.Instance.Info("ServiceController: Starting Initialization.");

            try
            {
                Singleton.GetInstanceOfInterface<IMultiSizeBufferPool>().AllocateBuffers();
                NPLog.Instance.DefaultInitialization();
                NPLog.Instance.Info("ServiceController: Initialization completed successfully.");
            }
            catch (Exception ex)
            {
                NPLog.Instance.Error("ServiceController: Initialization failed.", ex);
                throw;
            }
        }

        /// <summary>
        /// Đăng ký các instance của dịch vụ vào Singleton.axe ZASRBABEeev
        /// </summary>
        public static void RegisterSingleton()
        {
            // Application
            Singleton.Register<CommandController>();

            // Core
            Singleton.Register<ISessionManager, SessionManager>();

            Singleton.Register<IConnLimiter, ConnLimiter>(() =>
            new ConnLimiter(_networkConfig.MaxConnections));

            Singleton.Register<IFirewallRateLimit, FirewallRateLimit>(() =>
            new FirewallRateLimit(_networkConfig.MaxAllowedRequests,
            _networkConfig.TimeWindowInMilliseconds, _networkConfig.LockoutDurationInSeconds));

            Singleton.Register<IMultiSizeBufferPool, MultiSizeBufferPool>(() =>
            new MultiSizeBufferPool(_bufferConfig.BufferAllocationsString.ParseBufferAllocations(), _bufferConfig.TotalBuffers));
        }
    }
}