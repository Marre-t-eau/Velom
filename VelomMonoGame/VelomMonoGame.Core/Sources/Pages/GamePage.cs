using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using VelomMonoGame.Core.Sources.Bluetooth.Interfaces;
using VelomMonoGame.Core.Sources.InterfaceElements;
using VelomMonoGame.Core.Sources.Tools;

namespace VelomMonoGame.Core.Sources.Pages;

internal class GamePage : IPage
{
    public Vector2 Size { get; set; }
    public List<IElement> Elements { get; set; } = new List<IElement>();
    IBluetoothManager BluetoothManager { get; init; }

    ushort ActualTargetPower { get; set; } = 100;
    Text ActualTargetPowerText { get; set; }
    Text ActualPower { get; set; }
    Text ActualCadence { get; set; }
    Text ActualHeartRate { get; set; }

    public GamePage(Vector2 size, IBluetoothManager bluetoothManager, Layout layout)
    {
        Size = size;
        BluetoothManager = bluetoothManager;
        switch (layout)
        {
            case Layout.Control:
                PrepareControl();
                break;
            default:
                throw new System.ArgumentException("Unsupported layout type.");
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var element in Elements)
        {
            if (element is IDrawableElement drawableElement)
                drawableElement.Draw(spriteBatch);
        }
    }

    public void Update()
    {
        foreach (var element in Elements)
        {
            if (element is IUpdatableElement updatableElement)
                updatableElement.Update();
        }
    }

    private async void PrepareControl()
    {
        float marge = 10f;
        float bigMarge = 50f;
        // Buttons to control the power of the device
        if (BluetoothManager.CanSetPower)
        {
            // By default we set the power at 100
            await BluetoothManager.SetPower(ActualTargetPower);
            await BluetoothManager.StartControllingPower();
            // Add 2 buttons to control the power
            Button increasePowerButton = Button.CreateButtonWithText("+", Color.White, Color.Purple, IncreaseTargetPower);
            increasePowerButton.Position = new Vector2(marge, marge);
            Elements.Add(increasePowerButton);
            ActualTargetPowerText = new Text
            {
                Position = new Vector2(marge, increasePowerButton.Position.Y + increasePowerButton.Size.Y + marge),
                TextContent = ActualTargetPower.ToString(),
                Color = Color.DarkGray
            };
            Elements.Add(ActualTargetPowerText);
            Button decreasePowerButton = Button.CreateButtonWithText("-", Color.White, Color.Purple, DecreaseTargetPower);
            decreasePowerButton.Position = new Vector2(marge, ActualTargetPowerText.Position.Y + ActualTargetPowerText.Size.Y + marge);
            Elements.Add(decreasePowerButton);
        }
        Text powerText = new Text
        {
            TextContent = "Power : ",
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
        float width = bigMarge * 4 + powerText.Size.X + heartRateText.Size.X + FontBank.GetFont(FontsType.Default).MeasureString("9999").X + FontBank.GetFont(FontsType.Default).MeasureString("999").X;
        float height = FontBank.GetFontHeight(FontsType.Default) * 2 + bigMarge * 2;
        // Draw a rectangle to show the datas
        RectangleElement rectangle = new RectangleElement
        {
            Position = new Vector2(Size.X / 2 - width / 2, marge),
            Size = new Vector2(width, height),
            Texture = TextureBank.GetTextureColor(Color.MediumPurple)
        };
        Elements.Add(rectangle);
        powerText.Position = new Vector2(rectangle.Position.X + bigMarge, rectangle.Position.Y + rectangle.Size.Y / 2 - powerText.Size.Y / 2);
        Elements.Add(powerText);
        cadenceText.Position = new Vector2(powerText.Position.X + powerText.Size.X + FontBank.GetFont(FontsType.Default).MeasureString("9999").X + bigMarge * 2, rectangle.Position.Y + rectangle.Size.Y / 3 - cadenceText.Size.Y / 2);
        Elements.Add(cadenceText);
        heartRateText.Position = new Vector2(cadenceText.Position.X, rectangle.Position.Y + rectangle.Size.Y / 3 * 2 - heartRateText.Size.Y / 2);
        Elements.Add(heartRateText);
        ActualPower = new Text
        {
            Position = new Vector2(powerText.Position.X + powerText.Size.X, powerText.Position.Y),
            TextContent = BluetoothManager.AsPower ? "0" : "N/A",
            Color = Color.DarkGray
        };
        Elements.Add(ActualPower);
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
            ActualPower.TextContent = power.ToString();
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

    private async void IncreaseTargetPower()
    {
        ActualTargetPower += 5;
        ActualTargetPowerText.TextContent = ActualTargetPower.ToString();
        await BluetoothManager.SetPower(ActualTargetPower);
    }

    private async void DecreaseTargetPower()
    {
        ActualTargetPower -= 5;
        ActualTargetPowerText.TextContent = ActualTargetPower.ToString();
        await BluetoothManager.SetPower(ActualTargetPower);
    }

    public enum Layout
    {
        Control
    }
}
