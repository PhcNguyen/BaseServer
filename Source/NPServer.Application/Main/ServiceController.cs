using NPServer.Application.Handlers;
using NPServer.Core.Helpers;
using NPServer.Core.Interfaces.Memory;
using NPServer.Core.Interfaces.Network;
using NPServer.Core.Interfaces.Session;
using NPServer.Core.Memory.Buffer;
using NPServer.Core.Network;
using NPServer.Core.Network.Firewall;
using NPServer.Core.Session;
using NPServer.Infrastructure.Configuration;
using NPServer.Infrastructure.Helpers;
using NPServer.Infrastructure.Logging;
using NPServer.Shared.Services;
using System;

namespace NPServer.Application.Main;

/// <summary>
/// Lớp ServiceRegistry chịu trách nhiệm đăng ký các dịch vụ trong hệ thống.
/// </summary>
public static class ServiceController
{
    public static readonly BufferConfig BufferConfig = ConfigManager.Instance.GetConfig<BufferConfig>();
    public static readonly NetworkConfig NetworkConfig = ConfigManager.Instance.GetConfig<NetworkConfig>();

    public static readonly string VersionInfo =
        $"Version {AssemblyHelper.GetAssemblyInformationalVersion()} " +
        $"| {(System.Diagnostics.Debugger.IsAttached ? "Debug" : "Release")}";

    public static void Initialization()
    {
        RegisterSingleton();

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
    private static void RegisterSingleton()
    {
        // Application
        Singleton.Register<CommandDispatcher>();

        // Core
        Singleton.Register<IConnLimiter, ConnLimiter>(() =>
        new ConnLimiter(NetworkConfig.MaxConnections));

        Singleton.Register<ISessionManager, SessionManager>(() =>
        new SessionManager(Singleton.GetInstanceOfInterface<IConnLimiter>()));

        Singleton.Register<IFirewallRateLimit, FirewallRateLimit>(() =>
        new FirewallRateLimit(NetworkConfig.MaxAllowedRequests,
        NetworkConfig.TimeWindowInMilliseconds, NetworkConfig.LockoutDurationInSeconds));

        Singleton.Register<IMultiSizeBufferPool, MultiSizeBufferPool>(() =>
        new MultiSizeBufferPool(BufferConfig.BufferAllocationsString.ParseBufferAllocations(), BufferConfig.TotalBuffers));
    }
}