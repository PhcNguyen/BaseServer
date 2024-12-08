using NPServer.Infrastructure.Logging.Formatter;
using NPServer.Infrastructure.Logging.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NPServer.Infrastructure.Logging
{
    /// <summary>
    /// Lớp cơ sở cho hệ thống ghi nhật ký, cung cấp các phương thức cơ bản để ghi nhật ký.
    /// </summary>
    public abstract class NPLogBase
    {
        private readonly NPLogPublisher _logPublisher = new();
        protected bool _isTurned = true;

        /// <summary>
        /// Mức độ ghi nhật ký mặc định.
        /// </summary>
        public NPLog.Level DefaultLevel { get; set; } = NPLog.Level.INFO;

        /// <summary>
        /// Danh sách các thông điệp nhật ký.
        /// </summary>
        public IEnumerable<NPLogMessage> Messages => _logPublisher.Messages;

        public INPLogPublisher LoggerHandlerManager => _logPublisher;

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
        protected void LogInternal(NPLog.Level level, string message, string? callingClass, string? callingMethod)
        {
            if (!_isTurned) return;
            var logMessage = new NPLogMessage(level, message, DateTime.Now, callingClass, callingMethod);

            _logPublisher.Publish(logMessage);
        }

        /// <summary>
        /// Lấy tên của phương thức gọi.
        /// </summary>
        protected static string GetCallerMethodName()
        {
            var method = new StackTrace().GetFrame(3)?.GetMethod();
            return method?.Name ?? "UnknownMethod";
        }
    }
}