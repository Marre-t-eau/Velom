using System.Numerics;

namespace VelomGame.Rendering;

public class Camera : IDrawable
{
    public Vector3 Position { get; set; }
    public Vector3 Target { get; set; }
    public Vector3 Up { get; set; }

    public float FieldOfView { get; set; } = MathF.PI / 4; // 45 degrés
    public float AspectRatio { get; set; } = 1.0f; // Par défaut, carré
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 100.0f;

    public Scene Scene { get; init; }

    public Camera(Scene scene)
    {
        Up = Vector3.UnitY;
        Scene = scene;
        UpdateCamera();
    }

    public void UpdateCamera()
    {
        // Calculate the backward vector based on the player's rotation
        Vector3 extract = Vector3.Transform(new Vector3(-3, 2, 0), Scene.Player.Rotation);
        // Position = Scene.Player.Position + extract + Vector3.UnitY;
        Position = Scene.Player.Position + extract;
        Vector3 extractWithoutY = Vector3.Transform(new Vector3(-2, 0, 0), Scene.Player.Rotation);
        Target = Scene.Player.Position - extractWithoutY;
    }

    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(Position, Target, Up);
    }

    public Matrix4x4 GetProjectionMatrix()
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);
    }

    public Vector3 Project(Vector3 worldPosition, double canvasWidth, double canvasHeight)
    {
        // Matrices de transformation
        Matrix4x4 viewMatrix = GetViewMatrix();
        Matrix4x4 projectionMatrix = GetProjectionMatrix();

        // Transformation en coordonnées caméra
        Vector3 viewPosition = Vector3.Transform(worldPosition, viewMatrix);

        // Transformation en coordonnées projetées
        Vector3 projectedPosition = Vector3.Transform(viewPosition, projectionMatrix);

        // Normalisation des coordonnées homogènes
        if (projectedPosition.Z != 0)
        {
            projectedPosition.X /= projectedPosition.Z;
            projectedPosition.Y /= projectedPosition.Z;
        }

        // Conversion en coordonnées écran
        double screenX = (projectedPosition.X + 1) * 0.5f * canvasWidth;
        double screenY = (1 - projectedPosition.Y) * 0.5f * canvasHeight;

        return new Vector3((float)screenX, (float)screenY, projectedPosition.Z);
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        AspectRatio = dirtyRect.Width / dirtyRect.Height;

        // Draw the track points
        int indice = 0;
        foreach (var point in Scene.Map.TrackPoints)
        {
            // Projeter le point de la piste
            Vector3 projectedPoint = Project(point, dirtyRect.Width, dirtyRect.Height);

            if (projectedPoint.Z <= 0)
                continue;

            // Dessiner le point de la piste comme un petit cercle
            canvas.FillColor = indice%2 == 0 ? Colors.Green : Colors.Blue;
            canvas.FillCircle(projectedPoint.X, projectedPoint.Y, 5);
            indice++;
        }

        // Matrices de transformation
        Matrix4x4 viewMatrix = GetViewMatrix();
        Matrix4x4 projectionMatrix = GetProjectionMatrix();

        float canvasWidth = dirtyRect.Width;
        float canvasHeight = dirtyRect.Height;

        // Draw each object
        foreach (Object3D obj in Scene._objects)
        {
            // List to store projected vertices
            List<Vector3> projectedVertices = new List<Vector3>();

            // Project all vertices of the object
            foreach (var vertex in obj.Vertices)
            {
                // Apply rotation
                Vector3 rotatedVertex = Vector3.Transform(vertex, obj.Rotation);

                // Apply scaling
                Vector3 scaledVertex = rotatedVertex * obj.Size;

                // Transform to camera coordinates
                Vector3 viewPosition = Vector3.Transform(scaledVertex + obj.Position, viewMatrix);

                // Transform to projected coordinates
                Vector3 projectedPosition = Vector3.Transform(viewPosition, projectionMatrix);

                // Normalize homogeneous coordinates
                if (projectedPosition.Z != 0)
                {
                    projectedPosition.X /= projectedPosition.Z;
                    projectedPosition.Y /= projectedPosition.Z;
                }

                // Convert to screen coordinates
                float screenX = (projectedPosition.X + 1) * 0.5f * canvasWidth;
                float screenY = (1 - projectedPosition.Y) * 0.5f * canvasHeight;

                projectedVertices.Add(new Vector3(screenX, screenY, projectedPosition.Z));
            }

            // Draw the faces of the object
            foreach (var face in obj.Faces)
            {
                // Retrieve the projected vertices for this face
                var faceVertices = face.Select(index => projectedVertices[index]).ToList();

                // Create a path for the face
                var path = new PathF();
                path.MoveTo(faceVertices[0].X, faceVertices[0].Y);

                for (int i = 1; i < faceVertices.Count; i++)
                {
                    path.LineTo(faceVertices[i].X, faceVertices[i].Y);
                }

                // Close the path to form a polygon
                path.Close();

                // Fill the face with a semi-transparent color
                canvas.FillColor = obj.Color.WithAlpha(0.5f);
                canvas.FillPath(path);

                // Draw the edges of the face
                canvas.StrokeColor = obj.Color;
                canvas.StrokeSize = 1;
                canvas.DrawPath(path);
            }
        }
    }
}
