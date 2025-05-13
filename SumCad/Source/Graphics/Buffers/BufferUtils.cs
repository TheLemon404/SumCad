using Silk.NET.WebGPU;

using WebGPUBuffer = Silk.NET.WebGPU.Buffer;

namespace SumCad.Application.Buffers;

public unsafe class BufferUtils
{
    public static WebGPUBuffer* CreateVertexBuffer(GraphicsInstance graphicsInstance, float[] data)
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

    public static WebGPUBuffer* CreateIndexBuffer(GraphicsInstance graphicsInstance, ushort[] data)
    {
        uint size = (uint)data.Length * sizeof(ushort);
        BufferDescriptor descriptor = new BufferDescriptor()
        {
            MappedAtCreation = false,
            Size = size,
            Usage = BufferUsage.Index | BufferUsage.CopyDst
        };

        WebGPUBuffer* buffer = graphicsInstance.WebGPU.DeviceCreateBuffer(graphicsInstance.Device, descriptor);

        fixed (ushort* dataPtr = data)
        {
            graphicsInstance.WebGPU.QueueWriteBuffer(graphicsInstance.Queue, buffer, 0, dataPtr, size);
        }
        
        return buffer;
    }
    
    public static WebGPUBuffer* CreateUniformBuffer<T>(GraphicsInstance graphicsInstance, T data) where T : unmanaged
    {
        uint size = (uint) sizeof(T);
        BufferDescriptor descriptor = new BufferDescriptor()
        {
            MappedAtCreation = false,
            Size = size,
            Usage = BufferUsage.Uniform | BufferUsage.CopyDst
        };

        WebGPUBuffer* buffer = graphicsInstance.WebGPU.DeviceCreateBuffer(graphicsInstance.Device, descriptor);
        graphicsInstance.WebGPU.QueueWriteBuffer(graphicsInstance.Queue, buffer, 0, data, size);
        
        return buffer;
    }

    public static void WriteToUniformBuffer<T>(GraphicsInstance graphicsInstance, WebGPUBuffer* buffer, T data)
        where T : unmanaged
    {
        graphicsInstance.WebGPU.QueueWriteBuffer(graphicsInstance.Queue, buffer, 0, data, (uint)sizeof(T));
    }
}