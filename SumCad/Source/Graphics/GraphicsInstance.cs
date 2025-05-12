using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Extensions.WGPU;
using Silk.NET.Windowing;

namespace SumCad.Application;

public unsafe class GraphicsInstance
{
    private IWindow silkWindow;
    private Instance* instance;
    private Surface* surface;
    private Adapter* adapter;

    private CommandEncoder* currentCommandEncoder;
    private SurfaceTexture surfaceTexture;
    private TextureView* surfaceTextureView;

    public event Action OnInitialize;
    public event Action OnRender;
    public event Action OnDispose;

    public Queue* Queue { get; private set; }
    public Device* Device { get; private set; }
    public RenderPassEncoder* CurrentRenderPassEncoder { get; private set; }
    public WebGPU WebGPU { get; private set; }
    public TextureFormat PreferredTextureFormat => TextureFormat.Bgra8Unorm;
    
    private void CreateWebGPUAPI()
    {
        WebGPU = WebGPU.GetApi();
        Console.WriteLine("Created WebGPU API");
    }

    private void CreateInstance()
    {
        InstanceExtras extras = new InstanceExtras();
        extras.Backends = InstanceBackend.Vulkan;
        
        InstanceDescriptor descriptor = new InstanceDescriptor();
        descriptor.NextInChain = &extras.Chain;
        instance = WebGPU.CreateInstance(descriptor);
        Console.WriteLine("Created WebGPU Instance");
    }

    private void CreateSurface()
    {
        surface = silkWindow.CreateWebGPUSurface(WebGPU, instance);
        Console.WriteLine("Created WebGPU Surface");
    }

    private void CreateAdapter()
    {
        RequestAdapterOptions options = new RequestAdapterOptions
        {
            CompatibleSurface = surface,
            PowerPreference = PowerPreference.HighPerformance
        };

        PfnRequestAdapterCallback callback = PfnRequestAdapterCallback.From((status, wgpuAdapter, msgPtr, userDataPtr) =>
        {
            if (status == RequestAdapterStatus.Success)
            {
                this.adapter = wgpuAdapter;
                Console.WriteLine("Retrieved WebGPU Adapter");
            }
            else
            {
                string msg = Marshal.PtrToStringAnsi((IntPtr)msgPtr);
                Console.WriteLine($"Error while retrieving WebGPU adapter: {msg}");
            }
        });
        
        WebGPU.InstanceRequestAdapter(instance, options, callback, null);
    }

    private void CreateDevice()
    {
        PfnRequestDeviceCallback callback = PfnRequestDeviceCallback.From((status, device, msgPtr, userDataPtr) =>
        {
            if (status == RequestDeviceStatus.Success)
            {
                this.Device = device;
                Console.WriteLine("Retrieved WebGPU Device");
            }
            else
            {
                string msg = Marshal.PtrToStringAnsi((IntPtr)msgPtr);
                Console.WriteLine($"Error while retrieving WebGPU device: {msg}");
            }
        });
        
        DeviceDescriptor descriptor = new DeviceDescriptor();
        WebGPU.AdapterRequestDevice(adapter, descriptor, callback, null);
    }

    private void ConfigureSurface()
    {
        SurfaceConfiguration configuration = new SurfaceConfiguration();
        configuration.Device = Device;
        configuration.Width = (uint)silkWindow.Size.X;
        configuration.Height = (uint)silkWindow.Size.Y;
        configuration.Format = PreferredTextureFormat;
        configuration.PresentMode = PresentMode.Fifo;
        configuration.Usage = TextureUsage.RenderAttachment;
        
        WebGPU.SurfaceConfigure(surface, configuration);
    }

    private void ConfigureDebugFallback()
    {
        PfnErrorCallback callback = PfnErrorCallback.From((type, msgPtr, userDataPtr) =>
        {
            string msg = Marshal.PtrToStringAnsi((IntPtr)msgPtr);
            Console.WriteLine($"WGPU Unhandled error callback: {msg}");
        });
        
        WebGPU.DeviceSetUncapturedErrorCallback(Device, callback, null);
        Console.WriteLine("WebGPU Debug Callback Configured");
    }

    private void OnLoad()
    {
        
    }

    private void OnUpdate(double dt)
    {
        
    }

