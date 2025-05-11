using System.Runtime.InteropServices;
using Silk.NET.WebGPU;

namespace SumCad.Application;

public unsafe class ShaderModuleUtils
{ 
    public static ShaderModule* CreateShaderModule(GraphicsInstance graphicsInstance, string shaderName)
    {
        string shaderCode = File.ReadAllText($"../../../Resources/Shaders/{shaderName}");
        
        ShaderModuleWGSLDescriptor wgslDescriptor = new ShaderModuleWGSLDescriptor();
        wgslDescriptor.Code = (byte*)Marshal.StringToHGlobalAnsi(shaderCode);
        wgslDescriptor.Chain.SType = SType.ShaderModuleWgslDescriptor;

        ShaderModuleDescriptor descriptor = new ShaderModuleDescriptor();
        descriptor.NextInChain = (ChainedStruct*)&wgslDescriptor;
        
        ShaderModule* shaderModule = graphicsInstance.WebGPU.DeviceCreateShaderModule(graphicsInstance.Device, descriptor);
        
        Console.WriteLine("Shader Module Created");

        return shaderModule;
    }
}