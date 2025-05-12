using WebGPUBuffer = Silk.NET.WebGPU.Buffer;

namespace SumCad.Application.Buffers;

public unsafe abstract class BaseBuffer : IDisposable
{
    protected readonly GraphicsInstance graphicsInstance;

    public BaseBuffer(GraphicsInstance graphicsInstance)
    {
        this.graphicsInstance = graphicsInstance;
    }

    public WebGPUBuffer* Buffer { get; protected set; }
    
    public uint Size { get; protected set; }

    public void Dispose()
    {
        graphicsInstance.WebGPU.BufferDestroy(Buffer);
    }
}