    private void Window_OnRender(double dt)
    {
        PreRender();

        OnRender.Invoke();
        
        PostRender();
    }

    private void Window_OnResize(Vector2D<int> size)
    {
        SurfaceConfiguration configuration = new SurfaceConfiguration();
        configuration.Device = Device;
        configuration.Width = (uint)size.X;
        configuration.Height = (uint)size.Y;
        configuration.Format = PreferredTextureFormat;
        configuration.PresentMode = PresentMode.Fifo;
        configuration.Usage = TextureUsage.RenderAttachment;
        
        WebGPU.SurfaceConfigure(surface, configuration);
    }

    private void PreRender()
    {
        // - QUEUE
        Queue = WebGPU.DeviceGetQueue(Device);

        // - COMMAND ENCODER
        currentCommandEncoder = WebGPU.DeviceCreateCommandEncoder(Device, null);

        // - SURFACE TEXTURE
        WebGPU.SurfaceGetCurrentTexture(surface, ref surfaceTexture);
        surfaceTextureView = WebGPU.TextureCreateView(surfaceTexture.Texture, null);

        // - RENDER PASS ENCODER
        RenderPassColorAttachment* colorAttachments = stackalloc RenderPassColorAttachment[1];
        colorAttachments[0].View = surfaceTextureView;
        colorAttachments[0].LoadOp = LoadOp.Clear;
        colorAttachments[0].ClearValue = new Color(0.1, 0.9, 0.9, 1.0);
        colorAttachments[0].StoreOp = StoreOp.Store;

        RenderPassDescriptor renderPassDescriptor = new RenderPassDescriptor();
        renderPassDescriptor.ColorAttachments = colorAttachments;
        renderPassDescriptor.ColorAttachmentCount = 1;

        CurrentRenderPassEncoder = WebGPU.CommandEncoderBeginRenderPass(currentCommandEncoder, renderPassDescriptor);
    }

    private void PostRender()
    {
        // - END RENDER PASS
        WebGPU.RenderPassEncoderEnd(CurrentRenderPassEncoder);

        // - FINISH WITH COMMAND ENCODER
        CommandBuffer* commandBuffer = WebGPU.CommandEncoderFinish(currentCommandEncoder, null);

        // - PUT ENCODED COMMAND TO QUEUE
        WebGPU.QueueSubmit(Queue, 1, &commandBuffer);

        // - PRESENT SURFACE
        WebGPU.SurfacePresent(surface);

        // DISPOSE OF RESOURCES
        WebGPU.TextureViewRelease(surfaceTextureView);
        WebGPU.TextureRelease(surfaceTexture.Texture);
        WebGPU.RenderPassEncoderRelease(CurrentRenderPassEncoder);
        WebGPU.CommandBufferRelease(commandBuffer);
        WebGPU.CommandEncoderRelease(currentCommandEncoder);
    }
    
    public void CreateWindow(int width, int height, string title)
    {
        WindowOptions windowOptions = WindowOptions.Default;
        windowOptions.Size = new Vector2D<int>(width, height);
        windowOptions.Title = title;
        
        silkWindow = Window.Create(windowOptions);
        silkWindow.Initialize();
        
        CreateWebGPUAPI();
        CreateInstance();
        CreateSurface();
        CreateAdapter();
        CreateDevice();
        ConfigureSurface();
        ConfigureDebugFallback();
        
        silkWindow.Load += OnLoad;
        silkWindow.Update += OnUpdate;
        silkWindow.Render += Window_OnRender;
        silkWindow.Resize += Window_OnResize;
        
        // - QUEUE
        Queue = WebGPU.DeviceGetQueue(Device);

        OnInitialize.Invoke();
        
        silkWindow.Run();
    }
    
    public void Dispose()
    {
        OnDispose.Invoke();
        
        WebGPU.DeviceDestroy(Device);
        Console.WriteLine("WebGPU Device Destroyed");
        WebGPU.SurfaceRelease(surface);
        Console.WriteLine("WebGPU Surface Released");
        WebGPU.AdapterRelease(adapter);
        Console.WriteLine("WebGPU Adapter Released");
        WebGPU.InstanceRelease(instance);
        Console.WriteLine("WebGPU Instance Released");
    }
}