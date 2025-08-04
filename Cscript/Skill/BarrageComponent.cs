using Godot;
using System.Collections.Generic;
using System.Linq;
public class Barrage
{
    public int MaxComponents { get; set; }
    public List<BarrageComponent> Components;

    public Barrage(int maxComponents = 8)
    {
        MaxComponents = maxComponents;
        Components = new List<BarrageComponent>(maxComponents);
        for (int i = 0; i < maxComponents; i++)
            Components.Add(null);
    }

    public void Execute(SkillContext sc)
    {
        List<BarrageComponent> executeBc = [.. Components.Where(x => x != null)];
        var executor = new Executor(executeBc, sc);
        executor.Execute();
    }
    public static Barrage Test()
    {
        Barrage barrage = new();
        barrage.Components[0] = (BarrageComponent)Item.GetTemplate("AddDamage");
        barrage.Components[1] = (BarrageComponent)Item.GetTemplate("CircleFire");
        barrage.Components[2] = (BarrageComponent)Item.GetTemplate("Way3Fire");
        barrage.Components[3] = (BarrageComponent)Item.GetTemplate("MultiSpeedFire");
        barrage.Components[4] = (BarrageComponent)Item.GetTemplate("RandomFire");
        barrage.Components[5] = new BulletModule
        {
            bulletContext = new BulletContext(10, 2, 10, "Small", ColorBullet.Red)
        };
        return barrage;
    }
}

public abstract class BarrageComponent : Item, IParamable
{
    public override bool CanEquip => false;

    public virtual void ApplyParameters(Dictionary<string, object> parameters) { }

    public virtual void Execute(ref List<BulletContext> lbc, Executor executor)
    {
        if (this is IBarrageComponentEvent ibce)
            executor.Events.Add(ibce);
        executor.Continue(lbc);
    }
    public virtual object RandomSummonParam()
    {
        return GetTemplate(Name);
    }
}

public class Executor(IEnumerable<BarrageComponent> components, SkillContext s)
{
    public SkillContext sc = s;
    public Queue<BarrageComponent> PendingComponents = new(components);
    public List<IBarrageComponentEvent> Events = [];
    public int draw = 1;
    public List<BulletContext> PendingLbc = [];
    public void Execute(List<BulletContext> lbc = null)
    {
        lbc ??= [];
        if (PendingComponents.Count > 0 && draw > 0)
        {
            BarrageComponent next;
            do
            {
                next = PendingComponents.Dequeue();
            } while (next == null && PendingComponents.Count > 0);
            next?.Execute(ref lbc, this);
        }
        else
        {
            FireOnce(lbc);
        }
    }
    public void FireOnce(List<BulletContext> lbc)
    {
        foreach (var evt in Events)
            evt.ApplyTo(ref lbc);
        foreach (var bc in lbc)
        {
            _ = new Bullet(sc.User, Skill.NameSkill["Shoot"], bc.Damage, sc.User.Position, sc.GridOne.Position,
                bc.Point, bc.Angle, bc.Speed, bc.MaxDistance, bc.Shape, bc.Color);
        }
    }
    public void Continue(List<BulletContext> lbc)
    {
        Execute(lbc);
    }
}
public interface IBarrageComponentEvent
{
    public void ApplyTo(ref List<BulletContext> lbc);
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
