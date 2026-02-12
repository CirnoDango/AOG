using Godot;
using System;
using System.Linq;

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
    public virtual void OnTakeBulletDamage(Unit unit, Skill skill, ref Damage damage){; }
    public virtual void OnTakeBodyDamage(Unit unit, ref Damage damage){; }
    public virtual void OnDealDamage(Unit unit, ref Damage damage){; }
    public virtual void OnDealBodyDamage(Unit unit, ref Damage damage) {; }
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
                    Modulate = new Color(1, 1, 0)
                };
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

public class Frozen : Status
{
    public Frozen(float duration)
    {
        Name = "Frozen";
        Duration = duration;
    }
    public override void OnTakeBulletDamage(Unit unit, Skill skill, ref Damage damage)
    {
        damage *= 0.1f;
    }

    public override void OnTakeBodyDamage(Unit unit, ref Damage damage)
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
            unit.Up.Vision -= 8;
    }
    public override void OnQuit(Unit unit)
    {
        unit.Up.Vision += 8;
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
        Duration = 300;
        Layer = l;
    }
    public override void OnTakeBodyDamage(Unit unit, ref Damage damage)
    {
        Layer--;
        var (skill, weight) = unit.Us.skills.FirstOrDefault(x => x.skill.Name == "TreasureOrb");

        if (skill != null && skill.Level >= 3)
            damage -= Mathf.Min(damage.Value, 18);
        else
            damage -= Mathf.Min(damage.Value, 12);

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
        unit.Ue.OnCreateBullet += AddActivator;
        unit.Ue.OnUnitTurnEnd += Activate;
    }
    public override void OnQuit(Unit unit)
    {
        if (Layer > 1)
        {
            Layer--;
            Duration += 300;
        }
        else
            Quit(unit);
        unit.Ue.OnCreateBullet -= AddActivator;
        unit.Ue.OnUnitTurnEnd -= Activate;
    }
    public void AddActivator(Unit unit, Bullet bullet)
    {
        if (bullet.Shape == ShapeBullet.Yinyang) { return; }
        if (unit.Up.RandomEnemyInVision(out Unit target))
            activator = new SkillContext(unit, target.Up.CurrentGrid);
    }
    public void Activate(Unit unit)
    {
        var (skill, weight) = unit.Us.skills.FirstOrDefault(x => x.skill.Name == "TreasureOrb");
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
    public override void OnDealDamage(Unit unit, ref Damage damage)
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
        Quit(unit);
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
        unit.Ue.OnCheckSkillUsage.Add(OnCheckSkillUsage);
        CombineTime(unit, status);
    }
    public override void OnQuit(Unit unit)
    {
        unit.Ue.OnCheckSkillUsage.Remove(OnCheckSkillUsage);
        Quit(unit);
    }
    public bool OnCheckSkillUsage(Unit unit, SkillInstance si, bool init)
    {
        return init && si.Template.GetSpCost(si.Level) <= 0;
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
        unit.Ue.OnCheckSkillUsage.Add(OnCheckSkillUsage);
        CombineTime(unit, status);
    }
    public override void OnQuit(Unit unit)
    {
        unit.Ue.OnCheckSkillUsage.Remove(OnCheckSkillUsage);
        Quit(unit);
    }
    public bool OnCheckSkillUsage(Unit unit, SkillInstance si, bool init)
    {
        return init && si.Template.GetMpCost(si.Level) <= 0;
    }
}


public class Daze : Status
{
    public Daze(float duration)
    {
        Name = "Daze";
        Duration = duration;
    }
    public override void OnGet(Unit unit, Status status) 
    {
        unit.Ua.SpeedCombat -= 50;
        unit.Ua.BodyDamageAccuracy -= 0.5f;
        unit.Ua.BulletDamageAccuracy -= 0.5f;
        CombineTime(unit, status);
    }
    public override void OnQuit(Unit unit)
    {
        unit.Ua.SpeedCombat += 50;
        unit.Ua.BodyDamageAccuracy += 0.5f;
        unit.Ua.BulletDamageAccuracy += 0.5f;
        Quit(unit);
    }
}
public class Stun : Status
{
    public Stun(float duration)
    {
        Name = "Stun";
        Duration = duration;
    }
    public override void OnGet(Unit unit, Status status)
    {
        unit.Ua.SpeedCombat -= 50;
        CombineTime(unit, status);
    }
    public override void OnQuit(Unit unit)
    {
        unit.Ua.SpeedCombat += 50;
        Quit(unit);
    }
    public override void OnDealDamage(Unit unit, ref Damage damage)
    {
        damage *= 0.5f;
    }
}

public class Pinned : Status
{
    public Pinned(float duration)
    {
        Name = "Pinned";
        Duration = duration;
    }
    public override void OnGet(Unit unit, Status status)
    {
        unit.Ua.DamageEvasion -= 0.2f;
        unit.Ue.OnCheckMoveUsage.Add(Pin);
        CombineTime(unit, status);
    }
    public override void OnQuit(Unit unit)
    {
        unit.Ua.DamageEvasion += 0.2f;
        unit.Ue.OnCheckMoveUsage.Remove(Pin);
        Quit(unit);
    }
    private bool Pin(Unit unit, bool init)
    {
        return false;
    }
}

public class Burned : Status
{
    public int Layer
    {
        get => (int)Param;
        set
        {
            Param = value;
        }
    }
    public Burned(int layer, float duration)
    {
        Name = "Burned";
        Duration = duration;
        Layer = layer;
    }
    public override void OnDealBodyDamage(Unit unit, ref Damage damage)
    {
        damage *= 0.5f;
    }
    public override void OnGet(Unit unit, Status status)
    {
        foreach (var s in unit.Status)
        {
            if (s.Name == Name)
            {
                Duration = Math.Max(Duration, status.Duration);
                Layer = Math.Max(Layer, ((Burned)status).Layer);
                return;
            }
        }
        Get(unit);
        unit.Status.Add(status);
    }
    public override void OnQuit(Unit unit)
    {
        Quit(unit);
    }
    public override void OnTurnEnd(Unit unit)
    {
        unit.Ua.GetHp(-Layer);
    }
}