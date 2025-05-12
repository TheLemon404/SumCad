using System.Runtime.InteropServices;
using Silk.NET.WebGPU;
using SumCad.Application.Buffers;

namespace SumCad.Application;

public unsafe class UnlitRenderPipeline : IDisposable
{
    private readonly GraphicsInstance graphicsInstance;
    private RenderPipeline* renderPipeline;

    public UnlitRenderPipeline(GraphicsInstance graphicsInstance)
    {
        this.graphicsInstance = graphicsInstance;
    }
    public void Initialize()
    {
        ShaderModule* shaderModule = ShaderModuleUtils.CreateShaderModule(graphicsInstance, "unlit.wgsl");

        renderPipeline = RenderPipelineUtils.Create(graphicsInstance, shaderModule);
        
        graphicsInstance.WebGPU.ShaderModuleRelease(shaderModule);
    }

    public void Render(VertexBuffer vertexBuffer)
    {
        graphicsInstance.WebGPU.RenderPassEncoderSetPipeline(graphicsInstance.CurrentRenderPassEncoder, renderPipeline);
        
        graphicsInstance.WebGPU.RenderPassEncoderSetVertexBuffer(graphicsInstance.CurrentRenderPassEncoder, 0, vertexBuffer.Buffer, 0, vertexBuffer.Size);
        
        graphicsInstance.WebGPU.RenderPassEncoderDraw(graphicsInstance.CurrentRenderPassEncoder, 3, 1, 0, 0);
    }

    public void Dispose()
    {
        graphicsInstance.WebGPU.RenderPipelineRelease(renderPipeline);
    }
}