using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using VelomMonoGame.Core.Sources.Bluetooth.Interfaces;
using VelomMonoGame.Core.Sources.InterfaceElements;
using VelomMonoGame.Core.Sources.Tools;

namespace VelomMonoGame.Core.Sources.Pages;

internal abstract class GamePage : IPage
{
    public Vector2 Size { get; set; }
    public List<IElement> Elements { get; set; } = new List<IElement>();
    protected IBluetoothManager BluetoothManager
    {
        get
        {
            return Game.Services.GetService<IBluetoothManager>();
        }
    }
    protected VelomMonoGameGame Game { get; }
    protected Bike Bike { get; } = new Bike();

    protected ushort ActualPower { get; set; }
    protected Text ActualPowerText { get; set; }
    protected Text ActualSpeed { get; set; }
    protected Text ActualCadence { get; set; }
    protected Text ActualHeartRate { get; set; }
    protected Text TotalDistance { get; set; }
    protected Text TotalTime { get; set; }

    protected TimeSpan StartTime { get; set; }

    protected Scene Scene { get; set; }

    // Variables pour le contrôle du jeu
    protected bool IsPaused { get; set; } = false;
    protected TimeSpan TotalPausedTime { get; set; } = TimeSpan.Zero;
    protected Button StartButton { get; set; }
    protected Button PauseButton { get; set; }
    protected Button ResumeButton { get; set; }
    protected Button StopButton { get; set; }
    protected bool IsStarted { get; set; } = false;

    // Ajouter cette propriété pour le dialogue de confirmation
    private ConfirmationDialog StopConfirmationDialog { get; set; }

    public GamePage(VelomMonoGameGame game, Vector2 size)
    {
        Game = game;
        Scene = new Scene(game.GraphicsDevice, Bike);
        Size = size;
        PrepareControl();
        CreateControlButtons();
    }

    public virtual void Draw(GameTime gameTime)
    {
        Scene.Draw();

        SpriteBatch spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        spriteBatch.Begin();
        foreach (var element in Elements)
        {
            if (element is IDrawableElement drawableElement)
                drawableElement.Draw(spriteBatch);
        }
        spriteBatch.End();
    }

    public virtual void Update(GameTime gameTime)
    {
        List<IElement> tmpElements = new List<IElement>(Elements);
        foreach (var element in tmpElements)
        {
            if (element is IUpdatableElement updatableElement)
                updatableElement.Update();
        }

        if (!IsStarted)
            return;

        if (StartTime == default && IsStarted)
        {
            StartTime = gameTime.TotalGameTime;
        }

        // Si le jeu est en pause, incrémente TotalPausedTime à chaque frame
        if (IsPaused)
        {
            // Ajoute le temps écoulé depuis la dernière frame à TotalPausedTime
            TotalPausedTime += gameTime.ElapsedGameTime;
            return;
        }

        // Calculer le temps ajusté (temps écoulé moins les pauses)
        TimeSpan adjustedGameTime = gameTime.TotalGameTime - StartTime - TotalPausedTime;

        float speed = GetSpeed(); // m/s
        Bike.Distance += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        // Update the actual speed displayed
        ActualSpeed.TextContent = $"{(speed * 3.6f).ToString("F1")} km/h"; // Convert m/s to km/h
        // Update the total distance displayed
        TotalDistance.TextContent = $"{Bike.Distance.ToString("F1")} km";
        // Update the total time displayed
        TotalTime.TextContent = $"{adjustedGameTime.Minutes:00}:{adjustedGameTime.Seconds:00}";
    }

