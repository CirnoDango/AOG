using Godot;
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
        Weight = 0.3f;
        Description = $"bullet";
    }
    public override string GetDescription()
    {
        return $"{bulletContext.Color} {bulletContext.Shape}子弹。" +
            $"伤害：{bulletContext.damage:F1}, 速度：{bulletContext.Speed:F1}, 范围：{bulletContext.MaxDistance:F1}";
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
        if (parameters.TryGetValue("bulletContext", out var val))
        {
            bulletContext = (BulletContext)(Dictionary<string, object>)val;
        }
    }

    public override Item RandomSummonParam()
    {
        BulletModule bm = new()
        {
            bulletContext = new((float)GD.Randfn(8, 2), (float)Mathf.Pow(2, GD.Randfn(1.2, 0.5)), GD.RandRange(6, 12),
                (ShapeBullet)GD.RandRange(0, 39),
                (ColorBullet)GD.RandRange(0, 19))
        };
        return bm;
    }
}
public class AddDamage : BarrageComponent, IBarrageComponentEvent
{
    public float Bonus = 999;

    public AddDamage()
    {
        Name = "AddDamage";
        Weight = 0.3f;
    }
    public override string GetDescription()
    {
        return $"+{Bonus}伤害";
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
            bc.damage += Bonus;
    }
    public override void ApplyParameters(Dictionary<string, object> parameters)
    {
        if (parameters.TryGetValue("Bonus", out var val))
        {
            Bonus = (float)val;
        }
    }
    public override Item RandomSummonParam()
    {
        Bonus = GD.RandRange(2, 10);
        return this;
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