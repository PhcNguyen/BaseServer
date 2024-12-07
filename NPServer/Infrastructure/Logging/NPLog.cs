using NPServer.Infrastructure.Logging.Abstract;
using NPServer.Infrastructure.Logging.Formatter;
using NPServer.Infrastructure.Logging.Handlers;
using System;

namespace NPServer.Infrastructure.Logging
{
    /// <summary>
    /// Lớp chính cho hệ thống ghi nhật ký, cung cấp các phương thức để ghi nhật ký thông qua các handler khác nhau.
    /// </summary>
    public class NPLog : NPLogBase
    {
        /// <summary>
        /// Defines the log level for logging messages.
        /// </summary>
        public enum Level
        {
            NONE,
            INFO,
            WARNING,
            ERROR,
            CRITICAL
        }

        private static readonly Lazy<NPLog> _instance = new(() => new NPLog());

        /// <summary>
        /// Constructor riêng tư cho Singleton.
        /// </summary>
        private NPLog()
        { }

        /// <summary>
        /// Trả về instance duy nhất của <see cref="NPLog"/>.
        /// </summary>
        public static NPLog Instance => _instance.Value;

        /// <summary>
        /// Khởi tạo mặc định các handler.
        /// </summary>
        public void DefaultInitialization()
        {
            LoggerHandlerManager
                .AddHandler(new NPLogConsole())
                .AddHandler(new NPLogFile());
            Log(Level.INFO, "Default initialization");
        }

        /// <summary>
        /// Kiểm tra xem mức độ nhật ký có nên được ghi lại hay không.
        /// </summary>
        /// <param name="level">Mức độ nhật ký cần kiểm tra.</param>
        /// <returns>True nếu mức độ nhật ký lớn hơn hoặc bằng mức độ mặc định, ngược lại False.</returns>
        public bool ShouldLog(Level level) => level >= DefaultLevel;

        /// <summary>
        /// Ghi một thông điệp với mức độ mặc định.
        /// </summary>
        public void Log(string message) => Log(DefaultLevel, message);

        /// <summary>
        /// Ghi một thông điệp với mức độ chỉ định và thông tin chi tiết từ stack trace.
        /// </summary>
        /// <param name="level">Mức độ nhật ký.</param>
        /// <param name="message">Thông điệp nhật ký.</param>
        public void Log(Level level, string message) =>
            LogInternal(level, message, null, null);

        /// <summary>
        /// Ghi một thông điệp với mức độ và lớp chỉ định.
        /// </summary>
        /// <typeparam name="TClass">Lớp ghi nhật ký.</typeparam>
        /// <param name="level">Mức độ nhật ký.</param>
        /// <param name="message">Thông điệp nhật ký.</param>
        public void Log<TClass>(Level level, string message) where TClass : class =>
            LogInternal(level, message, typeof(TClass).Name, GetCallerMethodName());

        /// <summary>
        /// Ghi một thông điệp với mức độ và lớp chỉ định, cùng với delegate.
        /// </summary>
        public void Log<TClass, TFunc>(Level level, string message)
            where TClass : class
            where TFunc : Delegate =>
            LogInternal(level, message, typeof(TClass).Name, typeof(TFunc).Name);

        /// <summary>
        /// Ghi một thông điệp với mức độ INFO.
        /// </summary>
        public void Info(string message) => Log(Level.INFO, message);

        public void Info<TClass>(string message) where TClass : class =>
            Log<TClass>(Level.INFO, message);

        /// <summary>
        /// Ghi một thông điệp cảnh báo.
        /// </summary>
        public void Warning(string message) => Log(Level.WARNING, message);

        public void Warning<TClass>(string message) where TClass : class =>
            Log<TClass>(Level.WARNING, message);

        /// <summary>
        /// Ghi một thông điệp lỗi với một ngoại lệ.
        /// </summary>
        public void Error(Exception exception) =>
            Log(Level.ERROR, NPLogFormatter.FormatExceptionMessage(exception));

        /// <summary>
        /// Ghi một thông điệp lỗi với một thông điệp và ngoại lệ.
        /// </summary>
        public void Error(string message, Exception exception) =>
            Log(Level.ERROR, $"{message}: {NPLogFormatter.FormatExceptionMessage(exception)}");

        public void Error<TClass>(string message) where TClass : class =>
            Log<TClass>(Level.ERROR, message);

        public void Error<TClass>(Exception exception) where TClass : class =>
            Log<TClass>(Level.ERROR, NPLogFormatter.FormatExceptionMessage(exception));

        public void Error<TClass>(string message, Exception exception) where TClass : class =>
            Log<TClass>(Level.ERROR, $"{message}: {NPLogFormatter.FormatExceptionMessage(exception)}");
    }
}