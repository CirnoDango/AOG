using Godot;
using System.Collections.Generic;

public class CircleFire : BarrageComponent
{
    public CircleFire()
    {
        SpCost = 5;
        CoolDown = 200;
        group = "Fire";
    }
    public override void Activate(ref List<BulletContext> lbc)
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
public class Way3Fire : BarrageComponent
{
    public Way3Fire()
    {
        SpCost = 3;
        CoolDown = 100;
        group = "Fire";
    }
    public override void Activate(ref List<BulletContext> lbc)
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



public class RandomFire : BarrageComponent
{
    public RandomFire()
    {
        group = "Fire";
        SpCost = 5;
        CoolDown = 200;
    }
    public override void Activate(ref List<BulletContext> lbc)
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

public class PyramidFire : BarrageComponent
{
    public PyramidFire()
    {
        group = "Fire";
        SpCost = 5;
        CoolDown = 200;
    }
    public override void Activate(ref List<BulletContext> lbc)
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
