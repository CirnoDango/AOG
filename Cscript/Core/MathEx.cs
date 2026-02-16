using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public static float Randfna(double mean, double variance)
    {
        return (float)(mean + Math.Abs(GD.Randfn(0, variance)));
    }
    public static float Logistic(float zero, float x)
    {
        if(x > 0)
            return (float)(2 * (1 - zero) / (1 + Math.Exp(-2 * x / (1 - zero))) + 2 * (zero - 0.5));
        else
            return (float)(2 * zero / (1 + Math.Exp(-2 * x / zero)));
    }
    public static float HalfLogistic(float x)
    {
        if (x > 0)
            return 1 + x;
        else
            return (float)(2 / (1 + Math.Exp(-2 * x)));
    }
    public static bool Contain(float t, float dt, float et)
    {
        return et >= t && et < t + dt;
    }
    public static Dictionary<string, object> ConvertLong(Dictionary<string, object> dict)
    {
        var keys = dict.Keys.ToList();

        foreach (var key in keys)
        {
            dict[key] = FixValue(dict[key]);
        }

        return dict;
    }

    private static object FixValue(object value)
    {
        if (value == null)
            return null;

        // Int64 → Int32
        if (value is long l)
            return checked((int)l);

        // Double → Float
        if (value is double d)
            return (float)d;

        // Dictionary → 递归
        if (value is Dictionary<string, object> subDict)
            return ConvertLong(subDict);

        // List → 递归
        if (value is List<object> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = FixValue(list[i]);
            }
            return list;
        }

        return value;
    }
}

