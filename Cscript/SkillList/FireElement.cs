using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static MathEx;
using static System.Net.Mime.MediaTypeNames;
public class SummerRed : Skill
{
    public SummerRed()
    {
        SkillGroup = "FireElement";
        SpCost = 3;
        MpCost = 8;
        Cooldown = 700;
        Targeting = new TargetType(new TargetRuleAny(), 1, 12);
    }
    int[] t0 = [32, 40, 48, 48];
    int[] t1 = [3, 6, 9, 9];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], TextEx.Tr(Extra()[iLevel]));
    }
    protected override void StartActivate(SkillContext sc)
    {
        Bullet fireball = Bullet.CreateBullet(sc.User, this, new Damage(t0[iLevel], DamageType.fire),
               sc.User.Up.Position, sc.GridOne.Position,
               t1[iLevel], 12,
               ShapeBullet.Big, ColorBullet.Red);
        if (sc.Level == 4)
            fireball.OverrideUpdateEvents += SummerRedBulletUpdate4;
        else
            fireball.OverrideUpdateEvents += SummerRedBulletUpdate;
    }
    public static void SummerRedBulletUpdate(Bullet bullet, float time)
    {
        foreach (Vector2I pos in UpdateGrid(bullet, time / 100))
        {
            var grid = Scene.CurrentMap.GetGrid(pos);
            if (!Scene.CurrentMap.CheckGrid(pos) || !grid.IsTransparent)
            {
                bullet.Destroy();
                return;
            }

            var target = grid.unit;
            if (target != null && target.Friendness * bullet.creator.Friendness < 0)
            {
                bullet.Active(target);
                return;
            }
        }
        if (bullet.DistanceLeft <= 0 || bullet.image == null)
        {
            bullet.Destroy();
            return;
        }
        // 设置位置
        bullet.image.Position = bullet.PositionFloat * Setting.imagePx;
        // 设置朝向：因为默认朝上（0°），所以旋转方向就是速度向量的角度
        if (bullet.Speed.LengthSquared() > 0) // 避免除0
        {
            bullet.image.Rotation = Mathf.Atan2(bullet.Speed.Y, bullet.Speed.X) + Mathf.Pi / 2 - Mathf.Pi * bullet.AngleOfShapeBullet() / 180;
        }

        List<Vector2I> UpdateGrid(Bullet bullet, float deltaTime)
        {
            List<Vector2I> newGrids = [];

            if (bullet.DistanceLeft <= 0)
                return newGrids;

            // 移动前记录旧位置
            bullet.lastPosition = bullet.Position;

            // 计算实际移动距离
            float travelDistance = bullet.Speed.Length() * deltaTime;
            if (travelDistance > bullet.DistanceLeft)
                travelDistance = bullet.DistanceLeft;

            // 更新位置
            bullet.PositionFloat += bullet.Speed.Normalized() * travelDistance;
            bullet.DistanceLeft -= travelDistance;

            // 用整数格子记录路径
            Vector2I oldPos = bullet.lastPosition;
            Vector2I newPos = (Vector2I)bullet.PositionFloat.Round();

            if (oldPos != newPos)
            {
                var path = MathEx.GetLine(oldPos, newPos);
                // 只记录中间新格子（不包含起点）
                foreach (var p in path)
                {
                    if (p != oldPos)
                        newGrids.Add(p);
                }
            }

            bullet.Position = newPos;
            return newGrids;
        }
    }
    public static void SummerRedBulletUpdate4(Bullet bullet, float time)
    {
        foreach (Vector2I pos in UpdateGrid(bullet, time / 100))
        {
            var grid = Scene.CurrentMap.GetGrid(pos);
            if (!Scene.CurrentMap.CheckGrid(pos) || !grid.IsTransparent)
            {
                bullet.Destroy();
                return;
            }

            var target = grid.unit;
            if (target != null && target.Friendness * bullet.creator.Friendness < 0)
            {
                bullet.Active(target);
                return;
            }
        }
        if (bullet.DistanceLeft <= 0 || bullet.image == null)
        {
            bullet.Destroy();
            return;
        }
        // 设置位置
        bullet.image.Position = bullet.PositionFloat * Setting.imagePx;
        // 设置朝向：因为默认朝上（0°），所以旋转方向就是速度向量的角度
        if (bullet.Speed.LengthSquared() > 0) // 避免除0
        {
            bullet.image.Rotation = Mathf.Atan2(bullet.Speed.Y, bullet.Speed.X) + Mathf.Pi / 2 - Mathf.Pi * bullet.AngleOfShapeBullet() / 180;
        }

        List<Vector2I> UpdateGrid(Bullet bullet, float deltaTime)
        {
            List<Vector2I> newGrids = [];

            if (bullet.DistanceLeft <= 0)
                return newGrids;

            // 移动前记录旧位置
            bullet.lastPosition = bullet.Position;

            // 计算实际移动距离
            float travelDistance = bullet.Speed.Length() * deltaTime;
            if (travelDistance > bullet.DistanceLeft)
                travelDistance = bullet.DistanceLeft;

            // 更新位置
            bullet.PositionFloat += bullet.Speed.Normalized() * travelDistance;
            if ((int)bullet.DistanceLeft > (int)(bullet.DistanceLeft - travelDistance))
            {
                Bullet.CreateBullet(bullet.creator, bullet.skill, new Damage(8, DamageType.fire),
                    bullet.PositionFloat, bullet.PositionFloat + bullet.Speed.Rotated(90 / 57.3f),
                    0.6f, 1.5f,
                    ShapeBullet.Ring, ColorBullet.Orange);
                Bullet.CreateBullet(bullet.creator, bullet.skill, new Damage(8, DamageType.fire),
                    bullet.PositionFloat, bullet.PositionFloat + bullet.Speed.Rotated(-90 / 57.3f),
                    0.6f, 1.5f,
                    ShapeBullet.Ring, ColorBullet.Orange);
            }
            bullet.DistanceLeft -= travelDistance;

            // 用整数格子记录路径
            Vector2I oldPos = bullet.lastPosition;
            Vector2I newPos = (Vector2I)bullet.PositionFloat.Round();

            if (oldPos != newPos)
            {
                var path = MathEx.GetLine(oldPos, newPos);
                // 只记录中间新格子（不包含起点）
                foreach (var p in path)
                {
                    if (p != oldPos)
                        newGrids.Add(p);
                }
            }

            bullet.Position = newPos;
            return newGrids;
        }
    }
}
public class AgniShine : Skill
{
    public AgniShine()
    {
        SkillGroup = "FireElement";
        SpCost = 5;
        MpCost = 12;
        Cooldown = 900;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 10);
    }
    int[] t0 = [40, 60, 80, 80];
    int[] t1 = [20, 20, 20, 50];

    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], t1[iLevel]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        for (int i = 0; i < t0[iLevel]; i++)
        {
            Bullet fireball = Bullet.CreateBullet(sc.User, this, new Damage(10, DamageType.fire),
               sc.User.Up.Position, sc.User.Up.Position + MathEx.RandomV2(),
               (float)GD.RandRange(1.0, 2.0), 10,
               ShapeBullet.Water, ColorBullet.Red);
        }
    }
}
public class AgniRadiance : Skill
{
    public AgniRadiance()
    {
        SkillGroup = "FireElement";
        MpCost = 22;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 6);
    }
    int[] t0 = [5, 6, 7, 7];
    int[] t1 = [30, 35, 40, 40];
    int[] t2 = [40, 32, 24, 24];
    public override float GetCooldown()
    {
        return t2[iLevel] * 100;
    }
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], t1[iLevel], TextEx.Tr(Extra()[iLevel]));
    }
    protected override void StartActivate(SkillContext sc)
    {
        sc.User.GetStatus(new SAgniRadiance(t1[iLevel], 100 * t0[iLevel], this, Level));
    }
}
public class LavaCromlech : Skill
{
    public LavaCromlech()
    {
        SkillGroup = "FireElement";
        SpCost = 5;
        MpCost = 20;
        Cooldown = 1400;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 6);
    }
    int[] t0 = [36, 54, 72, 72];
    int[] t1 = [3, 3, 3, 6];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], t1[iLevel]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        for (int i = 0; i < t0[iLevel]; i++)
        {
            Bullet lavaball = Bullet.CreateBullet(sc.User, this, new Damage(16, DamageType.earth),
                sc.User.Up.Position, sc.User.Up.Position + MathEx.RandomV2(),
                (float)GD.RandRange(1.2, 2.4), (float)GD.RandRange(4f, 8f),
                ShapeBullet.Ring, ColorBullet.Orange);
            lavaball.OverrideUpdateEvents += LavaCromlechBulletUpdate;
        }
        
    }
    public static void LavaCromlechBulletUpdate(Bullet bullet, float time)
    {
        foreach (Vector2I pos in UpdateGrid(bullet, time / 100))
        {
            var grid = Scene.CurrentMap.GetGrid(pos);
            if (!Scene.CurrentMap.CheckGrid(pos) || !grid.IsTransparent)
            {
                bullet.Destroy();
                return;
            }

            var target = grid.unit;
            if (target != null && target.Friendness * bullet.creator.Friendness < 0)
            {
                // 命中检定
                float hitChance = MathEx.Logistic(0.8f, bullet.accuracy - target.Ua.DamageEvasion);
                if (GD.Randf() < hitChance)
                {
                    bullet.Active(target);
                    return;
                }
            }
        }
        if (bullet.DistanceLeft <= 0 || bullet.image == null)
        {
            if (bullet.Speed.Length() > 0.1f)
            {
                bullet.Speed = 0.01f * bullet.Speed.Normalized();
                bullet.DistanceLeft = 0.03f;
            }
            else
            {
                bullet.Destroy();
                return;
            }
        }
        // 设置位置
        bullet.image.Position = bullet.PositionFloat * Setting.imagePx;
        // 设置朝向：因为默认朝上（0°），所以旋转方向就是速度向量的角度
        if (bullet.Speed.LengthSquared() > 0) // 避免除0
        {
            bullet.image.Rotation = Mathf.Atan2(bullet.Speed.Y, bullet.Speed.X) + Mathf.Pi / 2 - Mathf.Pi * bullet.AngleOfShapeBullet() / 180;
        }

        List<Vector2I> UpdateGrid(Bullet bullet, float deltaTime)
        {
            List<Vector2I> newGrids = [];

            if (bullet.DistanceLeft <= 0)
                return newGrids;

            // 移动前记录旧位置
            bullet.lastPosition = bullet.Position;

            // 计算实际移动距离
            float travelDistance = bullet.Speed.Length() * deltaTime;
            if (travelDistance > bullet.DistanceLeft)
                travelDistance = bullet.DistanceLeft;

            // 更新位置
            bullet.PositionFloat += bullet.Speed.Normalized() * travelDistance;
            bullet.DistanceLeft -= travelDistance;

            // 用整数格子记录路径
            Vector2I oldPos = bullet.lastPosition;
            Vector2I newPos = (Vector2I)bullet.PositionFloat.Round();

            if (oldPos != newPos)
            {
                var path = MathEx.GetLine(oldPos, newPos);
                // 只记录中间新格子（不包含起点）
                foreach (var p in path)
                {
                    if (p != oldPos)
                        newGrids.Add(p);
                }
            }

            bullet.Position = newPos;
            return newGrids;
        }
    }
}
public class PhlogisticPillar : SpellCard
{
    public PhlogisticPillar()
    {
        SkillGroup = "FireElement";
        SpCost = 40;
        Duration = 400;
        Cooldown = 3600;
        Targeting = new TargetType(new TargetRuleAny(), 1, 9);
    }
    int[] t0 = [6, 9, 9, 9];
    int[] t1 = [30, 30, 30, 15];
    int[] t2 = [100, 200, 300, 300];
    private Vector2 zeroDir;
    public override float GetMpCost()
    {
        return t1[iLevel];
    }
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t2[iLevel] / 100);
    }
    protected override void OnSpellStart(SkillContext sc)
    {
        zeroDir = ((Vector2)(sc.GridOne.Position - sc.User.Up.Position)).Normalized() * t0[iLevel];
        Info.Print($"{sc.User.TrName} 展开了 {TrName} ");
        AddTimedEvent(Linspace(50, 250, 2), (ctx, advanceTime) =>
        {
            for (float i = -36; i <= 36; i += 36f)
            {
                Grid g; int maxD = t1[iLevel];
                do
                {
                    g = Scene.CurrentMap.GetGrid((Vector2I)(sc.User.Up.Position + maxD * zeroDir.Normalized().Rotated(i / 57.3f)));
                    maxD--;
                } while (g == null);
                List<Grid> target = sc.User.Up.RayCheck(g);
                foreach (Unit unit in target.Where(x => x.unit != null).Select(x => x.unit))
                {
                    unit.Ua.TakeBulletDamage(new Damage(18, DamageType.cold), sc.User, this);
                    unit.GetStatus(new Frozen(t2[iLevel]));
                }
                Animation.ShootLaser(sc.User.Up.Position, zeroDir.Rotated(i / 57.3f), Colors.LightSkyBlue);
            }
            for (float i = 144; i <= 216; i += 36f)
            {
                Grid g; int maxD = t1[iLevel];
                do
                {
                    g = Scene.CurrentMap.GetGrid((Vector2I)(sc.User.Up.Position + maxD * zeroDir.Normalized().Rotated(i / 57.3f)));
                    maxD--;
                } while (g == null);
                List<Grid> target = sc.User.Up.RayCheck(g);
                foreach (Unit unit in target.Where(x => x.unit != null).Select(x => x.unit))
                {
                    unit.Ua.TakeBulletDamage(new Damage(18, DamageType.cold), sc.User, this);
                    unit.GetStatus(new Frozen(t2[iLevel]));
                }
                Animation.ShootLaser(sc.User.Up.Position, zeroDir.Rotated(i / 57.3f), Colors.LightSkyBlue);
            }
            for (float i = -18; i <= 18; i += 36f)
            {
                Grid g; int maxD = t1[iLevel];
                do
                {
                    g = Scene.CurrentMap.GetGrid((Vector2I)(sc.User.Up.Position + maxD * zeroDir.Normalized()));
                    maxD--;
                } while (g == null);
                List<Grid> target = sc.User.Up.RayCheck(g);
                foreach (Unit unit in target.Where(x => x.unit != null).Select(x => x.unit))
                {
                    unit.Ua.TakeBulletDamage(new Damage(18, DamageType.fire), sc.User, this);
                    unit.GetStatus(new Burned(9, t2[iLevel]));
                }
                Animation.ShootLaser(sc.User.Up.Position, zeroDir.Rotated(i / 57.3f), Colors.OrangeRed);
            }
            for (float i = 162; i <= 198; i += 36f)
            {
                Grid g; int maxD = t1[iLevel];
                do
                {
                    g = Scene.CurrentMap.GetGrid((Vector2I)(sc.User.Up.Position + maxD * zeroDir.Normalized()));
                    maxD--;
                } while (g == null);
                List<Grid> target = sc.User.Up.RayCheck(g);
                foreach (Unit unit in target.Where(x => x.unit != null).Select(x => x.unit))
                {
                    unit.Ua.TakeBulletDamage(new Damage(18, DamageType.fire), sc.User, this);
                    unit.GetStatus(new Burned(9, t2[iLevel]));
                }
                Animation.ShootLaser(sc.User.Up.Position, zeroDir.Rotated(i / 57.3f), Colors.OrangeRed);
            }
        });
        AddTimedEvent(Linspace(150, 350, 2), (ctx, advanceTime) =>
        {
            for (float i = 54; i <= 126; i += 36f)
            {
                Grid g; int maxD = t1[iLevel];
                do
                {
                    g = Scene.CurrentMap.GetGrid((Vector2I)(sc.User.Up.Position + maxD * zeroDir.Normalized()));
                    maxD--;
                } while (g == null);
                List<Grid> target = sc.User.Up.RayCheck(g);
                foreach (Unit unit in target.Where(x => x.unit != null).Select(x => x.unit))
                {
                    unit.Ua.TakeBulletDamage(new Damage(18, DamageType.fire), sc.User, this);
                    unit.GetStatus(new Burned(9, t2[iLevel]));
                }
                Animation.ShootLaser(sc.User.Up.Position, zeroDir.Rotated(i / 57.3f), Colors.OrangeRed);
            }
            for (float i = 234; i <= 306; i += 36f)
            {
                Grid g; int maxD = t1[iLevel];
                do
                {
                    g = Scene.CurrentMap.GetGrid((Vector2I)(sc.User.Up.Position + maxD * zeroDir.Normalized()));
                    maxD--;
                } while (g == null);
                List<Grid> target = sc.User.Up.RayCheck(g);
                foreach (Unit unit in target.Where(x => x.unit != null).Select(x => x.unit))
                {
                    unit.Ua.TakeBulletDamage(new Damage(18, DamageType.fire), sc.User, this);
                    unit.GetStatus(new Burned(9, t2[iLevel]));
                }
                Animation.ShootLaser(sc.User.Up.Position, zeroDir.Rotated(i / 57.3f), Colors.OrangeRed);
            }
            for (float i = 72; i <= 108; i += 36f)
            {
                Grid g; int maxD = t1[iLevel];
                do
                {
                    g = Scene.CurrentMap.GetGrid((Vector2I)(sc.User.Up.Position + maxD * zeroDir.Normalized()));
                    maxD--;
                } while (g == null);
                List<Grid> target = sc.User.Up.RayCheck(g);
                foreach (Unit unit in target.Where(x => x.unit != null).Select(x => x.unit))
                {
                    unit.Ua.TakeBulletDamage(new Damage(18, DamageType.cold), sc.User, this);
                    unit.GetStatus(new Frozen(t2[iLevel]));
                }
                Animation.ShootLaser(sc.User.Up.Position, zeroDir.Rotated(i / 57.3f), Colors.LightSkyBlue);
            }
            for (float i = 252; i <= 288; i += 36f)
            {
                Grid g; int maxD = t1[iLevel];
                do
                {
                    g = Scene.CurrentMap.GetGrid((Vector2I)(sc.User.Up.Position + maxD * zeroDir.Normalized()));
                    maxD--;
                } while (g == null);
                List<Grid> target = sc.User.Up.RayCheck(g);
                foreach (Unit unit in target.Where(x => x.unit != null).Select(x => x.unit))
                {
                    unit.Ua.TakeBulletDamage(new Damage(18, DamageType.cold), sc.User, this);
                    unit.GetStatus(new Frozen(t2[iLevel]));
                }
                Animation.ShootLaser(sc.User.Up.Position, zeroDir.Rotated(i / 57.3f), Colors.LightSkyBlue);
            }
        });
    }
}

