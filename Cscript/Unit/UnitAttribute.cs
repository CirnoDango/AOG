using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
public class UnitAttribute(Unit unit)
{
    private Unit _parent = unit;
    private ColorRect hpBarAhead;
    private ColorRect spBarAhead;

    public float MaxHp
    {
        get => _maxHp;
        set { _maxHp = Math.Max(1, value); }
    }
    private float _maxHp = 100;
    public float CurrentHp = 100;
    private float _maxSp = 40;
    public float MaxSp
    {
        get => _maxSp;
        set { if (_maxSp != 0) { _maxSp = value; } }
    }
    public float CurrentSp = 0;
    public float MaxMp = 10;
    public float CurrentMp = 10;
    public float SpeedGlobal = 100;
    public float SpeedCombat = 100;
    public float SpeedMove = 100; 
    public float DamageBody = 100;
    public float DamageBullet = 100;
    public float HealRatio = 1f;
    public float BodyDamageAccuracy = 0;
    public float BulletDamageAccuracy = 0;
    public float DamageEvasion = 0;
    public float BulletGraze = 0;
    private int _str = 10, _dex = 10, _con = 10, _spi = 10, _mag = 10, _cun = 10;
    public float imageSizeFactor = 0.16f;
    public float CritRate = 0.05f;
    public DamageTypeSet DamageTypeSet { get; set; } = new();
    public int Str
    {
        get => _str - 10;
        set
        {
            int old = _str - 10;
            int now = value;
            DamageBody += (now - old) * 2;
            CritRate += (now - old) * 0.01f;
            _str = value + 10;
        }
    }
    public int Dex
    {
        get => _dex - 10;
        set
        {
            int old = _dex - 10;
            int now = value;
            SpeedGlobal += (now - old) * 1;
            BodyDamageAccuracy += (now - old) * 0.01f;
            DamageEvasion += (now - old) * 0.01f;
            _dex = value + 10;
        }
    }
    public int Con
    {
        get => _con - 10;
        set
        {
            int old = _con - 10;
            int now = value;
            MaxHp += (now - old) * 5;
            HealRatio += (now - old) * 0.02f;
            _con = value + 10;
        }
    }
    public int Spi
    {
        get => _spi - 10;
        set
        {
            int old = _spi - 10;
            int now = value;
            MaxSp += (now - old) * 2;
            DamageBullet += (now - old) * 1;
            BulletGraze += (now - old) * 0.01f;
            _spi = value + 10;
        }
    }
    public int Mag
    {
        get => _mag - 10;
        set
        {
            int old = _mag - 10;
            int now = value;
            MaxMp += (now - old) * 1;
            DamageBullet += (now - old) * 1.5f;
            _mag = value + 10;
        }
    }
    public int Cun
    {
        get => _cun - 10;
        set
        {
            int old = _cun - 10;
            int now = value;
            BulletDamageAccuracy += (now - old) * 0.01f;
            _parent.Inventory.MaxWeight += (now - old) * 2;
            _parent.Equipment.MaxEquipWeight += (now - old) * 2;
            _cun = value + 10;
        }
    }
    public int skillPoint { get; set; } = 0;
    public int talentPoint { get; set; } = 0;
    public int uaPoint { get; set; } = 0;
    // 可选：一个构造函数初始化所有属性
    public void UnitAtt(int str = 10, int dex = 10, int con = 10, int spi = 10, int mag = 10, int cun = 10)
    {
        //return;
        Str = str - 10;
        Dex = dex - 10;
        Con = con - 10;
        Spi = spi - 10;
        Mag = mag - 10;
        Cun = cun - 10;
    }
    // 可选：一个显示真实数值的方法
    public override string ToString()
    {
        return $"Str: {Str}, Dex: {Dex}, Con: {Con}, Spi: {Spi}, Mag: {Mag}, Cun: {Cun}";
    }
    public void InitializeHpSpBar()
    {
        // HP条背景
        var hpBarBack = new ColorRect
        {
            Color = Colors.DarkGray,
            Size = new Vector2(1 / imageSizeFactor, 16 / imageSizeFactor),
            Position = new Vector2(6 / imageSizeFactor, -8 / imageSizeFactor),
            Scale = new Vector2(1, 0.95f)
        };
        _parent.Up.sprite.AddChild(hpBarBack);

        // HP条前景（红色）
        hpBarAhead = new ColorRect
        {
            Size = new Vector2(1 / imageSizeFactor, 16 / imageSizeFactor), // 初始满血
            Position = Vector2.Zero
        };
        switch (_parent.Ego)
        {
            case UnitEgo.normal:
                hpBarAhead.Color = Colors.Red;
                break;
            case UnitEgo.elite:
                hpBarAhead.Color = Colors.Green;
                break;
            case UnitEgo.great:
                hpBarAhead.Color = Colors.Blue;
                break;
            case UnitEgo.boss:
                hpBarAhead.Color = Colors.Gold;
                break;
            case UnitEgo.eliteBoss:
                hpBarAhead.Color = Colors.Purple;
                break;
        }
        hpBarBack.AddChild(hpBarAhead);
        if (MaxSp == 0)
            return;
        // SP条背景（左边贴着 HP）
        var spBarBack = new ColorRect
        {
            Color = Colors.DarkGray,
            Size = new Vector2(1 / imageSizeFactor, 16 / imageSizeFactor),
            Position = new Vector2(5 / imageSizeFactor, -8 / imageSizeFactor),
            Scale = new Vector2(1, 0.95f)
        };

        _parent.Up.sprite.AddChild(spBarBack);

        // SP条前景（黄色）
        spBarAhead = new ColorRect
        {
            Color = Colors.Yellow,
            Size = new Vector2(1 / imageSizeFactor, 0 / imageSizeFactor), // 初始满
            Position = Vector2.Zero
        };
        spBarBack.AddChild(spBarAhead);
        UpdateHpBar();
        UpdateSpBar();
    }
    public void HealHp(float amount)
    {
        amount *= HealRatio;
        GetHp(amount);
    }
    public void GetHp(float amount)
    {
        CurrentHp = Math.Clamp(CurrentHp + amount, 0, MaxHp);
        UpdateHpBar();
        if (_parent == Player.PlayerUnit)
            G.I.PlayerStatusBar.UpdateStatusUI();
        if (CurrentHp == 0)
            if (_parent == Player.PlayerUnit)
                Unit.OnPlayerdied?.Invoke();
            else
                Scene.CurrentMap.DeleteUnit(_parent);
    }
    public void GetSp(float amount)
    {
        if (_parent.Us.currentSpellcard != null && amount > 0)
            return;
        CurrentSp = Math.Clamp(CurrentSp + amount, 0, MaxSp);
        UpdateSpBar();
        if (_parent == Player.PlayerUnit)
            G.I.PlayerStatusBar.UpdateStatusUI();
    }
    public void GetMp(float amount)
    {
        CurrentMp = Math.Clamp(CurrentMp + amount, 0, MaxMp);
        if (_parent == Player.PlayerUnit)
            G.I.PlayerStatusBar.UpdateStatusUI();
    }
    /// <summary>
    /// 造成子弹伤害，返回值为实际造成的伤害
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="user"></param>
    /// <param name="skill"></param>
    /// <returns></returns>
    public float TakeBulletDamage(Damage damage, Unit user, Skill skill, float crit = 0)
    {
        if(GD.Randf() < user.Ua.CritRate + crit)
        {
            Info.Print($"{user.TrName} ifCrit");
            user.Ue.Crit(user);
            switch (damage.Type)
            {
                default:
                    damage.Value *= 1.5f; break;
            } 
        }
        Damage amount = damage.ApplyModifiers(user.Ua.DamageTypeSet, DamageTypeSet);
        foreach (var status in _parent.Status)
            status.OnTakeBulletDamage(_parent, skill, ref amount);
        amount = user.Ue.DealBulletDamage(_parent, amount);
        amount = _parent.Ue.TakeBulletDamage(user, amount);
        amount *= user.Ua.DamageBullet / 100;
        float rd = 0;
        // 擦弹检定
        if (GD.Randf() < GrazePercent(this, user.Ua))
        {
            amount = _parent.Ue.Graze(_parent, amount);
            CurrentSp += amount.Value;
            if (CurrentSp > MaxSp)
            {
                float overflow = CurrentSp - MaxSp;
                CurrentSp = MaxSp;
                GetHp(-overflow);
                string msg = string.Format(TranslationServer.Translate("acOverflow"),
                    user.TrName, skill.TrName, _parent.TrName, new Damage(overflow, damage.Type).ToString());
                Info.Print(msg);
                rd = overflow;
            }
            else
            {
                string
                msg = string.Format(TranslationServer.Translate("acGraze"),
                    user.TrName, skill.TrName, _parent.TrName, amount.Value.ToString("F0"));
                Info.Print(msg);
            }
        }
        else
        {
            GetHp(-amount.Value);
            string msg = string.Format(TranslationServer.Translate("acOverflow"),
                user.TrName, skill.TrName, _parent.TrName, amount.ToString());
            Info.Print(msg);
            rd = amount.Value;
        }

        if (_parent.Us.currentSpellcard != null)
        {
            _parent.Us.currentSpellcard.CurrentDurability -= amount.Value;
            if (_parent.Us.currentSpellcard.CurrentDurability < 0)
            {
                float overflow = -_parent.Us.currentSpellcard.CurrentDurability;
                float dam = overflow * 2 + MaxHp / 3;
                dam = _parent.Ue.TakeSpellcardBreakDamage(_parent, dam);
                GetHp(-dam);
                string template = TranslationServer.Translate("acSpellBreak");
                string result = string.Format(template, user.TrName, skill.TrName, _parent.TrName, _parent.Us.currentSpellcard.TrName, damage.ToString());
                Info.Print(result);
                _parent.Us.currentSpellcard.OnSpellBreak(new SkillContext(_parent));
                rd = dam;
            }
        }
        else
        {
            
        }
        GetSp(0);
        return rd;
        
    }

