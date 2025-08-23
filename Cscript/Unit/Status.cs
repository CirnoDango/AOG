using Godot;
using System.ComponentModel;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

public abstract class Status
{
    public string Name { get; set; }
    public string TrName => $"st{Name}";
    public float Duration { get; set; } // 回合持续时间
    public float Param = -999;
    public Label label;
    public Label param;
    public virtual void OnTurnStart(Unit unit) { }
    public virtual void OnTurnEnd(Unit unit) { }
    public virtual void OnTakeBulletDamage(Unit unit, Skill skill, ref float damage){; }
    public virtual void OnTakeBodyDamage(Unit unit, ref float damage){; }
    public virtual void OnDealDamage(Unit unit, ref float damage){; }
    //public virtual void OnMove(Unit unit, ref Vector2I newPos) { }
    //public virtual void OnSkillUse(Unit unit, Skill skill) { }
    public virtual void OnGet(Unit unit, Status status) { Get(unit); }
    public virtual void OnQuit(Unit unit) { Quit(unit); }
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
                };
                param.Modulate = new Color(1, 1, 0);
                param.AddThemeFontSizeOverride("font_size", 200);
                s.AddChild(param);
                param.Position = new Vector2(240, 60);
            }
            s.AddChild(b);
            G.I.PlayerStatusBar.StatusRow.AddChild(s);
            G.I.PlayerStatusBar.StatusImages.Add(this, s);

        }
        string path = $"res://Assets/Status/{Name}Map.png";
        Texture2D texture = null;
        if (FileAccess.FileExists(path))
        {
            texture = GD.Load<Texture2D>(path);
        }
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
        var node = unit.StatusImages.FirstOrDefault(x => x.Key == status).Value;
        if (node != null)
        {
            unit.sprite.RemoveChild(node);
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
    public override void OnGet(Unit unit, Status status)
    {
        unit.TimeEnergy -= Duration;
        CombineTime(unit, status);
    }
}
public class Dark : Status
{
    public Dark(float duration)
    {
        Name = "Dark";
        Duration = duration;
    }
    public override void OnGet(Unit unit, Status status)
    {
        if (CombineTime(unit, status))
            unit.Vision -= 8;
    }
    public override void OnQuit(Unit unit)
    {
        unit.Vision += 8;
        Quit(unit);
    }
}
public class YinyangBall : Status
{
    public static SkillContext activator;
    public int Layer
    {
        get => (int)Param;
        set
        {
            Param = value;
        }
    }
    public YinyangBall(int l = 1)
    {
        Name = "YinyangBall";
        Duration = 500;
        Layer = l;
    }
    public override void OnTakeBodyDamage(Unit unit, ref float damage)
    {
        Layer--;
        var (skill, weight) = unit.skills.FirstOrDefault(x => x.skill.Name == "TreasureOrb");

        if (skill != null && skill.Level >= 3)
            damage -= Mathf.Min(damage, 18);
        else
            damage -= Mathf.Min(damage, 12);

        if (Layer == 0)
            Quit(unit);
    }
    public override void OnGet(Unit unit, Status status)
    {
        foreach (var s in unit.Status)
        {
            if (s.Name == Name)
            {
                s.Param += ((YinyangBall)status).Layer;
                return;
            }
        }
        Get(unit);
        unit.Status.Add(status);
        GameEvents.OnCreateBullet += AddActivator;
        GameEvents.OnUnitTurnEnd += Activate;
    }
    public override void OnQuit(Unit unit)
    {
        if (Layer > 1)
        {
            Layer--;
            Duration += 500;
        }
        else
            Quit(unit);
        GameEvents.OnCreateBullet -= AddActivator;
        GameEvents.OnUnitTurnEnd -= Activate;
    }
    public void AddActivator(Unit unit, Bullet bullet)
    {
        if (!unit.Status.Contains(this) || bullet.Shape == "YinYang") { return; }
        if (unit.RandomEnemyInVision(out Unit target))
            activator = new SkillContext(unit, target.CurrentGrid);
    }
    public void Activate(Unit unit)
    {
        var (skill, weight) = unit.skills.FirstOrDefault(x => x.skill.Name == "TreasureOrb");
        int level = 1;
        if (skill != null && skill.Level >= 2)
            level = 2;
        if (!unit.Status.Contains(this)) { return; }
        if (activator == null)
            return;
        new YinyangBallShoot().Activate(new SkillContext(activator.User, activator.GridOne, level));
        Layer--;
        activator = null;
    }
}
public class Weak : Status
{
    public Weak(float duration)
    {
        Name = "Weak";
        Duration = duration;
    }
    public override void OnDealDamage(Unit unit, ref float damage)
    {
        damage *= 0.7f;
    }
    public override void OnGet(Unit unit, Status status)
    {
        unit.Ua.Str -= 6;
        CombineTime(unit, status);
    }
    public override void OnQuit(Unit unit)
    {
        unit.Ua.Str += 6;
    }
}
public class SpiritSeal : Status
{
    public SpiritSeal(float duration)
    {
        Name = "SpiritSeal";
        Duration = duration;
    }
    public override void OnGet(Unit unit, Status status)
    {
        GameEvents.OnCheckSkillUsage.Add(OnCheckSkillUsage);
    }
    public override void OnQuit(Unit unit)
    {
        GameEvents.OnCheckSkillUsage.Remove(OnCheckSkillUsage);
    }
    public bool OnCheckSkillUsage(Unit unit, SkillInstance si, bool init)
    {
        if (unit.Status.Contains(this))
            return init && si.Template.GetSpCost(si.Level) <= 0;
        return init;
    }
}
public class MagicSeal : Status
{
    public MagicSeal(float duration)
    {
        Name = "MagicSeal";
        Duration = duration;
    }
    public override void OnGet(Unit unit, Status status)
    {
        GameEvents.OnCheckSkillUsage.Add(OnCheckSkillUsage);
    }
    public override void OnQuit(Unit unit)
    {
        GameEvents.OnCheckSkillUsage.Remove(OnCheckSkillUsage);
    }
    public bool OnCheckSkillUsage(Unit unit, SkillInstance si, bool init)
    {
        if (unit.Status.Contains(this))
            return init && si.Template.GetMpCost(si.Level) <= 0;
        return init;
    }
}
