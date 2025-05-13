namespace SumCad.Application.Buffers;

public unsafe class VertexBuffer : BaseBuffer
{
    public uint VertexCount;
    public VertexBuffer(GraphicsInstance graphicsInstance) : base(graphicsInstance)
    {
    }

    public void Initialize(float[] data, uint vertexCount)
    {
        VertexCount = vertexCount;
        Size = (uint)data.Length * sizeof(float);
        Buffer = BufferUtils.CreateVertexBuffer(graphicsInstance, data);
    }
}