    protected float marge = 10f;
    protected float bigMarge = 50f;
    private void PrepareControl()
    {
        Text powerText = new Text
        {
            TextContent = "Power : ",
            Color = Color.Black
        };
        Text speedText = new Text
        {
            TextContent = "Speed : ",
            Color = Color.Black
        };
        Text cadenceText = new Text
        {
            TextContent = "Cadence : ",
            Color = Color.Black
        };
        Text heartRateText = new Text
        {
            TextContent = "Heart Rate : ",
            Color = Color.Black
        };
        Text distanceText = new Text
        {
            TextContent = "Distance : ",
            Color = Color.Black
        };
        Text timeText = new Text
        {
            TextContent = "Time : ",
            Color = Color.Black
        };
        float width = bigMarge * 4 + powerText.Size.X + heartRateText.Size.X + distanceText.Size.X + FontBank.GetFont(FontsType.Default).MeasureString("999.9 km/h").X + FontBank.GetFont(FontsType.Default).MeasureString("999").X + FontBank.GetFont(FontsType.Default).MeasureString("999.9 km").X;
        float height = FontBank.GetFontHeight(FontsType.Default) * 2 + bigMarge * 2;
        // Draw a rectangle to show the datas
        RectangleElement rectangle = new RectangleElement
        {
            Position = new Vector2(Size.X / 2 - width / 2, marge),
            Size = new Vector2(width, height),
            Texture = TextureBank.GetTextureColor(Color.MediumPurple)
        };
        Elements.Add(rectangle);
        powerText.Position = new Vector2(rectangle.Position.X + bigMarge, rectangle.Position.Y + rectangle.Size.Y / 3 - powerText.Size.Y / 2);
        Elements.Add(powerText);
        speedText.Position = new Vector2(rectangle.Position.X + bigMarge, rectangle.Position.Y + rectangle.Size.Y / 3 * 2 - speedText.Size.Y / 2);
        Elements.Add(speedText);
        cadenceText.Position = new Vector2(powerText.Position.X + powerText.Size.X + FontBank.GetFont(FontsType.Default).MeasureString("999.9 km/h").X + bigMarge * 2, rectangle.Position.Y + rectangle.Size.Y / 3 - cadenceText.Size.Y / 2);
        Elements.Add(cadenceText);
        heartRateText.Position = new Vector2(cadenceText.Position.X, rectangle.Position.Y + rectangle.Size.Y / 3 * 2 - heartRateText.Size.Y / 2);
        Elements.Add(heartRateText);
        ActualPowerText = new Text
        {
            Position = new Vector2(powerText.Position.X + powerText.Size.X, powerText.Position.Y),
            TextContent = BluetoothManager.AsPower ? "0" : "N/A",
            Color = Color.DarkGray
        };
        Elements.Add(ActualPowerText);
        ActualSpeed = new Text
        {
            Position = new Vector2(speedText.Position.X + speedText.Size.X, speedText.Position.Y),
            TextContent = "0" + " km/h",
            Color = Color.DarkGray
        };
        Elements.Add(ActualSpeed);
        ActualCadence = new Text
        {
            Position = new Vector2(heartRateText.Position.X + heartRateText.Size.X, cadenceText.Position.Y),
            TextContent = BluetoothManager.AsCadence ? "0" : "N/A",
            Color = Color.DarkGray
        };
        Elements.Add(ActualCadence);
        ActualHeartRate = new Text
        {
            Position = new Vector2(heartRateText.Position.X + heartRateText.Size.X, heartRateText.Position.Y),
            TextContent = BluetoothManager.AsHeartRate ? "0" : "N/A",
            Color = Color.DarkGray
        };
        Elements.Add(ActualHeartRate);
        distanceText.Position = new Vector2(ActualCadence.Position.X + FontBank.GetFont(FontsType.Default).MeasureString("999").X + bigMarge, ActualCadence.Position.Y);
        Elements.Add(distanceText);
        timeText.Position = new Vector2(distanceText.Position.X, heartRateText.Position.Y);
        Elements.Add(timeText);
        TotalDistance = new Text
        {
            Position = new Vector2(distanceText.Position.X + distanceText.Size.X, distanceText.Position.Y),
            TextContent = "0 km",
            Color = Color.DarkGray
        };
        Elements.Add(TotalDistance);
        TotalTime = new Text
        {
            Position = new Vector2(TotalDistance.Position.X, timeText.Position.Y),
            TextContent = "00:00",
            Color = Color.DarkGray
        };
        Elements.Add(TotalTime);
        // Subscribe to the events to update the texts
        BluetoothManager.PowerUpdated += (sender, power) =>
        {
            ActualPower = power;
            ActualPowerText.TextContent = power.ToString();
        };
        BluetoothManager.CadenceUpdated += (sender, cadence) =>
        {
            ActualCadence.TextContent = cadence.ToString();
        };
        BluetoothManager.HeartRateUpdated += (sender, heartRate) =>
        {
            ActualHeartRate.TextContent = heartRate.ToString();
        };
    }

