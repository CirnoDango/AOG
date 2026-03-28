using Godot;
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

public class SKimontonkou : Status
{
    public Unit User;
    public Skill Skill;
    public int Number
    {
        get => (int)Param;
        set
        {
            Param = value;
        }
    }
    public SKimontonkou(int number, float duration, Unit user, Skill skill)
    {
        Duration = duration;
        Number = number;
        User = user;
        Skill = skill;
    }
    public override void OnGet(Unit unit, Status status)
    {
        unit.Ua.DamageEvasion += 0.2f;
        unit.Ua.BulletGraze += 0.2f;
        unit.Ue.OnGraze.Add(GrazeShoot);
        unit.Ue.OnEvade += EvadeShoot;
        CombineTime(unit, status);
    }
    public override void OnQuit(Unit unit)
    {
        unit.Ua.DamageEvasion -= 0.2f;
        unit.Ua.BulletGraze -= 0.2f;
        unit.Ue.OnGraze.Remove(GrazeShoot);
        unit.Ue.OnEvade -= EvadeShoot;
        base.OnQuit(unit);
    }
    private Damage GrazeShoot(Unit unit, Unit unit1, Damage d)
    {
        if(unit.Ua.CurrentSp >= 7)
        {
            unit.Ua.GetSp(-7);
            for(float a = 0; a < 359; a += 360 / Number)
            {
                var s = 4 - 2 * MathF.Pow(((a % 90 - 45) / 45), 2);
                Bullet.CreateBullet(unit, Skill, new Damage(10, DamageType.pierce),
                unit.Up.Position, unit.Up.Position + MathEx.RandomV2(), Vector2.Zero,
                a, s, 3*s, ShapeBullet.ArrowMedium, ColorBullet.Azure);
            }
        }
        return d;
    }
    private void EvadeShoot(Unit unit)
    {
        if (unit.Ua.CurrentSp >= 7)
        {
            unit.Ua.GetSp(-7);
            for (float a = 0; a < 359; a += 360 / Number)
            {
                var s = 4 - 2 * MathF.Pow(((a % 90 - 45) / 45), 2);
                Bullet.CreateBullet(unit, Skill, new Damage(10, DamageType.pierce),
                unit.Up.Position, unit.Up.Position + MathEx.RandomV2(), Vector2.Zero,
                a, s, 3 * s, ShapeBullet.ArrowMedium, ColorBullet.Azure);
            }
        }
    }
}