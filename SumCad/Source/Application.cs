namespace SumCad.Application;

public class Application
{
    private GraphicsInstance graphicsInstance;
    private UnlitRenderPipeline unlitRenderPipeline;
    public Application()
    {
        graphicsInstance = new GraphicsInstance();
        unlitRenderPipeline = new UnlitRenderPipeline(graphicsInstance);

        graphicsInstance.OnInitialize += () =>
        {
            unlitRenderPipeline.Initialize();
        };

        graphicsInstance.OnRender += () =>
        {
            unlitRenderPipeline.Render();
        };
    }

    public void Run()
    {
        graphicsInstance.CreateWindow(1000,700, "SumCad");
        
        graphicsInstance.Dispose();
    }
}