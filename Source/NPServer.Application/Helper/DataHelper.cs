using NPServer.Infrastructure.Helpers;

namespace NPServer.Application.Helper;

public static class DataHelper
{
    public static bool ValidateInput(string[] parts, int expectedLength) =>
        parts.Length == expectedLength;

    public static string[]? ParseInput(byte[] data, int expectedParts)
    {
        var input = ConverterHelper.ToString(data).Split(';');
        return input.Length == expectedParts ? input : null;
    }
}