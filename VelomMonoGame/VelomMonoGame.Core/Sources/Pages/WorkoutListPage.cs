using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VelomMonoGame.Core.Sources.Bluetooth.Interfaces;
using VelomMonoGame.Core.Sources.InterfaceElements;
using VelomMonoGame.Core.Sources.Objects;
using VelomMonoGame.Core.Sources.Tools;

namespace VelomMonoGame.Core.Sources.Pages;

internal class WorkoutListPage : IPage
{
    public Vector2 Size { get; set; }
    public List<IElement> Elements { get; set; } = new();
    private VelomMonoGameGame Game { get; }

    public WorkoutListPage(VelomMonoGameGame game, Vector2 size, IBluetoothManager bluetoothManager)
    {
        Size = size;
        Game = game;

        // Add Return button at the top
        Button returnButton = Button.CreateButtonWithText("Return", Color.White, Color.Purple, () =>
        {
            // Return to main page
            IPage page = new MainPage(Game, bluetoothManager);
            page.Size = size;
            Game.Page = page;
        });
        returnButton.Position = new Vector2(20, 20); // Position in top-right corner
        Elements.Add(returnButton);

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
        float y = returnButton.Position.Y + returnButton.Size.Y + 50;
        foreach (Workout workout in workouts)
        {
            // Calcul de la durée totale de l'entraînement
            TimeSpan totalDuration = CalculateTotalDuration(workout);
            string formattedDuration = FormatDuration(totalDuration);

            // Création du bouton avec nom et durée
            string buttonText = $"{workout.Name} ({formattedDuration})";
            Button button = Button.CreateButtonWithText(buttonText, Color.White, Color.Purple, () => Game.Page = new WorkoutGamePage(game, size, bluetoothManager, workout));
            button.Position = new Vector2(50, y);
            Elements.Add(button);
            y += FontBank.GetFontHeight(FontsType.Default) + button.Size.Y;
        }
    }

    /// <summary>
    /// Calcule la durée totale d'un entraînement en sommant les durées de tous les blocs
    /// </summary>
    private TimeSpan CalculateTotalDuration(Workout workout)
    {
        uint totalSeconds = 0;
        foreach (var block in workout.Blocks)
        {
            totalSeconds += block.Duration;
        }
        return TimeSpan.FromSeconds(totalSeconds);
    }

    /// <summary>
    /// Formate une durée en format lisible (hh:mm:ss ou mm:ss selon la longueur)
    /// </summary>
    private string FormatDuration(TimeSpan duration)
    {
        if (duration.Hours > 0)
        {
            return $"{duration.Hours:D1}:{duration.Minutes:D2}:{duration.Seconds:D2}";
        }
        else
        {
            return $"{duration.Minutes:D2}:{duration.Seconds:D2}";
        }
    }

    public void Draw(GameTime gameTime)
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
        foreach (IElement element in Elements)
        {
            if (element is IUpdatableElement updatable)
                updatable.Update();
        }
    }
}
