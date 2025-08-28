using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml.Linq;
using VelomMonoGame.Core.Sources.Bluetooth.Interfaces;
using VelomMonoGame.Core.Sources.InterfaceElements;
using VelomMonoGame.Core.Sources.Tools;

namespace VelomMonoGame.Core.Sources.Pages;

internal class MainPage : IPage
{
    private Vector2 size = Vector2.Zero;
    public Vector2 Size
    {
        get
        {
            return size;
        }
        set
        {
            size = value;
            StaticConnectedDeviceText.Position = new Vector2(Size.X / 4 - StaticConnectedDeviceText.Size.X/2, Size.Y / 6 - StaticConnectedDeviceText.Size.Y / 2);
            float xPos = StaticConnectedDeviceText.Position.X;
            float stringHeight = FontBank.GetFont(FontsType.Default).MeasureString("|").Y;
            StaticAsPowerText.Position = new Vector2(xPos, Size.Y - stringHeight * 8);
            StaticCanControlPowerText.Position = new Vector2(xPos, Size.Y - stringHeight * 6);
            StaticAsCadenceText.Position = new Vector2(xPos, Size.Y - stringHeight * 4);
            StaticAsHearthrateText.Position = new Vector2(xPos, Size.Y - stringHeight * 2);
            float resultXPos = StaticCanControlPowerText.Position.X + FontBank.GetFont(FontsType.Default).MeasureString(StaticCanControlPowerText.TextContent + " ").X;
            AsPowerResult.Position = new Vector2(resultXPos, StaticAsPowerText.Position.Y);
            CanControlPowerResult.Position = new Vector2(resultXPos, StaticCanControlPowerText.Position.Y);
            AsCadenceResult.Position = new Vector2(resultXPos, StaticAsCadenceText.Position.Y);
            AsHeartrateResult.Position = new Vector2(resultXPos, StaticAsHearthrateText.Position.Y);
            // Set the new position of GoToControlGame button
            GoToControlGame.Position = new Vector2(Size.X / 4 * 3 - GoToControlGame.Size.X / 2 - stringHeight, Size.Y / 3 - GoToControlGame.Size.Y / 2 - stringHeight / 2);
            GoToWorkouts.Position = new Vector2(Size.X / 4 * 3 - GoToWorkouts.Size.X / 2 - stringHeight, (Size.Y / 3) * 2 + GoToWorkouts.Size.Y / 2 + stringHeight / 2);
            // Update positions of inner elements of buttons*
            foreach (IElement element in GoToControlGame.Elements)
            {
                if (element is Text textElement)
                {
                    textElement.Position = new Vector2(GoToControlGame.Position.X + GoToControlGame.Size.X / 2 - textElement.Size.X / 2, GoToControlGame.Position.Y + GoToControlGame.Size.Y / 2 - textElement.Size.Y / 2);
                }
            }
            foreach (IElement element1 in GoToWorkouts.Elements)
            {
                if (element1 is Text textElement)
                {
                    textElement.Position = new Vector2(GoToWorkouts.Position.X + GoToWorkouts.Size.X / 2 - textElement.Size.X / 2, GoToWorkouts.Position.Y + GoToWorkouts.Size.Y / 2 - textElement.Size.Y / 2);
                }
            }
        }
    }
    public List<IElement> Elements { get; set; } = [];
    private Text StaticConnectedDeviceText { get; }
    private Text StaticAsPowerText { get; }
    private Text AsPowerResult { get; }
    private Text StaticCanControlPowerText { get; }
    private Text CanControlPowerResult { get; }
    private Text StaticAsCadenceText { get; }
    private Text AsCadenceResult { get; }
    private Text StaticAsHearthrateText { get; }
    private Text AsHeartrateResult { get; }
    private List<Text> ConnectedDevices { get; } = [];
    private IBluetoothManager BluetoothManager { get; }
    private VelomMonoGameGame Game { get; init; }
    private Button GoToControlGame { get; }
    private Button GoToWorkouts { get; }

