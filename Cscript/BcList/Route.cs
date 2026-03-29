using Godot;
using System.Collections.Generic;

public class SnakeRoute : BarrageComponent
{
    public SnakeRoute()
    {
        SpCost = 2;
    }
    public override void Activate(ref List<BulletContext> lbc)
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
            bullet.Speed = bullet.Speed.Rotated(MathEx.Deg2Rad(45));
        if ((int)(bullet.timeUsed / (100 * 1.5 * 3.14 / bullet.Speed.Length())) % 2 == 0)
        {
            bullet.Acceleration = bullet.Speed.Length() * bullet.Speed.Length() / 3 * bullet.Speed.Normalized().Rotated(MathEx.Deg2Rad(-90));
        }
        else
        {
            bullet.Acceleration = bullet.Speed.Length() * bullet.Speed.Length() / 3 * bullet.Speed.Normalized().Rotated(MathEx.Deg2Rad(+90));
        }
    }
}

public class ChaosRoute : BarrageComponent
{
    public ChaosRoute()
    {
    }
    public override void Activate(ref List<BulletContext> lbc)
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
public class LeftCircleRoute : BarrageComponent
{
    public LeftCircleRoute()
    {
    }
    public override void Activate(ref List<BulletContext> lbc)
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
        bullet.Acceleration = bullet.Speed.Length() * bullet.Speed.Length() / 6 * bullet.Speed.Normalized().Rotated(MathEx.Deg2Rad(-90));
    }
}
public class RightCircleRoute : BarrageComponent
{
    public RightCircleRoute()
    {
    }
    public override void Activate(ref List<BulletContext> lbc)
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
        bullet.Acceleration = bullet.Speed.Length() * bullet.Speed.Length() / 6 * bullet.Speed.Normalized().Rotated(MathEx.Deg2Rad(+90));
    }
}
