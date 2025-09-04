using Microsoft.Xna.Framework;
using VelomMonoGame.Core.Sources.Bluetooth.Interfaces;
using VelomMonoGame.Core.Sources.InterfaceElements;

namespace VelomMonoGame.Core.Sources.Pages;

internal class ControlGamePage : GamePage
{
    ushort ActualTargetPower { get; set; } = 100;
    Text ActualTargetPowerText { get; set; }
    private Checkbox ControlPowerCheckbox { get; set; }

    internal ControlGamePage(Game game, Vector2 size, IBluetoothManager bluetoothManager) : base(game, size, bluetoothManager)
    {
        PrepareControl();
        if (ControlPowerCheckbox.IsChecked)
        {
            bluetoothManager.StartControllingPower().Wait();
        }
    }

    private async void PrepareControl()
    {
        // Buttons to control the power of the device
        if (BluetoothManager.CanSetPower)
        {
            // Create checkbox for controlling power
            ControlPowerCheckbox = Checkbox.CreateCheckbox("Control power", true, OnPowerControlChanged);
            ControlPowerCheckbox.Position = new Vector2(marge, Size.Y - ControlPowerCheckbox.Size.Y - marge);
            Elements.Add(ControlPowerCheckbox);

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
    }

    private async void OnPowerControlChanged(bool isChecked)
    {
        if (isChecked)
        {
            await BluetoothManager.StartControllingPower();
            await BluetoothManager.SetPower(ActualTargetPower);
        }
        else
        {
            await BluetoothManager.StopControllingPower();
        }
    }

    private async void IncreaseTargetPower()
    {
        ActualTargetPower += 5;
        ActualTargetPowerText.TextContent = ActualTargetPower.ToString();
        if (ControlPowerCheckbox.IsChecked)
        {
            await BluetoothManager.SetPower(ActualTargetPower);
        }
    }

    private async void DecreaseTargetPower()
    {
        ActualTargetPower -= 5;
        ActualTargetPowerText.TextContent = ActualTargetPower.ToString();
        if (ControlPowerCheckbox.IsChecked)
        {
            await BluetoothManager.SetPower(ActualTargetPower);
        }
    }
}
