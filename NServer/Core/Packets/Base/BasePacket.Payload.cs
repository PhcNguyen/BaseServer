using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NServer.Core.Packets.Base
{
    public partial class BasePacket
    {
        private Memory<byte> _payload;

        /// <summary>
        /// Dữ liệu chính của gói tin.
        /// </summary>
        public Memory<byte> Payload
        {
            get => _payload;
            protected set
            {
                if (value.Length > int.MaxValue - _headerSize)
                    throw new ArgumentOutOfRangeException(nameof(value), "Payload too big.");
                _payload = value;
            }
        }

        public bool TrySetPayload(ReadOnlySpan<byte> newPayload)
        {
            if (newPayload.IsEmpty) return false;
            _payload = new Memory<byte>(newPayload.ToArray());
            return true;
        }

        public void SetPayload(string newPayload) =>
            _payload = new Memory<byte>(Encoding.UTF8.GetBytes(newPayload));

        public void SetPayload(Span<byte> newPayload)
        {
            if (newPayload.Length > int.MaxValue - _headerSize)
                throw new ArgumentOutOfRangeException(nameof(newPayload), "Payload quá lớn.");
            _payload = new Memory<byte>(newPayload.ToArray());
        }

        public void AppendToPayload(byte[] additionalData)
        {
            var combined = new byte[_payload.Length + additionalData.Length];
            _payload.Span.CopyTo(combined);
            additionalData.CopyTo(combined.AsSpan(_payload.Length));

            _payload = new Memory<byte>(combined);
        }

        public bool RemovePayloadSection(int startIndex, int length)
        {
            if (startIndex < 0 || startIndex >= _payload.Length || length < 0 || startIndex + length > _payload.Length)
                return false;

            var newPayload = new byte[_payload.Length - length];
            _payload.Span[..startIndex].CopyTo(newPayload);
            _payload.Span[(startIndex + length)..].CopyTo(newPayload.AsSpan(startIndex));

            _payload = new Memory<byte>(newPayload);
            return true;
        }

        public bool ReplacePayloadSection(int startIndex, byte[] newData)
        {
            if (startIndex < 0 || startIndex >= _payload.Length || newData == null)
                return false;

            var newPayload = new byte[_payload.Length - newData.Length + newData.Length];
            _payload.Span[..startIndex].CopyTo(newPayload);
            newData.CopyTo(newPayload.AsSpan(startIndex));
            _payload.Span[(startIndex + newData.Length)..].CopyTo(newPayload.AsSpan(startIndex + newData.Length));

            _payload = new Memory<byte>(newPayload);
            return true;
        }

        public void AppendMultiplePayloads(IEnumerable<byte[]> payloads)
        {
            int totalLength = _payload.Length + payloads.Sum(p => p.Length);
            var combined = new byte[totalLength];

            _payload.Span.CopyTo(combined);
            int offset = _payload.Length;

            foreach (var payload in payloads)
            {
                payload.CopyTo(combined.AsSpan(offset));
                offset += payload.Length;
            }

            _payload = new Memory<byte>(combined);
        }
    }
}