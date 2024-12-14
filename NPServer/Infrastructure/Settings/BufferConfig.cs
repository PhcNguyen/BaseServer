using NPServer.Infrastructure.Config;

namespace NPServer.Infrastructure.Settings;

internal sealed class BufferConfig : ConfigContainer
{
    public int TotalBuffers { get; private set; } = 1000;

    public string BufferAllocationsString { get; private set; } =
        "256,0.40; 512,0.25; 1024,0.15; 2048,0.10; 4096,0.05; 8192,0.03; 16384,0.02";

    [ConfigIgnore]
    public (int BufferSize, double Allocation)[] DefaultBufferAllocations { get; private set; } =
    [
        (256  , 0.40), (512  , 0.25),
        (1024 , 0.15), (2048 , 0.10),
        (4096 , 0.05), (8192 , 0.03),
        (16384, 0.02)
    ];
}