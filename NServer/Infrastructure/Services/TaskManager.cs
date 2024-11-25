using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            var taskList = tasks.Select(async task =>
            {
                await _semaphore.WaitAsync(cancellationToken); // Chờ nếu vượt quá giới hạn
                try
                {
                    await task();
                }
                finally
                {
                    _semaphore.Release(); // Giải phóng semaphore
                }
            });

            await Task.WhenAll(taskList); // Chờ tất cả tác vụ hoàn thành
        }
    }
}
