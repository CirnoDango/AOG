using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
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
        Barrage barrage = new(Convert.ToInt32(parameters["MaxComponents"]));
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
    public static Dictionary <string, object> GetParam(Barrage barrage)
    {
        var parameters = new Dictionary<string, object>();

        // MaxComponents
        parameters["MaxComponents"] = barrage.MaxComponents;

        // Components
        var components = new List<object>();

        foreach (var comp in barrage.Components)
        {
            if (comp == null)
            {
                components.Add(null);
                continue;
            }
            comp.GetParam();
            var compDict = new Dictionary<string, object>
            {
                ["Name"] = comp.Name,
                ["Parameters"] = comp.Params
            };

            components.Add(compDict);
        }

        parameters["Components"] = components;

        return parameters;
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
    public override void ApplyParameters(Dictionary<string, object> parameters)
    {
        base.ApplyParameters(parameters);
        Params = parameters;
    }
    public abstract void Activate(ref List<BulletContext> lbc);
    public override Item RandomSummonParam()
    {
        return CreateItem(Name);
    }
}

public class Executor
{
    public SkillContext sc;

    // 牌库
    public List<BarrageComponent> PendingComponents;

    // 子 -> 父
    public Dictionary<BarrageComponent, BarrageComponent> DrawPairs = new();

    // 终端牌
    public List<BarrageComponent> terminals = new();

    // 当前执行顺序
    public List<BarrageComponent> ExecuteOrder = new();

    private int deckIndex = 0;

    public List<BulletContext> lbc = [];
    public Executor(List<BarrageComponent> lbc, SkillContext s)
    {
        PendingComponents = lbc;
        sc = s;
    }

    public void Execute()
    {
        deckIndex = 0;
        DrawPairs.Clear();
        terminals.Clear();
        ExecuteOrder.Clear();

        if (PendingComponents.Count == 0)
            return;

        // 根节点虚拟
        var root = new DrawNode(null, 1);

        var stack = new Stack<DrawNode>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var node = stack.Peek();

            // 当前节点没有剩余抽牌次数
            if (node.remainingDraw == 0)
            {
                stack.Pop();

                // 如果是真实牌，并且 draw==0 → terminal
                if (node.component != null && node.component.draw == 0)
                    terminals.Add(node.component);

                continue;
            }

            // 没牌了
            if (deckIndex >= PendingComponents.Count)
                break;

            var next = PendingComponents[deckIndex++];
            node.remainingDraw--;

            if (next == null)
                continue;

            ExecuteOrder.Add(next);

            if (node.component != null)
                DrawPairs[next] = node.component;

            var child = new DrawNode(next, next.draw);

            stack.Push(child);
        }
        foreach(var t in terminals)
        {
            List<BulletContext> bc = [];
            var a = t;
            do
            {
                a.Activate(ref bc);
                a = DrawPairs.GetValueOrDefault(a);
            } while (a != null);
            lbc.AddRange(bc);
        }
        Fire(lbc);
    }

    class DrawNode
    {
        public BarrageComponent component;
        public int remainingDraw;

        public DrawNode(BarrageComponent c, int draw)
        {
            component = c;
            remainingDraw = draw;
        }
    }
    public void Fire(List<BulletContext> lbc)
    {
        foreach (var bc in lbc)
        {
            Bullet b = Bullet.CreateBullet(sc.User, Skill.NameSkill["Shoot"], bc.damage, sc.User.Up.Position, sc.GridOne.Position,
                bc.Point, bc.Angle, bc.Speed, bc.MaxDistance, bc.Shape, bc.Color);
            b.crit = bc.crit;
            b.NewUpdateEvents = bc.UpdateEvents;
            bc.AfterSummonEvents?.Invoke(b);
        }
    }
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
