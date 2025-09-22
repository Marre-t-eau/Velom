using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using VelomMonoGame.Core.Sources.Tools;

namespace VelomMonoGame.Core.Sources.InterfaceElements;

internal class Checkbox : IDrawableElement, IUpdatableElement
{
    private Vector2 _position = Vector2.Zero;
    public Vector2 Position
    {
        get { return _position; }
        set
        {
            _position = value;
            CheckboxRectangle = new Rectangle((int)_position.X, (int)_position.Y, (int)Size.X, (int)Size.Y);
            if (Label != null)
            {
                Label.Position = new Vector2(_position.X + Size.X + 5, _position.Y + (Size.Y - Label.Size.Y) / 2);
            }
        }
    }

    private Vector2 _size = new Vector2(30, 30);
    public Vector2 Size 
    { 
        get { return _size; }
        set
        {
            _size = value;
            CheckboxRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)_size.X, (int)_size.Y);
        }
    }

    public bool Visible { get; set; } = true;
    public bool IsUpdatable { get; set; } = true;

    public bool IsChecked { get; set; }
    public Text Label { get; set; }
    public Action<bool> OnValueChanged { get; set; }
    
    private Texture2D UncheckedTexture { get; set; }
    private Texture2D CheckedTexture { get; set; }
    private Rectangle CheckboxRectangle { get; set; }
    
    private bool wasPressed = false;

    public Checkbox()
    {
        UncheckedTexture = TextureBank.GetTextureColor(Color.White);
        CheckedTexture = TextureBank.GetTextureColor(Color.Purple);
        CheckboxRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!Visible)
            return;

        // Draw checkbox border
        spriteBatch.Draw(UncheckedTexture, new Rectangle((int)Position.X - 1, (int)Position.Y - 1, (int)Size.X + 2, (int)Size.Y + 2), Color.Black);
        
        // Draw checkbox background
        spriteBatch.Draw(UncheckedTexture, CheckboxRectangle, Color.White);
        
        // If checked, draw the checkmark
        if (IsChecked)
        {
            Rectangle innerRect = new Rectangle(
                (int)Position.X + 3, 
                (int)Position.Y + 3, 
                (int)Size.X - 6, 
                (int)Size.Y - 6);
            spriteBatch.Draw(CheckedTexture, innerRect, Color.Purple);
        }

        // Draw label if exists
        Label?.Draw(spriteBatch);
    }

    public void Update()
    {
        if (!IsUpdatable)
            return;

        bool isPressed = false;
        
        // Desktop mouse input
        if (Button.IsDesktop)
        {
            var mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            if (CheckboxRectangle.Contains(mouseState.Position) && 
                mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                isPressed = true;
            }
        }

        // Mobile touch input
        if (Button.IsMobile)
        {
            var touchState = Microsoft.Xna.Framework.Input.Touch.TouchPanel.GetState();
            foreach (var touch in touchState)
            {
                if (touch.State == Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Pressed && 
                    CheckboxRectangle.Contains(touch.Position.ToPoint()))
                {
                    isPressed = true;
                    break;
                }
            }
        }

        // Toggle on release
        if (!isPressed && wasPressed)
        {
            IsChecked = !IsChecked;
            OnValueChanged?.Invoke(IsChecked);
        }
        
        wasPressed = isPressed;
    }

    public static Checkbox CreateCheckbox(string labelText, bool initialValue, Action<bool> onValueChanged)
    {
        var checkbox = new Checkbox
        {
            IsChecked = initialValue,
            OnValueChanged = onValueChanged,
            Label = new Text
            {
                TextContent = labelText,
                Color = Color.Black
            }
        };
        
        // Position will be set when the checkbox position is set
        
        return checkbox;
    }
}
