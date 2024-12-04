using NServer.Infrastructure.Configuration.Internal;

namespace NServer.Infrastructure.Configuration;

public static class Setting
{
    // Network Settings
    public static readonly string? IPAddress = NetworkConfig.IPAddress;

    public static readonly int Port = NetworkConfig.Port;
    public static readonly int MaxConnections = NetworkConfig.MaxConnections;
    public static readonly int BytesPerSecond = NetworkConfig.BytesPerSecond;
    public static readonly int RequestDelayMilliseconds = NetworkConfig.RequestDelayMilliseconds;
    public static readonly int ConnectionLockoutDuration = NetworkConfig.ConnectionLockoutDuration;
    public static readonly int MaxConnectionsPerIpAddress = NetworkConfig.MaxConnectionsPerIpAddress;

    public static readonly System.TimeSpan Timeout = NetworkConfig.ClientSessionTimeout;
    public static readonly (int MaxRequests, System.TimeSpan TimeWindow) RateLimit = NetworkConfig.RateLimit;

    // Các cài đặt mạng bổ sung Socket
    public static readonly bool Blocking = NetworkConfig.Blocking;
    public static readonly bool KeepAlive = NetworkConfig.KeepAlive;
    public static readonly bool ReuseAddress = NetworkConfig.ReuseAddress;

    // Buffer Settings
    public static readonly int TotalBuffers = BufferConfig.TotalBuffers;

    public static readonly (int BufferSize, double Allocation)[] BufferAllocations = BufferConfig.BufferAllocations;
}