    internal MainPage(VelomMonoGameGame game, IBluetoothManager bluetoothManager)
    {
        Game = game;
        BluetoothManager = bluetoothManager;
        StaticConnectedDeviceText = new Text()
        {
            Color = Color.Black,
            Position = new Vector2(Size.X / 2, Size.Y / 6),
            TextContent = "Connected devices :"
        };
        Elements.Add(StaticConnectedDeviceText);
        if (bluetoothManager != null)
        {
            bluetoothManager.DiscoveredDevices.CollectionChanged += DiscoveredDevices_CollectionChanged;
        }
        float stringHeight = FontBank.GetFont(FontsType.Default).MeasureString("|").Y;
        StaticAsPowerText = new Text()
        {
            Color = Color.Black,
            Position = new Vector2(Size.X / 2, Size.Y - stringHeight * 8),
            TextContent = "As Power :"
        };
        Elements.Add(StaticAsPowerText);
        StaticCanControlPowerText = new Text()
        {
            Color = Color.Black,
            Position = new Vector2(Size.X / 2, Size.Y - stringHeight * 6),
            TextContent = "Can control power :"
        };
        Elements.Add(StaticCanControlPowerText);
        StaticAsCadenceText = new Text()
        {
            Color = Color.Black,
            Position = new Vector2(Size.X / 2, Size.Y - stringHeight * 4),
            TextContent = "As Cadence :"
        };
        Elements.Add(StaticAsCadenceText);
        StaticAsHearthrateText = new Text()
        {
            Color = Color.Black,
            Position = new Vector2(Size.X / 2, Size.Y - stringHeight * 2),
            TextContent = "As Heart Rate :"
        };
        Elements.Add(StaticAsHearthrateText);
        float resultXPos = StaticCanControlPowerText.Position.X + FontBank.GetFont(FontsType.Default).MeasureString(StaticCanControlPowerText.TextContent + " ").X;
        AsPowerResult = new Text()
        {
            Color = Color.DarkGray,
            Position = new Vector2(resultXPos, StaticAsPowerText.Position.Y),
            TextContent = bluetoothManager?.AsPower == true ? "Yes" : "No"
        };
        Elements.Add(AsPowerResult);
        CanControlPowerResult = new Text()
        {
            Color = Color.DarkGray,
            Position = new Vector2(resultXPos, StaticCanControlPowerText.Position.Y),
            TextContent = bluetoothManager?.CanSetPower == true ? "Yes" : "No"
        };
        Elements.Add(CanControlPowerResult);
        AsCadenceResult = new Text()
        {
            Color = Color.DarkGray,
            Position = new Vector2(resultXPos, StaticAsCadenceText.Position.Y),
            TextContent = bluetoothManager?.AsCadence == true ? "Yes" : "No"
        };
        Elements.Add(AsCadenceResult);
        AsHeartrateResult = new Text()
        {
            Color = Color.DarkGray,
            Position = new Vector2(resultXPos, StaticAsHearthrateText.Position.Y),
            TextContent = bluetoothManager?.AsHeartRate == true ? "Yes" : "No"
        };
        Elements.Add(AsHeartrateResult);
        GoToControlGame = Button.CreateButtonWithText("Go to control Game", Color.White, Color.Purple, () => Game.Page = new ControlGamePage(game, Size, bluetoothManager)); // Navigue vers GamePage
        GoToControlGame.Position = new Vector2(Size.X / 4 * 3 - GoToControlGame.Size.X / 2 - stringHeight, Size.Y / 3 - GoToControlGame.Size.Y / 2 - stringHeight / 2);
        Elements.Add(GoToControlGame);
        GoToWorkouts = Button.CreateButtonWithText("Go to workouts", Color.White, Color.Purple, () => Game.Page = new WorkoutListPage(game, Size, bluetoothManager));
        GoToWorkouts.Position = new Vector2(Size.X / 4 * 3 - GoToWorkouts.Size.X / 2 - stringHeight, (Size.Y / 3) * 2 + GoToWorkouts.Size.Y / 2 + stringHeight / 2);
        Elements.Add(GoToWorkouts);

    }

    private void DiscoveredDevices_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        ObservableCollection<IDeviceManager> collection = (ObservableCollection<IDeviceManager>)sender;
        Text textDevice = new Text()
        {
            Color = Color.DarkGray,
            TextContent = collection[collection.Count - 1].Name ?? "Unknown",
            Position = GetPositionOfDeviceText(ConnectedDevices.Count)
        };
        ConnectedDevices.Add(textDevice);
        Elements.Add(textDevice);
        AsPowerResult.TextContent = BluetoothManager.AsPower ? "Yes" : "No";
        CanControlPowerResult.TextContent = BluetoothManager.CanSetPower ? "Yes" : "No";
        AsCadenceResult.TextContent = BluetoothManager.AsCadence ? "Yes" : "No";
        AsHeartrateResult.TextContent = BluetoothManager.AsHeartRate ? "Yes" : "No";
        // We have everything we need, so we can stop the scan
        if (AsAllDevices())
            BluetoothManager.StopScan();
    }

    private Vector2 GetPositionOfDeviceText(int indice)
    {
        float marge = StaticConnectedDeviceText.Size.Y / 2;
        Vector2 position = new Vector2(StaticConnectedDeviceText.Position.X, StaticConnectedDeviceText.Position.Y + StaticConnectedDeviceText.Size.Y / 2 + marge + (indice * (FontBank.GetFont(FontsType.Default).MeasureString("|").Y + marge)));
        return position;
    }

    void IPage.Update(GameTime _)
    {
        // Update elements
        foreach (IElement element in Elements)
        {
            if (element is IUpdatableElement updatableElement)
            {
                updatableElement.Update();
            }
        }
        if (BluetoothManager == null)
            return;
        if (!BluetoothManager.IsScanning && !AsAllDevices())
        {
            BluetoothManager.StartScan();
        }
    }

    void IPage.Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        spriteBatch.Begin();
        foreach (IElement element in Elements)
        {
            if (element is IDrawableElement drawableElement)
            {
                drawableElement.Draw(spriteBatch);
            }
        }
        spriteBatch.End();
    }

    bool AsAllDevices()
    {
        return BluetoothManager.AsPower && BluetoothManager.CanSetPower && BluetoothManager.AsCadence && BluetoothManager.AsHeartRate;
    }
}
