namespace SumCad.Application.Buffers;

public unsafe class IndexBuffer : BaseBuffer
{
    public uint IndicesCount;

    public IndexBuffer(GraphicsInstance graphicsInstance) : base(graphicsInstance)
    {
        
    }

    public void Initialize(ushort[] data)
    {
        Size = (uint)data.Length * sizeof(ushort);
        Buffer = BufferUtils.CreateIndexBuffer(graphicsInstance, data);
        IndicesCount = (uint)data.Length;
    }
}