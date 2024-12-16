using System;

namespace NPServer.Core.Packets;

public partial class Packet
{
    /// <summary>
    /// Chữ ký để xác thực gói tin.
    /// </summary>
    private byte[] Signature = [];

    /// <summary> 
    /// Ký gói tin. 
    /// </summary> 
    private void SignPacket()
    {
        Signature = CreateSignature(ExtractSignableData());
    }

    /// <summary>
    /// Xác thực chữ ký gói tin.
    /// </summary>
    /// <returns>True nếu chữ ký hợp lệ, ngược lại là False.</returns>
    private bool VerifySignature()
    {
        byte[] expectedSignature = CreateSignature(ExtractSignableData());
        return Signature.Length == expectedSignature.Length &&
               Signature.AsSpan().SequenceEqual(expectedSignature);
    }

    /// <summary>
    /// Lấy dữ liệu cần ký từ gói tin.
    /// </summary>
    /// <returns>Mảng byte của dữ liệu cần ký.</returns>
    private ReadOnlyMemory<byte> ExtractSignableData()
    {
        byte[] payloadBytes = PayloadData.ToArray();
        byte[] cmdBytes = BitConverter.GetBytes(Cmd);

        byte[] combined = new byte[cmdBytes.Length + payloadBytes.Length];

        Buffer.BlockCopy(cmdBytes, 0, combined, 0, cmdBytes.Length);
        Buffer.BlockCopy(payloadBytes, 0, combined, cmdBytes.Length , payloadBytes.Length);

        return new ReadOnlyMemory<byte>(combined);
    }

    /// <summary>
    /// Tạo chữ ký xác thực gói tin dựa trên nội dung hiện tại.
    /// </summary>
    /// <returns>Chữ ký dạng byte array.</returns>
    private byte[] CreateSignature(ReadOnlyMemory<byte> signableData) => 
        System.Security.Cryptography.SHA256.HashData(signableData.Span);
}