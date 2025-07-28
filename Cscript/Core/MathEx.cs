using System;
using System.Collections.Generic;
using Godot;
public static class MathEx
{
    public static List<Vector2I> GetLine(Vector2I start, Vector2I end)
    {
        List<Vector2I> line = [];

        int x0 = start.X;
        int y0 = start.Y;
        int x1 = end.X;
        int y1 = end.Y;

        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);

        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        int err = dx - dy;

        while (true)
        {
            line.Add(new Vector2I(x0, y0));

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = 2 * err;

            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        return line;
    }
    public static float[] Linspace(float start, float end, int count)
    {
        var result = new float[count];
        if (count == 1)
        {
            result[0] = start;
            return result;
        }

        float step = (end - start) / (count - 1);
        for (int i = 0; i < count; i++)
        {
            result[i] = start + i * step;
        }
        return result;
    }
    public static Vector2 RandomV2(float radius = 1)
    {
        float a = Mathf.DegToRad(GD.RandRange(0, 360));
        return new Vector2(radius * Mathf.Cos(a), radius * Mathf.Sin(a));
    }
    public static Vector2 AngleV2(float angle, float radius = 1)
    {
        return new Vector2(radius * Mathf.Cos(Mathf.DegToRad(angle)), radius * Mathf.Sin(Mathf.DegToRad(angle)));
    }
    public static List<Vector2I> NearVectors =
        [
            new Vector2I(-1, -1), new Vector2I(0, -1), new Vector2I(1, -1),
            new Vector2I(-1, 0), new Vector2I(1, 0),
            new Vector2I(-1, 1), new Vector2I(0, 1), new Vector2I(1, 1)
        ];
    public static float Randn(double mean, double variance)
    {
        float i;
        do
        {
            i = (float)GD.Randfn(mean, Math.Pow(variance, 2));
        } while (i < 0);
        return i;
    }
}

