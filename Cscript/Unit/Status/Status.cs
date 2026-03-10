using Godot;
using System;
using System.Linq;
public enum StatusType
{
    Positive, Negative
}
public abstract class Status
{
    public string Name => GetType().Name;
    public string TrName => $"st{Name}";
    public virtual StatusType Type { get; set; } = StatusType.Positive;
    public float Duration { get; set; } // 回合持续时间
    public float Param = -999;
    public Label label;
    public Label param;
    public virtual void OnTurnStart(Unit unit) { }
    public virtual void OnTurnEnd(Unit unit) { }
    public virtual void OnTakeBulletDamage(Unit unit, Skill skill, ref Damage damage){; }
    public virtual void OnTakeBodyDamage(Unit unit, ref Damage damage){; }
    public virtual void OnDealDamage(Unit unit, ref Damage damage){; }
    public virtual void OnDealBodyDamage(Unit unit, ref Damage damage) {; }
    public virtual void OnGet(Unit unit, Status status) { Get(unit); unit.Status.Add(status); }
    public virtual void OnQuit(Unit unit) { Quit(unit); }
    public virtual string GetDescription()
    {
        return TextEx.Tr($"sd{Name}");
    }
    public string[] Extra(string index = "0")
    {
        string[] d = ["", "", "", $"sd{Name}{index}"];
        return d;
    }
    public void Get(Unit unit)
    {
        Info.Print($"{unit.TrName} 施加状态 st{Name} ！");
        if (unit == Player.PlayerUnit)
        {
            var s = new TextureRect
            {
                Texture = (Texture2D)GD.Load($"res://Assets/Status/{Name}.png"),
                CustomMinimumSize = new Vector2(400, 400)
            };
            var b = new TextureRect
            {
                Texture = (Texture2D)GD.Load($"res://Assets/UI/skillbox.png"),
                CustomMinimumSize = new Vector2(400, 400)
            };
            // 创建 Label 并放在右下角
            label = new Label
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
            if(Param != -999)
            {
                param = new Label
                {
                    Text = $"{Mathf.Round(Param)}",
                    CustomMinimumSize = new Vector2(200, 200),
                    SizeFlagsHorizontal = Control.SizeFlags.Fill,
                    SizeFlagsVertical = Control.SizeFlags.Fill,
                    AnchorRight = 1.0f,
                    AnchorBottom = 1.0f,
                    PivotOffset = new Vector2(200, 200),
                    Position = new Vector2(-200, -200),
                    Modulate = new Color(1, 1, 0)
                };
                param.AddThemeFontSizeOverride("font_size", 200);
                s.AddChild(param);
                param.Position = new Vector2(240, 60);
            }
            s.AddChild(b);
            G.I.PlayerStatusBar.StatusRow.AddChild(s);
            G.I.PlayerStatusBar.StatusImages.Add(this, s);
            s.MouseEntered += () =>
            {
                G.I.SkillBar.skillInfo.Text = GetDescription();
            };
            s.MouseExited += () => {
                G.I.SkillBar.skillInfo.Text = "";
            };
        }
        string path = $"res://Assets/Status/{Name}Map.png";
        Texture2D texture = null;
        texture = GD.Load<Texture2D>(path);
        if (texture == null)
            return;
        var sprite = new Sprite2D
        {
            Texture = texture,
            Scale = new Vector2(1 / unit.Ua.imageSizeFactor, 1 / unit.Ua.imageSizeFactor)
        };
        unit.Up.sprite.AddChild(sprite);
        unit.StatusImages.Add(this, sprite);
        
    }
    public void Quit(Unit unit)
    {
        Info.Print($"{unit.TrName} 不再 st{Name}");
        var status = unit.Status.FirstOrDefault(x => x.Name == Name);
        var node = unit.StatusImages.FirstOrDefault(x => x.Key == status).Value;
        if (node != null)
        {
            unit.Up.sprite.RemoveChild(node);
            unit.StatusImages.Remove(unit.StatusImages.FirstOrDefault(x => x.Key.Name == Name).Key);
        }
        unit.Status.Remove(unit.Status.FirstOrDefault(x => x.Name == Name));
        if (unit == Player.PlayerUnit)
        {
            var n = G.I.PlayerStatusBar.StatusImages[status];
            G.I.PlayerStatusBar.StatusRow.RemoveChild(n);
            G.I.PlayerStatusBar.StatusImages.Remove(G.I.PlayerStatusBar.StatusImages.FirstOrDefault(x => x.Key.Name == Name).Key);
        }
    }
    public bool CombineTime(Unit unit, Status status)
    {
        foreach (var s in unit.Status)
        {
            if (s.Name == Name)
            {
                Duration += status.Duration;
                return false;
            }
        }
        Get(unit);
        unit.Status.Add(status);
        return true;
    }
}