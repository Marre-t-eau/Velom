

namespace VelomGame.Drawables;

public class TextDrawable : IDrawable
{
    public string Text { get; set; } = string.Empty;

    void IDrawable.Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.SaveState();
        canvas.FontColor = Colors.White;
        canvas.FontSize = 20;
        canvas.DrawString(Text, dirtyRect, HorizontalAlignment.Center, VerticalAlignment.Center);
        canvas.RestoreState();
    }
}
