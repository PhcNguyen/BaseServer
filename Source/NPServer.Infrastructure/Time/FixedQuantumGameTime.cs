using System;

namespace NPServer.Infrastructure.Time;

/// <summary>
/// Đại diện cho thời gian trong game, được cập nhật theo các bước cố định (quantums).
/// </summary>
public sealed class FixedQuantumGameTime
{
    private TimeSpan _quantumGameTime; // Thời gian hiện tại theo quantum
    private TimeSpan _quantumSize;    // Kích thước của một bước quantum

    /// <summary>
    /// Khởi tạo một đối tượng FixedQuantumGameTime với kích thước bước thời gian xác định.
    /// </summary>
    /// <param name="quantumSize">Kích thước bước quantum.</param>
    public FixedQuantumGameTime(TimeSpan quantumSize)
    {
        // Lấy thời gian trong game hiện tại từ Clock
        _quantumGameTime = Clock.GameTime;

        // Đặt kích thước quantum ban đầu
        this.SetQuantumSize(quantumSize);
    }

    /// <summary>
    /// Cập nhật kích thước quantum cho thời gian.
    /// </summary>
    /// <param name="quantumSize">Kích thước bước quantum mới.</param>
    public void SetQuantumSize(TimeSpan quantumSize)
    {
        if (quantumSize.Ticks <= 0)
            throw new ArgumentException("Quantum size must be greater than zero.", nameof(quantumSize));

        _quantumSize = quantumSize;

        long numTimeQuantums = Clock.CalcNumTimeQuantums(_quantumGameTime, _quantumSize);
        _quantumGameTime = TimeSpan.FromTicks(numTimeQuantums * _quantumSize.Ticks);
    }

    /// <summary>
    /// Cập nhật thời gian quantum tới bước thời gian gần nhất dựa trên thời gian hiện tại.
    /// </summary>
    public void UpdateToNow()
    {
        // Lấy thời gian trong game thực tế
        TimeSpan gameTime = Clock.GameTime;

        // Tính số bước quantum từ thời gian hiện tại
        long numTimeQuantums = Clock.CalcNumTimeQuantums(gameTime - _quantumGameTime, _quantumSize);

        // Cập nhật _quantumGameTime thêm số lượng quantum đã tính
        _quantumGameTime += _quantumSize * numTimeQuantums;
    }

    /// <summary>
    /// Trả về chuỗi đại diện cho thời gian quantum dưới dạng microseconds.
    /// </summary>
    public override string ToString() => (_quantumGameTime.Ticks / 10).ToString();

    /// <summary>
    /// Chuyển đổi FixedQuantumGameTime thành TimeSpan, trả về giá trị quantum hiện tại.
    /// </summary>
    public static explicit operator TimeSpan(FixedQuantumGameTime fixedQuantumGameTime) => fixedQuantumGameTime._quantumGameTime;
}