using System.Numerics;
using VelomGame.Parser;

namespace VelomGame;

public class Scene
{
    public List<Object3D> _objects = new List<Object3D>();

    public Object3D Player { get; set; }

    public Map Map { get; set; } = new Map();

    public Scene()
    {
        Object3D? object3D = Object3DParser.Parse("bike_low_poly", size: .305f, color: Colors.Red);
        if (object3D != null)
        {
            Player = object3D;
            Player.Rotation = Quaternion.Identity;
            _objects.Add(Player);
        }
        Player.Position = Map.TrackPoints[0];
    }
}
