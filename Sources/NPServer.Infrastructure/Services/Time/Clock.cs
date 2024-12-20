using System;
using System.Diagnostics;

namespace NPServer.Infrastructure.Services.Time;

/// <summary>
/// Cung cấp các chức năng xử lý thời gian chính xác cho hệ thống.
/// </summary>
public static class Clock
{
    // Mốc thời gian trong game (Sat Dec 07 2024 08:24:35 GMT+0700), tính từ dữ liệu gói tin
    private const long GameTimeEpochTimestamp = 1733534675;

    // Mốc thời gian Unix chuẩn (1/1/1970)
    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    // Mốc thời gian trong game, được tính từ UnixEpoch
    private static readonly DateTime GameTimeEpoch = UnixEpoch.AddTicks(GameTimeEpochTimestamp * 10L);

    // Cơ sở thời gian, sử dụng Stopwatch để tính chính xác cao
    private static readonly DateTime _utcBase;

    private static readonly Stopwatch _utcStopwatch;

    static Clock()
    {
        _utcBase = DateTime.UtcNow; // Lấy thời gian UTC ban đầu
        _utcStopwatch = Stopwatch.StartNew(); // Khởi động Stopwatch
    }

    /// <summary>
    /// Trả về thời gian UTC hiện tại chính xác cao.
    /// </summary>
    public static DateTime UtcNowPrecise { get => _utcBase.Add(_utcStopwatch.Elapsed); }

    /// <summary>
    /// Trả về thời gian Unix hiện tại (kể từ 01/01/1970), dưới dạng TimeSpan.
    /// </summary>
    public static TimeSpan UnixTime { get => UtcNowPrecise - UnixEpoch; }

    /// <summary>
    /// Trả về thời gian game hiện tại (kể từ 7/12/2024), dưới dạng TimeSpan.
    /// </summary>
    public static TimeSpan GameTime { get => UtcNowPrecise - GameTimeEpoch; }

    /// <summary>
    /// Tính số lượng bước thời gian (quantums) trong một TimeSpan dựa trên kích thước bước.
    /// </summary>
    public static long CalcNumTimeQuantums(TimeSpan time, TimeSpan quantumSize)
    {
        return time.Ticks / quantumSize.Ticks;
    }

    /// <summary>
    /// Chuyển đổi timestamp Unix (milliseconds) thành DateTime.
    /// </summary>
    public static DateTime UnixTimeMillisecondsToDateTime(long timestamp)
    {
        return UnixEpoch.AddMilliseconds(timestamp);
    }

    /// <summary>
    /// Chuyển đổi timestamp Unix (microseconds) thành DateTime.
    /// </summary>
    public static DateTime UnixTimeMicrosecondsToDateTime(long timestamp)
    {
        return UnixEpoch.AddTicks(timestamp * 10);
    }

    /// <summary>
    /// Chuyển đổi timestamp game (milliseconds) thành DateTime.
    /// </summary>
    public static DateTime GameTimeMillisecondsToDateTime(long timestamp)
    {
        return GameTimeEpoch.AddMilliseconds(timestamp);
    }

    /// <summary>
    /// Chuyển đổi timestamp game (microseconds) thành DateTime.
    /// </summary>
    public static DateTime GameTimeMicrosecondsToDateTime(long timestamp)
    {
        return GameTimeEpoch.AddTicks(timestamp * 10);
    }

    /// <summary>
    /// Chuyển đổi DateTime thành TimeSpan đại diện cho thời gian Unix.
    /// </summary>
    public static TimeSpan DateTimeToUnixTime(DateTime dateTime)
    {
        return dateTime - UnixEpoch;
    }

    /// <summary>
    /// Chuyển đổi DateTime thành TimeSpan đại diện cho thời gian game.
    /// </summary>
    public static TimeSpan DateTimeToGameTime(DateTime dateTime)
    {
        return dateTime - GameTimeEpoch;
    }

    /// <summary>
    /// Chuyển đổi TimeSpan (thời gian Unix) thành DateTime.
    /// </summary>
    public static DateTime UnixTimeToDateTime(TimeSpan timeSpan)
    {
        return UnixEpoch.Add(timeSpan);
    }

    /// <summary>
    /// Chuyển đổi TimeSpan (thời gian game) thành DateTime.
    /// </summary>
    public static DateTime GameTimeToDateTime(TimeSpan timeSpan)
    {
        return GameTimeEpoch.Add(timeSpan);
    }

    /// <summary>
    /// So sánh và trả về khoảng thời gian lớn nhất giữa hai TimeSpan.
    /// </summary>
    public static TimeSpan Max(TimeSpan time1, TimeSpan time2)
    {
        return time1 > time2 ? time1 : time2;
    }

    /// <summary>
    /// So sánh và trả về khoảng thời gian nhỏ nhất giữa hai TimeSpan.
    /// </summary>
    public static TimeSpan Min(TimeSpan time1, TimeSpan time2)
    {
        return time1 < time2 ? time1 : time2;
    }
}