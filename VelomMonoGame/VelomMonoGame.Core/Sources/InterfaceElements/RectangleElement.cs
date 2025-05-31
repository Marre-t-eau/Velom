using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VelomMonoGame.Core.Sources.Tools;

namespace VelomMonoGame.Core.Sources.InterfaceElements;

internal class RectangleElement : IDrawableElement
{
    private Rectangle _rectangle = new Rectangle(0, 0, 0, 0);

    private Vector2 _size = Vector2.Zero;
    public Vector2 Size
    {
        get
        {
            return _size;
        }
        set
        {
            _size = value;
            // Update the rectangle size based on the new size
            _rectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)_size.X, (int)_size.Y);
        }
    }

    private Vector2 _position = Vector2.Zero;
    public Vector2 Position
    {
        get
        {
            return _position;
        }
        set
        {
            _position = value;
            // Update the rectangle position based on the new position
            _rectangle = new Rectangle((int)_position.X, (int)_position.Y, (int)Size.X, (int)Size.Y);
        }
    }

    public Texture2D Texture { get; set; } = TextureBank.GetTextureColor(Color.White);

    public void Draw(SpriteBatch spriteBatch)
    {
        // Draw the rectangle using the sprite batch
        spriteBatch.Draw(Texture, _rectangle, Color.White);
    }
}
