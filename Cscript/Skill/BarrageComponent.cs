using Godot;
using System.Collections.Generic;
public class Barrage
{
    public List<BarrageComponent> Components = [];
    public void Activate(SkillContext sc)
    {
        var executor = new Executor(Components, sc);
        executor.Execute();
    }
    public static Barrage Test()
    {
        Barrage barrage = new();
        var c1 = new AddDamage();
        var c2 = new CircleFire();
        var c3 = new BulletModule
        {
            bulletContext = new BulletContext(10, 2, 10, "Small", ColorBullet.Red)
        };
        barrage.Components = [c1, c2, c3];
        return barrage;
    }
}

public abstract class BarrageComponent : Item
{
    public override bool CanEquip => false;
    public virtual void Execute(ref List<BulletContext> lbc, Executor executor)
    {
        if (this is IBarrageComponentEvent ibce)
            executor.Events.Add(ibce);
        executor.Continue(lbc);
    }
}

public class Executor(IEnumerable<BarrageComponent> components, SkillContext s)
{
    public SkillContext sc = s;
    public Queue<BarrageComponent> PendingComponents = new(components);
    public List<IBarrageComponentEvent> Events = [];

    public void Execute(List<BulletContext> lbc = null)
    {
        if (PendingComponents.Count > 0)
        {
            var next = PendingComponents.Dequeue();
            next.Execute(ref lbc, this);
        }
        else
        {
            foreach (var evt in Events)
                evt.ApplyTo(ref lbc);
            foreach (var bc in lbc)
            {
                _ = new Bullet(sc.User, Skill.NameSkill["Shoot"], bc.Damage, sc.User.Position, sc.GridOne.Position,
                    bc.Point, bc.Angle, bc.Speed, bc.MaxDistance, bc.Shape, bc.Color);
            }
        }
    }

    public void Continue(List<BulletContext> lbc)
    {
        Execute(lbc);
    }
}
public class AddDamage : BarrageComponent, IBarrageComponentEvent    
{
    public float Bonus = 5;

    public AddDamage()
    {
        Name = "AddDamage";
        Weight = 0.3f;
        Description = "+5伤害";
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
            bc.Damage += Bonus;
    }
}
public class CircleFire : BarrageComponent, IBarrageComponentEvent
{
    public CircleFire()
    {
        Name = "CircleFire";
        Weight = 0.3f;
        Description = "环形弹";
    }
    public void ApplyTo(ref List<BulletContext> lbc)
    {
        List<BulletContext> nlbc = [];
        foreach (var bc in lbc)
        {
            for (int a = 0; a < 360; a += 30)
            {
                BulletContext nbc = bc.Clone();
                nbc.Angle = a;
                nlbc.Add(nbc);
            }
        }
        lbc = nlbc;
    }
}
public interface IBarrageComponentEvent
{
    public void ApplyTo(ref List<BulletContext> lbc);
}

public class BulletModule : BarrageComponent
{
    public BulletContext bulletContext;
    public BulletModule()
    {
        Name = "BulletModule";
        Weight = 0.3f;
        Description = "子弹";
    }
    public override void Execute(ref List<BulletContext> lbc, Executor executor)
    {
        var bullet = bulletContext;
        lbc = [bullet];
        executor.Continue(lbc);
    }
}

public class BulletContext(float damage, float speed, float maxDistance, string shape, ColorBullet color)
{
    public float Damage = damage;
    public float Speed = speed;
    public float MaxDistance = maxDistance;
    public string Shape = shape;
    public ColorBullet Color = color;

    public Vector2 Point = Vector2.Zero;
    public float Angle = 0;

    internal BulletContext Clone()
    {
        return new BulletContext(Damage, Speed, MaxDistance, Shape, Color)
        {
            Point = Point,
            Angle = Angle,
        };
    }
}
