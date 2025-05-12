using Silk.NET.WebGPU;

using WebGPUBuffer = Silk.NET.WebGPU.Buffer;

namespace SumCad.Application.Buffers;

public unsafe class BufferUtils
{
    public static WebGPUBuffer* Create(GraphicsInstance graphicsInstance, float[] data)
    {
        uint size = (uint)data.Length * sizeof(float);
        BufferDescriptor descriptor = new BufferDescriptor()
        {
            MappedAtCreation = false,
            Size = size,
            Usage = BufferUsage.Vertex | BufferUsage.CopyDst
        };

        WebGPUBuffer* buffer = graphicsInstance.WebGPU.DeviceCreateBuffer(graphicsInstance.Device, descriptor);

        fixed (float* dataPtr = data)
        {
            graphicsInstance.WebGPU.QueueWriteBuffer(graphicsInstance.Queue, buffer, 0, dataPtr, size);
        }
        
        return buffer;
    }
}