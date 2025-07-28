using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public enum Target
{
    Enemy, Self, Unit, Grid, Dash, Ray
}
public enum EffectType
{
    Activate, Passive
}

public class TargetType
{
    public Target Type { get; set; }
    public int Number { get; set; }
    public int Range { get; set; }
    public int BombRange { get; set; } = 0;
    public TargetType(Target type)
    {
        Type = type;
        if (type == Target.Self)
        {
            Number = 1;
            Range = 0;
        }
        else
        {
            Number = 1;
            Range = 1;
        }
    }
    public TargetType(Target type, int num, int ran, int bombrange = 0)
    {
        Type = type;
        Number = num;
        Range = ran;
        BombRange = bombrange;
    }
}
public class SkillContext
{
    public Unit User { get; set; }
    public List<Unit> UnitsTarget { get; set; } = [];
    public Unit UnitOne => UnitsTarget.FirstOrDefault();
    public List<Grid> GridsTarget { get; set; } = [];
    public Grid GridOne => GridsTarget.FirstOrDefault();
    public int level = 1;
    public SkillContext(Unit user, int level = 1)
    {
        User = user;
        this.level = level;
    }
    public SkillContext(Unit user, Unit Single, int level = 1)
    {
        User = user;
        UnitsTarget.Add(Single);
        this.level = level;
    }
    public SkillContext(Unit user, List<Unit> targets, int level = 1)
    {
        User = user;
        UnitsTarget = targets;
        this.level = level;
    }
    public SkillContext(Unit user, Grid Single, int level = 1)
    {
        User = user;
        GridsTarget.Add(Single);
        this.level = level;
    }
    public SkillContext(Unit user, List<Grid> targets, int level = 1)
    {
        User = user;
        GridsTarget = targets;
        this.level = level;
    }
    // 其他可能的参数，比如技能等级、buff状态等
}


public abstract class Skill
{
    // 以下为技能组、技能名组、激活技能静态属性
    public static List<Skill> SkillDeck { get; set; } = [];
    public static Dictionary<string, Skill> NameSkill { get; set; } =  [];
    public static SkillInstance CurrentSkill { get; set; }
    public string Name;
    public string TrName => $"sn{Name}";
    public string SkillGroup = "";
    public string TrSkillGroup => $"sg{SkillGroup}";
    //以下为技能属性，可根据等级变化而改动
    public string Description { get; set; }
    public float SpNeed { get; set; } = 0;
    public float SpCost { get; set; } = 0;
    public float MpCost { get; set; } = 0;
    public float Cooldown { get; set; } = 0;
    public float TimeCost { get; set; } = 100;
    public TargetType Targeting { get; set; }

    // 默认实现：返回固定字段，不覆写就等于固定
    public virtual float GetSpNeed(int level) => SpNeed;
    public virtual float GetSpCost(int level) => SpCost;
    public virtual float GetMpCost(int level) => MpCost;
    public virtual float GetCooldown(int level) => Cooldown;
    public virtual float GetTimeCost(int level) => TimeCost;
    public virtual TargetType GetTargeting(int level) => Targeting;
    public virtual string GetDescription(int level) => Description;
    // 技能图标，符卡，主动被动
    public Texture2D Texture { get; set; }
    public virtual bool IsSpellCard() => false;
    public EffectType EffectType { get; set; } = EffectType.Activate; // 激活或被动效果

