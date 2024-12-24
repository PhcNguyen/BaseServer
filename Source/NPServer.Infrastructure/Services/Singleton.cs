using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NPServer.Shared.Services
{
    /// <summary>
    /// Lớp Singleton dùng để quản lý và khởi tạo các instance duy nhất của các class.
    /// </summary>
    public static class Singleton
    {
        private static readonly ConcurrentDictionary<Type, Lazy<object>> _instances = new();

        /// <summary>
        /// Trả về danh sách các loại đã được đăng ký.
        /// </summary>
        public static IEnumerable<Type> GetAllRegisteredTypes() => _instances.Keys;

        /// <summary>
        /// Đăng ký một instance cụ thể của một class vào Singleton.
        /// </summary>
        /// <typeparam name="TClass">Loại của class cần đăng ký.</typeparam>
        public static void Register<TClass>() where TClass : class, new()
        {
            _instances[typeof(TClass)] = new Lazy<object>(() => new TClass());
        }

        /// <summary>
        /// Đăng ký một class với hàm khởi tạo tùy chọn vào Singleton.
        /// </summary>
        /// <typeparam name="TClass">Loại của class cần đăng ký.</typeparam>
        /// <param name="initializer">Hàm khởi tạo để tạo instance.</param>
        /// <exception cref="ArgumentNullException">Ném ra nếu hàm khởi tạo là null.</exception>
        public static void Register<TClass>(Func<TClass> initializer) where TClass : class
        {
            if (initializer == null)
                throw new ArgumentNullException(nameof(initializer), "Initializer function cannot be null.");

            _instances[typeof(TClass)] = new Lazy<object>(() => initializer());
        }

        /// <summary>
        /// Đăng ký một interface và lớp cài đặt tương ứng trong Singleton.
        /// </summary>
        /// <typeparam name="TInterface">Interface cần đăng ký.</typeparam>
        /// <typeparam name="TImplementation">Lớp cài đặt của interface.</typeparam>
        public static void Register<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface, new()
        {
            _instances[typeof(TInterface)] = new Lazy<object>(() => new TImplementation());
        }

        /// <summary>
        /// Đăng ký một interface và lớp cài đặt tương ứng với hàm khởi tạo tùy chỉnh.
        /// </summary>
        /// <typeparam name="TInterface">Interface cần đăng ký.</typeparam>
        /// <typeparam name="TImplementation">Lớp cài đặt của interface.</typeparam>
        /// <param name="initializer">Hàm khởi tạo trả về instance của TImplementation.</param>
        public static void Register<TInterface, TImplementation>(Func<TImplementation> initializer)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            if (initializer == null)
                throw new ArgumentNullException(nameof(initializer), "Initializer function cannot be null.");

            _instances[typeof(TInterface)] = new Lazy<object>(() => initializer());
        }

        /// <summary>
        /// Kiểm tra xem instance của một class đã được tạo hay chưa.
        /// </summary>
        /// <typeparam name="TClass">Loại của class cần kiểm tra.</typeparam>
        /// <returns>True nếu instance đã được tạo, ngược lại False.</returns>
        public static bool IsInstanceCreated<TClass>() where TClass : class
        {
            return _instances.ContainsKey(typeof(TClass));
        }

        /// <summary>
        /// Lấy instance của một class singleton. Tự động khởi tạo nếu chưa tồn tại.
        /// </summary>
        /// <typeparam name="TClass">Loại của class cần lấy instance.</typeparam>
        /// <returns>Instance duy nhất của class.</returns>
        public static TClass GetInstance<TClass>() where TClass : class, new()
        {
            return (TClass)_instances.GetOrAdd(typeof(TClass), _ => new Lazy<object>(() => new TClass())).Value;
        }

        /// <summary>
        /// Lấy instance của một class singleton với hàm khởi tạo tùy chọn.
        /// </summary>
        /// <typeparam name="TClass">Loại của class cần lấy instance.</typeparam>
        /// <param name="initializer">Hàm khởi tạo tùy chọn.</param>
        /// <returns>Instance duy nhất của class.</returns>
        public static TClass GetInstance<TClass>(Func<TClass> initializer) where TClass : class
        {
            if (initializer == null) throw new ArgumentNullException(nameof(initializer), "Initializer function cannot be null.");

            return (TClass)_instances.GetOrAdd(typeof(TClass), _ => new Lazy<object>(() => initializer())).Value;
        }

        /// <summary>
        /// Lấy instance của một class singleton với các tham số khởi tạo.
        /// </summary>
        /// <typeparam name="TClass">Loại của class cần lấy instance.</typeparam>
        /// <param name="args">Các tham số khởi tạo.</param>
        /// <returns>Instance duy nhất của class.</returns>
        /// <exception cref="InvalidOperationException">Ném ra nếu không thể tạo instance.</exception>
        public static TClass GetInstance<TClass>(params object[] args) where TClass : class
        {
            try
            {
                return (TClass)_instances.GetOrAdd(typeof(TClass), _ => new Lazy<object>(() => Activator.CreateInstance(typeof(TClass), args)!)).Value;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create instance of {typeof(TClass)}", ex);
            }
        }

        /// <summary>
        /// Lấy instance của một interface từ Singleton.
        /// </summary>
        /// <typeparam name="TInterface">Interface cần lấy instance.</typeparam>
        /// <returns>Instance của interface.</returns>
        /// <exception cref="InvalidOperationException">Ném ra nếu instance chưa được đăng ký.</exception>
        public static TInterface GetInstanceOfInterface<TInterface>() where TInterface : class
        {
            if (!_instances.ContainsKey(typeof(TInterface)))
            {
                throw new InvalidOperationException($"No instance registered for {typeof(TInterface).FullName}.");
            }

            return _instances[typeof(TInterface)].Value as TInterface
                   ?? throw new InvalidOperationException($"Instance registered for {typeof(TInterface).FullName} is null.");
        }

        /// <summary>
        /// ResetForPool instance của class singleton và gọi Dispose nếu cần.
        /// </summary>
        /// <typeparam name="TClass">Loại của class cần reset instance.</typeparam>
        public static void ResetInstance<TClass>() where TClass : class
        {
            if (_instances.TryRemove(typeof(TClass), out var lazyInstance) && lazyInstance.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        /// <summary>
        /// ResetAll instance của class singleton và gọi Dispose nếu cần.
        /// </summary>
        public static void ResetAll()
        {
            foreach (var lazyInstance in _instances.Values)
            {
                if (lazyInstance.Value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _instances.Clear();
        }
    }
}