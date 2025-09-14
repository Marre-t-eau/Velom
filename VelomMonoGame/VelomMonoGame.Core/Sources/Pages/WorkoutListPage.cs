using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
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

    // Liste complète des entraînements
    private List<Workout> Workouts { get; set; } = new();

    // Boutons de défilement
    private Button UpButton { get; set; }
    private Button DownButton { get; set; }

    // Pour gérer le défilement
    private int CurrentScrollIndex { get; set; } = 0;
    private List<Button> CurrentWorkoutButtons { get; set; } = new();

    private float startWorkoutListY;

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

        startWorkoutListY = returnButton.Position.Y + returnButton.Size.Y + 50;

        // Load the workouts
        LoadWorkouts();

        // Créer les boutons de défilement
        CreateScrollButtons(bluetoothManager, size);

        // Afficher les premiers entraînements
        RefreshWorkoutList(bluetoothManager, size);
    }

    private void LoadWorkouts()
    {
        IFileProvider fileProvider = Game.Services.GetService(typeof(IFileProvider)) as IFileProvider;
        Workouts = new List<Workout>();

        // Liste tous les fichiers JSON dans le dossier Workouts
        var files = fileProvider.ListFiles("Workouts", "*.json");
        foreach (var file in files)
        {
            // Récupère le contenu du fichier
            string json = fileProvider.GetFileContent($"Workouts/{file}");
            var workout = JsonSerializer.Deserialize<Workout>(json);
            if (workout != null)
                Workouts.Add(workout);
        }
    }

    private void CreateScrollButtons(IBluetoothManager bluetoothManager, Vector2 size)
    {
        if (!Workouts.Any())
            return;

        // Bouton pour défiler vers le haut
        UpButton = Button.CreateButtonWithText("^", Color.White, Color.Purple, () =>
        {
            if (CurrentScrollIndex > 0)
            {
                CurrentScrollIndex--;
                RefreshWorkoutList(bluetoothManager, size);
            }
        });

        // Bouton pour défiler vers le bas
        DownButton = Button.CreateButtonWithText("v", Color.White, Color.Purple, () =>
        {
            if (CurrentScrollIndex < Workouts.Count - 1) // To display at least 1
            {
                CurrentScrollIndex++;
                RefreshWorkoutList(bluetoothManager, size);
            }
        });

        // Positionnement des boutons de défilement
        float buttonWidth = 50;
        float buttonHeight = 50;
        // Get the workout with the biggest name
        float workoutMaxTextSize = FontBank.GetFont(FontsType.Default).MeasureString($"{Workouts[0].Name} ({FormatDuration(TimeSpan.Zero)})").X;
        for (int i = 1; i < Workouts.Count; i++)
        {
            float textSize = FontBank.GetFont(FontsType.Default).MeasureString($"{Workouts[i].Name} ({FormatDuration(TimeSpan.Zero)})").X;
            if (textSize > workoutMaxTextSize)
                workoutMaxTextSize = textSize;
        }
        // Calculate the width of the button based on the biggest name
        float stringHeight = FontBank.GetFontHeight(FontsType.Default);
        float leftMargin = workoutMaxTextSize + stringHeight * 2 + 250; // Max workout button size width + left marge + marge

        UpButton.Size = new Vector2(buttonWidth, buttonHeight);
        DownButton.Size = new Vector2(buttonWidth, buttonHeight);

        UpButton.Position = new Vector2(leftMargin, startWorkoutListY);
        DownButton.Position = new Vector2(leftMargin, Size.Y - startWorkoutListY);

        Elements.Add(UpButton);
        Elements.Add(DownButton);
    }

    private void RefreshWorkoutList(IBluetoothManager bluetoothManager, Vector2 size)
    {
        if (!Workouts.Any())
            return;

        // Supprimer les boutons d'entraînement existants
        foreach (var button in CurrentWorkoutButtons)
        {
            Elements.Remove(button);
        }
        CurrentWorkoutButtons.Clear();

        // Calculer les limites de l'affichage actuel
        int startIdx = CurrentScrollIndex;

        // Ajouter les nouveaux boutons d'entraînement
        float y = startWorkoutListY; // Position de départ après le bouton de retour
        for (int i = startIdx; i < Workouts.Count; i++)
        {
            Workout workout = Workouts[i];

            // Calcul de la durée totale
            TimeSpan totalDuration = CalculateTotalDuration(workout);
            string formattedDuration = FormatDuration(totalDuration);

            // Création du bouton
            string buttonText = $"{workout.Name} ({formattedDuration})";
            Button button = Button.CreateButtonWithText(buttonText, Color.White, Color.Purple,
                () => Game.Page = new WorkoutGamePage(Game, size, bluetoothManager, workout));

            button.Position = new Vector2(150, y);
            Elements.Add(button);
            CurrentWorkoutButtons.Add(button);
            _ = button.Size.X;

            y += FontBank.GetFontHeight(FontsType.Default) + button.Size.Y + 10;
        }

        // Mettre à jour l'état des boutons de défilement
        UpButton.Background = CurrentScrollIndex > 0 ?
            TextureBank.GetTextureColor(Color.Purple) : TextureBank.GetTextureColor(Color.Gray);

        DownButton.Background = CurrentScrollIndex < Workouts.Count - 1 ?
            TextureBank.GetTextureColor(Color.Purple) : TextureBank.GetTextureColor(Color.Gray);
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
        List<IElement> elementsCopy;
        elementsCopy = new List<IElement>(Elements);
        foreach (IElement element in elementsCopy)
        {
            if (element is IDrawableElement drawable)
                drawable.Draw(spriteBatch);
        }
        spriteBatch.End();
    }

    public void Update(GameTime gameTime)
    {
        List<IElement> elementsCopy;
        elementsCopy = new List<IElement>(Elements);
        foreach (IElement element in elementsCopy)
        {
            if (element is IUpdatableElement updatable)
                updatable.Update();
        }
    }
}
