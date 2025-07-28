using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Rest : Skill
{
    public Rest()
    {
        Name = "Rest";
        Description = "休息一回合";
        Targeting = new TargetType(Target.Self);
    }

    protected override void StartActivate(SkillContext context)
    {
    }
}
public class Move : Skill
{
    public Move()
    {
        Name = "Move";
        Description = "移动一个格子";
        Targeting = new TargetType(Target.Grid, 1, 1);
    }

    protected override void StartActivate(SkillContext sc)
    {
        sc.User.MoveTo(sc.GridOne);
    }
}
public class Interact : Skill
{
    public Interact()
    {
        Name = "Interact";
        Description = "与场景环境互动";

        Targeting = new TargetType(Target.Grid, 1, 0);
    }
    public override float GetTimeCost(int level) => 0;
    protected override void StartActivate(SkillContext sc)
    {
        if (sc.GridOne.InteractableObjects.Count == 0)
        {
            Info.Print($"{sc.User.TrName} 没有可以互动的对象。");
            return;
        }
        foreach(IInteractable ii in sc.GridOne.InteractableObjects.ToList())
        {
            ii.Interact(sc.User);
        }
    }
}

public class Attack : Skill
{
    public Attack()
    {
        Name = "Attack";
        Description = "体术攻击";
        Targeting = new TargetType(Target.Enemy, 1, 1);
    }

    protected override void StartActivate(SkillContext sc)
    {
        float damage = 10 + sc.User.Ua.Str - sc.UnitOne.Ua.Dex;
        sc.UnitOne.CheckBodyHit(damage, sc.User, this);
    }
}
public class Shoot : Skill
{
    public Shoot()
    {
        Name = "Shoot";
        SkillGroup = "General";
        Description = "发射弹幕！";
        Targeting = new TargetType(Target.Grid, 1, 8);
    }
    public override float GetSpCost(int level) => 0;
    public override float GetCooldown(int level) => 300;
    protected override void StartActivate(SkillContext sc)
    {
        _ = new Bullet(sc.User, this, 6, sc.User.Position, sc.GridOne.Position, 1, 8, "Small", ColorBullet.DeepGreen);
        _ = new Bullet(sc.User, this, 6, sc.User.Position, sc.GridOne.Position, 1.5f, 8, "Small", ColorBullet.DeepGreen);
        _ = new Bullet(sc.User, this, 6, sc.User.Position, sc.GridOne.Position, 2, 8, "Small", ColorBullet.DeepGreen);
    }
}

public class DiyShoot : Skill
{
    private List<BulletParams> bulletParamsList;

    public DiyShoot(string name, List<BulletParams> bullets)
    {
        Name = name;
        SkillGroup = "General";
        Description = "自定义发射弹幕";
        Targeting = new TargetType(Target.Grid, 1, 10);

        bulletParamsList = bullets;
    }
    public override float GetSpCost(int level) => 0;
    public override float GetCooldown(int level) => 300;
    protected override void StartActivate(SkillContext sc)
    {
        foreach (var p in bulletParamsList)
        {
            switch (p.Mode)
            {
                case BulletParams.ModeType.Basic:
                    _ = new Bullet(sc.User, this, p.Damage, sc.User.Position, sc.GridOne.Position, p.Speed, p.MaxDistance, p.Shape, p.Color, p.Advance);
                    break;

                case BulletParams.ModeType.OffsetVector:
                    _ = new Bullet(sc.User, this, p.Damage, sc.User.Position, sc.GridOne.Position, p.PointOffset, p.SpeedOffset, p.Speed, p.MaxDistance, p.Shape, p.Color, p.Advance);
                    break;

                case BulletParams.ModeType.OffsetAngle:
                    _ = new Bullet(sc.User, this, p.Damage, sc.User.Position, sc.GridOne.Position, p.PointOffset, p.AngleOffsetDeg, p.Speed, p.MaxDistance, p.Shape, p.Color, p.Advance);
                    break;
            }
        }

    }

    public record BulletParams
    {
        // 公共参数
        public float Damage { get; init; }
        public float Speed { get; init; }
        public float MaxDistance { get; init; }
        public string Shape { get; init; }
        public ColorBullet Color { get; init; }
        public float Advance { get; init; }

        // 模式选择字段（内部用于区分构造类型）
        public enum ModeType { Basic, OffsetVector, OffsetAngle }
        public ModeType Mode { get; init; }

        // 基本模式（只使用目标点）
        public BulletParams(float damage, float speed, float maxDistance, string shape, ColorBullet color, float advance = 0)
        {
            Mode = ModeType.Basic;
            Damage = damage;
            Speed = speed;
            MaxDistance = maxDistance;
            Shape = shape;
            Color = color;
            Advance = advance;
        }

        // 起点偏移 + 速度向量偏移
        public BulletParams(float damage, Vector2 pointOffset, Vector2 speedOffset, float speed, float maxDistance, string shape, ColorBullet color, float advance = 0)
        {
            Mode = ModeType.OffsetVector;
            Damage = damage;
            PointOffset = pointOffset;
            SpeedOffset = speedOffset;
            Speed = speed;
            MaxDistance = maxDistance;
            Shape = shape;
            Color = color;
            Advance = advance;
        }

        // 起点偏移 + 速度角度偏移
        public BulletParams(float damage, Vector2 pointOffset, float angleDeg, float speed, float maxDistance, string shape, ColorBullet color, float advance = 0)
        {
            Mode = ModeType.OffsetAngle;
            Damage = damage;
            PointOffset = pointOffset;
            AngleOffsetDeg = angleDeg;
            Speed = speed;
            MaxDistance = maxDistance;
            Shape = shape;
            Color = color;
            Advance = advance;
        }

        // 附加字段（用于偏移构造）
        public Vector2 PointOffset { get; init; }
        public Vector2 SpeedOffset { get; init; }
        public float AngleOffsetDeg { get; init; }
    }

}

