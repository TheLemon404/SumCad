using SumCad.Application.Buffers;

namespace SumCad.Application;

public class Application
{
    private GraphicsInstance graphicsInstance;
    private UnlitRenderPipeline unlitRenderPipeline;
    private VertexBuffer buffer;
    public Application()
    {
        graphicsInstance = new GraphicsInstance();
        unlitRenderPipeline = new UnlitRenderPipeline(graphicsInstance);
        
        buffer = new VertexBuffer(graphicsInstance);

        graphicsInstance.OnInitialize += () =>
        {
            unlitRenderPipeline.Initialize();
            buffer.Initialize(new float[]
            {
                -0.5f, -0.5f, 0f, 1, 0, 0, 1,
                0.5f, -0.5f, 0f, 0, 1, 0, 1,
                0f, 0.5f, 0f, 0, 0, 1, 1,
            });
        };

        graphicsInstance.OnRender += () =>
        {
            unlitRenderPipeline.Render(buffer);
        };
        
        graphicsInstance.OnDispose += () =>
        {
            unlitRenderPipeline.Dispose();
        };
    }

    public void Run()
    {
        graphicsInstance.CreateWindow(1000,700, "SumCad");
        
        graphicsInstance.Dispose();
        buffer.Dispose();
    }
}