    // Ajouter cette nouvelle méthode pour afficher le dialogue de confirmation
    private void ShowStopConfirmation()
    {
        // Créer le dialogue de confirmation
        StopConfirmationDialog = new ConfirmationDialog(
            "Do you want to stop and return to the main page ?",
            Size / 2,
            // Action à exécuter si l'utilisateur confirme
            () =>
            {
                SetButtonVisibility(start: true, pause: false, resume: false);
                OnGameStopped();
                Game.Page = new MainPage(Game);
            },
            cancelMessage: "No",
            confirmMessage: "Yes"
        );

        // Positionner le dialogue au centre de l'écran
        StopConfirmationDialog.Position = new Vector2(
            Size.X / 2 - StopConfirmationDialog.Size.X / 2,
            Size.Y / 2 - StopConfirmationDialog.Size.Y / 2
        );

        // Ajouter le dialogue à la liste des éléments
        Elements.Add(StopConfirmationDialog);
    }

    private float GetSpeed()
    {
        // Constantes physiques
        const float rho = 1.2f; // densité de l'air (kg/m³)
        const float Cx = 0.9f; // coefficient de traînée
        const float S = 0.5f; // surface frontale (m²)
        const float rolling = 10.0f; // résistance au roulement (W, simplifié)

        float totalPower = Math.Max(ActualPower - rolling, 0f);
        float speed = (float)Math.Pow(totalPower / (0.5f * rho * Cx * S), 1.0 / 3.0); // m/s

        return speed;
    }

    protected virtual void CreateControlButtons()
    {
        // Création du bouton Démarrer
        StartButton = Button.CreateButtonWithText("Start", Color.White, Color.Green, () =>
        {
            if (!IsStarted)
            {
                IsStarted = true;
                StartTime = Game.Services.GetService<GameTime>()?.TotalGameTime ?? TimeSpan.Zero;
                SetButtonVisibility(start: false, pause: true, resume: false);
                OnGameStarted();
            }
        });
        StartButton.Position = new Vector2(Size.X / 2 - StartButton.Size.X / 2, Size.Y - StartButton.Size.Y - marge);
        Elements.Add(StartButton);

        // Création du bouton Pause
        PauseButton = Button.CreateButtonWithText("Pause", Color.White, Color.Gray, () =>
        {
            if (IsStarted && !IsPaused)
            {
                IsPaused = true;
                SetButtonVisibility(start: false, pause: false, resume: true);
                OnGamePaused();
            }
        });
        PauseButton.Position = new Vector2(Size.X / 2 - PauseButton.Size.X / 2, Size.Y - PauseButton.Size.Y - marge);
        Elements.Add(PauseButton);

        // Création du bouton Reprendre
        ResumeButton = Button.CreateButtonWithText("Resume", Color.White, Color.Gray, () =>
        {
            if (IsPaused)
            {
                IsPaused = false;
                SetButtonVisibility(start: false, pause: true, resume: false);
                OnGameResumed();
            }
        });
        ResumeButton.Position = new Vector2(Size.X / 2 - ResumeButton.Size.X / 2, Size.Y - PauseButton.Size.Y - marge);
        Elements.Add(ResumeButton);

        // Création du bouton Arrêter
        StopButton = Button.CreateButtonWithText("Stop", Color.White, Color.Red, () =>
        {
            ShowStopConfirmation();
        });
        StopButton.Position = new Vector2(Size.X - StopButton.Size.X - marge, Size.Y - StopButton.Size.Y - marge);
        Elements.Add(StopButton);

        // Initial state: only Start visible
        SetButtonVisibility(start: true, pause: false, resume: false);
    }

    // Méthodes virtuelles qui peuvent être surchargées par les classes dérivées
    protected virtual void OnGameStarted() { }
    protected virtual void OnGamePaused()
    {
        // Par défaut, arrêter le contrôle de la puissance pendant la pause
        BluetoothManager?.StopControllingPower().Wait();
    }
    protected virtual void OnGameResumed() { }
    protected virtual void OnGameStopped()
    {
        // Par défaut, arrêter le contrôle de la puissance
        BluetoothManager?.StopControllingPower().Wait();
    }

    private void SetButtonVisibility(bool start, bool pause, bool resume)
    {
        StartButton.Visible = start;
        StartButton.IsUpdatable = start;
        PauseButton.Visible = pause;
        PauseButton.IsUpdatable = pause;
        ResumeButton.Visible = resume;
        ResumeButton.IsUpdatable = resume;
    }
}
