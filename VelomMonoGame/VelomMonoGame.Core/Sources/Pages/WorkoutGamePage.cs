using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using VelomMonoGame.Core.Sources.InterfaceElements;
using VelomMonoGame.Core.Sources.Objects;
using VelomMonoGame.Core.Sources.Tools;

namespace VelomMonoGame.Core.Sources.Pages;

internal class WorkoutGamePage : GamePage
{
    private Workout Workout { get; init; }

    private Dictionary<WorkBlock, RectangleElement> Backgrounds { get; } = new();

    private Text CurrentTimeInBlock { get; set; }

    private WorkBlock LastWorkBlock { get; set; }
    private ushort LastPowerSet { get; set; }

    public WorkoutGamePage(VelomMonoGameGame game, Vector2 size, Workout workout) : base(game, size)
    {
        Workout = workout;
        workout.FTP = SaveManager.LoadUserData().FTP;
        PrepareWorkout();
    }

    private void PrepareWorkout()
    {
        float backgroundWidth = marge * 2 + FontBank.GetFont(FontsType.Default).MeasureString("999 - 999 W | 999 rpm | 99:99").X;
        float y = 0;
        foreach (WorkBlock block in Workout.Blocks)
        {
            RectangleElement rectangle = new RectangleElement
            {
                Position = new Vector2(0, y),
                Size = new Vector2(backgroundWidth, FontBank.GetFontHeight(FontsType.Default) + marge),
                Texture = TextureBank.GetTextureColor(Color.DarkRed)
            };
            Elements.Add(rectangle);
            Backgrounds.Add(block, rectangle);
            Text text = new Text
            {
                Position = new Vector2(marge, y + marge / 2),
                Color = Color.White
            };
            TimeSpan duration = new TimeSpan(0, 0, (int)block.Duration);
            if (block.TargetPowerStart.HasValue || block.TargetPowerEnd.HasValue)
            {
                if (block.TargetPowerStart.HasValue && block.TargetPowerEnd.HasValue && block.TargetPowerStart.Value != block.TargetPowerEnd.Value)
                {
                    // It's a ramp
                    text.TextContent = $"{block.TargetPowerStart.Value.ToString().PadLeft(3, ' ')} - {block.TargetPowerEnd.Value,3} W | {(block.TargetCadence.HasValue ? block.TargetCadence.Value.ToString().PadLeft(3, ' ') : "  -")} rpm | {duration.Minutes:00}:{duration.Seconds:00}";
                }
                else
                {
                    // It's constant
                    ushort targetPower = block.TargetPowerStart ?? block.TargetPowerEnd.Value;
                    text.TextContent = $"{targetPower} W | {(block.TargetCadence.HasValue ? block.TargetCadence.Value.ToString().PadLeft(3, ' ') : "  -")} rpm | {duration.Minutes:00}:{duration.Seconds:00}";
                }
            }
            else
            {
                text.TextContent = $"  - W | {(block.TargetCadence.HasValue ? block.TargetCadence.Value.ToString().PadLeft(3, ' ') : "  -")} rpm | {duration.Minutes:00}:{duration.Seconds:00}";
            }
            Elements.Add(text);
            y += rectangle.Size.Y;
        }
        CurrentTimeInBlock = new Text
        {
            Position = new Vector2(backgroundWidth + marge, marge / 2),
            Color = Color.Black,
            TextContent = "00:00"
        };
        Elements.Add(CurrentTimeInBlock);
        if (Workout.Blocks.Count > 0)
        {
            LastWorkBlock = Workout.Blocks[0];
            UpdateTargetPower(LastWorkBlock, 0);
        }
    }

    private TimeSpan timeInWorkout = TimeSpan.Zero;
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (StartTime == TimeSpan.Zero)
            return;
        // Set the right position of the current time in block
        timeInWorkout = gameTime.TotalGameTime - StartTime - TotalPausedTime;
        WorkBlock currentBlock = GetCurrentWorkBlock();
        if (currentBlock == null)
        {
            // Workout is finished
            return;
        }
        RectangleElement currentRectangle = Backgrounds[currentBlock];
        CurrentTimeInBlock.Position = new Vector2(CurrentTimeInBlock.Position.X, currentRectangle.Position.Y + marge / 2);
        // Update the text with the current time
        double timeInSecondInCurrentBlock = timeInWorkout.TotalSeconds;
        int indice = 0;
        while (currentBlock != Workout.Blocks[indice])
        {
            timeInSecondInCurrentBlock -= Workout.Blocks[indice].Duration;
            indice++;
        }
        TimeSpan timeSpanInCurrentBlock = new TimeSpan(0, 0, (int)timeInSecondInCurrentBlock);
        CurrentTimeInBlock.TextContent = $"{timeSpanInCurrentBlock.Minutes:00}:{timeSpanInCurrentBlock.Seconds:00}";
        currentRectangle.Texture = TextureBank.GetTextureColor(Color.Blue);
        UpdateTargetPower(currentBlock, (int)timeInSecondInCurrentBlock);
        if (LastWorkBlock != currentBlock)
        {
            // The block changed
            Backgrounds[LastWorkBlock].Texture = TextureBank.GetTextureColor(Color.Green);
            LastWorkBlock = currentBlock;
        }
    }

    protected WorkBlock GetCurrentWorkBlock()
    {
        double time = timeInWorkout.TotalSeconds;
        foreach (WorkBlock workBlock in Workout.Blocks)
        {
            time -= workBlock.Duration;
            if (time < 0)
            {
                return workBlock;
            }
        }
        return null;
    }

    private void UpdateTargetPower(WorkBlock currentBlock, int secondsInBlock)
    {
        if (currentBlock.TargetPowerStart.HasValue || currentBlock.TargetPowerEnd.HasValue)
        {
            // If the current workBlock is constant
            ushort powerToSet;
            if (currentBlock.TargetPowerStart.HasValue && currentBlock.TargetPowerEnd.HasValue && currentBlock.TargetPowerStart.Value != currentBlock.TargetPowerEnd.Value)
            {
                // It's a ramp with step of 5 watts
                double powerDifference = currentBlock.TargetPowerEnd.Value - currentBlock.TargetPowerStart.Value;
                double powerPerSecond = powerDifference / currentBlock.Duration;
                double rawPower = currentBlock.TargetPowerStart.Value + (powerPerSecond * secondsInBlock);
                // Arrondir à l'entier le plus proche multiple de 5
                powerToSet = (ushort)(Math.Round(rawPower / 5.0) * 5);
            }
            else
            {
                // It's constant
                powerToSet = currentBlock.TargetPowerStart ?? currentBlock.TargetPowerEnd.Value;
            }
            if (powerToSet != LastPowerSet)
            {
                BluetoothManager.StartControllingPower().Wait();
                BluetoothManager.SetPower(powerToSet).Wait();
                LastPowerSet = powerToSet;
            }
        }
        else
        {
            BluetoothManager.StopControllingPower().Wait();
        }
    }
}
