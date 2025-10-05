using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using VelomMonoGame.Core.Sources.InterfaceElements;
using VelomMonoGame.Core.Sources.Objects;
using VelomMonoGame.Core.Sources.Tools;

namespace VelomMonoGame.Core.Sources.Pages;

internal class WorkoutEditPage : IPage
{
    public Vector2 Size { get; set; }
    public List<IElement> Elements { get; set; } = [];

    private VelomMonoGameGame Game { get; }
    private Workout Workout { get; set; }

    private int BlockStartIndex { get; set; } = 0;

    ConfirmationDialog ConfirmationDialog { get; set; }
    List<IElement> BlockElements = [];

    internal WorkoutEditPage(VelomMonoGameGame game, Workout workout, string fileName)
    {
        Game = game;
        Size = game.Page.Size;
        Workout = workout;

        Button cancelButton = Button.CreateButtonWithText("Cancel", Color.White, Color.Red, () =>
        {
            // If there are unsaved changes, show a confirmation dialog
            if (ConfirmationDialog == null)
            {
                ConfirmationDialog = new ConfirmationDialog("Discard changes?", Size / 2, () =>
                {
                    Game.Page = new WorkoutListPage(Game, Size);
                },
                onCancel: () =>
                {
                    Elements.Remove(ConfirmationDialog);
                    ConfirmationDialog = null;
                });
                ConfirmationDialog.Position = new Vector2(
                    Size.X / 2 - ConfirmationDialog.Size.X / 2,
                    Size.Y / 2 - ConfirmationDialog.Size.Y / 2
                );
                Elements.Add(ConfirmationDialog);
            }
        });
        cancelButton.Position = new Vector2(10, Size.Y - cancelButton.Size.Y - 10);
        Elements.Add(cancelButton);


        Button deleteButton = Button.CreateButtonWithText("Delete", Color.White, Color.Red, () =>
        {
            // Affiche une confirmation simple
            ConfirmationDialog = new ConfirmationDialog(
                "Do you want to delete this workout?",
                Size / 2,
                onConfirm: () =>
                {
                    if (!string.IsNullOrEmpty(fileName))
                        SaveManager.DeleteWorkout(fileName);
                    Game.Page = new WorkoutListPage(Game, Size);
                },
                onCancel: () =>
                {
                    // Retire la boîte de dialogue
                    Elements.Remove(ConfirmationDialog);
                    ConfirmationDialog = null;
                }
            );
            ConfirmationDialog.Position = new Vector2(
                Size.X / 2 - ConfirmationDialog.Size.X / 2,
                Size.Y / 2 - ConfirmationDialog.Size.Y / 2
            );
            Elements.Add(ConfirmationDialog);
        });
        deleteButton.Position = new Vector2(10, 10);
        Elements.Add(deleteButton);

        Button saveButton = Button.CreateButtonWithText("Save", Color.White, Color.Green, () =>
        {
            SaveManager.SaveWorkout(workout, fileName);
            Game.Page = new WorkoutListPage(Game, Size);
        });
        saveButton.Position = new Vector2(Size.X - saveButton.Size.X - 10, Size.Y - saveButton.Size.Y - 10);
        Elements.Add(saveButton);

        Text nameText = new Text
        {
            TextContent = "Name : ",
            Color = Color.Black,
            Position = new Vector2(Size.X / 3, 100)
        };
        Elements.Add(nameText);

        TextBox nameTextBox = new TextBox
        {
            Position = new Vector2(nameText.Position.X + nameText.Size.X, nameText.Position.Y),
            Text = Workout.Name ?? string.Empty,
            MaxLength = 32,
            OnTextChanged = s => Workout.Name = s
        };
        Elements.Add(nameTextBox);

        Button addBlockButton = Button.CreateButtonWithText("Add Block", Color.White, Color.Purple, () =>
        {
            Workout.Blocks.Add(new WorkBlock());
            RefreshBlocks();
        });
        addBlockButton.Position = new Vector2(Size.X - 10 - addBlockButton.Size.X, Size.Y / 2 - addBlockButton.Size.Y / 2);
        Elements.Add(addBlockButton);

        // Bouton scroll haut
        Button upButton = Button.CreateButtonWithText("^", Color.White, Color.Gray, null);
        upButton.Position = new Vector2(Size.X / 8 * 7 - upButton.Size.X, 250);
        Elements.Add(upButton);

        // Bouton scroll bas
        Button downButton = Button.CreateButtonWithText("v", Color.White, Color.Purple, null);
        downButton.Position = new Vector2(Size.X / 8 * 7 - downButton.Size.X, Size.Y - downButton.Size.Y - 250);
        Elements.Add(downButton);

        upButton.OnClick = () =>
        {
            if (BlockStartIndex > 0)
            {
                BlockStartIndex--;
                RefreshBlocks();
                if (BlockStartIndex > 0)
                {
                    downButton.Background = TextureBank.GetTextureColor(Color.Purple);
                }
                else
                {
                    upButton.Background = TextureBank.GetTextureColor(Color.Gray);
                }
            }
        };
        downButton.OnClick = () =>
        {
            upButton.Background = TextureBank.GetTextureColor(Color.Purple);
            if (BlockStartIndex < Workout.Blocks.Count - 1)
            {
                BlockStartIndex++;
                RefreshBlocks();
                if (BlockStartIndex < Workout.Blocks.Count - 1)
                {
                    downButton.Background = TextureBank.GetTextureColor(Color.Purple);
                }
                else
                {
                    downButton.Background = TextureBank.GetTextureColor(Color.Gray);
                }
            }
        };

        RefreshBlocks();
    }

