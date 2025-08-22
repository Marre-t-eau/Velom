using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace VelomMonoGame.Core.Sources;

internal class Map
{
    internal List<Vector3> TrackPoints { get; init; } = [];

    private GraphicsDevice GraphicsDevice { get; }

    public Map(GraphicsDevice graphicsDevice)
    {
        GraphicsDevice = graphicsDevice;
        // By default we make a track with 2 lines of 1000 points each and a semi circle to attach them.

        // Create the first line of points
        for (int i = 0; i < 1000; i++)
        {
            TrackPoints.Add(new Vector3(i, 0, 0));
        }

        float radius = 1000 / MathHelper.TwoPi;

        // Calculate the first middle point of the demi-circle
        Vector3 middlePointDemiCircle = new Vector3(1000, 0, radius);

        // Draw a circle with the middlePointDemiCircle as center and the radius calculated above.
        for (int i = 0; i < 500; i++)
        {
            float angle = -MathHelper.PiOver2 + MathHelper.Pi * i / 499;
            TrackPoints.Add(new Vector3(
                middlePointDemiCircle.X + radius * (float)Math.Cos(angle),
                0,
                middlePointDemiCircle.Z + radius * (float)Math.Sin(angle)
            ));
        }

        // Create the second line of points
        for (int i = 1; i < 1000; i++)
        {
            TrackPoints.Add(new Vector3(1000 - i, 0, radius * 2));
        }

        // Calculate the second middle point of the demi-circle
        middlePointDemiCircle = new Vector3(0, 0, radius);
        // Draw a circle with the middlePointDemiCircle as center and the radius calculated above.
        for (int i = 0; i < 500; i++)
        {
            float angle = MathHelper.PiOver2 + MathHelper.Pi * i / 499;
            TrackPoints.Add(new Vector3(
                middlePointDemiCircle.X + radius * (float)Math.Cos(angle),
                0,
                middlePointDemiCircle.Z + radius * (float)Math.Sin(angle)
            ));
        }
    }

    public void Draw()
    {
        // Draw a line between track points
        for (int i = 0; i < TrackPoints.Count - 1; i++)
        {
            GraphicsDevice.DrawUserPrimitives(
                PrimitiveType.LineList,
                new[]
                {
                    new VertexPositionColor(TrackPoints[i], i % 2 == 0 ? Color.Red : Color.Green),
                    new VertexPositionColor(TrackPoints[i + 1], i % 2 == 0 ? Color.Red : Color.Green)
                },
                0,
                1
            );
        }
    }
}
