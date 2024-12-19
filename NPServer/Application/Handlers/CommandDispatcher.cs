using NPServer.Core.Commands.Abstract;
using NPServer.Core.Commands.Interfaces;
using NPServer.Infrastructure.Logging;
using System;
using System.Collections.Immutable;

namespace NPServer.Application.Handlers;

/// <summary>
/// Xử lý việc phân phối và thực thi các lệnh trong server.
/// </summary>
internal sealed class CommandDispatcher : AbstractCommandDispatcher
{
    /// <summary>
    /// ImmutableArray chứa các namespace nơi các trình xử lý lệnh được triển khai.
    /// </summary>
    private static readonly ImmutableArray<string> TargetNamespaces =
    [
        "NServer.Application.Handlers.System",
        "NServer.Application.Handlers.Authentication"
    ];

    /// <summary>
    /// Khởi tạo một instance mới của lớp <see cref="CommandDispatcher"/>.
    /// </summary>
    public CommandDispatcher() : base([.. TargetNamespaces])
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
    public (object Result, object? AdditionalData) HandleCommand(ICommandInput input)
    {
        // Kiểm tra đầu vào.
        if (input == null || !Enum.IsDefined(input.Command))
        {
            return ("Invalid command input.", null);
        }

        // Kiểm tra nếu lệnh tồn tại trong bộ nhớ cache.
        if (!CommandDelegateCache.TryGetValue(input.Command, out var commandInfo))
        {
            return ($"Unknown command: {input.Command}", null);
        }

        var (requiredRole, func) = commandInfo;

        // Kiểm tra quyền của người dùng.
        if (input.UserRole < requiredRole)
        {
            return ($"Permission denied for command: {input.Command}", null);
        }

        try
        {
            // Thực thi hàm xử lý lệnh và trả về kết quả.
            var result = func(input);
            return result is not null
                ? (result, null)
                : ("Command executed successfully, but no result was returned.", null);
        }
        catch (Exception ex)
        {
            // Ghi log lỗi chi tiết.
            NPLog.Instance.Error<CommandDispatcher>(
                $"Error executing command: {input.Command}. Exception: {ex.Message}"
            );
            return ($"Error executing command: {input.Command}", null);
        }
    }
}