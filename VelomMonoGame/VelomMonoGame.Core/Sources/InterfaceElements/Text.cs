using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VelomMonoGame.Core.Sources.Tools;

namespace VelomMonoGame.Core.Sources.InterfaceElements;

internal class Text : IDrawableElement
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public string TextContent { get; set; } = string.Empty;
    public Color Color { get; set; } = Color.White;
    public SpriteFont Font { get; set; } = FontBank.GetFont(FontsType.Default);
    public Vector2 Size
    {
        get
        {
            return Font.MeasureString(TextContent);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(Font, TextContent, Position, Color);
    }
}
