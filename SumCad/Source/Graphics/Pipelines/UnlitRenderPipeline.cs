using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using SumCad.Application.Buffers;
using SumCad.Application.Scene;

namespace SumCad.Application;

public unsafe class UnlitRenderPipeline : IDisposable
{
    private readonly GraphicsInstance graphicsInstance;
    private RenderPipeline* renderPipeline;
    
    private Matrix4X4<float> transform = Matrix4X4<float>.Identity;
    private UniformBuffer<Matrix4X4<float>> transformBuffer;

    private Camera camera;
    
    private BindGroupLayout* transformBindGroupLayout; // Describes the data
    private BindGroupLayout* cameraBindGroupLayout; // Describes the data
    private BindGroup* transformBindGroup; // The actual data
    private BindGroup* cameraBindGroup; // The actual data
    
    public UnlitRenderPipeline(GraphicsInstance graphicsInstance, Camera camera)
    {
        this.graphicsInstance = graphicsInstance;
        this.camera = camera;
    }

    public Matrix4X4<float> Transform
    {
        get => transform;
        set
        {
            transform = value;
            transformBuffer.Update(transform);
        }
    }

    private void CreateResources(GraphicsInstance graphicsInstance)
    {
        transformBuffer = new UniformBuffer<Matrix4X4<float>>(graphicsInstance);
        transformBuffer.Initialize(transform);
    }

    private void CreateBindGroupLayouts()
    {
        BindGroupLayoutEntry* transformBindGroupLayoutEntries = stackalloc BindGroupLayoutEntry[1];
        
        transformBindGroupLayoutEntries[0] = new BindGroupLayoutEntry();
        transformBindGroupLayoutEntries[0].Binding = 0;
        transformBindGroupLayoutEntries[0].Visibility = ShaderStage.Vertex;
        transformBindGroupLayoutEntries[0].Buffer = new BufferBindingLayout()
        {
            Type = BufferBindingType.Uniform
        };
        
        BindGroupLayoutDescriptor transformBindGroupLayoutDescriptor = new BindGroupLayoutDescriptor()
        {
            Entries = transformBindGroupLayoutEntries,
            EntryCount = 1
        };

        transformBindGroupLayout =
            graphicsInstance.WebGPU.DeviceCreateBindGroupLayout(graphicsInstance.Device, transformBindGroupLayoutDescriptor);
        
        BindGroupLayoutEntry* cameraBindGroupLayoutEntries = stackalloc BindGroupLayoutEntry[1];
        
        cameraBindGroupLayoutEntries[0] = new BindGroupLayoutEntry();
        cameraBindGroupLayoutEntries[0].Binding = 0;
        cameraBindGroupLayoutEntries[0].Visibility = ShaderStage.Vertex;
        cameraBindGroupLayoutEntries[0].Buffer = new BufferBindingLayout()
        {
            Type = BufferBindingType.Uniform
        };
        
        BindGroupLayoutDescriptor cameraBindGroupLayoutDescriptor = new BindGroupLayoutDescriptor()
        {
            Entries = cameraBindGroupLayoutEntries,
            EntryCount = 1
        };

        cameraBindGroupLayout =
            graphicsInstance.WebGPU.DeviceCreateBindGroupLayout(graphicsInstance.Device, cameraBindGroupLayoutDescriptor);
    }

