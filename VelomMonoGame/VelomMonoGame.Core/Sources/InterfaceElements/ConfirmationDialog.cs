using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using VelomMonoGame.Core.Sources.Tools;

namespace VelomMonoGame.Core.Sources.InterfaceElements
{
    internal class ConfirmationDialog : IDrawableElement, IUpdatableElement
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; private set; }
        public bool Visible { get; set; } = true;
        public bool IsUpdatable { get; set; } = true;

        private Text MessageText { get; }
        private Button ConfirmButton { get; }
        private Button CancelButton { get; }
        private RectangleElement Background { get; }
        public List<IElement> Elements { get; private set; } = new();

        public ConfirmationDialog(string message, Vector2 center, Action onConfirm, Action onCancel = null, string cancelMessage = "Cancel", string confirmMessage = "Confirm")
        {
            float padding = 20f;

            // Texte du message
            MessageText = new Text
            {
                TextContent = message,
                Color = Color.Black
            };

            float stringHeight = FontBank.GetFontHeight();
            float minWidth = FontBank.GetFont().MeasureString(confirmMessage).X + FontBank.GetFont().MeasureString(cancelMessage).X + padding * 3;

            // Calculer la taille du dialogue en fonction du contenu
            float dialogWidth = Math.Max(MessageText.Size.X + padding * 2, minWidth);
            float dialogHeight = stringHeight * 3 + padding * 3;
            
            Size = new Vector2(dialogWidth, dialogHeight);

            // Fond du dialogue
            Background = new RectangleElement
            {
                Size = Size,
                Texture = TextureBank.GetTextureColor(Color.White)
            };
            Background.Position = new Vector2(center.X - Background.Size.X / 2, center.Y - Background.Size.Y / 2);
            Elements.Add(Background);

            // Positionnement du texte
            MessageText.Position = new Vector2(Background.Position.X + padding, Background.Position.Y + padding);
            Elements.Add(MessageText);

            // Bouton Confirmer
            ConfirmButton = Button.CreateButtonWithText(confirmMessage, Color.White, Color.Green, () =>
            {
                Visible = false;
                IsUpdatable = false;
                onConfirm?.Invoke();
            });
            ConfirmButton.Position = new Vector2(Background.Position.X + Background.Size.X - padding - ConfirmButton.Size.X, Background.Position.Y + Background.Size.Y - padding - ConfirmButton.Size.Y);
            Elements.Add(ConfirmButton);

            // Bouton Annuler
            CancelButton = Button.CreateButtonWithText(cancelMessage, Color.White, Color.Red, () =>
            {
                Visible = false;
                IsUpdatable = false;
                onCancel?.Invoke();
            });
            CancelButton.Position = new Vector2(Background.Position.X + padding, Background.Position.Y + Background.Size.Y - padding - CancelButton.Size.Y);
            Elements.Add(CancelButton);
        }

        public void Update()
        {
            if (!IsUpdatable) return;

            // Mettre à jour les éléments internes
            foreach (var element in Elements)
            {
                if (element is IUpdatableElement updatable)
                    updatable.Update();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible) return;

            // Dessiner un rectangle semi-transparent pour assombrir l'arrière-plan
            Texture2D overlay = TextureBank.GetTextureColor(new Color(0, 0, 0, 128));
            Rectangle screenRect = new Rectangle(0, 0, (int)spriteBatch.GraphicsDevice.Viewport.Width, 
                                                     (int)spriteBatch.GraphicsDevice.Viewport.Height);
            spriteBatch.Draw(overlay, screenRect, Color.White);

            // Dessiner un cadre autour du dialogue
            Texture2D border = TextureBank.GetTextureColor(Color.Black);
            Rectangle borderRect = new Rectangle((int)Position.X - 2, (int)Position.Y - 2, 
                                                (int)Size.X + 4, (int)Size.Y + 4);
            spriteBatch.Draw(border, borderRect, Color.White);

            // Dessiner les éléments du dialogue
            foreach (var element in Elements)
            {
                if (element is IDrawableElement drawable)
                    drawable.Draw(spriteBatch);
            }
        }
    }
}
