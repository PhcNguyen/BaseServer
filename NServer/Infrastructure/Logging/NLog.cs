using NServer.Infrastructure.Logging.Base;
using NServer.Infrastructure.Logging.Enums;
using NServer.Infrastructure.Logging.Formatter;
using NServer.Infrastructure.Logging.Handlers;
using System;

namespace NServer.Infrastructure.Logging
{
    /// <summary>
    /// Lớp chính cho hệ thống ghi nhật ký, cung cấp các phương thức để ghi nhật ký thông qua các handler khác nhau.
    /// </summary>
    public class NLog : NLogBase
    {
        private static readonly Lazy<NLog> _instance = new(() => new NLog());

        /// <summary>
        /// Constructor riêng tư cho Singleton.
        /// </summary>
        private NLog()
        { }

        /// <summary>
        /// Trả về instance duy nhất của <see cref="NLog"/>.
        /// </summary>
        public static NLog Instance => _instance.Value;

        /// <summary>
        /// Khởi tạo mặc định các handler.
        /// </summary>
        public void DefaultInitialization()
        {
            LoggerHandlerManager
                .AddHandler(new NLogConsole())
                .AddHandler(new NLogFile());
            Log(NLogLevel.INFO, "Default initialization");
        }

        /// <summary>
        /// Kiểm tra xem mức độ nhật ký có nên được ghi lại hay không.
        /// </summary>
        /// <param name="level">Mức độ nhật ký cần kiểm tra.</param>
        /// <returns>True nếu mức độ nhật ký lớn hơn hoặc bằng mức độ mặc định, ngược lại False.</returns>
        public bool ShouldLog(NLogLevel level) => level >= DefaultLevel;


        /// <summary>
        /// Ghi một thông điệp với mức độ mặc định.
        /// </summary>
        public void Log(string message) => Log(DefaultLevel, message);

        /// <summary>
        /// Ghi một thông điệp với mức độ chỉ định và thông tin chi tiết từ stack trace.
        /// </summary>
        /// <param name="level">Mức độ nhật ký.</param>
        /// <param name="message">Thông điệp nhật ký.</param>
        public void Log(NLogLevel level, string message)
        {
            var (callingClass, callingMethod) = GetCallerInfo();
            base.Log(level, message, callingClass, callingMethod);
        }

        /// <summary>
        /// Ghi một thông điệp với mức độ và lớp chỉ định.
        /// </summary>
        /// <typeparam name="TClass">Lớp ghi nhật ký.</typeparam>
        /// <param name="level">Mức độ nhật ký.</param>
        /// <param name="message">Thông điệp nhật ký.</param>
        public void Log<TClass>(NLogLevel level, string message) where TClass : class
        {
            string callingClass = typeof(TClass).Name;
            string callingMethod = GetCallerMethodName();
            base.Log(level, message, callingClass, callingMethod);
        }


        /// <summary>
        /// Ghi một thông điệp với mức độ INFO.
        /// </summary>
        public void Info(string message) => Log(NLogLevel.INFO, message);

        public void Info<TClass>(string message) where TClass : class =>
            Log<TClass>(NLogLevel.INFO, message);

        /// <summary>
        /// Ghi một thông điệp cảnh báo.
        /// </summary>
        public void Warning(string message) => Log(NLogLevel.WARNING, message);

        public void Warning<TClass>(string message) where TClass : class =>
            Log<TClass>(NLogLevel.WARNING, message);


        /// <summary>
        /// Ghi một thông điệp lỗi với một ngoại lệ.
        /// </summary>
        public void Error(Exception exception) =>
            Log(NLogLevel.ERROR, NLogFormatter.FormatExceptionMessage(exception));

        /// <summary>
        /// Ghi một thông điệp lỗi với một thông điệp và ngoại lệ.
        /// </summary>
        public void Error(string message, Exception exception) =>
            Log(NLogLevel.ERROR, $"{message}: {NLogFormatter.FormatExceptionMessage(exception)}");

        public void Error<TClass>(string message) where TClass : class =>
            Log<TClass>(NLogLevel.ERROR, message);

        public void Error<TClass>(Exception exception) where TClass : class =>
            Log<TClass>(NLogLevel.ERROR, NLogFormatter.FormatExceptionMessage(exception));

        public void Error<TClass>(string message, Exception exception) where TClass : class =>
            Log<TClass>(NLogLevel.ERROR, $"{message}: {NLogFormatter.FormatExceptionMessage(exception)}");
    }
}