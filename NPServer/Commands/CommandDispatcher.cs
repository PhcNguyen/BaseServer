using NPServer.Commands.Abstract;
using NPServer.Commands.Interfaces;
using NPServer.Infrastructure.Logging;

namespace NPServer.Commands;

/// <summary>
/// Xử lý việc phân phối và thực thi các lệnh trong server.
/// </summary>
internal sealed class CommandDispatcher : AbstractCommandDispatcher
{
    // Mảng tĩnh chứa các namespace nơi các trình xử lý lệnh được triển khai.
    private static readonly string[] TargetNamespaces =
    [
        "NServer.Application.Handlers.Implementations",
    ];

    /// <summary>
    /// Khởi tạo một instance mới của lớp <see cref="CommandDispatcher"/>.
    /// </summary>
    public CommandDispatcher() : base(TargetNamespaces)
    {
    }

    /// <summary>
    /// Xử lý và thực thi một lệnh dựa trên đầu vào.
    /// </summary>
    /// <param name="input">Đầu vào lệnh chứa tên lệnh và ngữ cảnh người dùng.</param>
    /// <returns>
    /// Một tuple, phần tử đầu tiên là kết quả thực thi lệnh hoặc thông báo lỗi, 
    /// và phần tử thứ hai là dữ liệu bổ sung (nếu có).
    /// </returns>
    public (object, object?) HandleCommand(ICommandInput input)
    {
        // Kiểm tra nếu lệnh tồn tại trong bộ nhớ cache ủy quyền lệnh.
        if (!CommandDelegateCache.TryGetValue(input.Command, out var commandInfo))
            return ($"Unknown command: {input.Command}", null);


        var (requiredRole, func) = commandInfo;

        // Kiểm tra nếu vai trò của người dùng đủ quyền thực thi lệnh.
        if (input.UserRole < requiredRole)
            return ($"Permission denied for command: {input.Command}", null);

        try
        {
            // Thực thi hàm xử lý lệnh.
            if (func(input) is not object result)
                throw new System.InvalidOperationException("Invalid result type from command handler.");

            return (result, null);
        }
        catch (System.Exception ex)
        {
            // Ghi log lỗi khi thực thi lệnh.
            NPLog.Instance.Error<CommandDispatcher>(
                    $"Error executing command: {input.Command}. Exception: {ex.Message}");
            return ($"Error executing command: {input.Command}", null);
        }
    }
}
