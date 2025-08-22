using Microsoft.Xna.Framework.Graphics;
using System;

namespace VelomMonoGame.Core.Sources;

internal class Object3D
{
    public Model Model { get; }

    public Object3D(Model model)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model), "Model cannot be null.");
    }
}
