using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using VelomMonoGame.Core.Sources.InterfaceElements;

namespace VelomMonoGame.Core.Sources.Pages;

internal interface IPage
{
    // Properties
    Vector2 Size { get; set; }
    List<IElement> Elements { get; set; }

    // Methods
    void Update(GameTime gameTime);
    void Draw();
}
