using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using static MathEx;
public class MesmerizingMisdirection: Skill
{
    public MesmerizingMisdirection()
    {
        Name = "MesmerizingMisdirection";
        SkillGroup = "Time";
        SpCost = 5;
        MpCost = 3;
        Cooldown = 600;
        Targeting = new TargetType(Target.Enemy, 1, 10);
    }
    int[] t0 = [18, 27, 36, 36];
    int[] t1 = [100, 100, 100, 200];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], t1[iLevel]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        Vector2 offset = MathEx.RandomV2(2);
        for (int i = 0; i < t0[iLevel] / 9; i++)
        {
            for(int a = 20; a < 360; a += 40)
            {
                Bullet.CreateBullet(sc.User, this, new Damage(8, DamageType.pierce), sc.User.Up.Position, sc.UnitOne.Up.Position,
                    offset * i / 3, a, 2 - 0.33f * i, 10, ShapeBullet.ArrowSmall, ColorBullet.Red);
            }
        }
    }
    public override void AwakeBullet(SkillContext sc, Bullet bullet)
    {
        if(G.I.Fsm.currentState is EnemySkillState || G.I.Fsm.currentState is PlayerSkillState)
        {
            bullet.accuracy += 0.5f;
            bullet.damage += bullet.damage * t1[iLevel] / 100;
        }
    }
}
public class ImaginaryVerticalTime : Skill
{
    public ImaginaryVerticalTime()
    {
        Name = "ImaginaryVerticalTime";
        SkillGroup = "Time";
        Cooldown = 2000;
        Targeting = new TargetType(Target.Self, 1, 8);
    }
    int[] t0 = [30, 25, 20, 20];
    float[] t1 = [0.15f, 0.10f, 0.06f, 0.06f];
    int[] t2 = [2, 2, 2, 3];
    public override float GetSpCost()
    {
        return t0[iLevel];
    }
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t2[iLevel], (int)(100 * t1[iLevel]));
    }
    protected override void StartActivate(SkillContext sc)
    {
        sc.User.GetStatus(new SImaginaryVerticalTime(t2[iLevel] - 1,
            t1[iLevel], 500, this));
    }
}
public class PerfectSquare: Skill
{
    public PerfectSquare()
    {
        Name = "PerfectSquare";
        SkillGroup = "Time";
        MpCost = 25;
        Cooldown = 2000;
        Targeting = new TargetType(Target.Self, 1, 3);
    }
    float[] t0 = [0.2f, 0.3f, 0.4f, 0.4f];
    int[] t1 = [3, 4, 5, 5];
    float[] t2 = [0.4f, 0.55f, 0.7f, 0.7f];
    float[] t3 = [0.3f, 0.3f, 0.3f, 1];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), (int)(100 * t0[iLevel]), t1[iLevel],
            (int)(100 * t2[iLevel]), (int)(100 * t3[iLevel]));
    }
    protected override void StartActivate(SkillContext sc)
    {
        sc.User.GetStatus(new SPerfectSquare(t0[iLevel], t1[iLevel], t2[iLevel],
            t3[iLevel], this));
    }
}
public class LunaDial: Skill
{
    public LunaDial()
    {
        Name = "LunaDial";
        SkillGroup = "Time";
        SpCost = 10;
        MpCost = 20;
        Cooldown = 2400;
    }
    int[] t0 = [2, 3, 4, 4];
    int[] t1 = [0, 0, 0, 50];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], TextEx.Tr(Extra()[iLevel]));
    }
    public override TargetType GetTargeting()
    {
        return new TargetType(Target.Unit, 1, new int[] { 3, 5, 7, 7 }[iLevel]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        sc.UnitOne.GetStatus(new SLunaDial(t1[iLevel], 100 * t0[iLevel], sc.User, this));
    }
}
public class SakuyasWorld : SpellCard
{
    public SakuyasWorld()
    {
        Name = "SakuyasWorld";
        SkillGroup = "Time";
        SpCost = 40;
        MpCost = 30;
        Cooldown = 4200;
        Duration = 500;
        Targeting = new TargetType(Target.Self, 1, 6);
    }
    int[] t0 = [3, 4, 5, 5];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], TextEx.Tr(Extra()[iLevel]));
    }
    protected override void OnSpellStart(SkillContext sc)
    {
        Info.Print($"{sc.User.TrName} 展开了 {TrName} ！");
        sc.User.TimeEnergy += Duration;
        if (sc.Level == 4)
            sc.User.Ua.DamageBullet += 0.33f;
    }
    public override void OnSpellEnd(SkillContext sc)
    {
        if (sc.Level == 4)
            sc.User.Ua.DamageBullet -= 0.33f;
        base.OnSpellEnd(sc);
    }
}

