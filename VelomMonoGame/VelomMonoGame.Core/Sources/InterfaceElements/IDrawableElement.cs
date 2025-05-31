using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VelomMonoGame.Core.Sources.InterfaceElements;

internal interface IDrawableElement : IElement
{
    Vector2 Size { get; }
    Vector2 Position { get; set; }
    void Draw(SpriteBatch spriteBatch);
}
