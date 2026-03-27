using Godot;
using System;

public partial class AnimationManager : Node2D, IRegisterToG
{
    [Export] public PackedScene Laser;
    [Export] public PackedScene DamageNumber;
    [Export] public PackedScene Particle;
    [Export] public Control UI;
    public void RegisterToG(G g)
    {
        g.AnimationManager = this;
    }
}

public class Animation
{
    public static Vector2 GridToUI(Vector2 grid)
    {
        return (grid - Player.PlayerUnit.Up.Position + new Vector2(-0.25f, -0.25f)) * 480 + new Vector2(13200, 5400);
    }
    public static void CreateLaser(Vector2 position, Vector2 offset, Color color, float length = 0)
    {
        position = GridToUI(position);
        offset = (Vector2I)offset * 30;
        var laser = G.I.AnimationManager.Laser.Instantiate<Laser>();
        G.I.AnimationManager.UI.AddChild(laser);
        laser.GlobalPosition = position;
        if (length == 0)
            length = offset.Length();
        laser.Fire(offset, color, length * Setting.imagePx);
    }
    public static void CreateDamageNumber(Vector2 position, int damage, Color color)
    {
        position += new Vector2((float)GD.RandRange(-0.25, 0.25), (float)GD.RandRange(-0.25, 0.25));
        position = GridToUI(position);
        var number = G.I.AnimationManager.DamageNumber.Instantiate<DamageNumber>();
        G.I.AnimationManager.UI.AddChild(number);
        number.GlobalPosition = position;
        number.Init(damage, color);
    }
    public static void CreateParticle(Vector2 position, Color color, int number)
    {
        position = GridToUI(position);
        var par = G.I.AnimationManager.Particle.Instantiate<CpuParticles2D>();
        par.Amount = number;
        G.I.AnimationManager.UI.AddChild(par);
        par.GlobalPosition = position;
        par.Color = color;
        par.Emitting = true;
    }
}