    // 抽象的释放方法，具体由子类实现
    protected virtual void StartActivate(SkillContext sc) { }
    protected void EndActive(SkillContext sc, SkillInstance si)
    {
        if (si != null)
        {
            si.CurrentCooldown += GetCooldown(sc.level);
        }
        //用速度修正timecost
        float RealTimeCost = GetTimeCost(sc.level);
        if (Name == "Move")
            RealTimeCost /= (sc.User.Ua.SpeedGlobal * sc.User.Ua.SpeedMove / 10000);
        else
            RealTimeCost /= (sc.User.Ua.SpeedGlobal * sc.User.Ua.SpeedCombat / 10000);
        GameTime.Update(sc.User, RealTimeCost);
        G.I.Fsm.ChangeState(Fsm.updateState);
    }
    public virtual void Activate(SkillContext sc, SkillInstance si = null)
    {
        sc.User.GetSp(-GetSpCost(sc.level));
        sc.User.GetMp(-GetMpCost(sc.level));
        if (SkillGroup != "")
            Info.Print($"{sc.User.TrName} 执行 {TrName}");
        StartActivate(sc);
        EndActive(sc, si);
    }
    public string SkillInfo(int level = -1)
    {
        if (level == -1)
            level = Player.PlayerUnit.GetSkill(Name).Level;
        string text = "";
        if (IsSpellCard())
            text += "【 符卡 】";
        text += $" {TrName} \n";
        switch (EffectType)
        {
            case EffectType.Activate:
                text += " 主动技能 \n"; break;
            case EffectType.Passive:
                text += " 被动技能 \n"; break;
        }
        if (this is SpellCard spell)
            text += $" 持续时间 ：{Math.Round(spell.Duration / 100):F0} 回合 \n";
        if (GetCooldown(level) > 0)
            text += $" 冷却时间 ：{Math.Round(GetCooldown(level) / 100):F0} 回合 \n";
        if (GetSpNeed(level) != 0) { text += $" SP要求 ：{GetSpNeed(level)}\n"; }
        if (GetSpCost(level) > 0)
        {
            text += $" SP消耗 ：{GetSpCost(level)}";
            if (this is SpellCard spel)
                text += $" * {Math.Round(spel.Duration / 100):F0}";
            text += "\n";
        }
        if (GetSpCost(level) < 0) { text += $" SP累积 ：{-GetSpCost(level)}\n"; }
        if (GetMpCost(level) != 0) { text += $" MP消耗 ：{GetMpCost(level)}\n"; }
        text += GetDescription(level);
        return TextEx.TrN(text);
    }
    public static void LoadSkillDeck()
    {
        SkillDeck = SkillLoader.LoadAllSkills();
        SkillDeck.AddRange([new DiyShoot("WaterShoot",
[
    new(4, Vector2.Zero, 0, 2.5f, 10, "Small", ColorBullet.Blue),
    new(4, Vector2.Zero, 30, 2.5f, 10, "Small", ColorBullet.Blue),
    new(4, Vector2.Zero, 60, 2.5f, 10, "Small", ColorBullet.Blue),
    new(4, Vector2.Zero, 90, 2.5f, 10, "Small", ColorBullet.Blue),
    new(4, Vector2.Zero, 120, 2.5f, 10, "Small", ColorBullet.Blue),
    new(4, Vector2.Zero, 150, 2.5f, 10, "Small", ColorBullet.Blue),
    new(4, Vector2.Zero, 180, 2.5f, 10, "Small", ColorBullet.Blue),
    new(4, Vector2.Zero, 210, 2.5f, 10, "Small", ColorBullet.Blue),
    new(4, Vector2.Zero, 240, 2.5f, 10, "Small", ColorBullet.Blue),
    new(4, Vector2.Zero, 270, 2.5f, 10, "Small", ColorBullet.Blue),
    new(4, Vector2.Zero, 300, 2.5f, 10, "Small", ColorBullet.Blue),
    new(4, Vector2.Zero, 330, 2.5f, 10, "Small", ColorBullet.Blue),
]),
        new DiyShoot("StarShoot",
[
    new(8, 4, 10, "Star", ColorBullet.Red),
    new(8, 3.5f, 10, "Star", ColorBullet.Orange),
    new(8, 3, 10, "Star", ColorBullet.Yellow),
    new(6, Vector2.Zero, -20, 2.5f, 10, "Small", ColorBullet.Golden),
    new(6, Vector2.Zero, -10, 2.5f, 10, "Small", ColorBullet.Golden),
    new(6, Vector2.Zero, 10, 2.5f, 10, "Small", ColorBullet.Golden),
    new(6, Vector2.Zero, 20, 2.5f, 10, "Small", ColorBullet.Golden),

])
        ]);
        foreach (var skill in SkillDeck)
        {
            NameSkill.Add(skill.Name, skill);
        }
    }

    public virtual void AwakeBullet(SkillContext sc, Bullet bullet)
    {

    }
    public virtual void ActivateBullet(SkillContext sc, Bullet bullet)
    {

    }
    public virtual void OnLearn(Unit unit)
    {
    }
    public virtual void OffLearn(Unit unit)
    {
    }
    public string EffectTr()
    {
        return TranslationServer.Translate($"s{Name}");
    }
}
public class SkillInstance
{
    public Skill Template { get; set; }
    public int Level { get; set; } = 1;
    public float CurrentCooldown { get; set; }
    public Unit User;
    public float Duration;         // 持续时间
    public float TimeElapsed = 0;  // 已经持续了多久
    public bool IsActive = false;
    public List<(float triggerTime, Action<SkillContext, float> action)> timedEvents = new();
    public int eventIndex = 0;

    public SkillInstance(Skill template, int level = 1)
    {
        Template = template;
        CurrentCooldown = 0;
        if (template is SpellCard spell)
        {
            Duration = spell.Duration;
            timedEvents = spell.timedEvents;
        }

        Level = level;
    }

