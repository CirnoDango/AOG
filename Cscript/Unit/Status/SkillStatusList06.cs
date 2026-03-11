using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

public class SScarletNetherworld : Status
{
    public float Damage;
    public Unit User;
    public Skill Skill;
    public SScarletNetherworld(float damage, float duration, Unit user, Skill skill)
    {
        Damage = damage;
        Duration = duration;
        User = user;
        Skill = skill;
    }
    public override void OnGet(Unit unit, Status status)
    {
        if (!CombineTime(unit, status))
            return;
        if (unit.symbol.Contains("吸血鬼"))
        {
            unit.Ua.Mag += 5;
            unit.Ua.Spi += 5;
        }
        else
        {
            unit.Ua.Str -= 5;
            unit.Ua.Dex -= 5;
            unit.Ua.Con -= 5;
        }

    }
    public override void OnQuit(Unit unit)
    {
        if (unit.symbol.Contains("吸血鬼"))
        {
            unit.Ua.Mag -= 5;
            unit.Ua.Spi -= 5;
        }
        else
        {
            unit.Ua.Str += 5;
            unit.Ua.Dex += 5;
            unit.Ua.Con += 5;
        }
        base.OnQuit(unit);
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
    public override string GetDescription()
    {
        return string.Format(base.GetDescription(), FireBuff, Extra()[level - 1]);
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
            if (level == 4)
                target.Ua.TakeBulletDamage(new Damage(20, DamageType.sonic), unit, Parent);
        }
    }
}
public class SSpiralLightSteps : Status
{
    private Skill Parent;
    private int level;
    public float Speed
    {
        get => (float)Param;
        set
        {
            Param = value;
        }
    }
    public SSpiralLightSteps(float speed, float duration, Skill parent, int level)
    {
        Duration = duration;
        Speed = speed;
        Parent = parent;
        this.level = level;
    }
    public override string GetDescription()
    {
        return string.Format(base.GetDescription(), Speed, Extra()[level - 1]);
    }
    public override void OnGet(Unit unit, Status status)
    {
        unit.Ua.SpeedMove += Speed;
        unit.Ue.OnUnitMove += MoveDamage;
        CombineTime(unit, status);
    }
    public override void OnQuit(Unit unit)
    {
        unit.Ua.SpeedMove -= Speed;
        unit.Ue.OnUnitMove -= MoveDamage;
        Quit(unit);
    }
    public void MoveDamage(Unit unit)
    {
        foreach (Unit target in unit.Up.CurrentGrid.NearGrids(2).Where(x => x != unit.Up.CurrentGrid).Select(g => g.unit))
        {
            target?.Ua.TakeBulletDamage(new Damage(15, DamageType.sonic), unit, Parent);
            if (GD.Randf() < 0.2)
                target?.GetStatus(new Daze(300));
        }
    }
}
public class SCrimsonEnergyRelease : Status
{
    private int level;
    int[] t0 = [4, 6, 8, 8];
    public int Layer
    {
        get => (int)Param;
        set
        {
            Param = value;
        }
    }
    public SCrimsonEnergyRelease(int l = 1)
    {
        Duration = 100;
        Layer = l;
    }
    public override string GetDescription()
    {
        return string.Format( base.GetDescription(), t0[level - 1] * Layer);
    }
    public override void OnTakeBulletDamage(Unit unit, Skill skill, ref Damage damage)
    {
        damage -= damage * t0[level - 1] * Layer / 100f;
    }
    public override void OnDealBodyDamage(Unit unit, ref Damage damage)
    {
        damage += damage * t0[level - 1] * Layer / 100f;
    }
    public override void OnGet(Unit unit, Status status)
    {
        foreach (var s in unit.Status)
        {
            if (s.Name == Name)
            {
                s.Param += ((SCrimsonEnergyRelease)status).Layer;
                s.Param = Math.Min(s.Param, 10);
                return;
            }
        }
        Get(unit);
        unit.Status.Add(status);
        ((SCrimsonEnergyRelease)status).level =
            unit.Us.skills.FirstOrDefault(x => x.skill.Name == "CrimsonEnergyRelease").skill.Level;
    }
    public override void OnQuit(Unit unit)
    {
        if (level == 4 || Layer > 2)
        {
            Layer -= 2;
            Duration += 100;
        }
        else
            Quit(unit);
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
        Duration = 500;
        rcd = cd;
        Count = count;
        Cd = cd;
        Parent = parent;
    }
    public override string GetDescription()
    {
        return string.Format(base.GetDescription(), Count + 1);
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
            if (Count < 0.5f)
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
        Duration = 500;
        Speedup = speedup;
        Range = range;
        Speeddown = speeddown;
        AccDown = accDown;
        Parent = parent;
    }
    public override string GetDescription()
    {
        return string.Format(base.GetDescription(), Speedup, Range, Speeddown, AccDown);
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
        else if (!user.Up.CurrentGrid.NearGrids(Range).Select(x => x.Position).Contains(bullet.Position)
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