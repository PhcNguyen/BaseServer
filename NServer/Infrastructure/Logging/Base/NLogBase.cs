using NServer.Infrastructure.Logging.Enums;
using NServer.Infrastructure.Logging.Formatter;
using NServer.Infrastructure.Logging.Handlers;
using NServer.Infrastructure.Logging.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NServer.Infrastructure.Logging.Base
{
    /// <summary>
    /// Lớp cơ sở cho hệ thống ghi nhật ký, cung cấp các phương thức cơ bản để ghi nhật ký.
    /// </summary>
    public abstract class NLogBase
    {
        private readonly NLogPublisher _logPublisher = new();
        protected bool _isTurned = true;

        /// <summary>
        /// Mức độ ghi nhật ký mặc định.
        /// </summary>
        public NLogLevel DefaultLevel { get; set; } = NLogLevel.INFO;

        /// <summary>
        /// Danh sách các thông điệp nhật ký.
        /// </summary>
        public IEnumerable<LogMessage> Messages => _logPublisher.Messages;

        public INLogPublisher LoggerHandlerManager => _logPublisher;

        /// <summary>
        /// Thiết lập hoặc lấy trạng thái lưu trữ thông điệp nhật ký.
        /// </summary>
        public bool StoreLogMessages
        {
            get => _logPublisher.StoreLogMessages;
            set => _logPublisher.StoreLogMessages = value;
        }

        /// <summary>
        /// Bật ghi nhật ký.
        /// </summary>
        public void On() => _isTurned = true;

        /// <summary>
        /// Tắt ghi nhật ký.
        /// </summary>
        public void Off() => _isTurned = false;

        /// <summary>
        /// Ghi một thông điệp với mức độ chỉ định.
        /// </summary>
        protected void Log(NLogLevel level, string message, string callingClass, string callingMethod)
        {
            if (!_isTurned) return;
            var logMessage = new LogMessage(level, message, DateTime.Now, callingClass, callingMethod);

            _logPublisher.Publish(logMessage);
        }

        /// <summary>
        /// Lấy thông tin về lớp và phương thức gọi.
        /// </summary>
        protected static (string CallingClass, string CallingMethod) GetCallerInfo()
        {
            var stackFrame = new StackTrace().GetFrame(2); // Bỏ qua các frame không cần thiết
            var methodBase = stackFrame?.GetMethod();
            var callingClass = methodBase?.ReflectedType?.Name ?? "UnknownClass";
            var callingMethod = methodBase?.Name ?? "UnknownMethod";
            return (callingClass, callingMethod);
        }

        /// <summary>
        /// Lấy tên của phương thức gọi.
        /// </summary>
        protected static string GetCallerMethodName()
        {
            var method = new StackTrace().GetFrame(2)?.GetMethod();
            return method?.Name ?? "UnknownMethod";
        }
    }
}