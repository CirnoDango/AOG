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
    public override void Activate(ref List<BulletContext> lbc)
    {
        var bullet = new BulletContext(bulletContext.damage, bulletContext.Speed,
            bulletContext.MaxDistance, bulletContext.Shape, bulletContext.Color);
        lbc = [bullet];
    }
    public override void ApplyParameters(Dictionary<string, object> parameters)
    {
        base.ApplyParameters(parameters);
        if (parameters.TryGetValue("bulletContext", out var val))
        {
            bulletContext = (BulletContext)(Dictionary<string, object>)val;
        }
    }
    public override void GetParam()
    {
        Params = new Dictionary<string, object>
        {
            {
                "bulletContext",
                new Dictionary<string, object>
                {
                    {"damage", bulletContext.damage.Value},
                    {"speed", bulletContext.Speed},
                    {"maxDistance", bulletContext.MaxDistance},
                    {"shape", bulletContext.Shape},
                    {"type", bulletContext.damage.Type},
                    {"color", bulletContext.Color},
                }
            }
        };
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
