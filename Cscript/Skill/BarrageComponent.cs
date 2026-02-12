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
                barrage.Components[i] = (BarrageComponent)Item.CreateItem(s).RandomSummonParam();
            else if (name is Dictionary<string, object> t)
            {
                BarrageComponent bc = (BarrageComponent)Item.CreateItem((string)t["Name"]);
                bc.ApplyParameters((Dictionary<string, object>)t["Parameters"]);
                barrage.Components[i] = bc;
            }
            i++;
        }
        return barrage;
    }
    public Barrage RandomSummonParam()
    {
        int draw2num = (MaxComponents - 2) / 3;
        int fixNum = MaxComponents - 3 * draw2num - 2;
        bool fire = true;
        for (int i = 0; i < MaxComponents; i++)
        {
            if (i < draw2num)
                Components[i] = (BarrageComponent)Item.CreateItem(GroupList("Draw2")[GD.RandRange(0, GroupList("Draw2").Count - 1)].Name).RandomSummonParam();
            else if (i < draw2num + fixNum)
                Components[i] = (BarrageComponent)Item.CreateItem(GroupList("")[GD.RandRange(0, GroupList("").Count - 1)].Name).RandomSummonParam();
            else if (fire)
            {
                Components[i] = (BarrageComponent)Item.CreateItem(GroupList("Fire")[GD.RandRange(0, GroupList("Fire").Count - 1)].Name).RandomSummonParam();
                fire = false;
            }
            else
            {
                Components[i] = (BarrageComponent)new BulletModule().RandomSummonParam();
                fire = true;
            }
        }
        return this;

        List<Item> GroupList(string groupName) =>
            [.. Item.ItemDeck.Where(x => x is BarrageComponent bc && bc.group == groupName)];
    }
    public void Execute(SkillContext sc)
    {
        List<BarrageComponent> executeBc = [.. Components.Where(x => x != null)];
        var executor = new Executor(executeBc, sc);
        executor.Execute();
    }
}

public abstract class BarrageComponent : Item
{
    public override bool CanEquip => false;
    public string group = "";
    public float SpCost = 0;
    public float CoolDown = 0;
    public int draw = 1;
    public override float Weight => 0;
    public override void ApplyParameters(Dictionary<string, object> parameters) { }
    
    public virtual void Execute(ref List<BulletContext> lbc, Executor executor)
    {
        if (this is IBarrageComponentEvent ibce)
            executor.Events.Add(ibce);
        executor.Continue(lbc);
    }
    public override Item RandomSummonParam()
    {
        return CreateItem(Name);
    }
}

public class Executor(IEnumerable<BarrageComponent> components, SkillContext s)
{
    public SkillContext sc = s;
    public Queue<BarrageComponent> PendingComponents = new(components);
    public Dictionary<BarrageComponent, BarrageComponent> DrawPairs = [];
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
            draw--;
            draw += next.draw;
            next?.Execute(ref lbc, this);
        }
    }
    public void FireOnce(List<BulletContext> lbc)
    {
        Events.Reverse();
        foreach (var evt in Events)
            evt.ApplyTo(ref lbc);
        foreach (var bc in lbc)
        {
            Bullet b = Bullet.CreateBullet(sc.User, Skill.NameSkill["Shoot"], bc.damage, sc.User.Up.Position, sc.GridOne.Position,
                bc.Point, bc.Angle, bc.Speed, bc.MaxDistance, bc.Shape, bc.Color);
            b.crit = bc.crit;
            b.NewUpdateEvents = bc.UpdateEvents;
            bc.AfterSummonEvents?.Invoke(b);
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

public class BulletContext(Damage damage, float speed, float maxDistance, ShapeBullet shape, ColorBullet color)
{
    public Damage damage = damage;
    public float Speed = speed;
    public float MaxDistance = maxDistance;
    public ShapeBullet Shape = shape;
    public ColorBullet Color = color;
    public float crit = 0;
    public Vector2 Point = Vector2.Zero;
    public float Angle = 0;
    public Action<Bullet, float> UpdateEvents { get; set; }
    public Action<Bullet> AfterSummonEvents { get; set; }
    internal BulletContext Clone()
    {
        return new BulletContext(damage, Speed, MaxDistance, Shape, Color)
        {
            Point = Point,
            Angle = Angle,
            UpdateEvents = UpdateEvents,
            AfterSummonEvents = AfterSummonEvents
        };
    }

    public static explicit operator BulletContext(Dictionary<string, object> dict)
    {
        return new BulletContext(
            new Damage(Convert.ToSingle(dict["damage"]), (DamageType)Enum.Parse(typeof(DamageType), (string)dict["type"])),
            Convert.ToSingle(dict["speed"]),
            Convert.ToSingle(dict["maxDistance"]),
            (ShapeBullet)Enum.Parse(typeof(ShapeBullet), (string)dict["shape"]),
            (ColorBullet)Enum.Parse(typeof(ColorBullet), (string)dict["color"])
        );
    }
}
