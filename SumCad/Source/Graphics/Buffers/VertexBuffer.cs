namespace SumCad.Application.Buffers;

public unsafe class VertexBuffer : BaseBuffer
{
    public VertexBuffer(GraphicsInstance graphicsInstance) : base(graphicsInstance)
    {
    }

    public void Initialize(float[] data)
    {
        Size = (uint)data.Length * sizeof(float);
        Buffer = BufferUtils.Create(graphicsInstance, data);
    }
}