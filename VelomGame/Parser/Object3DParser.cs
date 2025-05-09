using System.Globalization;
using System.Numerics;

namespace VelomGame.Parser;

public static class Object3DParser
{
    public static Object3D? Parse(string objectFileName, float size, Color color)
    {
        Stream objectFileStream = FileSystem.OpenAppPackageFileAsync($"Assets/3DModels/{objectFileName}.obj").Result;

        if (objectFileStream == null)
            return null;

        using StreamReader reader = new StreamReader(objectFileStream);

        // Listes pour stocker les sommets et les faces
        List<Vector3> vertices = new List<Vector3>();
        List<int[]> faces = new List<int[]>();

        string? line;
        // Lire le fichier ligne par ligne
        while ((line = reader.ReadLine()) != null)
        {
            // Ignorer les lignes vides ou les commentaires
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            // Découper la ligne en tokens
            var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Identifier le type de ligne
            switch (tokens[0])
            {
                case "v": // Définition d'un sommet
                    vertices.Add(ParseVertex(tokens));
                    break;

                case "f": // Définition d'une face
                    faces.Add(ParseFace(tokens));
                    break;

                default:
                    // Ignorer les autres types de lignes
                    break;
            }
        }

        // Calculer la position moyenne des sommets pour définir la position de l'objet
        Vector3 position = CalculateCenter(vertices);

        // Créer un Object3D avec les données extraites
        Object3D object3D = new Object3D(position, size, color);
        object3D.Vertices.AddRange(vertices);
        object3D.Faces.AddRange(faces);

        return object3D;
    }

    private static Vector3 ParseVertex(string[] tokens)
    {
        // Les coordonnées des sommets sont définies après le "v"
        float x = float.Parse(tokens[1], CultureInfo.InvariantCulture);
        float y = float.Parse(tokens[2], CultureInfo.InvariantCulture);
        float z = float.Parse(tokens[3], CultureInfo.InvariantCulture);

        return new Vector3(x, y, z);
    }

    private static int[] ParseFace(string[] tokens)
    {
        // Les indices des sommets sont définis après le "f"
        // Exemple : "f 1 2 3" ou "f 1/1 2/2 3/3"
        return tokens.Skip(1)
                     .Select(t => int.Parse(t.Split('/')[0]) - 1) // Convertir en index 0-based
                     .ToArray();
    }

    private static Vector3 CalculateCenter(List<Vector3> vertices)
    {
        if (vertices.Count == 0)
            return Vector3.Zero;

        float x = vertices.Average(v => v.X);
        float y = vertices.Average(v => v.Y);
        float z = vertices.Average(v => v.Z);

        return new Vector3(x, y, z);
    }
}
