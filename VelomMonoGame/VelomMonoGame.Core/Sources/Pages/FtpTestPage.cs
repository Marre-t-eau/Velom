using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using VelomMonoGame.Core.Sources.Bluetooth.Interfaces;
using VelomMonoGame.Core.Sources.InterfaceElements;
using VelomMonoGame.Core.Sources.Objects;
using VelomMonoGame.Core.Sources.Tools;

namespace VelomMonoGame.Core.Sources.Pages;

internal class FtpTestPage : WorkoutGamePage
{
    private ushort targetValue = 100;
    private Checkbox targetControlCheckbox;
    private Button setTargetUpButton;
    private Button setTargetDownButton;
    private Text targetValueText;

    public FtpTestPage(VelomMonoGameGame game, Vector2 size)
        : base(game, size, GetFTPWorkout())
    {
        targetValue = SaveManager.LoadUserData().FTP;
        AddFtpTestControls();
        Action baseAction = StopButton.OnClick;
        StopButton.OnClick = () =>
        {
            GameLogs.Add(new GameLogEntry { Type = GameLogEntryType.Stop });
            ushort ftpValue = CalculateFtp();
            if (ftpValue > 0)
            {
                ShowFtpSaveConfirmation(ftpValue);
            }
            else
            {
                ShowStopConfirmation();
            }
        };
    }

    private void AddFtpTestControls()
    {
        IBluetoothManager bluetoothManager = Game.Services.GetService<IBluetoothManager>();
        float marge = 10;
        // Checkbox pour activer/désactiver le contrôle de la cible
        targetControlCheckbox = Checkbox.CreateCheckbox("Set target", false, (isChecked) =>
        {
            if (isChecked)
            {
                bluetoothManager.StartControllingPower().Wait();
                bluetoothManager.SetPower(targetValue).Wait();
            }
            else
            {
                bluetoothManager.StopControllingPower();
            }
        });
        targetControlCheckbox.Position = new Vector2(marge, Size.Y - targetControlCheckbox.Size.Y - marge);
        Elements.Add(targetControlCheckbox);

        // Bouton pour diminuer la cible
        setTargetDownButton = Button.CreateButtonWithText("-", Color.White, Color.Purple, () =>
        {
            if (IsInTwentyMinuteBlock())
            {
                if (targetValue >= 5)
                    targetValue -= 5;
                targetValueText.TextContent = targetValue.ToString();
                if (targetControlCheckbox.IsChecked)
                {
                    bluetoothManager.SetPower(targetValue);
                }
            }
        });
        setTargetDownButton.Position = new Vector2(marge, targetControlCheckbox.Position.Y - marge - setTargetDownButton.Size.Y);
        Elements.Add(setTargetDownButton);

        // Texte affichant la valeur cible
        targetValueText = new Text
        {
            TextContent = targetValue.ToString(),
            Color = Color.Black
        };
        targetValueText.Position = new Vector2(marge, setTargetDownButton.Position.Y - marge - targetValueText.Size.Y);
        Elements.Add(targetValueText);

        // Bouton pour augmenter la cible
        setTargetUpButton = Button.CreateButtonWithText("+", Color.White, Color.Purple, () =>
        {
            if (IsInTwentyMinuteBlock())
            {
                targetValue += 5;
                targetValueText.TextContent = targetValue.ToString();
                if (targetControlCheckbox.IsChecked)
                {
                    bluetoothManager.SetPower(targetValue).Wait();
                }
            }
        });
        setTargetUpButton.Position = new Vector2(marge, targetValueText.Position.Y - marge - setTargetUpButton.Size.Y);
        Elements.Add(setTargetUpButton);
    }

    private bool IsInTwentyMinuteBlock()
    {
        return GetCurrentWorkBlock().Duration == 1200;
    }

    public override void Update(GameTime gameTime)
    {
        bool active = IsInTwentyMinuteBlock();
        targetControlCheckbox.IsUpdatable = active;
        setTargetUpButton.IsUpdatable = active;
        setTargetDownButton.IsUpdatable = active;
        base.Update(gameTime);

        // Change l'apparence des boutons selon l'état
        var color = active ? Color.Purple : Color.LightGray;
        setTargetUpButton.Background = TextureBank.GetTextureColor(color);
        setTargetDownButton.Background = TextureBank.GetTextureColor(color);
        targetControlCheckbox.Label.Color = active ? Color.Black : Color.Gray;
        targetValueText.Color = active ? Color.Black : Color.Gray;
    }

