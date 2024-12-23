using System;

namespace NPServer.Common.Interfaces.Packets;

public partial interface IPacket
{
    /// <summary>
    /// Chuyển đổi gói tin thành mảng byte.
    /// </summary>
    /// <returns>Mảng byte đại diện cho gói tin.</returns>
    byte[] Pack();

    /// <summary>
    /// Giải mã gói tin từ mảng byte.
    /// </summary>
    /// <param name="data">Dữ liệu dạng byte cần giải mã.</param>
    /// <returns>Trả về `true` nếu thành công, ngược lại `false`.</returns>
    bool UnPack(ReadOnlySpan<byte> data);

    /// <summary>
    /// Chuyển đổi gói tin thành định dạng JSON.
    /// </summary>
    /// <returns>Chuỗi JSON đại diện cho gói tin.</returns>
    string ToJson();

    /// <summary>
    /// Kiểm tra sự tương đương giữa hai gói tin.
    /// </summary>
    /// <param name="obj">Gói tin để so sánh.</param>
    /// <returns>True nếu tương đương, ngược lại False.</returns>
    bool Equals(object? obj);

    /// <summary>
    /// Tạo một bản sao của gói tin hiện tại.
    /// </summary>
    /// <returns>Bản sao của gói tin.</returns>
    IPacket Clone();

    /// <summary>
    /// Kiểm tra tính hợp lệ của chữ ký (signature) của gói tin.
    /// </summary>
    /// <returns>True nếu chữ ký hợp lệ, ngược lại False.</returns>
    bool ValidateSignature();
}
