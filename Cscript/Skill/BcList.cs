using Godot;
using System.Collections.Generic;
public class BulletModule : BarrageComponent
{
    public BulletContext bulletContext;
    public readonly static List<string> bulletShapes =
        ["ArrowBig","Grain","Needle","Ring","Small","Square","Star"];
    public BulletModule()
    {
        Name = "BulletModule";
        Weight = 0.3f;
        Description = "子弹";
    }
    public override void Execute(ref List<BulletContext> lbc, Executor executor)
    {
        var bullet = bulletContext;
        executor.FireOnce(lbc);
        lbc = [bullet];
        executor.draw--;
        executor.Continue(lbc);
    }
    public override void ApplyParameters(Dictionary<string, object> parameters)
    {
        bulletContext = (BulletContext)(parameters["bulletContext"]);
    }

    public override object RandomSummonParam()
    {
        BulletContext bc = new((float)GD.Randfn(10, 2), (float)Mathf.Pow(2, GD.Randfn(1, 1)), GD.RandRange(6, 12),
            bulletShapes[GD.RandRange(0, bulletShapes.Count - 1)],
            (ColorBullet)GD.RandRange(0, 15));
        return bc;
    }
}
public class AddDamage : BarrageComponent, IBarrageComponentEvent
{
    public float Bonus = 5;

    public AddDamage()
    {
        Name = "AddDamage";
        Weight = 0.3f;
        Description = $"+{Bonus}伤害";
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
            bc.Damage += Bonus;
    }
    public override void ApplyParameters(Dictionary<string, object> parameters)
    {
        Bonus = (float)(parameters["Bonus"] ?? 0);
    }
    public override object RandomSummonParam()
    {
        AddDamage instance = new()
        {
            Bonus = GD.RandRange(2, 10)
        };
        return instance;
    }
}
public class CircleFire : BarrageComponent, IBarrageComponentEvent
{
    public CircleFire()
    {
        Name = "CircleFire";
        Weight = 0.3f;
        Description = "环形弹";
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        List<BulletContext> nlbc = [];
        foreach (var bc in lbc)
        {
            for (int a = 0; a < 360; a += 30)
            {
                BulletContext nbc = bc.Clone();
                nbc.Angle += a;
                nlbc.Add(nbc);
            }
        }
        lbc = nlbc;
    }
}
public class Way3Fire : BarrageComponent, IBarrageComponentEvent
{
    public Way3Fire()
    {
        Name = "Way3Fire";
        Weight = 0.3f;
        Description = "三向弹";
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        List<BulletContext> nlbc = [];
        foreach (var bc in lbc)
        {
            for (int a = -10; a <= 10; a += 10)
            {
                BulletContext nbc = bc.Clone();
                nbc.Angle += a;
                nlbc.Add(nbc);
            }
        }
        lbc = nlbc;
    }
}

public class MultiSpeedFire : BarrageComponent, IBarrageComponentEvent
{
    public MultiSpeedFire()
    {
        Name = "MultiSpeedFire";
        Weight = 0.3f;
        Description = "差速弹";
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        List<BulletContext> nlbc = [];
        foreach (var bc in lbc)
        {
            for (int v = 0; v < 3; v += 2)
            {
                BulletContext nbc = bc.Clone();
                nbc.Speed += v;
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
        Weight = 0.3f;
        Description = "随机弹";
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        List<BulletContext> nlbc = [];
        foreach (var bc in lbc)
        {
            for (int i = 0; i < 10; i++)
            {
                BulletContext nbc = bc.Clone();
                nbc.Angle += GD.RandRange(-30, 30);
                nbc.Speed *= (float)GD.RandRange(0.5f, 2f);
                nlbc.Add(nbc);
            }
        }
        lbc = nlbc;
    }
}