using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class StaticShape : BarrageComponent
{
    public StaticShape()
    {
        SpCost = 4;
    }
    public override void Activate(ref List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
        {
            bc.Offset = bc.Speed * Vector2.Right.Rotated(MathEx.Deg2Rad(bc.Angle));
        }
        float mAngle = lbc.Select(x=>x.Angle).Average();
        float mSpeed = lbc.Select(x=>x.Speed).Average();
        foreach (var bc in lbc)
        {
            bc.Angle = mAngle;
            bc.Speed = mSpeed;
        }
    }
}
