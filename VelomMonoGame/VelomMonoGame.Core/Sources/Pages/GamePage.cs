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
    protected IBluetoothManager BluetoothManager { get; init; }
    protected Game Game { get; }
    protected Bike Bike { get; } = new Bike();

    protected ushort ActualPower { get; set; }
    protected Text ActualPowerText { get; set; }
    protected Text ActualSpeed { get; set; }
    protected Text ActualCadence { get; set; }
    protected Text ActualHeartRate { get; set; }

    protected Scene Scene { get; set; }

    public GamePage(Game game, Vector2 size, IBluetoothManager bluetoothManager)
    {
        Game = game;
        Scene = new Scene(game.GraphicsDevice, Bike);
        Size = size;
        BluetoothManager = bluetoothManager;
        PrepareControl();
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
        float speed = GetSpeed(); // m/s
        Bike.Distance += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        // Update the actual speed displayed
        ActualSpeed.TextContent = $"{(speed * 3.6f).ToString("F1")} km/h"; // Convert m/s to km/h
        foreach (var element in Elements)
        {
            if (element is IUpdatableElement updatableElement)
                updatableElement.Update();
        }
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
        float width = bigMarge * 4 + powerText.Size.X + heartRateText.Size.X + FontBank.GetFont(FontsType.Default).MeasureString("999.9 km/h").X + FontBank.GetFont(FontsType.Default).MeasureString("999").X;
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
}
