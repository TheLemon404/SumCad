using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using SumCad.Application.Buffers;

namespace SumCad.Application;

public unsafe class UnlitRenderPipeline : IDisposable
{
    private readonly GraphicsInstance graphicsInstance;
    private RenderPipeline* renderPipeline;
    
    private Matrix4X4<float> transform = Matrix4X4<float>.Identity;
    private UniformBuffer<Matrix4X4<float>> transformBuffer;
    private BindGroupLayout* transformBindGroupLayout; // Describes the data
    private BindGroup* transformBindGroup; // The actual data
    
    public UnlitRenderPipeline(GraphicsInstance graphicsInstance)
    {
        this.graphicsInstance = graphicsInstance;
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
        BindGroupLayoutEntry* bindGroupLayoutEntries = stackalloc BindGroupLayoutEntry[1];
        bindGroupLayoutEntries[0] = new BindGroupLayoutEntry();
        bindGroupLayoutEntries[0].Binding = 0;
        bindGroupLayoutEntries[0].Visibility = ShaderStage.Vertex;
        bindGroupLayoutEntries[0].Buffer = new BufferBindingLayout()
        {
            Type = BufferBindingType.Uniform
        };
        
        BindGroupLayoutDescriptor descriptor = new BindGroupLayoutDescriptor()
        {
            Entries = bindGroupLayoutEntries,
            EntryCount = 1
        };

        transformBindGroupLayout =
            graphicsInstance.WebGPU.DeviceCreateBindGroupLayout(graphicsInstance.Device, descriptor);
    }

    private void CreateBindGroups()
    {
        BindGroupEntry* bindGroupEntries = stackalloc BindGroupEntry[1];

        bindGroupEntries[0] = new BindGroupEntry();
        bindGroupEntries[0].Binding = 0;
        bindGroupEntries[0].Buffer = transformBuffer.Buffer;
        bindGroupEntries[0].Size = transformBuffer.Size;
        
        BindGroupDescriptor descriptor = new BindGroupDescriptor()
        {
            Layout = transformBindGroupLayout,
            Entries = bindGroupEntries,
            EntryCount = 1
        };
        
        transformBindGroup = graphicsInstance.WebGPU.DeviceCreateBindGroup(graphicsInstance.Device, descriptor);
    }
    
    public void Initialize()
    {
        CreateBindGroupLayouts();
        
        ShaderModule* shaderModule = ShaderModuleUtils.CreateShaderModule(graphicsInstance, "unlit.wgsl", "Unlit Render Pipeline Shader");

        BindGroupLayout** bindGroupLayouts = stackalloc BindGroupLayout*[1];
        bindGroupLayouts[0] = transformBindGroupLayout;
        
        PipelineLayoutDescriptor pipelineLayoutDescriptor = new PipelineLayoutDescriptor()
        {
            BindGroupLayouts = bindGroupLayouts,
            BindGroupLayoutCount = 1,
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
        graphicsInstance.WebGPU.RenderPassEncoderSetPipeline(graphicsInstance.CurrentRenderPassEncoder, renderPipeline);
        
        graphicsInstance.WebGPU.RenderPassEncoderSetBindGroup(graphicsInstance.CurrentRenderPassEncoder, 0, transformBindGroup, 0, 0);
        
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