public class SAgniRadiance : Status
{
    private Skill Parent;
    private int level;
    public float FireBuff
    {
        get => (float)Param;
        set
        {
            Param = value;
        }
    }
    public SAgniRadiance(float fireBuff, float duration, Skill parent, int level)
    {
        Duration = duration;
        FireBuff = fireBuff;
        Parent = parent;
        this.level = level;
    }
    public override void OnGet(Unit unit, Status status)
    {
        unit.Ua.DamageTypeSet += new DamageTypeSet(DamageType.fire, FireBuff / 100f, 1);
        unit.Ua.DamageTypeSet += new DamageTypeSet(DamageType.fire, 1, -1);
        CombineTime(unit, status);
    }
    public override void OnQuit(Unit unit)
    {
        unit.Ua.DamageTypeSet -= new DamageTypeSet(DamageType.fire, FireBuff / 100f, 1);
        unit.Ua.DamageTypeSet -= new DamageTypeSet(DamageType.fire, 1, -1);
        Quit(unit);
    }
    public override void OnTurnEnd(Unit unit)
    {
        foreach (Unit target in unit.Up.CurrentGrid.NearGrids(3).Where(x => x != unit.Up.CurrentGrid).Select(g => g.unit))
        {
            if (GD.Randf() < 0.2f)
                target?.GetStatus(new Stun(200));
            if (level - 1 == 4)
                target.Ua.TakeBulletDamage(new Damage(20, DamageType.sonic), unit, Parent);
        }
    }
}  