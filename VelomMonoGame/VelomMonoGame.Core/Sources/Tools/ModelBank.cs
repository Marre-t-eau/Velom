using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace VelomMonoGame.Core.Sources.Tools;

internal static class ModelBank
{
    static ContentManager Content { get; set; } = null!;

    internal static void Initialize(ContentManager content)
    {
        Content = content;
    }

    private static Dictionary<string, Model> models = new Dictionary<string, Model>();

    public static Model GetModel(string modelName)
    {
        if (Content == null)
            throw new InvalidOperationException("Content is not initialized. Call Initialize() first.");
        if (!models.ContainsKey(modelName))
        {
            models.Add(modelName, Content.Load<Model>($"Objects/{modelName}"));
        }
        return models[modelName];
    }
}
