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
        sc.User.Up.MoveTo(sc.GridOne);
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
        sc.UnitOne.Ua.CheckBodyHit(damage, sc.User, this);
        sc.User.Ue.Attack(sc);
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
        Bullet.CreateBullet(sc.User, this, 6, sc.User.Up.Position, sc.GridOne.Position, 1, 8, ShapeBullet.Micro, ColorBullet.Green);
        Bullet.CreateBullet(sc.User, this, 6, sc.User.Up.Position, sc.GridOne.Position, 1.5f, 8, ShapeBullet.Micro, ColorBullet.Green);
        Bullet.CreateBullet(sc.User, this, 6, sc.User.Up.Position, sc.GridOne.Position, 2, 8, ShapeBullet.Micro, ColorBullet.Green);
    }
}
