using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VelomMonoGame.Core.Sources.InterfaceElements;
using VelomMonoGame.Core.Sources.Objects;
using VelomMonoGame.Core.Sources.Tools;

namespace VelomMonoGame.Core.Sources.Pages;

internal class WorkoutListPage : IPage
{
    public Vector2 Size { get; set; }
    public List<IElement> Elements { get; set; } = new();
    private Game Game { get; }

    public WorkoutListPage(Game game, Vector2 size)
    {
        Size = size;
        Game = game;

        // Load the workouts
        string workoutsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "Workouts");
        var workoutFiles = Directory.GetFiles(workoutsDir, "*.json");
        var workouts = new List<Workout>();

        foreach (var file in workoutFiles)
        {
            string json = File.ReadAllText(file);
            var workout = JsonSerializer.Deserialize<Workout>(json);
            if (workout != null)
                workouts.Add(workout);
        }

        // Exemple de liste d'entraînements
        float y = FontBank.GetFontHeight(FontsType.Default);
        foreach (Workout workout in workouts)
        {
            Button button = Button.CreateButtonWithText(workout.Name, Color.White, Color.Purple, () =>
            {
                // Action lors du clic sur le bouton (par exemple, charger l'entraînement)
                Console.WriteLine($"Workout selected: {workout.Name}");
            });
            button.Position = new Vector2(50, y);
            Elements.Add(button);
            y += FontBank.GetFontHeight(FontsType.Default) + button.Size.Y;
        }
    }

    public void Draw()
    {
        SpriteBatch spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        spriteBatch.Begin();
        foreach (IElement element in Elements)
        {
            if (element is IDrawableElement drawable)
                drawable.Draw(spriteBatch);
        }
        spriteBatch.End();
    }

    public void Update(GameTime gameTime)
    {
        // Rien à mettre ici pour une simple liste
    }
}
