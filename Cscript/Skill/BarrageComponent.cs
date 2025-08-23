using Godot;
using System;
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
    public static explicit operator Barrage(Dictionary<string, object> parameters)
    {
        Barrage barrage = new((int)parameters["MaxComponents"]);
        if (!parameters.TryGetValue("Components", out _) || 
            parameters["Components"] is not IEnumerable<object> componentNames || 
            !componentNames.Any())
            return barrage.RandomSummonParam();
        int i = 0;
        foreach (var name in componentNames)
        {
            if (i >= barrage.MaxComponents) break;
            if (name is string s)
                barrage.Components[i] = (BarrageComponent)Item.GetItemName(s).RandomSummonParam();
            else if (name is Dictionary<string, object> t)
            {
                BarrageComponent bc = (BarrageComponent)Item.GetItemName((string)t["Name"]);
                bc.ApplyParameters((Dictionary<string, object>)t["Parameters"]);
                barrage.Components[i] = bc;
            }
            i++;
        }
        return barrage;
    }
    public Barrage RandomSummonParam()
    {
        var bcDeck = Item.ItemDeck.Where(x => x is BarrageComponent && x is not BulletModule).ToList();
        for(int i = 0; i < MaxComponents - 1; i++)
        {
            Components[i] = (BarrageComponent)Item.GetItemName(bcDeck[GD.RandRange(0, bcDeck.Count - 1)].Name).RandomSummonParam();
        }
        Components[^1] = (BarrageComponent)new BulletModule().RandomSummonParam();
        return this;
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
        barrage.Components[0] = (BarrageComponent)Item.GetItemName("AddDamage");
        barrage.Components[1] = (BarrageComponent)Item.GetItemName("CircleFire");
        barrage.Components[2] = (BarrageComponent)Item.GetItemName("Way3Fire");
        barrage.Components[3] = (BarrageComponent)Item.GetItemName("MultiSpeedFire");
        barrage.Components[4] = (BarrageComponent)Item.GetItemName("RandomFire");
        barrage.Components[5] = new BulletModule
        {
            bulletContext = new BulletContext(10, 2, 10, "Small", ColorBullet.Red)
        };
        return barrage;
    }
}

public abstract class BarrageComponent : Item
{
    public override bool CanEquip => false;

    public override void ApplyParameters(Dictionary<string, object> parameters) { }

    public virtual void Execute(ref List<BulletContext> lbc, Executor executor)
    {
        if (this is IBarrageComponentEvent ibce)
            executor.Events.Add(ibce);
        executor.Continue(lbc);
    }
    public override Item RandomSummonParam()
    {
        return GetItemName(Name);
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
            Bullet.CreateBullet(sc.User, Skill.NameSkill["Shoot"], bc.damage, sc.User.Position, sc.GridOne.Position,
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
    public float damage = damage;
    public float Speed = speed;
    public float MaxDistance = maxDistance;
    public string Shape = shape;
    public ColorBullet Color = color;

    public Vector2 Point = Vector2.Zero;
    public float Angle = 0;

    internal BulletContext Clone()
    {
        return new BulletContext(damage, Speed, MaxDistance, Shape, Color)
        {
            Point = Point,
            Angle = Angle,
        };
    }

    public static explicit operator BulletContext(Dictionary<string, object> dict)
    {
        return new BulletContext(
            Convert.ToSingle(dict["damage"]),
            Convert.ToSingle(dict["speed"]),
            Convert.ToSingle(dict["maxDistance"]),
            (string)dict["shape"],
            (ColorBullet)Enum.Parse(typeof(ColorBullet), (string)dict["color"])
        );
    }
}
