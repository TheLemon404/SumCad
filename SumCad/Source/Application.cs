using Silk.NET.Maths;
using SumCad.Application.Buffers;
using SumCad.Application.Scene;

namespace SumCad.Application;

public class Application
{
    private GraphicsInstance graphicsInstance;
    private UnlitRenderPipeline unlitRenderPipeline;
    private VertexBuffer buffer;
    private IndexBuffer indexBuffer;
    public Application()
    {
        graphicsInstance = new GraphicsInstance();
        
        buffer = new VertexBuffer(graphicsInstance);
        indexBuffer = new IndexBuffer(graphicsInstance);

        float scl = 1;
        int sclDir = 1;

        graphicsInstance.OnInitialize += () =>
        {
            unlitRenderPipeline = new UnlitRenderPipeline(graphicsInstance, new Camera(graphicsInstance));

            unlitRenderPipeline.Initialize();
            buffer.Initialize(new float[]
            {
                -0.5f, -0.5f, 0f, 1, 0, 0, 1,
                0.5f, -0.5f, 0f, 0, 1, 0, 1,
                -0.5f, 0.5f, 0f, 0, 0, 1, 1,
                0.5f, 0.5f, 0f, 0, 1, 0, 1
            }, 4);
            
            indexBuffer.Initialize(new ushort[]
            {
                0, 1, 2,
                1, 3, 2
            });
        };

        graphicsInstance.OnRender += () =>
        {
            if (scl > 2)
            {
                sclDir = -1;
            }
            else if (scl < 0.5)
            {
                sclDir = 1;
            }

            scl += 0.001f * sclDir;

            unlitRenderPipeline.Transform = Matrix4X4.CreateScale<float>(scl, scl, 1.0f);
            unlitRenderPipeline.Render(buffer, indexBuffer);
        };
        
        graphicsInstance.OnDispose += () =>
        {
            unlitRenderPipeline.Dispose();
            buffer.Dispose();
            indexBuffer.Dispose();
        };
    }

    public void Run()
    {
        graphicsInstance.CreateWindow(1000,700, "SumCad");
        
        graphicsInstance.Dispose();
        buffer.Dispose();
    }
}