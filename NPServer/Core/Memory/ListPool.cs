using NPServer.Infrastructure.Logging;
using System.Collections.Generic;

namespace NPServer.Core.Memory;

/// <summary>
/// Cung cấp một pool các instance <see cref="List{T}"/> có thể tái sử dụng, tương tự như ArrayPool.
/// </summary>
public class ListPool<T>
{
    private readonly Stack<List<T>> _listStack = new();
    private int _totalCount = 0;

    public static ListPool<T> Instance { get; } = new();

    /// <summary>
    /// Lấy một instance <see cref="List{T}"/> từ pool.
    /// </summary>
    public List<T> Rent()
    {
        lock (_listStack)
        {
            if (_listStack.Count == 0)
            {
                NPLog.Instance.Trace($"Rent(): Tạo một instance mới của List<{typeof(T).Name}> (TotalCount={++_totalCount})");
                return [];
            }

            return _listStack.Pop();
        }
    }

    /// <summary>
    /// Xóa dữ liệu trong <see cref="List{T}"/> được cung cấp và đưa nó trở lại pool.
    /// </summary>
    public void Return(List<T> list)
    {
        list.Clear();

        lock (_listStack)
        {
            _listStack.Push(list);
        }
    }
}