public class SImaginaryVerticalTime : Status
{
    private Skill Parent;
    private SkillContext rsc;
    private Skill rsi;
    private float rcd;
    private bool active = false;
    public float Count
    {
        get => (float)Param;
        set
        {
            Param = value;
        }
    }
    public float Cd;
    public SImaginaryVerticalTime(int count, float cd, float duration, Skill parent)
    {
        Name = "ImaginaryVerticalTime";
        Duration = 500;
        rcd = cd;
        Count = count;
        Cd = cd;
        Parent = parent;
    }
    public override void OnGet(Unit unit, Status status)
    {
        unit.Ue.OnUseSkill += BulletboxCheck;
        CombineTime(unit, status);
    }
    public override void OnQuit(Unit unit)
    {
        if (active)
        {
            if (rsi != null && rsc != null)
                rsi.Activate(rsc);
            Count--;
            if(Count < 0.5f)
                Quit(unit);
            else
                Duration = rcd * rsi.CurrentCooldown;
        }
        else
        {
            Quit(unit);
        }
    }
    public void BulletboxCheck(Unit unit, SkillContext sc, Skill si)
    {
        if (si != null && si.Name == "BarrageSet")
        {
            unit.Ue.OnUseSkill -= BulletboxCheck;
            active = true;
            rsc = sc;
            rsi = si;
            Duration = rcd * si.CurrentCooldown;
        }
    }
}

public class SPerfectSquare : Status
{
    private Skill Parent;
    public float Speedup
    {
        get => (float)Param;
        set
        {
            Param = value;
        }
    }
    public int Range;
    public float Speeddown;
    public float AccDown;
    public List<Bullet> bullets = [];
    public Unit user;
    public SPerfectSquare(float speedup, int range, float speeddown, float accDown, Skill parent)
    {
        Name = "PerfectSquare";
        Duration = 500;
        Speedup = speedup;
        Range = range;
        Speeddown = speeddown;
        Parent = parent;
    }
    public override void OnGet(Unit unit, Status status)
    {
        unit.Ua.SpeedGlobal += Speedup;
        user = unit;
        GameEvents.OnBulletMove += CheckBulletMove;
        CombineTime(unit, status);
    }
    public override void OnQuit(Unit unit)
    {
        unit.Ua.SpeedGlobal -= Speedup;
        GameEvents.OnBulletMove -= CheckBulletMove;
        Quit(unit);
    }
    private void CheckBulletMove(Bullet bullet)
    {
        if (user.Up.CurrentGrid.NearGrids(Range).Select(x => x.Position).Contains(bullet.Position)
            && !bullets.Contains(bullet))
        {
            bullets.Add(bullet);
            bullet.Speed *= (1 - Speeddown);
            bullet.accuracy *= (1 - AccDown);
        }
        else if(!user.Up.CurrentGrid.NearGrids(Range).Select(x => x.Position).Contains(bullet.Position)
            && bullets.Contains(bullet))
        {
            bullets.Remove(bullet);
            bullet.Speed /= (1 - Speeddown);
            bullet.accuracy /= (1 - AccDown);
        }
    }
}

public class SLunaDial : Status
{
    public float Damage;
    public Unit User;
    public Skill Skill;
    public SLunaDial(float damage, float duration, Unit user, Skill skill)
    {
        Name = "LunaDial";
        Damage = damage;
        Duration = duration;
        User = user;
        Skill = skill;
    }
    public override void OnGet(Unit unit, Status status)
    {
        unit.TimeEnergy -= Duration;
        CombineTime(unit, status);
    }
    public override void OnQuit(Unit unit)
    {
        if (Damage > 0)
            unit.Ua.TakeBulletDamage(new Damage(50, DamageType.timespace), User, Skill);
        base.OnQuit(unit);
    }
}