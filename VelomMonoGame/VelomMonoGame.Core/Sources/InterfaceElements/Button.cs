using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using VelomMonoGame.Core.Sources.Tools;

namespace VelomMonoGame.Core.Sources.InterfaceElements;

internal class Button : IDrawableElement, IUpdatableElement
{
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
            UpdateButtonRectangle();
            // Set the position of the elements in the middle of the button
            foreach (IElement element in Elements)
            {
                if (element is IDrawableElement drawableElement)
                {
                    drawableElement.Position = new Vector2(Position.X + Size.X / 2 - drawableElement.Size.X / 2, Position.Y + Size.Y / 2 - drawableElement.Size.Y / 2);
                }
            }
        }
    }
    public List<IElement> Elements { get; set; } = [];
    internal Texture2D Background { get; set; } = TextureBank.GetTextureColor(Color.White);
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
            UpdateButtonRectangle();
        }
    }
    internal Action OnClick { get; set; } = null;

    private Rectangle ButtonRectangle { get; set; } = new Rectangle(0, 0, 0, 0);

    public bool Visible { get; set; } = true;

    public bool IsUpdatable { get; set; } = true;

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!Visible)
            return;

        // Draw the background
        spriteBatch.Draw(Background, new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y), Color.White);

        // Draw the contents
        foreach (IElement element in Elements)
        {
            if (element is IDrawableElement drawableElement)
            {
                drawableElement.Draw(spriteBatch);
            }
        }
    }

    /// <summary>
    /// Indicates if the game is running on a mobile platform.
    /// </summary>
    public readonly static bool IsMobile = OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();

    /// <summary>
    /// Indicates if the game is running on a desktop platform.
    /// </summary>
    public readonly static bool IsDesktop = OperatingSystem.IsMacOS() || OperatingSystem.IsLinux() || OperatingSystem.IsWindows();

    private bool wasPressed = false;

    public void Update()
    {
        if (!IsUpdatable)
            return;

        bool isPressed = false;
        if (IsDesktop)
        {
            // Check for mouse click
            MouseState mouseState = Mouse.GetState();
            if (ButtonRectangle.Contains(mouseState.Position) && mouseState.LeftButton == ButtonState.Pressed)
            {
                isPressed = true;
            }
        }

        if (IsMobile)
        {
            // Check for touch input
            TouchCollection touchState = TouchPanel.GetState();
            foreach (TouchLocation touch in touchState)
            {
                if (IsPressed(touch) && ButtonRectangle.Contains(touch.Position.ToPoint()))
                {
                    isPressed = true;
                    break;
                }
            }
        }

        if (isPressed && !wasPressed)
        {
            OnClick?.Invoke();
        }
        wasPressed = isPressed;
    }

    private bool IsPressed(TouchLocation touch)
    {
        if (touch.State == TouchLocationState.Pressed)
            return true;
        if (touch.State == TouchLocationState.Moved)
        {
            if (touch.TryGetPreviousLocation(out TouchLocation previousTouch))
            {
                return touch.Position == previousTouch.Position;
            }
        }
        return false;
    }

    public static Button CreateButtonWithText(string text, Color textColor, Color backgroundColor, Action action)
    {
        float stringHeight = FontBank.GetFontHeight(FontsType.Default);
        Text textOfButton = new Text
        {
            TextContent = text,
            Color = textColor
        };
        Button button = new Button
        {
            Background = TextureBank.GetTextureColor(backgroundColor),
            OnClick = action,
            Size = new Vector2(textOfButton.Size.X + stringHeight * 2, textOfButton.Size.Y + stringHeight)
        };
        // Set the position of the text in the middle of the button
        textOfButton.Position = new Vector2(button.Size.X / 2 - textOfButton.Size.X / 2, button.Size.Y / 2 - textOfButton.Size.Y / 2);
        button.Elements.Add(textOfButton);
        return button;
    }

    private void UpdateButtonRectangle()
    {
        ButtonRectangle = new Rectangle((int)_position.X, (int)_position.Y, (int)_size.X, (int)_size.Y);
    }
}
