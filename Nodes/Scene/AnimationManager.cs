using Godot;
using System;

public partial class AnimationManager : Node2D, IRegisterToG
{
    [Export] public PackedScene LaserScene;
    public void RegisterToG(G g)
    {
        g.AnimationManager = this;
    }
}

public class Animation
{
    public static void ShootLaser(Vector2 position, Vector2 offset, Color color, float length = 0)
    {
        var laser = G.I.AnimationManager.LaserScene.Instantiate<Laser>();
        Root.rootnode.AddChild(laser);
        laser.GlobalPosition = Setting.imagePx * Setting.rootnodeScale * position;
        if (length == 0)
            length = offset.Length();
        laser.Fire(offset, color, length * Setting.imagePx);
    }
}
