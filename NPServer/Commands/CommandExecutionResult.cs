namespace NPServer.Commands;

public sealed class CommandExecutionResult(short statusCode, string? message = null, object? data = null)
{
    public short StatusCode { get; } = statusCode;
    public string? Message { get; } = message;
    public object? Data { get; } = data;

    public static CommandExecutionResult Success(object? data = null) => new(0, "Success", data);

    public static CommandExecutionResult Error(string message) => new(-1, message);
}