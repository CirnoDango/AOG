using Godot;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
public class Frozen : Status
{
    public override StatusType Type => StatusType.Negative;
    public Frozen(float duration)
    {
        Duration = duration;
    }
    public override void OnTakeBulletDamage(Unit unit, Skill skill, ref Damage damage)
    {
        damage *= 0.1f;
    }

    public override void OnTakeBodyDamage(Unit unit, ref Damage damage)
    {
        damage *= 0.1f;
    }
    public override void OnGet(Unit unit, Status status)
    {
        unit.TimeEnergy -= Duration;
        CombineTime(unit, status);
    }
}
public class Dark : Status
{
    public override StatusType Type => StatusType.Negative;
    public Dark(float duration)
    {
        Duration = duration;
    }
    public override void OnGet(Unit unit, Status status)
    {
        if (CombineTime(unit, status))
            unit.Up.Vision -= 8;
    }
    public override void OnQuit(Unit unit)
    {
        unit.Up.Vision += 8;
        Quit(unit);
    }
}
public class YinyangBall : Status
{
    public static SkillContext activator;
    private int value;
    public int Layer
    {
        get => (int)Param;
        set
        {
            Param = value;
        }
    }
    public YinyangBall(Unit unit, int l = 1)
    {
        Duration = 300;
        Layer = l;
        value = 12;
        var s = unit.Us.skills.FirstOrDefault(x => x.skill.Name == "TreasureOrb").skill;
        if (s != null && s.Level >= 3)
            value = 18;
    }
    public override string GetDescription()
    {
        return string.Format( base.GetDescription(), value);
    }
    public override void OnTakeBodyDamage(Unit unit, ref Damage damage)
    {
        Layer--;
        damage -= Mathf.Min(damage.Value, value);

        if (Layer == 0)
            Quit(unit);
    }
    public override void OnGet(Unit unit, Status status)
    {
        foreach (var s in unit.Status)
        {
            if (s.Name == Name)
            {
                s.Param += ((YinyangBall)status).Layer;
                return;
            }
        }
        Get(unit);
        unit.Status.Add(status);
        unit.Ue.OnCreateBullet += AddActivator;
        unit.Ue.OnUnitTurnEnd += Activate;
    }
    public override void OnQuit(Unit unit)
    {
        if (Layer > 1)
        {
            Layer--;
            Duration += 300;
        }
        else
            Quit(unit);
        unit.Ue.OnCreateBullet -= AddActivator;
        unit.Ue.OnUnitTurnEnd -= Activate;
    }
    public void AddActivator(Unit unit, Bullet bullet)
    {
        if (bullet.Shape == ShapeBullet.Yinyang) { return; }
        if (unit.Up.RandomEnemyInVision(out Unit target))
            activator = new SkillContext(unit, target.Up.CurrentGrid);
    }
    public void Activate(Unit unit)
    {
        var (skill, weight) = unit.Us.skills.FirstOrDefault(x => x.skill.Name == "TreasureOrb");
        int level = 1;
        if (skill != null && skill.Level >= 2)
            level = 2;
        if (!unit.Status.Contains(this)) { return; }
        if (activator == null)
            return;
        new YinyangBallShoot().Activate(new SkillContext(activator.User, activator.GridOne, level));
        Layer--;
        activator = null;
    }
}
public class Weak : Status
{
    public override StatusType Type => StatusType.Negative;
    public Weak(float duration)
    {
        Duration = duration;
    }
    public override void OnDealDamage(Unit unit, ref Damage damage)
    {
        damage *= 0.7f;
    }
    public override void OnGet(Unit unit, Status status)
    {
        unit.Ua.Str -= 6;
        CombineTime(unit, status);
    }
    public override void OnQuit(Unit unit)
    {
        unit.Ua.Str += 6;
        Quit(unit);
    }
}
public class SpiritSeal : Status
{
    public override StatusType Type => StatusType.Negative;
    public SpiritSeal(float duration)
    {
        Duration = duration;
    }
    public override void OnGet(Unit unit, Status status)
    {
        unit.Ue.OnCheckSkillUsage.Add(OnCheckSkillUsage);
        CombineTime(unit, status);
    }
    public override void OnQuit(Unit unit)
    {
        unit.Ue.OnCheckSkillUsage.Remove(OnCheckSkillUsage);
        Quit(unit);
    }
    public bool OnCheckSkillUsage(Unit unit, Skill si, bool init)
    {
        return init && si.GetSpCost() <= 0;
    }
}
public class MagicSeal : Status
{
    public override StatusType Type => StatusType.Negative;
    public MagicSeal(float duration)
    {
        Duration = duration;
    }
    public override void OnGet(Unit unit, Status status)
    {
        unit.Ue.OnCheckSkillUsage.Add(OnCheckSkillUsage);
        CombineTime(unit, status);
    }
    public override void OnQuit(Unit unit)
    {
        unit.Ue.OnCheckSkillUsage.Remove(OnCheckSkillUsage);
        Quit(unit);
    }
    public bool OnCheckSkillUsage(Unit unit, Skill si, bool init)
    {
        return init && si.GetMpCost() <= 0;
    }
}


