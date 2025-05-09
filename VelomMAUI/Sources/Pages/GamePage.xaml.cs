using System.Diagnostics;
using System.Numerics;
using VelomGame.Drawables;
using VelomGame.Rendering;
using VelomGame;

namespace Velom.Sources.Pages;

public partial class GamePage : ContentPage
{
    private bool _isRunning;
    private int _frameGameCount;
    private int _frameInterfaceCount;
    private Stopwatch _stopwatch;
    private Camera _camera;

    public GamePage()
    {
        InitializeComponent();
        // Initialiser la caméra
        _camera = new Camera(
            new Scene()
        );

        // Create a TextDrawable to display FPS
        TextDrawable textDrawable = new TextDrawable
        {
            Text = "FPS: 0 | 0"
        };

        GameGraphicsView.Drawable = _camera;
        InterfaceGraphicsView.Drawable = textDrawable;

        // Initialize the stopwatch and frame counter
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
        _frameGameCount = 0;
        _frameInterfaceCount = 0;

        // Start the game loop
        _isRunning = true;
        Task.Run(StartGameLoop);
        Task.Run(() => StartInterfaceLoop(textDrawable));
    }

    int ind = 0;
    private float _progress = 0.0f; // Variable pour suivre la progression entre les points
    private const float _speed = .5f; // Vitesse de progression (ajustez selon vos besoins)
    private async void StartGameLoop()
    {
        const int targetFrameTime = 1000 / 60; // Target frame time in milliseconds (16.67ms for 60 FPS)
        Stopwatch frameStopwatch = new Stopwatch();

        while (_isRunning)
        {
            frameStopwatch.Restart(); // Start measuring the frame time

            // Obtenir les points actuels et suivants
            var currentPoint = _camera.Scene.Map.TrackPoints[ind];
            var nextPoint = _camera.Scene.Map.TrackPoints[(ind + 1) % _camera.Scene.Map.TrackPoints.Count()];

            // Interpoler la position entre le point actuel et le prochain
            _camera.Scene.Player.Position = Vector3.Lerp(currentPoint, nextPoint, _progress);

            // Calculer la direction et la rotation
            var direction = Vector3.Normalize(nextPoint - currentPoint);
            float angle = -MathF.Atan2(direction.Z, direction.X);
            _camera.Scene.Player.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, angle);

            // Mettre à jour la caméra
            _camera.UpdateCamera();

            // Augmenter la progression
            _progress += _speed;

            // Si la progression atteint 1, passer au prochain segment
            if (_progress >= 1.0f)
            {
                _progress = 0.0f; // Réinitialiser la progression
                ind = (ind + 1) % _camera.Scene.Map.TrackPoints.Count(); // Passer au prochain point
            }

            await RunOnMainThreadAsync(() =>
            {
                GameGraphicsView.Invalidate();
            });

            _frameGameCount++;

            // Calculate the elapsed time for the frame
            frameStopwatch.Stop();
            int elapsedTime = (int)frameStopwatch.ElapsedMilliseconds;

            // Wait for the remaining time to achieve the target frame time
            int delayTime = targetFrameTime - elapsedTime;
            if (delayTime > 0)
            {
                //await Task.Delay(delayTime);
            }
        }
    }

    private async Task RunOnMainThreadAsync(Action action)
    {
        var tcs = new TaskCompletionSource();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                action();
                tcs.SetResult(); // Signal that the action has completed
            }
            catch (Exception ex)
            {
                tcs.SetException(ex); // Signal that an exception occurred
            }
        });

        await tcs.Task; // Wait for the action to complete
    }


    private async void StartInterfaceLoop(TextDrawable textDrawable)
    {
        while (_isRunning)
        {
            _frameInterfaceCount++;

            // Calculate FPS every second
            if (_stopwatch.ElapsedMilliseconds >= 1000)
            {
                // Update the TextDrawable with the FPS value
                int gameFps = _frameGameCount;
                int interfaceFps = _frameInterfaceCount;
                textDrawable.Text = $"FPS: {gameFps} | {interfaceFps}";

                _frameGameCount = 0;
                _frameInterfaceCount = 0;

                _stopwatch.Restart();

                // Redraw the InterfaceView
                await RunOnMainThreadAsync(() =>
                {
                    InterfaceGraphicsView.Invalidate();
                });
            }
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Stop the game loop when the page is closed
        _isRunning = false;
    }
}