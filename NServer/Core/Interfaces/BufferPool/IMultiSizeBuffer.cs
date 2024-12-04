namespace NServer.Core.Interfaces.BufferPool;

public interface IMultiSizeBuffer
{
    void AllocateBuffers();

    byte[] RentBuffer(int size = 256);

    void ReturnBuffer(byte[] buffer);

    double GetAllocationForSize(int size);
}