public class Daze : Status
{
    public override StatusType Type => StatusType.Negative;
    public Daze(float duration)
    {
        Duration = duration;
    }
    public override void OnGet(Unit unit, Status status)
    {
        unit.Ua.SpeedCombat -= 50;
        unit.Ua.BodyDamageAccuracy -= 0.5f;
        unit.Ua.BulletDamageAccuracy -= 0.5f;
        CombineTime(unit, status);
    }
    public override void OnQuit(Unit unit)
    {
        unit.Ua.SpeedCombat += 50;
        unit.Ua.BodyDamageAccuracy += 0.5f;
        unit.Ua.BulletDamageAccuracy += 0.5f;
        Quit(unit);
    }
}
public class Stun : Status
{
    public override StatusType Type => StatusType.Negative;
    public Stun(float duration)
    {
        Duration = duration;
    }
    public override void OnGet(Unit unit, Status status)
    {
        unit.Ua.SpeedCombat -= 50;
        CombineTime(unit, status);
    }
    public override void OnQuit(Unit unit)
    {
        unit.Ua.SpeedCombat += 50;
        Quit(unit);
    }
    public override void OnDealDamage(Unit unit, ref Damage damage)
    {
        damage *= 0.5f;
    }
}

public class Pinned : Status
{
    public override StatusType Type => StatusType.Negative;
    public Pinned(float duration)
    {
        Duration = duration;
    }
    public override void OnGet(Unit unit, Status status)
    {
        unit.Ua.DamageEvasion -= 0.2f;
        unit.Ue.OnCheckMoveUsage.Add(Pin);
        CombineTime(unit, status);
    }
    public override void OnQuit(Unit unit)
    {
        unit.Ua.DamageEvasion += 0.2f;
        unit.Ue.OnCheckMoveUsage.Remove(Pin);
        Quit(unit);
    }
    private bool Pin(Unit unit, bool init)
    {
        return false;
    }
}
public class Slow : Status
{
    public override StatusType Type => StatusType.Negative;
    public Slow(float duration)
    {
        Duration = 50;
    }
    public override void OnGet(Unit unit, Status status)
    {
        unit.Ua.SpeedGlobal -= 50;
        CombineTime(unit, status);
    }
    public override void OnQuit(Unit unit)
    {
        unit.Ua.SpeedGlobal += 50;
        Quit(unit);
    }
}
public class Burned : Status
{
    public override StatusType Type => StatusType.Negative;
    public int Layer
    {
        get => (int)Param;
        set
        {
            Param = value;
        }
    }
    public Burned(int layer, float duration)
    {
        Duration = duration;
        Layer = layer;
    }
    public override void OnDealBodyDamage(Unit unit, ref Damage damage)
    {
        damage *= 0.5f;
    }
    public override void OnGet(Unit unit, Status status)
    {
        foreach (var s in unit.Status)
        {
            if (s.Name == Name)
            {
                Duration = Math.Max(Duration, status.Duration);
                Layer = Math.Max(Layer, ((Burned)status).Layer);
                return;
            }
        }
        Get(unit);
        unit.Status.Add(status);
    }
    public override void OnQuit(Unit unit)
    {
        Quit(unit);
    }
    public override void OnTurnEnd(Unit unit)
    {
        unit.Ua.GetHp(-Layer);
    }
}

public class BulletShield : Status
{
    public float Value
    {
        get => (int)Param;
        set
        {
            Param = value;
        }
    }
    public BulletShield(float value, float time)
    {
        Duration = time;
        Value = value;
    }
    public override string GetDescription()
    {
        return string.Format(base.GetDescription(), Value);
    }
    public override void OnTakeBulletDamage(Unit unit, Skill skill, ref Damage damage)
    {
        var v = Mathf.Min(damage.Value, Value);
        damage -= v;
        Value -= v;
        if (Value == 0)
            Quit(unit);
    }
}

public class NumberShield : Status
{
    public float Layer
    {
        get => (int)Param;
        set
        {
            Param = value;
        }
    }
    public NumberShield(float value, float time)
    {
        Duration = time;
        Layer = value;
    }
    public override string GetDescription()
    {
        return string.Format(base.GetDescription(), Layer);
    }
    public override void OnTakeBulletDamage(Unit unit, Skill skill, ref Damage damage)
    {
        damage.Value = 0;
        Layer--;
        if (Layer == 0)
            Quit(unit);
    }
}
