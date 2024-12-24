using System;
using System.Threading;

namespace NPServer.Application.Helper;

internal static class RetryHelper
{
    /// <summary>
    /// Thử lại một hành động bất đồng bộ với số lần cố định và độ trễ giữa các lần thử.
    /// </summary>
    /// <param name="action">Hành động cần thực hiện.</param>
    /// <param name="maxRetries">Số lần thử tối đa.</param>
    /// <param name="delayMs">Độ trễ (ms) giữa các lần thử.</param>
    /// <param name="onRetry">Hành động khi retry.</param>
    /// <param name="onFailure">Hành động khi thất bại.</param>
    public static void Execute(Func<bool> action, int maxRetries, int delayMs, Action<int> onRetry, Action onFailure)
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try { if (action()) return; }
            catch { /* Giữ nguyên xử lý lỗi. */ }

            onRetry(attempt + 1);
            Thread.Sleep(delayMs);
        }

        onFailure();
    }
}