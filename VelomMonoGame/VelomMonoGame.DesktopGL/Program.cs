using System;
using VelomMonoGame.Core;
using VelomMonoGame.Core.Sources.Bluetooth.Interfaces;

internal class Program
{
    /// <summary>
    /// The main entry point for the application. 
    /// This creates an instance of your game and calls it's Run() method 
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    private static void Main(string[] args)
    {
        IBluetoothManager bluetoothManager = null;
#if DEBUG
        bluetoothManager = new VelomMonoGame.DesktopGL.Sources.Debug.MockBluetoothManager();
#endif
        var fileProvider = new DesktopFileProvider(AppDomain.CurrentDomain.BaseDirectory);

        using var game = new VelomMonoGameGame();
        game.Services.AddService(typeof(IFileProvider), fileProvider);
        game.Services.AddService(typeof(IBluetoothManager), bluetoothManager);
        game.Run();
    }
}