using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace VelomMonoGame.Core.Sources.Tools;

internal static class FontBank
{
    static ContentManager Content { get; set; } = null!;

    internal static void Initialize(ContentManager content)
    {
        Content = content;
    }

    private static Dictionary<FontsType, SpriteFont> fonts = new Dictionary<FontsType, SpriteFont>();
    public static SpriteFont GetFont(FontsType font = FontsType.Default)
    {
        if (Content == null)
            throw new InvalidOperationException("Content is not initialized. Call Initialize() first.");

        if (!fonts.ContainsKey(font))
        {
            fonts.Add(font, Content.Load<SpriteFont>("Fonts/Hud"));
        }

        return fonts[font];
    }

    public static float GetFontHeight(FontsType font = FontsType.Default)
    {
        if (Content == null)
            throw new InvalidOperationException("Content is not initialized. Call Initialize() first.");

        return GetFont(font).MeasureString("|").Y;
    }
}
internal enum FontsType
{
    Default
}
