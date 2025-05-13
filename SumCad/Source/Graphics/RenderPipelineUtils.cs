using System.Runtime.InteropServices;
using Silk.NET.WebGPU;

namespace SumCad.Application;

public unsafe class RenderPipelineUtils
{
    public static RenderPipeline* Create(GraphicsInstance graphicsInstance, ShaderModule* shaderModule, PipelineLayout* pipelineLayout = null, string vertexFnName = "main_vs", string fragmentFnName = "main_fs")
    {
        VertexAttribute* vertexAttribute = stackalloc VertexAttribute[2];
        
        vertexAttribute[0].Format = VertexFormat.Float32x3;
        vertexAttribute[0].ShaderLocation = 0;
        vertexAttribute[0].Offset = 0;
        
        vertexAttribute[1].Format = VertexFormat.Float32x4;
        vertexAttribute[1].ShaderLocation = 1;
        vertexAttribute[1].Offset = sizeof(float) * 3;
        
        VertexBufferLayout layout = new VertexBufferLayout()
        {
            StepMode = VertexStepMode.Vertex,
            Attributes = vertexAttribute,
            AttributeCount = 2,
            ArrayStride = 7 * sizeof(float)
        };
        
        VertexState vertexState = new VertexState();
        vertexState.Module = shaderModule;
        vertexState.EntryPoint = (byte*)Marshal.StringToHGlobalAnsi(vertexFnName);
        vertexState.Buffers = &layout;
        vertexState.BufferCount = 1;

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
        fragmentState.EntryPoint = (byte*)Marshal.StringToHGlobalAnsi(fragmentFnName);
        fragmentState.Targets = colorTargetState;
        fragmentState.TargetCount = 1;

        RenderPipelineDescriptor descriptor = new RenderPipelineDescriptor();
        descriptor.Layout = pipelineLayout;
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
        
        Console.WriteLine("Created Render Pipeline (Unlit)");

        return graphicsInstance.WebGPU.DeviceCreateRenderPipeline(graphicsInstance.Device, descriptor);
    }
}