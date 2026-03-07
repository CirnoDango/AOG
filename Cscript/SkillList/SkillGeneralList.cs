using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Rest : Skill
{
    public Rest()
    {
        Description = "休息一回合";
        Targeting = new TargetType(new TargetRuleSelf());
    }

    protected override void StartActivate(SkillContext context)
    {
    }
}
public class Move : Skill
{
    public Move()
    {
        Description = "移动一个格子";
        Targeting = new TargetType(new TargetRuleGrid(), 1, 1);
    }

    protected override void StartActivate(SkillContext sc)
    {
        sc.User.Up.MoveTo(sc.GridOne);
    }
}
public class Interact : Skill
{
    public Interact()
    {
        Description = "与场景环境互动";

        Targeting = new TargetType(new TargetRuleGrid(), 1, 0);
    }
    public override float GetTimeCost() => 0;
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
        Description = "体术攻击";
        Targeting = new TargetType(new TargetRuleEnemy(), 1, 1);
    }

    protected override void StartActivate(SkillContext sc)
    {
        var weapons = sc.User.Equipment.EquippedItems.Where(x => x is IWeapon).ToList();
        if (weapons.Count > 0)
        {
            float accK = 0; float damK = 1;
            foreach (IWeapon weapon in weapons.Cast<IWeapon>())
            {
                accK -= 0.1f; damK *= 0.8f;
                sc.UnitOne.Ua.CheckBodyHit(weapon.BaseDamage() * damK, weapon.Accurency + accK, weapon.CritRate, sc.User, this);
            }
        }
        else
            sc.UnitOne.Ua.CheckBodyHit(new Damage(10+sc.User.Ua.Str, DamageType.strike), 0.8f, 0, sc.User, this);
        sc.User.Ue.Attack(sc);
    }
}
public class Shoot : Skill
{
    public Shoot()
    {
        SkillGroup = "General";
        Description = "发射弹幕！";
        Targeting = new TargetType(new TargetRuleAny(), 1, 8);
    }
    public override float GetSpCost() => 0;
    public override float GetCooldown() => 300;
    protected override void StartActivate(SkillContext sc)
    {
        Bullet.CreateBullet(sc.User, this, new Damage(6, DamageType.nature), sc.User.Up.Position, sc.GridOne.Position, 1, 8, ShapeBullet.Micro, ColorBullet.Green);
        Bullet.CreateBullet(sc.User, this, new Damage(6, DamageType.nature), sc.User.Up.Position, sc.GridOne.Position, 1.5f, 8, ShapeBullet.Micro, ColorBullet.Green);
        Bullet.CreateBullet(sc.User, this, new Damage(6, DamageType.nature), sc.User.Up.Position, sc.GridOne.Position, 2, 8, ShapeBullet.Micro, ColorBullet.Green);
    } 
}
