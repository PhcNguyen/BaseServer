using NServer.Infrastructure.Logging.Formatter;
using System;

namespace NServer.Infrastructure.Logging.Filter;

/// <summary>
/// Lớp dùng để lọc các thông điệp nhật ký dựa trên mức độ nhật ký.
/// </summary>
public class FilterByLevel
{
    /// <summary>
    /// Mức độ nhật ký được lọc.
    /// </summary>
    public NLogLevel FilteredLevel { get; set; }

    /// <summary>
    /// Cờ để chỉ ra rằng mức độ nhật ký phải chính xác bằng mức độ được lọc.
    /// </summary>
    public bool ExactlyLevel { get; set; }

    /// <summary>
    /// Cờ để chỉ ra rằng chỉ các thông điệp có mức độ cao hơn hoặc bằng mức độ được lọc mới được giữ lại.
    /// </summary>
    public bool OnlyHigherLevel { get; set; }

    /// <summary>
    /// Khởi tạo một <see cref="FilterByLevel"/> mới với mức độ nhật ký chỉ định.
    /// </summary>
    /// <param name="level">Mức độ nhật ký cần lọc.</param>
    public FilterByLevel(NLogLevel level)
    {
        FilteredLevel = level;
        ExactlyLevel = true;
        OnlyHigherLevel = true;
    }

    /// <summary>
    /// Khởi tạo một <see cref="FilterByLevel"/> mới với các giá trị mặc định.
    /// </summary>
    public FilterByLevel()
    {
        ExactlyLevel = false;
        OnlyHigherLevel = true;
    }

    /// <summary>
    /// Bộ lọc để xác định xem thông điệp nhật ký có nên được giữ lại hay không.
    /// </summary>
    public Predicate<LogMessage> Filter => logMessage =>
    {
        if (ExactlyLevel)
            return FilterPredicates.ByLevelExactly(logMessage.Level, FilteredLevel);
        return OnlyHigherLevel
            ? FilterPredicates.ByLevelHigher(logMessage.Level, FilteredLevel)
            : FilterPredicates.ByLevelLower(logMessage.Level, FilteredLevel);
    };
}