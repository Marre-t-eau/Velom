using System.Numerics;

namespace VelomGame;

public class Object3D
{
    public Vector3 Position { get; set; }
    public float Size { get; set; }
    public Quaternion Rotation { get; set; } = Quaternion.Identity;
    public Color Color { get; set; }
    public List<Vector3> Vertices { get; } = new List<Vector3>();
    public List<int[]> Faces { get; } = new List<int[]>();

    public Object3D(Vector3 position, float size, Color color)
    {
        Position = position;
        Size = size;
        Color = color;
    }
}
