using System.Numerics;

namespace VelomGame;

public class Map
{
    public Vector3[] TrackPoints { get; private set; }


    public Map()
    {
        TrackPoints = GenerateTrackPoints(1000);
    }

    private static Vector3[] GenerateTrackPoints(int totalMeters)
    {
        // Dimensions de la piste (en mètres)
        float straightLength = totalMeters/4; // Longueur des lignes droites
        float curveRadius = totalMeters/8; // Rayon des courbes

        // Calcul des segments
        int straightPoints = (int)(straightLength); // Points pour chaque ligne droite
        int curvePoints = (int)((MathF.PI * curveRadius) / 2); // Points pour chaque demi-courbe

        Vector3[] points = new Vector3[straightPoints * 2 + curvePoints * 2];

        // Générer les points pour la première ligne droite
        int index = 0;
        for (int i = 0; i < straightPoints; i++)
        {
            points[index++] = new Vector3(i, 0, 0); // Ligne droite horizontale
        }

        // Générer les points pour la première courbe
        for (int i = 0; i < curvePoints; i++)
        {
            float angle = MathF.PI * i / curvePoints; // Angle en radians
            float x = straightLength + curveRadius * MathF.Sin(angle);
            float z = curveRadius * (1 - MathF.Cos(angle));
            points[index++] = new Vector3(x, 0, z);
        }

        // Générer les points pour la deuxième ligne droite
        for (int i = 0; i < straightPoints; i++)
        {
            points[index++] = new Vector3(straightLength - i, 0, 2 * curveRadius);
        }

        // Générer les points pour la deuxième courbe
        for (int i = 0; i < curvePoints; i++)
        {
            float angle = MathF.PI * i / curvePoints; // Angle en radians
            float x = -curveRadius * MathF.Sin(angle);
            float z = 2 * curveRadius - curveRadius * (1 - MathF.Cos(angle));
            points[index++] = new Vector3(x, 0, z);
        }

        return points;
    }
}
