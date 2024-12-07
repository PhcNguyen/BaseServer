using System;

namespace NPServer.Infrastructure.Configuration.Abstract
{
    /// <summary>
    /// Thuộc tính để bỏ qua các thuộc tính khi khởi tạo các container cấu hình.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ConfigIgnoreAttribute : Attribute { }
}
