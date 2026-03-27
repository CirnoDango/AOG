using Godot;
using System;

public partial class Laser : Control
{
    public float Length = 400f;
    [Export] public float Width = 80f;
    [Export] public float GrowTime = 0.15f;   // 激光延伸时间
    [Export] public float LifeTime = 1.0f;    // 激光持续时间
    public Color BeamColor = new Color(1f, 0.2f, 0.2f, 1f); // 主颜色

    private Line2D line;
    private float time;
    private bool active = false;
    private Vector2 direction = Vector2.Right;
    private float currentLength = 0f;

    public override void _Ready()
    {
        line = GetNode<Line2D>("Line2D");
        line.Width = Width;
        line.Gradient = new Gradient();
        line.Points = [Vector2.Zero, Vector2.Zero];
        Visible = false;
    }

    public void Fire(Vector2 offset, Color color, float length)
    {
        direction = offset.Normalized();
        currentLength = 0f;
        time = 0f;
        Length = length;
        BeamColor = color;
        line.Gradient.Colors = 
        [
            BeamColor,
            BeamColor,
        ];
        active = true;
        Visible = true;
    }

    public override void _Process(double delta)
    {
        delta *= 4.8;
        if (!active)
            return;
        time += (float)delta;
        // 延伸阶段
        if (time < GrowTime)
        {
            float t = time / GrowTime;
            currentLength = Mathf.Lerp(0, Length, t);
        }
        else
        {
            currentLength = Length;
        }

        // 更新激光端点
        line.Points =
        [
            Vector2.Zero,
            direction * currentLength
        ];

        // 淡出阶段
        if (time > LifeTime)
        {
            float fade = 1f - (time - LifeTime);
            line.Modulate = new Color(1, 1, 1, Mathf.Clamp(fade, 0, 1));

            if (fade <= 0)
            {
                active = false;
                Visible = false;
            }
        }
    }
}

