namespace NPServer.Infrastructure.Logging.Filter;

/// <summary>
/// Lớp chứa các phương thức để lọc các thông điệp nhật ký dựa trên mức độ nhật ký.
/// </summary>
public static class FilterPredicates
{
    /// <summary>
    /// Kiểm tra xem mức độ nhật ký có cao hơn hoặc bằng mức độ được lọc hay không.
    /// </summary>
    /// <param name="logMessLevel">Mức độ của thông điệp nhật ký.</param>
    /// <param name="filterLevel">Mức độ cần lọc.</param>
    /// <returns>True nếu mức độ của thông điệp nhật ký cao hơn hoặc bằng mức độ cần lọc, ngược lại False.</returns>
    public static bool ByLevelHigher(NPLogBase.Level logMessLevel, NPLogBase.Level filterLevel) =>
        (int)logMessLevel >= (int)filterLevel;

    /// <summary>
    /// Kiểm tra xem mức độ nhật ký có thấp hơn hoặc bằng mức độ được lọc hay không.
    /// </summary>
    /// <param name="logMessLevel">Mức độ của thông điệp nhật ký.</param>
    /// <param name="filterLevel">Mức độ cần lọc.</param>
    /// <returns>True nếu mức độ của thông điệp nhật ký thấp hơn hoặc bằng mức độ cần lọc, ngược lại False.</returns>
    public static bool ByLevelLower(NPLogBase.Level logMessLevel, NPLogBase.Level filterLevel) =>
        (int)logMessLevel <= (int)filterLevel;

    /// <summary>
    /// Kiểm tra xem mức độ nhật ký có chính xác bằng mức độ được lọc hay không.
    /// </summary>
    /// <param name="logMessLevel">Mức độ của thông điệp nhật ký.</param>
    /// <param name="filterLevel">Mức độ cần lọc.</param>
    /// <returns>True nếu mức độ của thông điệp nhật ký chính xác bằng mức độ cần lọc, ngược lại False.</returns>
    public static bool ByLevelExactly(NPLogBase.Level logMessLevel, NPLogBase.Level filterLevel) =>
        (int)logMessLevel == (int)filterLevel;
}