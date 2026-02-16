using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using static MathEx;
public class YinyangBallShoot : Skill
{
    public YinyangBallShoot()
    {
        Name = "YinyangBallShoot";
        SkillGroup = "Private";
        Targeting = new TargetType(Target.Grid, 1, 12);
    }
    public override void Activate(SkillContext sc, SkillInstance si = null)
    {
        sc.User.Ua.GetSp(-GetSpCost(sc.Level));
        sc.User.Ua.GetMp(-GetMpCost(sc.Level));
        if (SkillGroup != "")
            Info.Print($"{sc.User.TrName} 执行 {TrName}");
        StartActivate(sc);
        G.I.Fsm.ChangeState(Fsm.UpdateState);
    }
    protected override void StartActivate(SkillContext sc)
    {
        Bullet.CreateBullet(sc.User, this, new Damage(12, DamageType.spirit), sc.User.Up.Position, sc.GridOne.Position,
            4, 12, ShapeBullet.Yinyang, (ColorBullet)GD.RandRange(0, 15));
    }
    public override void ActivateBullet(SkillContext sc, Bullet bullet)
    {
        if (sc.Level >= 2)
            sc.UnitOne.GetStatus(new Weak(100));
    }
}
public class DreamOrb : Skill
{
    public DreamOrb()
    {
        Name = "DreamOrb";
        SkillGroup = "YinyangBall";
        SpCost = 3;
        Cooldown = 500;
    }
    int[] t0 = [12, 12, 12, 12];
    int[] t1 = [5, 7, 9, 9];
    int[] t2 = [1, 1, 1, 2];
    public override TargetType GetTargeting(int level)
    {
        return new TargetType(Target.Grid, 1, t0[level - 1]);
    }
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t1[level - 1], t2[level - 1]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        for(float a = 0; a < 360 ; a += 360 / t1[sc.Level - 1])
        {
            Bullet.CreateBullet(sc.User, this, new Damage(6, DamageType.strike), sc.User.Up.Position, sc.GridOne.Position,
                Vector2.Right.Rotated(a / 57.3f), a, (float)GD.RandRange(3f, 4f)
                ,t0[sc.Level - 1], ShapeBullet.Micro, ColorBullet.Red, 0, sc.GridOne);
        }
        sc.User.GetStatus(new YinyangBall(t2[sc.Level - 1]));
    }
}
public class TreasureOrb : Skill
{
    public TreasureOrb()
    {
        Name = "TreasureOrb";
        SkillGroup = "YinyangBall";
        EffectType = EffectType.Passive;
    }
    private Action<Unit> OnTurnEnd;
    private int turnCount = 0;
    public override string GetDescription(int level)
    {
        return TextEx.Tr($"sTreasureOrb sTreasureOrb{level - 1} ");
    }
    public override void OnLoad(Unit unit)
    {
        OnTurnEnd = (movingUnit) =>
        {
            if (Scene.CurrentMap.WakeUnits.Count <= 1) return;
            turnCount++;
            if (turnCount >= 5)
            {
                unit.GetStatus(new YinyangBall(1));
                turnCount -= 5;
            }
        };

        unit.Ue.OnUnitTurnEnd += OnTurnEnd;
    }
    public override void OffLearn(Unit unit)
    {
        unit.Ue.OnUnitTurnEnd -= OnTurnEnd;
    }
}
public class LightToShade : Skill 
{
    public LightToShade()
    {
        Name = "LightToShade";
        SkillGroup = "YinyangBall";
        SpCost = 10;
        Cooldown = 1200;
        Targeting = new TargetType(Target.Grid, 1, 8);
    }
    int[] t0 = { 3, 5, 7, 7 };
    string[] extra = ["", "", "", " sLightToShade0 "];
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1], extra[level - 1]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        for(float v = 1.5f; v <= 3; v += 0.5f)
        {
            for (float a = -15; a <= 15; a += 15)
            {
                Bullet.CreateBullet(sc.User, this, new Damage(8, DamageType.spirit), sc.User.Up.Position, sc.GridOne.Position,
                    Vector2.Zero, a, v, 8, ShapeBullet.Square, ColorBullet.Red);
            }
        }
        for(int i = 0; i < t0[sc.Level - 1]; i++)
        {
            Bullet b = Bullet.CreateBullet(sc.User, this, new Damage(12, DamageType.spirit), sc.User.Up.Position, sc.GridOne.Position,
                    Vector2.Zero, GD.RandRange(-10, 10), 4, 12, ShapeBullet.Yinyang, ColorBullet.Red);
            if (sc.User != Player.PlayerUnit)
                b.image.Visible = false;
            else
                b.image.Modulate = new Color(1, 1, 1, 0.5f);
        }
    }
    public override void AwakeBullet(SkillContext sc, Bullet bullet)
    {
        if (sc.Level == 4 && bullet.Shape == ShapeBullet.Yinyang)
        {
            sc.User.GetStatus(new YinyangBall(1));
        }
    }
}
public class YinyangScatter : Skill
{
    public YinyangScatter()
    {
        Name = "YinyangScatter";
        SkillGroup = "YinyangBall";
        Cooldown = 1200;
        Targeting = new TargetType(Target.Self, 1, 6);
    }
    int[] k = [4, 6, 8, 10];
    public override float GetSpCost(int level)
    {
        if (level == 4)
            return 30;
        return 0;
    }
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), k[level-1]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        int l = 0;
        if (sc.User.Status.FirstOrDefault(x => x is YinyangBall yb) is YinyangBall yb)
        {
            l = k[sc.Level - 1] * yb.Layer;
            yb.Quit(sc.User);
        }
        
        for (int i = 0; i < l; i++)
        {
            Bullet b = Bullet.CreateBullet(sc.User, this, new Damage(12, DamageType.spirit), sc.User.Up.Position, sc.User.Up.Position + Vector2I.Left,
                    Vector2.Zero, GD.RandRange(0, 360), 4, 12, ShapeBullet.Yinyang, ColorBullet.Red);
        }
    }
}
public class DreamSeal : SpellCard
{
    public DreamSeal()
    {
        Name = "DreamSeal";
        SkillGroup = "YinyangBall";
        SpNeed = 50;
        SpCost = 6;
        Cooldown = 2600;
        Targeting = new TargetType(Target.Self, 1, 10);
    }
    int[] t0 = { 24,30,36,36 };
    int[] t1 = { 400,500,600,600 };
    string[] t2 = ["", "", "", " sDreamSeal0 "];
    public override float GetDuration(int level)
    {
        return t1[level - 1];
    }
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1], t2[level - 1]);
    }
    protected override void OnSpellStart(SkillContext sc)
    {
        Info.Print($"{sc.User.TrName} 展开了 {TrName} ！");
        sc.User.GetStatus(new YinyangBall(2));
        sc.User.GetStatus(new YinyangBall(2));
        AddTimedEvent(Linspace(20, GetDuration(sc.Level), t0[sc.Level - 1]), (ctx, advanceTime) =>
        {
            var bullet = Bullet.CreateBullet(sc.User, this, new Damage(22, DamageType.strike), sc.User.Up.Position, sc.User.Up.Position + RandomV2(), 
                new Vector2(0, 0), Vector2.Right, (float)GD.RandRange(1.0, 4.0), 12, 
                ShapeBullet.Ring, 0 , advanceTime, sc.User.Up.CurrentGrid);
        });
        AddTimedEvent(Linspace(0, GetDuration(sc.Level) - 100, (int)GetDuration(sc.Level)/100), (ctx, advanceTime) =>
        {
            sc.User.GetStatus(new YinyangBall());
        });
    }
    public override void ActivateBullet(SkillContext sc, Bullet bullet)
    {
        sc.UnitOne.GetStatus(new SpiritSeal(300));
        if (sc.Level == 4)
            sc.UnitOne.GetStatus(new MagicSeal(300));
    }
}