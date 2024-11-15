namespace NETServer.Application.Enums
{
    /// <summary>
    /// Enum mô tả các trạng thái kiểm tra hợp lệ của dữ liệu hoặc lệnh.
    /// </summary>
    internal enum ValidationStatus
    {
        /// <summary>
        /// Dữ liệu hoặc lệnh hợp lệ.
        /// </summary>
        Valid,

        /// <summary>
        /// Dữ liệu hoặc lệnh không hợp lệ.
        /// </summary>
        Invalid,

        /// <summary>
        /// Dữ liệu hoặc lệnh trống.
        /// </summary>
        Empty,

        /// <summary>
        /// Dữ liệu hoặc lệnh quá lớn, vượt quá giới hạn kích thước.
        /// </summary>
        TooLarge,

        /// <summary>
        /// Dữ liệu hoặc lệnh có định dạng không hợp lệ.
        /// </summary>
        FormatError,

        /// <summary>
        /// Lệnh không hợp lệ hoặc không nhận diện được.
        /// </summary>
        InvalidCommand
    }
}
