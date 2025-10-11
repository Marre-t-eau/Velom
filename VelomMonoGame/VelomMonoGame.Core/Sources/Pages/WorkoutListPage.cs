using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
    private List<(string fileName, Workout workout)> Workouts { get; set; } = new();

    // Boutons de défilement
    private Button UpButton { get; set; }
    private Button DownButton { get; set; }

    // Pour gérer le défilement
    private int CurrentScrollIndex { get; set; } = 0;
    private List<Button> CurrentWorkoutButtons { get; set; } = new();

    private float startWorkoutListY;

    public WorkoutListPage(VelomMonoGameGame game, Vector2 size)
    {
        Size = size;
        Game = game;

        // Add Return button at the top
        Button returnButton = Button.CreateButtonWithText("Return", Color.White, Color.Purple, () =>
        {
            // Return to main page
            Game.Page = new MainPage(Game);
        });
        returnButton.Position = new Vector2(20, 20); // Position in top-left corner
        Elements.Add(returnButton);

        startWorkoutListY = returnButton.Position.Y + returnButton.Size.Y + 50;

        // Button to create a new workout
        Button createButton = Button.CreateButtonWithText("Create", Color.White, Color.Purple, () =>
        {
            // Ouvre la page d'édition sans entraînement (création)
            Game.Page = new WorkoutEditPage(Game, new Workout(), null);
        });
        createButton.Position = new Vector2(Size.X - 20 - createButton.Size.X, 20);
        Elements.Add(createButton);

        // Load the workouts
        LoadWorkouts();

        // Créer les boutons de défilement
        CreateScrollButtons(size);

        // Afficher les premiers entraînements
        RefreshWorkoutList(size);
    }

    private void LoadWorkouts()
    {
        foreach (string fileName in SaveManager.GetAllWorkoutFiles())
        {
            var workout = SaveManager.GetWorkout(fileName);
            if (workout != null)
                Workouts.Add((fileName, workout));
        }
    }

    private void CreateScrollButtons(Vector2 size)
    {
        if (!Workouts.Any())
            return;

        // Bouton pour défiler vers le haut
        UpButton = Button.CreateButtonWithText("^", Color.White, Color.Purple, () =>
        {
            if (CurrentScrollIndex > 0)
            {
                CurrentScrollIndex--;
                RefreshWorkoutList(size);
            }
        });

        // Bouton pour défiler vers le bas
        DownButton = Button.CreateButtonWithText("v", Color.White, Color.Purple, () =>
        {
            if (CurrentScrollIndex < Workouts.Count - 1) // To display at least 1
            {
                CurrentScrollIndex++;
                RefreshWorkoutList(size);
            }
        });

        // Positionnement des boutons de défilement
        float buttonWidth = 50;
        float buttonHeight = 50;
        float leftMargin = Size.X / 4 * 3;

        UpButton.Size = new Vector2(buttonWidth, buttonHeight);
        DownButton.Size = new Vector2(buttonWidth, buttonHeight);

        UpButton.Position = new Vector2(leftMargin, startWorkoutListY);
        DownButton.Position = new Vector2(leftMargin, Size.Y - startWorkoutListY);

        Elements.Add(UpButton);
        Elements.Add(DownButton);
    }

    private void RefreshWorkoutList(Vector2 size)
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
            int index = i;
            Workout workout = Workouts[index].workout;

            Button editButton = Button.CreateButtonWithText("Edit", Color.White, Color.Purple, () =>
            {
                Game.Page = new WorkoutEditPage(Game, workout, Workouts[index].fileName);
            });
            editButton.Position = new Vector2(Size.X / 4, y);
            if (!string.IsNullOrEmpty(Workouts[index].fileName))
            {
                Elements.Add(editButton);
                CurrentWorkoutButtons.Add(editButton);
            }

            // Calcul de la durée totale
            TimeSpan totalDuration = CalculateTotalDuration(workout);
            string formattedDuration = FormatDuration(totalDuration);

            // Création du bouton
            string buttonText = $"{workout.Name} ({formattedDuration})";
            Button button = Button.CreateButtonWithText(buttonText, Color.White, Color.Purple,
                () => Game.Page = new WorkoutGamePage(Game, size, workout));

            button.Position = new Vector2(editButton.Position.X + editButton.Size.X + 150, y);
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
