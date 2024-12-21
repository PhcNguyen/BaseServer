using NPServer.Core.Interfaces.Memory;
using NPServer.Infrastructure.Logging;
using System.Collections.Generic;

namespace NPServer.Core.Memory;

/// <summary>
/// Lưu trữ các instance <see cref="IPoolable"/> để tái sử dụng sau.
/// </summary>
public sealed class ObjectPool
{
    private readonly Stack<IPoolable> _objects = new();

    /// <summary>
    /// Tổng số đối tượng đã tạo.
    /// </summary>
    public int TotalCount { get; private set; }

    /// <summary>
    /// Số đối tượng sẵn có trong pool.
    /// </summary>
    public int AvailableCount { get => _objects.Count; } 

    /// <summary>
    /// Tạo mới nếu cần và trả về một instance của <typeparamref name="T"/>.
    /// </summary>
    public T Get<T>() where T : IPoolable, new()
    {
        if (AvailableCount == 0)
        {
            T @object = new();

            TotalCount++;
            NPLog.Instance.Trace($"Get<T>(): Đã tạo một instance mới của {typeof(T).Name} (TotalCount={TotalCount})");

            return @object;
        }

        return (T)_objects.Pop();
    }

    /// <summary>
    /// Trả lại một instance của <typeparamref name="T"/> vào pool để tái sử dụng sau.
    /// </summary>
    public void Return<T>(T @object) where T : IPoolable, new()
    {
        @object.ResetForPool();
        _objects.Push(@object);
    }
}