    private void CreateBindGroups()
    {
        BindGroupEntry* transformBindGroupEntries = stackalloc BindGroupEntry[1];

        transformBindGroupEntries[0] = new BindGroupEntry();
        transformBindGroupEntries[0].Binding = 0;
        transformBindGroupEntries[0].Buffer = transformBuffer.Buffer;
        transformBindGroupEntries[0].Size = transformBuffer.Size;
        
        BindGroupDescriptor transformBindGroupDescriptor = new BindGroupDescriptor()
        {
            Layout = transformBindGroupLayout,
            Entries = transformBindGroupEntries,
            EntryCount = 1
        };
        
        transformBindGroup = graphicsInstance.WebGPU.DeviceCreateBindGroup(graphicsInstance.Device, transformBindGroupDescriptor);
        
        BindGroupEntry* cameraBindGroupEntries = stackalloc BindGroupEntry[1];

        cameraBindGroupEntries[0] = new BindGroupEntry();
        cameraBindGroupEntries[0].Binding = 0;
        cameraBindGroupEntries[0].Buffer = camera.Buffer.Buffer;
        cameraBindGroupEntries[0].Size = camera.Buffer.Size;
        
        BindGroupDescriptor cameraBindGroupDescriptor = new BindGroupDescriptor()
        {
            Layout = cameraBindGroupLayout,
            Entries = cameraBindGroupEntries,
            EntryCount = 1
        };
        
        cameraBindGroup = graphicsInstance.WebGPU.DeviceCreateBindGroup(graphicsInstance.Device, cameraBindGroupDescriptor);
    }
    
    public void Initialize()
    {
        CreateBindGroupLayouts();
        
        ShaderModule* shaderModule = ShaderModuleUtils.CreateShaderModule(graphicsInstance, "unlit.wgsl", "Unlit Render Pipeline Shader");

        BindGroupLayout** bindGroupLayouts = stackalloc BindGroupLayout*[2];
        bindGroupLayouts[0] = transformBindGroupLayout;
        bindGroupLayouts[1] = cameraBindGroupLayout;
        
        PipelineLayoutDescriptor pipelineLayoutDescriptor = new PipelineLayoutDescriptor()
        {
            BindGroupLayouts = bindGroupLayouts,
            BindGroupLayoutCount = 2,
        };

        PipelineLayout* pipelineLayout =
            graphicsInstance.WebGPU.DeviceCreatePipelineLayout(graphicsInstance.Device, pipelineLayoutDescriptor);

        renderPipeline = RenderPipelineUtils.Create(graphicsInstance, shaderModule, pipelineLayout);
        
        CreateResources(graphicsInstance);
        CreateBindGroups();
        
        graphicsInstance.WebGPU.ShaderModuleRelease(shaderModule);
    }

    public void Render(VertexBuffer vertexBuffer, IndexBuffer? indexBuffer = null)
    {
        camera.Update();
        
        graphicsInstance.WebGPU.RenderPassEncoderSetPipeline(graphicsInstance.CurrentRenderPassEncoder, renderPipeline);
        
        graphicsInstance.WebGPU.RenderPassEncoderSetBindGroup(graphicsInstance.CurrentRenderPassEncoder, 0, transformBindGroup, 0, 0);
        graphicsInstance.WebGPU.RenderPassEncoderSetBindGroup(graphicsInstance.CurrentRenderPassEncoder, 1, cameraBindGroup, 0, 0);
        
        graphicsInstance.WebGPU.RenderPassEncoderSetVertexBuffer(graphicsInstance.CurrentRenderPassEncoder, 0, vertexBuffer.Buffer, 0, vertexBuffer.Size);

        if (indexBuffer != null)
        {
            graphicsInstance.WebGPU.RenderPassEncoderSetIndexBuffer(graphicsInstance.CurrentRenderPassEncoder, indexBuffer.Buffer, IndexFormat.Uint16, 0, indexBuffer.Size);
            graphicsInstance.WebGPU.RenderPassEncoderDrawIndexed(graphicsInstance.CurrentRenderPassEncoder, indexBuffer.IndicesCount, 1, 0, 0, 0);
        }
        else
        {
            graphicsInstance.WebGPU.RenderPassEncoderDraw(graphicsInstance.CurrentRenderPassEncoder, vertexBuffer.VertexCount, 1, 0, 0);
        }
    }

    public void Dispose()
    {
        transformBuffer.Dispose();
        graphicsInstance.WebGPU.RenderPipelineRelease(renderPipeline);
    }
}