using System;
using static System.Collections.Specialized.BitVector32;

public class SFlowerWitherAway : Status
{
    public Unit User;
    public Skill Skill;
    int realCon = -999;
    int lastCon;
    int setCon;
    public float Damage
    {
        get => (float)Param;
        set
        {
            Param = value;
        }
    }
    public SFlowerWitherAway(float damage, int con, float duration, Unit user, Skill skill)
    {
        Damage = damage;
        Duration = duration;
        setCon = con;
        User = user;
        Skill = skill;
    }
    public override void OnGet(Unit unit, Status status)
    {
        CombineTime(unit, status);
        SetCon(unit);
    }
    public override void OnQuit(Unit unit)
    {
        unit.Ua.Con = realCon;
        base.OnQuit(unit);
    }
    public override void OnTurnEnd(Unit unit)
    {
        unit.Ua.TakeBodyDamage(new Damage(Damage, DamageType.cold), User, Skill);
        SetCon(unit);
    }
    public override void OnTurnStart(Unit unit)
    {
        SetCon(unit);
    }
    private void SetCon(Unit unit)
    {
        if (realCon == -999)
        {
            realCon = unit.Ua.Con;
            lastCon = realCon;
        }
        else
            realCon += (unit.Ua.Con - lastCon);
        unit.Ua.Con = Math.Min(realCon, setCon);
        lastCon = unit.Ua.Con;
    }
}