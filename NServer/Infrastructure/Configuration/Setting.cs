using System;
using System.Collections.Generic;
using System.Security.Authentication;

namespace NServer.Infrastructure.Configuration
{
    public static class Setting
    {
        public static readonly byte VERSION = (byte)123;

        // Network Settings
        public static readonly int Port = NetworkConfig.Port;

        public static readonly string? IPAddress = NetworkConfig.IPAddress;
        public static readonly int MaxConnections = NetworkConfig.MaxConnections;
        public static readonly int BytesPerSecond = NetworkConfig.BytesPerSecond;
        public static readonly int MaxConnectionsPerIpAddress = NetworkConfig.MaxConnectionsPerIpAddress;
        public static readonly int RequestDelayMilliseconds = NetworkConfig.RequestDelayMilliseconds;
        public static readonly int ConnectionLockoutDuration = NetworkConfig.ConnectionLockoutDuration;
        public static readonly TimeSpan Timeout = NetworkConfig.ClientSessionTimeout;
        public static readonly (int MaxRequests, TimeSpan TimeWindow) RateLimit = NetworkConfig.RateLimit;

        // Các cài đặt mạng bổ sung Socket
        public static readonly bool Blocking = NetworkConfig.Blocking;

        public static readonly bool KeepAlive = NetworkConfig.KeepAlive;
        public static readonly bool ReuseAddress = NetworkConfig.ReuseAddress;

        // Buffer Settings
        public static readonly int TotalBuffers = BufferConfig.TotalBuffers;

        public static readonly Dictionary<int, double> BufferAllocations = BufferConfig.BufferAllocations;

        // Security Settings
        public static readonly bool IsSslEnabled = SecurityConfig.IsSslEnabled;

        public static readonly bool IsClientCertificateRequired = SecurityConfig.IsClientCertificateRequired;
        public static readonly bool IsCertificateRevocationCheckEnabled = SecurityConfig.IsCertificateRevocationCheckEnabled;
        public static readonly string SslPassword = SecurityConfig.SslPassword;
        public static readonly string SslPrivateKeyPath = SecurityConfig.SslPrivateKeyPath;
        public static readonly string SslCsrCertificatePath = SecurityConfig.SslCsrCertificatePath;
        public static readonly string SslCrtCertificatePath = SecurityConfig.SslCrtCertificatePath;
        public static readonly string SslPfxCertificatePath = SecurityConfig.SslPfxCertificatePath;
        public static readonly SslProtocols SupportedSslProtocols = SecurityConfig.SupportedSslProtocols;
    }
}