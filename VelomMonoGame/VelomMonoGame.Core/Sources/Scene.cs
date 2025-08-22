using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace VelomMonoGame.Core.Sources;

internal class Scene
{
    private const float bikeScale = 0.005f; // Ajustez si besoin

    private BasicEffect _effect;
    Bike Bike { get; }
    Map Map { get; }
    private Matrix _bikeWorld;
    private Vector3 _cameraPosition;
    private Vector3 _cameraTarget;

    private GraphicsDevice GraphicsDevice { get; }

    internal Scene(GraphicsDevice graphicsDevice, Bike bike)
    {
        Bike = bike;

        _effect = new BasicEffect(graphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = false
        };

        GraphicsDevice = graphicsDevice;
        Map = new Map(graphicsDevice);

        // --- Positionnement et orientation du vélo ---
        Vector3 start = Map.TrackPoints[0];
        Vector3 next = Map.TrackPoints[1];
        Vector3 direction = Vector3.Normalize(next - start);
        float angleY = (float)Math.Atan2(direction.Z, direction.X);

        _bikeWorld = Matrix.CreateTranslation(start)
            * Matrix.CreateRotationY(angleY)
            * Matrix.CreateScale(bikeScale);

        // Caméra alignée derrière et au-dessus le vélo
        Vector3 cameraOffset = new Vector3(-5, 2, 0);
        Matrix rotation = Matrix.CreateRotationY(angleY);
        _cameraPosition = start + Vector3.Transform(cameraOffset, rotation);
        _cameraTarget = start;
    }

    internal void Draw()
    {
        // Matrices de transformation
        _effect.World = Matrix.Identity;
        _effect.View = Matrix.CreateLookAt(
            _cameraPosition,
            _cameraTarget,
            Vector3.Up
        );
        _effect.Projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4,
            GraphicsDevice.Viewport.AspectRatio,
            0.1f, 100f);

        _effect.CurrentTechnique.Passes[0].Apply();

        // Calcul de la position actuelle du vélo
        int idx = (int)Bike.Distance;
        float t = Bike.Distance - idx;
        int maxIdx = Map.TrackPoints.Count - 2;
        idx %= maxIdx;
        Vector3 posA = Map.TrackPoints[idx];
        Vector3 posB = Map.TrackPoints[idx + 1];
        Vector3 pos = Vector3.Lerp(posA, posB, t);

        // --- Grille centrée sur le vélo ---
        List<VertexPositionColor> gridLines = new();
        int gridMin = -10, gridMax = 10;
        Color gridColor = Color.LightGray;

        for (int z = gridMin; z <= gridMax; z++)
        {
            gridLines.Add(new VertexPositionColor(new Vector3(gridMin, 0, z) + pos, gridColor));
            gridLines.Add(new VertexPositionColor(new Vector3(gridMax, 0, z) + pos, gridColor));
        }
        for (int x = gridMin; x <= gridMax; x++)
        {
            gridLines.Add(new VertexPositionColor(new Vector3(x, 0, gridMin) + pos, gridColor));
            gridLines.Add(new VertexPositionColor(new Vector3(x, 0, gridMax) + pos, gridColor));
        }

        GraphicsDevice.DrawUserPrimitives(
            PrimitiveType.LineList,
            gridLines.ToArray(),
            0,
            gridLines.Count / 2
        );
        // Display the Map
        Map.Draw();

        // --- DESSIN DU MODELE ---
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.RasterizerState = RasterizerState.CullNone;

        // Direction pour l'orientation
        Vector3 dir = Vector3.Normalize(posB - posA);
        float angleY = -(float)Math.Atan2(dir.Z, dir.X);

        _bikeWorld = Matrix.CreateScale(bikeScale)
            * Matrix.CreateRotationY(angleY)
            * Matrix.CreateTranslation(pos);
        _effect.World = _bikeWorld;

        foreach (ModelMesh mesh in Bike.Model.Meshes)
        {
            foreach (BasicEffect effect in mesh.Effects)
            {
                effect.EnableDefaultLighting();
                effect.World = _effect.World;
                effect.View = _effect.View;
                effect.Projection = _effect.Projection;
            }
            mesh.Draw();
        }

        Vector3 cameraOffset = new Vector3(-5, 2, 0);
        Matrix rotation = Matrix.CreateRotationY(angleY);
        _cameraPosition = pos + Vector3.Transform(cameraOffset, rotation);
        _cameraTarget = pos;
    }
}
