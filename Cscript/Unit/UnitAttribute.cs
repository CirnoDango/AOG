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

    public float MaxHp = 100;
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
    public int Str
    {
        get => _str - 10;
        set
        {
            int old = _str - 10;
            int now = value;
            DamageBody += (now - old) * 2;
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
    public float TakeBulletDamage(float amount, Unit user, Skill skill)
    {
        foreach (var status in _parent.Status)
            status.OnTakeBulletDamage(_parent, skill, ref amount);
        amount = user.Ue.TakeBulletDamage(_parent, amount);
        amount *= user.Ua.DamageBullet / 100;
        float rd = 0;
        if (_parent.Us.currentSpellcard != null)
        {
            CurrentSp -= amount;
            if (CurrentSp < 0)
            {
                float overflow = -CurrentSp;
                CurrentSp = 0;
                float damage = overflow * 2 + MaxHp / 3;
                GetHp(-damage);
                string template = TranslationServer.Translate("击破符卡");
                string result = string.Format(template, user.TrName, skill.TrName, _parent.TrName, _parent.Us.currentSpellcard.TrName, $"{damage:F0}");
                Info.Print(result);

                _parent.Us.currentSpellcard.OnSpellBreak(new SkillContext(_parent));
                rd = damage;
            }
            else
            {
                string msg;
                msg = string.Format(
                    TranslationServer.Translate("技能被擦弹_减少SP"),
                    user.TrName, skill.TrName, _parent.TrName, amount.ToString("F0")
                );
                Info.Print(msg);
            }
        }
        else
        {
            // 擦弹检定
            if (GD.Randf() < MathEx.Logistic(1 - 0.7f * (CurrentSp / MaxSp), BulletGraze - user.Ua.BulletDamageAccuracy))
            {
                CurrentSp += amount;
                if (CurrentSp > MaxSp)
                {
                    float overflow = CurrentSp - MaxSp;
                    CurrentSp = MaxSp;
                    GetHp(-overflow);
                    string msg = string.Format(TranslationServer.Translate("技能击中_溢出伤害"),
                        user.TrName, skill.TrName, _parent.TrName, overflow.ToString("F0"));
                    Info.Print(msg);
                    rd = overflow;
                }
                else
                {
                    string
                    msg = string.Format(TranslationServer.Translate("技能被擦弹_增加SP"),
                        user.TrName, skill.TrName, _parent.TrName, amount.ToString("F0"));
                    Info.Print(msg);
                }
            }
            else
            {
                GetHp(-amount);
                string msg = string.Format(TranslationServer.Translate("技能击中_溢出伤害"),
                    user.TrName, skill.TrName, _parent.TrName, amount.ToString("F0"));
                Info.Print(msg);
                rd = amount;
            }
        }
        GetSp(0);
        return rd;
    }
    public bool CheckBodyHit(float amount, Unit user, Skill skill)
    {
        float dice = MathEx.Logistic(0.8f, user.Ua.BodyDamageAccuracy - DamageEvasion);
        if (GD.Randf() < dice)
        {
            TakeBodyDamage(amount, user, skill);
            return true;
        }
        else
        {
            string msg = string.Format(TranslationServer.Translate("skill.evaded"), user.TrName, skill.TrName, _parent.TrName);
            Info.Print(msg);
            return false;
        }
    }
    public float TakeBodyDamage(float amount, Unit user, Skill skill)
    {
        amount = user.Ue.TakeBodyDamage(_parent, amount);
        foreach (var status in _parent.Status.ToList())
            status.OnTakeBodyDamage(_parent, ref amount);
        amount *= user.Ua.DamageBody / 100;
        amount *= Math.Min(1, (float)(1 / Math.Round(Math.Pow((user.Up.Position - _parent.Up.Position).Length(), 0.5f))));
        if (amount < 0) { amount = 0; }
        GetHp(-amount);
        string msg = string.Format(TranslationServer.Translate("skill.hit_damage"), user.TrName, skill.TrName, _parent.TrName, amount.ToString("F0"));
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
            spBarAhead.Color = new Color(1, 1, 0);
    }
    public string Description()
    {
        string text = $@"{_parent.Name}
HP:{CurrentHp:F0}/{MaxHp:F0}
SP:{CurrentSp:F0}/{MaxSp:F0}
MP:{CurrentMp:F0}/{MaxMp:F0}
力量 :{Str + 10}  敏捷 :{Dex + 10}  体质 :{Con + 10} 
灵力 :{Spi + 10}  魔力 :{Mag + 10}  灵巧 :{Cun + 10}
体术命中 :{BodyDamageAccuracy * 100:F1}  弹幕命中 :{BulletDamageAccuracy * 100:F1}
闪避 :{DamageEvasion * 100:F1}  擦弹 :{BulletGraze * 100:F1}
当前状态:";
        foreach (Status s in _parent.Status)
        {
            text += $"{s.Name}({(s.Duration / 100):F0}回合)；";
        }
        if (_parent.Status.Count == 0)
            text += "无";
        text += "\n持有技能：\n";
        foreach ((SkillInstance si, _) in _parent.Us.skills)
        {
            if (si.Template.SkillGroup != "")
                text += $"{si.Template.Name}\n";
        }
        return text;
    }
}