    private void ShowFtpSaveConfirmation(ushort ftpValue)
    {
        var ftpDialog = new ConfirmationDialog(
            $"Nouveau FTP détecté : {ftpValue:F0} W\nVoulez-vous le sauvegarder ?",
            Size / 2,
            // Action si confirmé
            () =>
            {
                // Sauvegarder le FTP (à adapter selon votre gestion utilisateur)
                var userData = SaveManager.LoadUserData();
                userData.FTP = ftpValue;
                SaveManager.SaveUserData(userData);
                ShowStopConfirmation();
            },
            // Action si annulé
            () =>
            {
                ShowStopConfirmation();
            },
            cancelMessage: "Non",
            confirmMessage: "Oui"
        );

        ftpDialog.Position = new Vector2(
            Size.X / 2 - ftpDialog.Size.X / 2,
            Size.Y / 2 - ftpDialog.Size.Y / 2
        );

        Elements.Add(ftpDialog);
    }

    // Ajoutez cette méthode pour calculer la puissance moyenne sur les 20 dernières minutes
    protected ushort CalculateFtp()
    {
        List<GameLogEntry> gameLogs = GameLogs.OrderBy(gl => gl.Timestamp).ToList();

        // Keep only the entries between the start and the stop and remove pauses, adjusting the timestamps accordingly
        DateTime start = GameLogs.First(gl => gl.Type == GameLogEntryType.Start).Timestamp;
        DateTime stop = GameLogs.Last(gl => gl.Type == GameLogEntryType.Stop).Timestamp;
        DateTime pauseStart = DateTime.Now;
        TimeSpan totalPaused = TimeSpan.Zero;
        bool isPaused = false;
        foreach (var log in gameLogs.ToList())
        {
            if (log.Type == GameLogEntryType.Pause)
            {
                pauseStart = log.Timestamp;
                isPaused = true;
            }
            if (!isPaused && log.Timestamp < start || log.Timestamp > stop)
                gameLogs.Remove(log);
            if (log.Type == GameLogEntryType.Resume)
            {
                isPaused = false;
                totalPaused += log.Timestamp - pauseStart;
            }
        }

        // Remove all logs before the 20 minutes bloc
        DateTime start20Bloc = start.AddSeconds(1500);
        gameLogs.RemoveAll(gl => gl.Timestamp < start20Bloc);
        DateTime stop20Bloc = start.AddSeconds(1200);
        gameLogs.RemoveAll(gl => gl.Timestamp > stop20Bloc);

        List<int> last20Powers = new List<int>();
        foreach (var log in gameLogs)
        {
            if (log.Type == GameLogEntryType.Power)
            {
                if (ushort.TryParse(log.Data, out ushort power))
                    last20Powers.Add(power);
            }
        }

        if (last20Powers.Count == 0)
            return 0;

        return (ushort)last20Powers.Average();
    }

    private static Workout GetFTPWorkout()
    {
        return new Workout
        {
            Blocks = new List<WorkBlock>
            {
                new WorkBlock
                {
                    Duration = 600,
                    TargetPowerStart = 50,
                    TargetPowerEnd = 80,
                    PowerType = TargetPowerType.PercentFTP
                },
                new WorkBlock
                {
                    Duration = 60,
                    TargetPowerStart = 110,
                    TargetPowerEnd = 110,
                    PowerType = TargetPowerType.PercentFTP
                },
                new WorkBlock
                {
                    Duration = 60,
                    TargetPowerStart = 60,
                    TargetPowerEnd = 60,
                    PowerType = TargetPowerType.PercentFTP
                },
                new WorkBlock
                {
                    Duration = 60,
                    TargetPowerStart = 110,
                    TargetPowerEnd = 110,
                    PowerType = TargetPowerType.PercentFTP
                },
                new WorkBlock
                {
                    Duration = 60,
                    TargetPowerStart = 60,
                    TargetPowerEnd = 60,
                    PowerType = TargetPowerType.PercentFTP
                },
                new WorkBlock
                {
                    Duration = 60,
                    TargetPowerStart = 110,
                    TargetPowerEnd = 110,
                    PowerType = TargetPowerType.PercentFTP
                },
                new WorkBlock
                {
                    Duration = 600,
                    TargetPowerStart = 60,
                    TargetPowerEnd = 60,
                    PowerType = TargetPowerType.PercentFTP
                },
                new WorkBlock
                {
                    Duration = 1200
                },
                new WorkBlock
                {
                    Duration = 600,
                    TargetPowerStart = 60,
                    TargetPowerEnd = 60,
                    PowerType = TargetPowerType.PercentFTP
                }
            }
        };
    }
}