using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using NServer.Infrastructure.Logging;

namespace NServer.Infrastructure.Services
{
    /// <summary>
    /// Lớp quản lý các tác vụ song song với giới hạn đồng thời.
    /// </summary>
    internal class TaskManager(int maxDegreeOfParallelism)
    {
        private readonly SemaphoreSlim _semaphore = new(maxDegreeOfParallelism);

        /// <summary>
        /// Thực thi một danh sách các tác vụ với giới hạn đồng thời.
        /// </summary>
        /// <typeparam name="T">Kiểu kết quả của từng tác vụ.</typeparam>
        /// <param name="tasks">Danh sách các tác vụ.</param>
        /// <param name="cancellationToken">Token hủy bỏ.</param>
        /// <returns>Một danh sách kết quả từ các tác vụ.</returns>
        public async Task ExecuteTasksAsync(IEnumerable<Func<Task>> tasks, CancellationToken cancellationToken)
        {
            // Lọc ra các task null (nếu có)
            tasks = tasks.Where(task => task != null).ToList();

            var taskList = tasks.Select(async task =>
            {
                await _semaphore.WaitAsync(cancellationToken);
                try
                {
                    await task();
                }
                catch (Exception ex)
                {
                    NLog.Instance.Error($"Task execution failed: {ex.Message}");
                }
                finally
                {
                    _semaphore.Release();
                }
            });

            // Chờ tất cả các tác vụ hoàn thành
            await Task.WhenAll(taskList);
        }
    }
}
