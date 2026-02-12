using Godot;
using System;
using System.Collections.Generic;
public class BulletModule : BarrageComponent
{
    private BulletContext _bulletContext;
    public BulletContext bulletContext
    {
        get => _bulletContext;
        set
        {
            _bulletContext = value;
            Texture = Bullet.ReadImage(bulletContext.Color, bulletContext.Shape).Texture;
        }
    }
    public BulletModule()
    {
        Name = "BulletModule";
        group = "BulletModule";
        SpCost = 5;
        CoolDown = 100;
        draw = 0;
    }
    public override string GetDescription()
    {
        return $"{bulletContext.Color} {bulletContext.Shape}子弹\n" +
            $"伤害：{bulletContext.damage:F1}\n速度：{bulletContext.Speed:F1}\n距离：{bulletContext.MaxDistance:F1}";
    }
    public override void Execute(ref List<BulletContext> lbc, Executor executor)
    {
        var bullet = new BulletContext(bulletContext.damage, bulletContext.Speed, 
            bulletContext.MaxDistance, bulletContext.Shape, bulletContext.Color);
        lbc = [bullet];
        executor.FireOnce(lbc);
        executor.Continue(lbc);
    }
    public override void ApplyParameters(Dictionary<string, object> parameters)
    {
        if (parameters.TryGetValue("bulletContext", out var val))
        {
            bulletContext = (BulletContext)(Dictionary<string, object>)val;
        }
    }

    public override Item RandomSummonParam()
    {
        BulletModule bm = new()
        {
            bulletContext = new(new Damage((float)GD.Randfn(8, 2), (DamageType)GD.RandRange(0, Enum.GetValues(typeof(DamageType)).Length - 1)), (float)Mathf.Pow(2, GD.Randfn(1.2, 0.5)), GD.RandRange(6, 12),
                (ShapeBullet)GD.RandRange(0, 29),
                (ColorBullet)GD.RandRange(0, 19))
        };
        return bm;
    }
}
public class AddDamage : BarrageComponent, IBarrageComponentEvent
{
    public AddDamage()
    {
        Name = "AddDamage";
        CoolDown = 200;
        SpCost = 6;
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
            bc.damage += 5;
    }
}
public class SnakeRoute : BarrageComponent, IBarrageComponentEvent
{
    public SnakeRoute()
    {
        Name = "SnakeRoute";
        SpCost = 2;
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
        {
            bc.Speed *= 2;
            bc.MaxDistance *= 1.5f;
            bc.UpdateEvents += BulletUpdate;
        }
    }
    public static void BulletUpdate(Bullet bullet, float time)
    {
        if (MathEx.Contain(bullet.timeUsed, time, 0))
            bullet.Speed = bullet.Speed.Rotated(45 / 57.3f);
        if((int)(bullet.timeUsed / (100*1.5*3.14/bullet.Speed.Length())) % 2 == 0)
        {
            bullet.Acceleration = bullet.Speed.Length() * bullet.Speed.Length() / 3 * bullet.Speed.Normalized().Rotated(-90 / 57.3f);
        }
        else
        {
            bullet.Acceleration = bullet.Speed.Length() * bullet.Speed.Length() / 3 * bullet.Speed.Normalized().Rotated(+90 / 57.3f);
        }
    }
}

