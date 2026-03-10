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
        SkillGroup = "Time";
        SpCost = 5;
        MpCost = 3;
        Cooldown = 600;
        Targeting = new TargetType(new TargetRuleEnemy(), 1, 10);
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
        SkillGroup = "Time";
        Cooldown = 2000;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 8);
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
        SkillGroup = "Time";
        MpCost = 25;
        Cooldown = 2000;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 3);
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
        return new TargetType(new TargetRuleUnit(), 1, new int[] { 3, 5, 7, 7 }[iLevel]);
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
        SkillGroup = "Time";
        SpCost = 40;
        MpCost = 30;
        Cooldown = 4200;
        Duration = 500;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 6);
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

