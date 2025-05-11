using System.Runtime.InteropServices;
using Silk.NET.WebGPU;

namespace SumCad.Application;

public unsafe class UnlitRenderPipeline
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

        VertexState vertexState = new VertexState();
        vertexState.Module = shaderModule;
        vertexState.EntryPoint = (byte*)Marshal.StringToHGlobalAnsi("main_vs");

        BlendState* blendState = stackalloc BlendState[1];
        blendState[0].Color = new BlendComponent()
        {
            SrcFactor = BlendFactor.One,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
            Operation = BlendOperation.Add
        };
        blendState[0].Alpha = new BlendComponent()
        {
            SrcFactor = BlendFactor.One,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
            Operation = BlendOperation.Add
        };
        
        ColorTargetState* colorTargetState = stackalloc ColorTargetState[1];
        colorTargetState[0].WriteMask = ColorWriteMask.All;
        colorTargetState[0].Format = graphicsInstance.PreferredTextureFormat;
        colorTargetState[0].Blend = blendState;
        
        FragmentState fragmentState = new FragmentState();
        fragmentState.Module = shaderModule;
        fragmentState.EntryPoint = (byte*)Marshal.StringToHGlobalAnsi("main_fs");
        fragmentState.Targets = colorTargetState;
        fragmentState.TargetCount = 1;

        RenderPipelineDescriptor descriptor = new RenderPipelineDescriptor();
        descriptor.Vertex = vertexState;
        descriptor.Fragment = &fragmentState;
        descriptor.Multisample = new MultisampleState()
        {
            Mask = 0xFFFFFFF,
            Count = 1,
            AlphaToCoverageEnabled = false
        };
        descriptor.Primitive = new PrimitiveState()
        {
            CullMode = CullMode.Back,
            FrontFace = FrontFace.Ccw,
            Topology = PrimitiveTopology.TriangleList
        };

        renderPipeline = graphicsInstance.WebGPU.DeviceCreateRenderPipeline(graphicsInstance.Device, descriptor);
        Console.WriteLine("Created Render Pipeline (Unlit)");
    }

    public void Render()
    {
        graphicsInstance.WebGPU.RenderPassEncoderSetPipeline(graphicsInstance.CurrentRenderPassEncoder, renderPipeline);
        graphicsInstance.WebGPU.RenderPassEncoderDraw(graphicsInstance.CurrentRenderPassEncoder, 3, 1, 0, 0);
    }
}