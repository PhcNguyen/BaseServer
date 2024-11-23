﻿namespace NServer.Infrastructure.Configuration
{
    internal class BufferConfig
    {
        public static readonly int TotalBuffers = 200; // TU DONG TANG KHI THIEU
        public readonly static Dictionary<int, double> BufferAllocations = new()
        {
            { 256, 0.12 },
            { 512, 0.15 },
            { 1024, 0.35 },
            { 2048, 0.10 },
            { 4096, 0.20 },
            { 8192, 0.05 },
            { 8192 * 2, 0.03 }
        };
}
}