using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AddDamage : BarrageComponent
{
    public AddDamage()
    {
        CoolDown = 200;
        SpCost = 6;
    }
    public override void Activate(ref List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
            bc.damage += 5;
    }
}

public class GreatSpread : BarrageComponent
{
    public GreatSpread()
    {
        CoolDown = -300;
    }
    public override void Activate(ref List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
        {
            bc.Angle = (float)GD.RandRange(0.0, 360.0);
        }
    }
}
public class SpeedUp : BarrageComponent
{
    public SpeedUp()
    {
        CoolDown = 100;
        SpCost = 2;
    }
    public override void Activate(ref List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
        {
            bc.Speed *= 1.5f;
        }
    }
}
public class SpeedDown : BarrageComponent
{
    public SpeedDown()
    {
        CoolDown = 100;
        SpCost = 2;
    }
    public override void Activate(ref List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
        {
            bc.Speed *= 0.6f;
        }
    }
}
public class RangeUp : BarrageComponent
{
    public RangeUp()
    {
        CoolDown = 100;
        SpCost = 4;
    }
    public override void Activate(ref List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
        {
            bc.MaxDistance = Math.Min(12, bc.MaxDistance * 1.5f);
        }
    }
}
public class CritUp : BarrageComponent
{
    public CritUp()
    {
        CoolDown = 100;
        SpCost = 6;
    }
    public override void Activate(ref List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
        {
            bc.crit += 0.15f;
        }
    }
}
public class AccUp : BarrageComponent
{
    public AccUp()
    {
        CoolDown = 100;
        SpCost = 6;
    }
    public override void Activate(ref List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
        {
            bc.AfterSummonEvents += Buff;
        }
    }
    public static void Buff(Bullet b)
    {
        b.accuracy += 0.15f;
    }
}