public class ChaosRoute : BarrageComponent, IBarrageComponentEvent
{
    public ChaosRoute()
    {
        Name = "ChaosRoute";
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
        {
            bc.MaxDistance *= 1.5f;
            bc.UpdateEvents += BulletUpdate;
        }
    }
    public static void BulletUpdate(Bullet bullet, float time)
    {
        bullet.Acceleration = MathEx.RandomV2(GD.RandRange(4, 25));
    }
}
public class LeftCircleRoute : BarrageComponent, IBarrageComponentEvent
{
    public LeftCircleRoute()
    {
        Name = "LeftCircleRoute";
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
        {
            bc.Speed *= 1.5f;
            bc.MaxDistance *= 1.5f;
            bc.UpdateEvents += BulletUpdate;
        }
    }
    public static void BulletUpdate(Bullet bullet, float time)
    {
        bullet.Acceleration = bullet.Speed.Length() * bullet.Speed.Length() / 6 * bullet.Speed.Normalized().Rotated(-90 / 57.3f);
    }
}
public class RightCircleRoute : BarrageComponent, IBarrageComponentEvent
{
    public RightCircleRoute()
    {
        Name = "RightCircleRoute";
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
        {
            bc.Speed *= 1.5f;
            bc.MaxDistance *= 1.5f;
            bc.UpdateEvents += BulletUpdate;
        }
    }
    public static void BulletUpdate(Bullet bullet, float time)
    {
        bullet.Acceleration = bullet.Speed.Length() * bullet.Speed.Length() / 6 * bullet.Speed.Normalized().Rotated(+90 / 57.3f);
    }
}
public class GreatSpread : BarrageComponent, IBarrageComponentEvent
{
    public GreatSpread()
    {
        Name = "GreatSpread";
        CoolDown = -300;
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
        {
            bc.Angle = (float)GD.RandRange(0.0, 360.0);
        }
    }
}
public class SpeedUp : BarrageComponent, IBarrageComponentEvent
{
    public SpeedUp()
    {
        Name = "SpeedUp";
        CoolDown = 100;
        SpCost = 2;
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
        {
            bc.Speed *= 1.5f;
        }
    }
}
public class SpeedDown : BarrageComponent, IBarrageComponentEvent
{
    public SpeedDown()
    {
        Name = "SpeedDown";
        CoolDown = 100;
        SpCost = 2;
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
        {
            bc.Speed *= 0.6f;
        }
    }
}
public class RangeUp : BarrageComponent, IBarrageComponentEvent
{
    public RangeUp()
    {
        Name = "RangeUp";
        CoolDown = 100;
        SpCost = 4;
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
        {
            bc.MaxDistance = Math.Min(12, bc.MaxDistance * 1.5f);
        }
    }
}
public class CritUp : BarrageComponent, IBarrageComponentEvent
{
    public CritUp()
    {
        Name = "CritUp";
        CoolDown = 100;
        SpCost = 6;
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
        {
            bc.crit += 0.15f;
        }
    }
}
public class AccUp : BarrageComponent, IBarrageComponentEvent
{
    public AccUp()
    {
        Name = "AccUp";
        CoolDown = 100;
        SpCost = 6;
    }
    public void ApplyTo(ref List<BulletContext> lbc)
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
public class MultiSpeedClone : BarrageComponent, IBarrageComponentEvent
{
    public MultiSpeedClone()
    {
        Name = "MultiSpeedClone";
        SpCost = 5;
        CoolDown = 200;
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        List<BulletContext> nlbc = [];
        foreach (var bc in lbc)
        {
            for (int v = 0; v < 3; v += 1)
            {
                BulletContext nbc = bc.Clone();
                nbc.Speed += v;
                nbc.damage /= 2;
                nlbc.Add(nbc);
            }
        }
        lbc = nlbc;
    }
}
public class Draw2 : BarrageComponent, IBarrageComponentEvent
{
    public Draw2()
    {
        Name = "Draw2";
        CoolDown = 200;
        draw = 2;
        group = "Draw2";
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        return;
    }
}
public class CircleFire : BarrageComponent, IBarrageComponentEvent
{
    public CircleFire()
    {
        Name = "CircleFire";
        SpCost = 5;
        CoolDown = 200;
        group = "Fire";
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        if (lbc.Count == 0) return;
        List<BulletContext> nlbc = [];
        for (float a = 0; a < 360; a += 22.5f)
        {
            BulletContext nbc = lbc[0].Clone();
            nbc.Angle += a;
            nlbc.Add(nbc);
        }
        lbc = nlbc;
    }
}
public class Way3Fire : BarrageComponent, IBarrageComponentEvent
{
    public Way3Fire()
    {
        Name = "Way3Fire";
        SpCost = 3;
        CoolDown = 100;
        group = "Fire";
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        if (lbc.Count == 0) return;
        List<BulletContext> nlbc = [];
        for (int a = -10; a <= 10; a += 10)
        {
            for (float v = 0.5f; v <= 1.25f; v += 0.25f)
            {
                BulletContext nbc = lbc[0].Clone();
                nbc.Angle += a;
                nbc.Speed *= v;
                nlbc.Add(nbc);
            }
        }
        lbc = nlbc;
    }
}



public class RandomFire : BarrageComponent, IBarrageComponentEvent
{
    public RandomFire()
    {
        Name = "RandomFire";
        group = "Fire";
        SpCost = 5;
        CoolDown = 200;
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        if (lbc.Count == 0) return;
        List<BulletContext> nlbc = [];
        for (int i = 0; i < 12; i++)
        {
            BulletContext nbc = lbc[0].Clone();
            nbc.Angle += GD.RandRange(-30, 30);
            nbc.Speed *= (float)GD.RandRange(0.5f, 2f);
            nlbc.Add(nbc);
        }
        lbc = nlbc;
    }
}

public class PyramidFire : BarrageComponent, IBarrageComponentEvent
{
    public PyramidFire()
    {
        Name = "PyramidFire";
        group = "Fire";
        SpCost = 5;
        CoolDown = 200;
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        if (lbc.Count == 0) return;
        List<BulletContext> nlbc = [];
        for (int i = 0; i <= 3; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                BulletContext nbc = lbc[0].Clone();
                nbc.Angle += -8 * i + 16 * j;
                nbc.Speed += 3 - i;
                nlbc.Add(nbc);
            }
        }
        lbc = nlbc;
    }
}