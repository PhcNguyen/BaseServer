using NPServer.Infrastructure.Logging.Filter;
using NPServer.Infrastructure.Logging.Formatter;
using NPServer.Infrastructure.Logging.Interfaces;
using System;
using System.Collections.Generic;

namespace NPServer.Infrastructure.Logging
{
    /// <summary>
    /// Quản lý các handler ghi nhật ký và lưu trữ thông điệp nhật ký nếu cần.
    /// </summary>
    internal class NPLogPublisher : INPLogPublisher
    {
        private readonly IList<INPLogTarget> _loggerHandlers;
        private readonly IList<NPLogMessage> _messages;

        /// <summary>
        /// Lấy danh sách các thông điệp nhật ký đã lưu trữ.
        /// </summary>
        public IEnumerable<NPLogMessage> Messages => _messages;

        /// <summary>
        /// Thiết lập hoặc lấy trạng thái lưu trữ thông điệp nhật ký.
        /// </summary>
        public bool StoreLogMessages { get; set; }

        /// <summary>
        /// Khởi tạo một <see cref="NPLogPublisher"/> mới.
        /// </summary>
        public NPLogPublisher()
        {
            _loggerHandlers = [];
            _messages = [];
            StoreLogMessages = false;
        }

        /// <summary>
        /// Khởi tạo một <see cref="NPLogPublisher"/> mới với lựa chọn lưu trữ thông điệp nhật ký.
        /// </summary>
        /// <param name="storeLogMessages">True nếu cần lưu trữ thông điệp nhật ký, ngược lại False.</param>
        public NPLogPublisher(bool storeLogMessages)
        {
            _loggerHandlers = [];
            _messages = [];
            StoreLogMessages = storeLogMessages;
        }

        /// <summary>
        /// Công khai một thông điệp nhật ký.
        /// </summary>
        /// <param name="logMessage">Thông điệp nhật ký cần công khai.</param>
        public void Publish(NPLogMessage logMessage)
        {
            if (StoreLogMessages) _messages.Add(logMessage);
            foreach (var loggerHandler in _loggerHandlers) loggerHandler.Publish(logMessage);
        }

        /// <summary>
        /// Thêm một handler ghi nhật ký.
        /// </summary>
        /// <param name="loggerHandler">Handler ghi nhật ký cần thêm.</param>
        /// <returns>Instance hiện tại của <see cref="NPLogPublisher"/>.</returns>
        public INPLogPublisher AddHandler(INPLogTarget loggerHandler)
        {
            if (loggerHandler != null) _loggerHandlers.Add(loggerHandler);
            return this;
        }

        /// <summary>
        /// Thêm một handler ghi nhật ký với bộ lọc.
        /// </summary>
        /// <param name="loggerHandler">Handler ghi nhật ký cần thêm.</param>
        /// <param name="filter">Bộ lọc để xác định xem thông điệp nhật ký có nên được xử lý hay không.</param>
        /// <returns>Instance hiện tại của <see cref="NPLogPublisher"/>.</returns>
        public INPLogPublisher AddHandler(INPLogTarget loggerHandler, Predicate<NPLogMessage> filter)
        {
            if (filter == null || loggerHandler == null) return this;

            return AddHandler(new FilteredHandler()
            {
                Filter = filter,
                Handler = loggerHandler
            });
        }

        /// <summary>
        /// Xóa một handler ghi nhật ký.
        /// </summary>
        /// <param name="loggerHandler">Handler ghi nhật ký cần xóa.</param>
        /// <returns>True nếu xóa thành công, ngược lại False.</returns>
        public bool RemoveHandler(INPLogTarget loggerHandler)
        {
            return _loggerHandlers.Remove(loggerHandler);
        }
    }
}