    public virtual bool CanUse(Unit user)
    {
        if (CurrentCooldown <= 0 && user.CurrentSp >= Template.GetSpNeed(user.GetSkill(Name).Level) && user.CurrentMp >= Template.GetMpCost(user.GetSkill(Name).Level) &&
            Template.SkillGroup != "" && Template.EffectType == EffectType.Activate)
            return true;
        return false;
    }

    public void Use(SkillContext sc)
    {
        //if (!CanUse()) return;
        Template.Activate(sc);
        CurrentCooldown = Template.GetCooldown(sc.level);
    }

    public void Update(SkillContext context, float delta)
    {
        if (Template is SpellCard spellCard)
        {
            spellCard.Update(context, delta);
        }
    }
    public void Activate(SkillContext sc)
    {
        Template.Activate(sc, this);
    }
    public string Name => Template.Name;
    public Texture2D Texture => Template.Texture;
    
    public TargetType Targeting => Template.GetTargeting(Level);
}

public abstract class SpellCard : Skill
{
    public static List<SkillInstance> currentSpellcards = [];
    //public Unit User;
    public float Duration;         // 持续时间
    //protected float TimeElapsed = 0;  // 已经持续了多久
    //protected bool IsActive = false;
    public List<(float triggerTime, Action<SkillContext, float> action)> timedEvents = new();
    //protected int eventIndex = 0;
    public override bool IsSpellCard() => true;
    protected void AddTimedEvent(float time, Action<SkillContext, float> action)
    {
        timedEvents.Add((time, action));
        timedEvents.Sort((a, b) => a.triggerTime.CompareTo(b.triggerTime));
    }
    protected void AddTimedEvent(float[] time, Action<SkillContext, float> action)
    {
        foreach(var t in time)
        {
            AddTimedEvent(t, action);
        }
    }

    protected override void StartActivate(SkillContext sc)
    {
        SkillInstance si = sc.User.GetSkill(Name);
        si.IsActive = true;
        si.TimeElapsed = 0;
        si.User = sc.User;
        currentSpellcards.Add(si);
        sc.User.currentSpellcard = this;
        sc.User.UpdateSpBar();
        if (sc.User == Player.PlayerUnit)
            G.I.PlayerStatusBar.UpdateStatusUI();
        OnSpellStart(sc);
    }
    public override void Activate(SkillContext sc, SkillInstance si = null)
    {
        StartActivate(sc);
        EndActive(sc, si);
    }

    public void Update(SkillContext sc, float delta)
    {
        SkillInstance si = sc.User.GetSkill(Name);
        if (!si.IsActive)
            return;
        float previousTime = si.TimeElapsed;
        si.TimeElapsed += delta;
        sc.User.GetSp(-GetSpCost(sc.level) * delta / 100);
        // 触发事件，传入 advanceTime
        while (si.eventIndex < timedEvents.Count && si.TimeElapsed >= timedEvents[si.eventIndex].triggerTime)
        {
            var (triggerTime, action) = timedEvents[si.eventIndex];
            float advanceTime = si.TimeElapsed - triggerTime;
            action.Invoke(sc, advanceTime);
            si.eventIndex++;
        }

        OnSpellUpdate(sc, delta);
        if (si.TimeElapsed >= Duration)
        {
            OnSpellEnd(sc);
        }
    }
    protected abstract void OnSpellStart(SkillContext sc);
    protected virtual void OnSpellUpdate(SkillContext sc, float delta) { }
    public virtual void OnSpellEnd(SkillContext sc)
    {
        SkillInstance si = sc.User.GetSkill(Name);
        timedEvents.Clear();
        si.eventIndex = 0;
        currentSpellcards.Remove(si);
        si.IsActive = false;
        sc.User.currentSpellcard = null;
        sc.User.UpdateSpBar();
        if (sc.User == Player.PlayerUnit)
            G.I.PlayerStatusBar.UpdateStatusUI();
    }
    public virtual void OnSpellBreak(SkillContext sc)
    {
        OnSpellEnd(sc);
    }
}

public static class SkillLoader
{
    public static List<Skill> LoadAllSkills()
    {
        var skillType = typeof(Skill); // 或 typeof(SpellCard) 如果你只要 spellcard
        var skills = new List<Skill>();

        // 获取当前程序集下所有 Skill 的非抽象子类
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => skillType.IsAssignableFrom(t) && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null);

        foreach (var t in types)
        {
            Skill instance = Activator.CreateInstance(t) as Skill;
            if (instance != null)
            {
                skills.Add(instance);
            }  
        }

        return skills;
    }
}


