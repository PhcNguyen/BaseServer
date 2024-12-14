namespace NPServer.Core.Interfaces.Memory;

public interface IMultiSizeBufferPool
{
    void AllocateBuffers();

    byte[] RentBuffer(int size = 256);

    void ReturnBuffer(byte[] buffer);

    double GetAllocationForSize(int size);
}