using System;

namespace NPServer.Infrastructure.Logging.Formatter;

/// <summary>
/// Lớp đại diện cho một thông điệp nhật ký.
/// </summary>
/// <param name="level">Mức độ của thông điệp nhật ký.</param>
/// <param name="text">Nội dung của thông điệp nhật ký.</param>
/// <param name="dateTime">Thời gian của thông điệp nhật ký. Nếu không được cung cấp, sử dụng thời gian hiện tại.</param>
/// <param name="callingClass">Tên lớp gọi.</param>
/// <param name="callingMethod">Tên phương thức gọi.</param>
/// <remarks>
/// Khởi tạo một <see cref="NPLogMessage"/> mới.
/// </remarks>
public class NPLogMessage(NPLogLevel level, string text, DateTime? dateTime = null, string? callingClass = null, string? callingMethod = null)
{
    /// <summary>
    /// Thời gian của thông điệp nhật ký.
    /// </summary>
    public DateTime DateTime { get; } = dateTime ?? DateTime.Now;

    /// <summary>
    /// Mức độ của thông điệp nhật ký.
    /// </summary>
    public NPLogLevel Level { get; } = level;

    /// <summary>
    /// Nội dung của thông điệp nhật ký.
    /// </summary>
    public string Text { get; } = text ?? throw new ArgumentNullException(nameof(text));

    /// <summary>
    /// Tên lớp gọi.
    /// </summary>
    public string? CallingClass { get; } = callingClass;

    /// <summary>
    /// Tên phương thức gọi.
    /// </summary>
    public string? CallingMethod { get; } = callingMethod;

    /// <summary>
    /// Phương thức <see cref="ToString"/> ghi đè để định dạng thông điệp nhật ký.
    /// </summary>
    /// <returns>Chuỗi định dạng của thông điệp nhật ký.</returns>
    public override string ToString()
    {
        // Reuse NLogFormatter instance if possible
        return new NPLogFormatter().ApplyFormat(this);
    }
}