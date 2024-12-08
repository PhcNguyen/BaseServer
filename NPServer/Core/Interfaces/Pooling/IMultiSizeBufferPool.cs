namespace NPServer.Core.Interfaces.Pooling
{
    public interface IMultiSizeBufferPool
    {
        void AllocateBuffers();

        byte[] RentBuffer(int size = 256);

        void ReturnBuffer(byte[] buffer);

        double GetAllocationForSize(int size);
    }
}