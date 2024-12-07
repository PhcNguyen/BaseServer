﻿using NPServer.Infrastructure.Configuration.Abstract;

namespace NPServer.Infrastructure.Configuration.Default
{
    internal class BufferConfig : ConfigContainer
    {
        public int TotalBuffers { get; private set; } = 1000;

        [ConfigIgnore]
        public (int BufferSize, double Allocation)[] BufferAllocations { get; private set; } =
        [
            (256  , 0.40), (512  , 0.25),
            (1024 , 0.15), (2048 , 0.10),
            (4096 , 0.05), (8192 , 0.03),
            (16384, 0.02)
        ];
    }
}