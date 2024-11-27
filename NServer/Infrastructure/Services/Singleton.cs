using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace NServer.Infrastructure.Services
{
    /// <summary>
    /// Lớp Singleton dùng để quản lý và khởi tạo các instance duy nhất của các class.
    /// </summary>
    internal class Singleton
    {
        private static readonly ConcurrentDictionary<Type, object> _instances = new();

        /// <summary>
        /// Trả về danh sách các loại đã được đăng ký.
        /// </summary>
        public static IEnumerable<Type> GetAllRegisteredTypes() => _instances.Keys;

        /// <summary>
        /// Kiểm tra xem instance của một class đã được tạo hay chưa.
        /// </summary>
        /// <typeparam name="T">Loại của class cần kiểm tra.</typeparam>
        /// <returns>True nếu instance đã được tạo, ngược lại False.</returns>
        public static bool IsInstanceCreated<T>() where T : class
        {
            return _instances.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Lấy instance của một class singleton. Tự động khởi tạo nếu chưa tồn tại.
        /// </summary>
        /// <typeparam name="T">Loại của class cần lấy instance.</typeparam>
        /// <returns>Instance duy nhất của class.</returns>
        public static T GetInstance<T>() where T : class, new()
        {
            return (T)_instances.GetOrAdd(typeof(T), _ => new T());
        }

        /// <summary>
        /// Lấy instance của một class singleton với hàm khởi tạo tùy chọn.
        /// </summary>
        /// <typeparam name="T">Loại của class cần lấy instance.</typeparam>
        /// <param name="initializer">Hàm khởi tạo tùy chọn.</param>
        /// <returns>Instance duy nhất của class.</returns>
        public static T GetInstance<T>(Func<T>? initializer = null) where T : class
        {
            return (T)_instances.GetOrAdd(typeof(T), _ => initializer?.Invoke() ?? Activator.CreateInstance<T>());
        }

        /// <summary>
        /// Lấy instance của một class singleton với các tham số khởi tạo.
        /// </summary>
        /// <typeparam name="T">Loại của class cần lấy instance.</typeparam>
        /// <param name="args">Các tham số khởi tạo.</param>
        /// <returns>Instance duy nhất của class.</returns>
        public static T GetInstance<T>(params object[] args) where T : class
        {
            return (T)_instances.GetOrAdd(typeof(T), _ => (T)Activator.CreateInstance(typeof(T), args)!);
        }

        /// <summary>
        /// Đăng ký một instance cụ thể của một class vào Singleton.
        /// </summary>
        /// <typeparam name="T">Loại của class cần đăng ký.</typeparam>
        /// <param name="instance">Instance cụ thể cần đăng ký.</param>
        public static void Register<T>(T instance) where T : class
        {
            _instances[typeof(T)] = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        /// <summary>
        /// Reset instance của class singleton và gọi Dispose nếu cần.
        /// </summary>
        /// <typeparam name="T">Loại của class cần reset instance.</typeparam>
        public static void ResetInstance<T>() where T : class
        {
            if (_instances.TryRemove(typeof(T), out var instance) && instance is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        /// <summary>
        /// Reset tất cả instance của class singleton và gọi Dispose nếu cần.
        /// </summary>
        public static void ResetAll()
        {
            foreach (var instance in _instances.Values)
            {
                if (instance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _instances.Clear();
        }
    }
}