using WebGPUBuffer = Silk.NET.WebGPU.Buffer;

namespace SumCad.Application.Buffers;

public unsafe class UniformBuffer<T> : IDisposable where T : unmanaged
{    
    private readonly GraphicsInstance graphicsInstance;
    private readonly string label;

    public UniformBuffer(GraphicsInstance graphicsInstance, string label = "")
    {
        this.graphicsInstance = graphicsInstance;
        this.label = label;
    }

    public WebGPUBuffer* Buffer { get; private set; }
    public uint Size { get; private set; }

    public void Initialize(T data)
    {
        Size = (uint) sizeof(T);
        Buffer = BufferUtils.CreateUniformBuffer(graphicsInstance, data);
    }

    public void Update(T data)
    {
        BufferUtils.WriteToUniformBuffer(graphicsInstance, Buffer, data);
    }

    public void Dispose()
    {
        graphicsInstance.WebGPU.BufferDestroy(Buffer);
    }
}