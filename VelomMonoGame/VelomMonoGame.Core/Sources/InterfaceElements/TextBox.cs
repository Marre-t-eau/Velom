using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using VelomMonoGame.Core.Sources.Tools;

namespace VelomMonoGame.Core.Sources.InterfaceElements;

public class  TextBox : IDrawableElement, IUpdatableElement
{
    public Vector2 Position { get; set; }
    public Vector2 Size
    {
        get
        {
            float stringHeight = FontBank.GetFontHeight();
            return new Vector2(stringHeight + FontBank.GetFont().MeasureString(new string('W', MaxLength)).X, stringHeight);
        }
    }
    public bool Visible { get; set; } = true;
    public bool IsUpdatable { get; set; } = true;

    public string Text { get; set; } = "";
    public Color TextColor { get; set; } = Color.Black;
    public Color BackgroundColor { get; set; } = Color.White;
    public Color BorderColor { get; set; } = Color.Black;

    public bool IsFocused { get; set; } = false;
    public int MaxLength { get; set; } = 8;

    public Action<string> OnTextChanged { get; set; }

    private Texture2D _backgroundTexture;
    private Texture2D _borderTexture;

    private KeyboardState _previousKeyboardState = Keyboard.GetState();

    public bool IsDigitOnly { get; set; } = false;

    // Ajoutez ce délégué pour demander l'ouverture du clavier virtuel
    public static Action<TextBox> OnRequestVirtualKeyboard { get; set; }

    public TextBox()
    {
        _backgroundTexture = TextureBank.GetTextureColor(BackgroundColor);
        _borderTexture = TextureBank.GetTextureColor(BorderColor);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!Visible) return;

        // Draw border
        spriteBatch.Draw(_borderTexture, new Rectangle((int)Position.X - 2, (int)Position.Y - 2, (int)Size.X + 4, (int)Size.Y + 4), BorderColor);

        // Draw background
        spriteBatch.Draw(_backgroundTexture, new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y), BackgroundColor);

        // Draw text
        var font = FontBank.GetFont(FontsType.Default);
        string displayText = Text + (IsFocused && (DateTime.Now.Millisecond % 1000 < 500) ? "|" : "");
        spriteBatch.DrawString(font, displayText, new Vector2(Position.X + 5, Position.Y + (Size.Y - font.MeasureString(displayText).Y) / 2), TextColor);
    }

    public void Update()
    {
        if (!IsUpdatable || !Visible) return;


        var rect = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        // Gestion du focus (clic souris)
        if (Button.IsDesktop)
        {
            var mouse = Mouse.GetState();
            if (mouse.LeftButton == ButtonState.Pressed && rect.Contains(mouse.Position))
            {
                IsFocused = true;
            }
            else if (mouse.LeftButton == ButtonState.Pressed && !rect.Contains(mouse.Position))
            {
                IsFocused = false;
            }
        }
        else // Mobile
        {
            var touchCollection = TouchPanel.GetState();
            foreach (var touch in touchCollection)
            {
                if (IsPressed(touch) && rect.Contains(touch.Position))
                {
                    if (!IsFocused)
                    {
                        IsFocused = true;
                        OnRequestVirtualKeyboard?.Invoke(this); // Demande d'ouverture du clavier virtuel
                    }
                }
            }
        }

        // Saisie clavier si focus
        if (IsFocused && Button.IsDesktop)
        {
            KeyboardState currentState = Keyboard.GetState();
            bool shift = currentState.IsKeyDown(Keys.LeftShift) || currentState.IsKeyDown(Keys.RightShift);

            foreach (Keys key in currentState.GetPressedKeys())
            {
                // Si la touche vient d'être pressée (n'était pas pressée à la frame précédente)
                if (!_previousKeyboardState.IsKeyDown(key))
                {
                    if (key == Keys.Back && Text.Length > 0)
                    {
                        Text = Text.Substring(0, Text.Length - 1);
                        OnTextChanged?.Invoke(Text);
                    }
                    else
                    {
                        char? c = KeyToChar(key, shift);
                        if (IsDigitOnly && c.HasValue && !char.IsDigit(c.Value))
                        {
                            c = null; // Ignore non-digit characters
                        }
                        if (c.HasValue && Text.Length < MaxLength)
                        {
                            Text += c.Value;
                            OnTextChanged?.Invoke(Text);
                        }
                    }
                }
            }

            _previousKeyboardState = currentState;
        }
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

    // Méthode utilitaire pour convertir Keys en char (simplifiée)
    private char? KeyToChar(Keys key, bool shift)
    {
        if (key >= Keys.A && key <= Keys.Z)
            return (char)((shift ? 'A' : 'a') + (key - Keys.A));
        if (key >= Keys.D0 && key <= Keys.D9)
            return (char)('0' + (key - Keys.D0));
        if (key == Keys.Space)
            return ' ';
        return null;
    }

    public void SetTextFromMobile(string newText)
    {
        if (IsDigitOnly && !int.TryParse(newText, out _))
            return;
        if (newText.Length > MaxLength)
            newText = newText.Substring(0, MaxLength);
        Text = newText;
        OnTextChanged?.Invoke(Text);
    }
}
