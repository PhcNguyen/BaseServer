using NPServer.Infrastructure.Configuration;

namespace NPServer.Core.Memory.Buffer;

/// <summary>
/// Configuration for buffer settings.
/// </summary>
public sealed class BufferConfig : ConfigContainer
{
    /// <summary>
    /// Tổng số lượng buffers được tạo.
    /// </summary>
    public int TotalBuffers { get; private set; } = 1000;

    /// <summary>
    /// Chuỗi phân bổ buffer dạng "kích thước, tỷ lệ; kích thước, tỷ lệ; ...".
    /// </summary>
    public string BufferAllocationsString { get; private set; } =
        "256,0.40; 512,0.25; 1024,0.15; 2048,0.10; 4096,0.05; 8192,0.03; 16384,0.02";

    /// <summary>
    /// Mảng chứa thông tin về kích thước buffer và tỷ lệ phân bổ mặc định.
    /// </summary>
    [ConfigIgnore]
    public (int BufferSize, double Allocation)[] DefaultBufferAllocations { get; private set; } =
    [
        (256, 0.40), (512, 0.25),
        (1024, 0.15), (2048, 0.10),
        (4096, 0.05), (8192, 0.03),
        (16384, 0.02)
    ];
}