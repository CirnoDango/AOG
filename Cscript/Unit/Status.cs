using Godot;
using System.ComponentModel;
using System.Linq;

public abstract class Status
{
    public string Name { get; set; }
    public string TrName => $"st{Name}";
    public float Duration { get; set; } // 回合持续时间
    public virtual void OnTurnStart(Unit unit) { }
    public virtual void OnTurnEnd(Unit unit) { }
    public virtual void OnTakeBulletDamage(Unit unit, Skill skill, ref float damage){; }
    public virtual void OnTakeBodyDamage(Unit unit, ref float damage){; }
    public virtual void OnDealDamage(Unit unit, ref float damage){; }
    //public virtual void OnMove(Unit unit, ref Vector2I newPos) { }
    //public virtual void OnSkillUse(Unit unit, Skill skill) { }
    public virtual void OnGet(Unit unit) { Get(unit); }
    public virtual void OnQuit(Unit unit) { Quit(unit); }
    public void Get(Unit unit)
    {
        Info.Print($"{unit.TrName} 施加状态 st{Name}！");
        if (unit == Player.PlayerUnit)
        {
            var s = new TextureRect
            {
                Texture = (Texture2D)GD.Load($"res://assets/Status/{Name}.png"),
                CustomMinimumSize = new Vector2(400, 400)
            };
            var b = new TextureRect
            {
                Texture = (Texture2D)GD.Load($"res://Cscript/UI/UI/skillbox.png"),
                CustomMinimumSize = new Vector2(400, 400)
            };
            // 创建 Label 并放在右下角
            var label = new Label
            {
                Text = $"{Duration/100:F0}",
                CustomMinimumSize = new Vector2(200, 200),
                SizeFlagsHorizontal = Control.SizeFlags.Fill,
                SizeFlagsVertical = Control.SizeFlags.Fill,
                AnchorRight = 1.0f,
                AnchorBottom = 1.0f,
                PivotOffset = new Vector2(200, 200), // 原点从中心偏移到左上角
                Position = new Vector2(-200, -200),   // 相对于右下角向左上偏移200

            };
            label.AddThemeFontSizeOverride("font_size", 200);
            s.AddChild(label);
            label.Position = new Vector2(240,240);
            s.AddChild(b);
            G.I.PlayerStatusBar.StatusRow.AddChild(s);
            G.I.PlayerStatusBar.StatusImages.Add(this, s);
        }
        var texture = (Texture2D)GD.Load($"res://assets/Status/{Name}Map.png");
        if (texture == null)
            return;
        var sprite = new Sprite2D
        {
            Texture = texture,
            Scale = new Vector2(1 / unit.imageSizeFactor, 1 / unit.imageSizeFactor)
        };
        unit.sprite.AddChild(sprite);
        unit.StatusImages.Add(this, sprite);
        
    }
    public void Quit(Unit unit)
    {
        Info.Print($"{unit.TrName} 不再 st{Name}");
        var status = unit.Status.FirstOrDefault(x => x.Name == Name);
        var node = unit.StatusImages[status];
        unit.sprite.RemoveChild(node);
        unit.StatusImages.Remove(unit.StatusImages.FirstOrDefault(x => x.Key.Name == Name).Key);
        unit.Status.Remove(unit.Status.FirstOrDefault(x => x.Name == Name));
        if (unit == Player.PlayerUnit)
        {
            var n = G.I.PlayerStatusBar.StatusImages[status];
            G.I.PlayerStatusBar.StatusRow.RemoveChild(n);
            G.I.PlayerStatusBar.StatusImages.Remove(G.I.PlayerStatusBar.StatusImages.FirstOrDefault(x => x.Key.Name == Name).Key);
        }
    }
}

public class Frozen : Status
{
    public Frozen(float duration)
    {
        Name = "Frozen";
        Duration = duration;
    }
    public override void OnTakeBulletDamage(Unit unit, Skill skill, ref float damage)
    {
        damage *= 0.1f;
    }

    public override void OnTakeBodyDamage(Unit unit, ref float damage)
    {
        damage *= 0.1f;
    }
    public override void OnGet(Unit unit)
    {
        unit.TimeEnergy -= Duration;
        Get(unit);
    }
}
public class Dark : Status
{
    public Dark(float duration)
    {
        Name = "Dark";
        Duration = duration;
    }
    public override void OnGet(Unit unit)
    {
        unit.Vision -= 8;
        Get(unit);
    }
    public override void OnQuit(Unit unit)
    {
        unit.Vision += 8;
        Quit(unit);
    }
}
