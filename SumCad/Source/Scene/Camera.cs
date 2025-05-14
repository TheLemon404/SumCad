using Silk.NET.Maths;
using SumCad.Application.Buffers;

namespace SumCad.Application.Scene;

public class Camera
{
    public Vector3D<float> Position { get; set; } = new  Vector3D<float>(0, 0, -3);
    public Vector3D<float> Target { get; set; } = new  Vector3D<float>(0, 0, 0);
    public Vector3D<float> Up { get; set; } = new  Vector3D<float>(0, 1, 0);

    public float AspectRatio { get; set; } = 1.0f;
    public float FieldOfView { get; set; } = 60.0f;
    public float Near { get; set; } = 0.1f;
    public float Far { get; set; } = 1000.0f;

    public UniformBuffer<Matrix4X4<float>> Buffer { get; set; } = null;

    public Camera(GraphicsInstance graphicsInstance)
    {
        Buffer = new UniformBuffer<Matrix4X4<float>>(graphicsInstance, "PerspectiveCameraBuffer");
        Buffer.Initialize(Matrix4X4<float>.Identity);
    }

    public void Update()
    {
        Matrix4X4<float> perspective = Matrix4X4.CreatePerspectiveFieldOfView(float.DegreesToRadians(FieldOfView), AspectRatio, Near, Far);
        
        Matrix4X4<float> view = Matrix4X4.CreateLookAt(-Position, Target, Up);
        
        Buffer.Update(view * perspective);
    }
}