    public static float GrazePercent(UnitAttribute ua, UnitAttribute ub)
    {
        return MathEx.Logistic(1 - 0.7f * (ua.CurrentSp / ua.MaxSp), ua.BulletGraze - ub.BulletDamageAccuracy);
    }
    public static float GrazePercent(UnitAttribute ua)
    {
        return MathEx.Logistic(1 - 0.7f * (ua.CurrentSp / ua.MaxSp), ua.BulletGraze);
    }
    public bool CheckBodyHit(Damage damage, float acc, float crit, Unit user, Skill skill)
    {
        float dice = MathEx.Logistic(acc, user.Ua.BodyDamageAccuracy - DamageEvasion);
        if (GD.Randf() < dice)
        {
            if (GD.Randf() < user.Ua.CritRate + crit)
            {
                Info.Print($"{user.TrName} ifCrit");
                switch (damage.Type)
                {
                    default:
                        damage.Value *= 1.5f; break;
                }
            }
            TakeBodyDamage(damage, user, skill);
            return true;
        }
        else
        {
            string msg = string.Format(TranslationServer.Translate("skill.evaded"), user.TrName, skill.TrName, _parent.TrName);
            Info.Print(msg);
            return false;
        }
    }
    public bool CheckBodyHit(Damage damage, Unit user, Skill skill)
    {
        float dice = MathEx.Logistic(0.8f, user.Ua.BodyDamageAccuracy - DamageEvasion);
        if (GD.Randf() < dice)
        {
            if (GD.Randf() < user.Ua.CritRate)
            {
                Info.Print($"{user.TrName} ifCrit");
                switch (damage.Type)
                {
                    default:
                        damage.Value *= 1.5f; break;
                }
            }
            TakeBodyDamage(damage, user, skill);
            return true;
        }
        else
        {
            string msg = string.Format(TranslationServer.Translate("skill.evaded"), user.TrName, skill.TrName, _parent.TrName);
            Info.Print(msg);
            return false;
        }
    }
    public Damage TakeBodyDamage(Damage damage, Unit user, Skill skill)
    {
        Damage amount = damage.ApplyModifiers(user.Ua.DamageTypeSet, DamageTypeSet);
        amount = user.Ue.DealBodyDamage(_parent, amount);
        amount = _parent.Ue.TakeBodyDamage(user, amount);
        foreach (var status in _parent.Status.ToList())
            status.OnTakeBodyDamage(_parent, ref amount);
        amount *= user.Ua.DamageBody / 100;
        amount *= Math.Min(1, (float)(1 / Math.Round(Math.Pow((user.Up.Position - _parent.Up.Position).Length(), 0.5f))));
        if (amount.Value < 0) { amount.Value = 0; }
        GetHp(-amount.Value);
        string msg = string.Format(TranslationServer.Translate("skill.hit_damage"),
            user.TrName, skill.TrName, _parent.TrName, amount.ToString());
        Info.Print(msg);
        return amount;
    }
    public void UpdateHpBar()
    {
        float ratio = (float)CurrentHp / MaxHp;
        hpBarAhead.Size = new Vector2(1 / imageSizeFactor, 16 * ratio / imageSizeFactor);
        hpBarAhead.Position = new Vector2(0, 16 * (1 - ratio) / imageSizeFactor);
    }
    public void UpdateSpBar()
    {
        if (MaxSp == 0)
            return;
        float ratio = (float)CurrentSp / MaxSp;
        spBarAhead.Size = new Vector2(1 / imageSizeFactor, 16 * ratio / imageSizeFactor);
        spBarAhead.Position = new Vector2(0, 16 * (1 - ratio) / imageSizeFactor);
        if (_parent.Us.currentSpellcard != null)
            spBarAhead.Color = new Color(1, 0.5f, 0);
        else
            spBarAhead.Color = new Color(120f/255, 200f/255, 0);
    }
    public string Description()
    {
        string text1 =
        $"{TextEx.Tr(unit.TrName)}\n" +
        $"HP:{unit.Ua.CurrentHp:F0}/{unit.Ua.MaxHp:F0}\n" +
        $"SP:{unit.Ua.CurrentSp:F0}/{unit.Ua.MaxSp:F0}\n" +
        $"MP:{unit.Ua.CurrentMp:F0}/{unit.Ua.MaxMp:F0}\n" +
        $"{TextEx.Tr("当前状态")}: ";

        foreach (Status s in unit.Status)
        {
            text1 += $"{s.TrName}({(s.Duration / 100):F0} {TextEx.Tr("回合")} )；";
        }

        if (unit.Status.Count == 0)
            text1 += $" {TextEx.Tr("无")} ";
        string text2 =
        $"\n{TextEx.Tr("力量")} :{unit.Ua.Str + 10}  {TextEx.Tr("敏捷")} :{unit.Ua.Dex + 10}  {TextEx.Tr("体质")} :{unit.Ua.Con + 10}\n" +
        $"{TextEx.Tr("灵力")} :{unit.Ua.Spi + 10}  {TextEx.Tr("魔力")} :{unit.Ua.Mag + 10}  {TextEx.Tr("灵巧")} :{unit.Ua.Cun + 10}\n" +
        $"{TextEx.Tr("体术命中")} :{unit.Ua.BodyDamageAccuracy * 100:F1}  {TextEx.Tr("弹幕命中")} :{unit.Ua.BulletDamageAccuracy * 100:F1}\n" +
        $"{TextEx.Tr("闪避")} :{unit.Ua.DamageEvasion * 100:F1}  {TextEx.Tr("擦弹")} :{unit.Ua.BulletGraze * 100:F1}\n" +
        $"{TextEx.Tr("体术伤害")} :{unit.Ua.DamageBody:F1}%\n" +
        $"{TextEx.Tr("弹幕伤害")} :{unit.Ua.DamageBullet:F1}%\n" +
        $"{TextEx.Tr("整体速度")} :{unit.Ua.SpeedGlobal:F1}%\n" +
        $"{TextEx.Tr("战斗速度")} :{unit.Ua.SpeedCombat:F1}%\n" +
        $"{TextEx.Tr("移动速度")} :{unit.Ua.SpeedMove:F1}%\n";
        return text1 + text2;
    }
}

