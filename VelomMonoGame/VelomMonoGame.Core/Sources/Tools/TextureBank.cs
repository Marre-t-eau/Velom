using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace VelomMonoGame.Core.Sources.Tools;

internal static class TextureBank
{
    private static GraphicsDevice graphicsDevice;
    internal static void Initialize(GraphicsDevice device)
    {
        graphicsDevice = device;
    }

    private static Dictionary<Color, Texture2D> colorTexture = new Dictionary<Color, Texture2D>();
    public static Texture2D GetTextureColor(Color color)
    {
        if (graphicsDevice == null)
            throw new InvalidOperationException("GraphicsDevice is not initialized. Call Initialize() first.");

        if (!colorTexture.ContainsKey(color))
        {
            Texture2D texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new[] { color });
            colorTexture.Add(color, texture);
        }
        return colorTexture[color];
    }
}