    private void RefreshBlocks()
    {
        foreach (IElement element in BlockElements)
        {
            Elements.Remove(element);
        }
        float y = 250;
        for (int i = BlockStartIndex; i < Workout.Blocks.Count; i++)
        {
            WorkBlock workBlock = Workout.Blocks[i];
            Button deleteButton = Button.CreateButtonWithText("Delete", Color.White, Color.Red, () => { Workout.Blocks.Remove(workBlock);RefreshBlocks(); });
            deleteButton.Position = new Vector2(Size.X / 8, y);
            BlockElements.Add(deleteButton);
            Elements.Add(deleteButton);
            Text durationText = new Text { Color = Color.Black, TextContent = "Duration: " };
            durationText.Position = new Vector2(deleteButton.Position.X + deleteButton.Size.X + 100, (deleteButton.Elements[0] as Text).Position.Y);
            BlockElements.Add(durationText);
            Elements.Add(durationText);
            TextBox durationInput = new TextBox { MaxLength = 3, Text = workBlock.Duration.ToString(), IsDigitOnly = true, OnTextChanged = (number) => { workBlock.Duration = uint.Parse(number); } };
            durationInput.Position = new Vector2(durationText.Position.X + durationText.Size.X, durationText.Position.Y);
            BlockElements.Add(durationInput);
            Elements.Add(durationInput);
            Text powerText = new Text { Color = Color.Black, TextContent = " Power: " };
            powerText.Position = new Vector2(durationInput.Position.X + durationInput.Size.X, durationText.Position.Y);
            BlockElements.Add(powerText);
            Elements.Add(powerText);
            TextBox powerStart = new TextBox { MaxLength = 3, Text = workBlock.TargetPowerStart.ToString(), IsDigitOnly = true, OnTextChanged = (number) => { workBlock.TargetPowerStart = ushort.Parse(number); } };
            powerStart.Position = new Vector2(powerText.Position.X + powerText.Size.X, durationText.Position.Y);
            BlockElements.Add(powerStart);
            Elements.Add(powerStart);
            Text toPowerText = new Text { Color = Color.Black, TextContent = " - " };
            toPowerText.Position = new Vector2(powerStart.Position.X + powerStart.Size.X, durationText.Position.Y);
            BlockElements.Add(toPowerText);
            Elements.Add(toPowerText);
            TextBox powerEnd = new TextBox { MaxLength = 3, Text = workBlock.TargetPowerEnd.ToString(), IsDigitOnly = true, OnTextChanged = (number => { workBlock.TargetPowerEnd = ushort.Parse(number); }) };
            powerEnd.Position = new Vector2(toPowerText.Position.X + toPowerText.Size.X, durationText.Position.Y);
            BlockElements.Add(powerEnd);
            Elements.Add(powerEnd);
            Button typePowerButton = Button.CreateButtonWithText(workBlock.PowerType == TargetPowerType.PercentFTP ? "%" : "W", Color.White, Color.Purple, null);
            typePowerButton.OnClick = () =>
            {
                workBlock.PowerType = workBlock.PowerType == TargetPowerType.PercentFTP ? TargetPowerType.Watts : TargetPowerType.PercentFTP;
                (typePowerButton.Elements[0] as Text).TextContent = workBlock.PowerType == TargetPowerType.PercentFTP ? "%" : "W";
            };
            typePowerButton.Position = new Vector2(powerEnd.Position.X + powerEnd.Size.X + 10, y);
            BlockElements.Add(typePowerButton);
            Elements.Add(typePowerButton);
            Text cadenceText = new Text { Color = Color.Black, TextContent = " Cadence: " };
            cadenceText.Position = new Vector2(typePowerButton.Position.X + typePowerButton.Size.X, durationText.Position.Y);
            BlockElements.Add(cadenceText);
            Elements.Add(cadenceText);
            TextBox cadenceInput = new TextBox { MaxLength = 3, Text = workBlock.TargetCadence.ToString(), IsDigitOnly = true, OnTextChanged = (number) => { workBlock.TargetCadence = ushort.Parse(number); } };
            cadenceInput.Position = new Vector2(cadenceText.Position.X + cadenceText.Size.X, durationText.Position.Y);
            BlockElements.Add(cadenceInput);
            Elements.Add(cadenceInput);
            y += typePowerButton.Size.Y + 50;
        }
    }

    public void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        spriteBatch.Begin();
        foreach (var element in Elements)
        {
            if (element is IDrawableElement drawable)
                drawable.Draw(spriteBatch);
        }
        spriteBatch.End();
    }

    public void Update(GameTime gameTime)
    {
        List<IElement> elementsCopy = new List<IElement>(Elements);
        foreach (var element in elementsCopy)
        {
            if (element is IUpdatableElement updatable)
                updatable.Update();
        }
    }
}
