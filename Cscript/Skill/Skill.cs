using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
public abstract class Skill
{
    // 以下为技能组、技能名组、激活技能静态属性
    public static List<Skill> SkillDeck { get; set; } = [];
    public static Dictionary<string, Skill> NameSkill { get; set; } = [];
    public static Skill CurrentSkill { get; set; }
    public static List<Skill> ContinueSkills { get; set; } = [];
    public string Name => GetType().Name;
    public string[] Extra(string index = "0")
    {
        string[] d = ["","","",$"s{Name}{index}"];
        return d;
    }
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
    // 原Skill属性
    public int Level { get; set; } = 1;
    #pragma warning disable IDE1006 // 命名样式
    public int iLevel => Level - 1;
    #pragma warning restore IDE1006 // 命名样式
    public float CurrentCooldown { get; set; }
    public Unit User { get; set; }
    public float Duration;         // 持续时间
    public float TimeElapsed = 0;  // 已经持续了多久
    public bool IsActive = false;
    public List<(float triggerTime, Action<SkillContext, float> action)> timedEvents = new();
    public int eventIndex = 0;
    public Skill Clone()
    {
        // 先做一次浅拷贝以保留具体子类类型和只读/值类型字段
        var clone = (Skill)MemberwiseClone();

        // 深拷贝可变引用字段，避免模板与克隆共用同一实例
        if (Targeting != null)
        {
            clone.Targeting = new TargetType(Targeting.TargetRule, Targeting.Number, Targeting.Range, Targeting.BombRange);
        }

        // 复制 timedEvents 列表（Action 委托仍然引用原方法，这是预期行为）
        clone.timedEvents = new List<(float triggerTime, Action<SkillContext, float> action)>(timedEvents);

        // 重置运行时相关状态，避免从模板继承活动状态
        clone.User = null;
        clone.CurrentCooldown = 0;
        clone.TimeElapsed = 0;
        clone.IsActive = false;
        clone.eventIndex = 0;

        // Texture/字符串等通常可以共享引用（不可变或可复用）
        // 如果需要对 Texture 做深拷贝，应在此处处理（通常不必要）

        return clone;
    }
    // 默认实现：返回固定字段，不覆写就等于固定
    public virtual float GetSpNeed() => SpNeed;
    public virtual float GetSpCost() => SpCost;
    public virtual float GetMpCost() => MpCost;
    public virtual float GetCooldown() => Cooldown;
    public virtual float GetTimeCost() => TimeCost;
    public virtual TargetType GetTargeting() => Targeting;
    public virtual string GetDescription() => Description;
    // 技能图标，符卡，主动被动
    public Texture2D Texture { get; set; }
    public virtual bool IsSpellCard() => false;
    public EffectType EffectType { get; set; } = EffectType.Activate; // 激活或被动效果

    // 抽象的释放方法，具体由子类实现
    protected virtual void StartActivate(SkillContext sc) { }
    protected void EndActive(SkillContext sc)
    {
        CurrentCooldown += GetCooldown();
        GameEvents.UseSkill(sc.User, sc, this);
        sc.User.Ue.UseSkill(sc, this);
        //用速度修正timecost
        float RealTimeCost = GetTimeCost();
        if (Name == "Move")
            RealTimeCost /= (sc.User.Ua.SpeedGlobal * sc.User.Ua.SpeedMove / 10000);
        else
            RealTimeCost /= (sc.User.Ua.SpeedGlobal * sc.User.Ua.SpeedCombat / 10000);
        sc.User.Ue.UnitTurnEnd();
        sc.User.TimeEnergy -= RealTimeCost;
        if (G.I.Fsm.currentState is not UpdatingState)
            G.I.Fsm.ChangeState(new UpdatingState(G.I.Fsm));
    }
    public virtual void Activate(SkillContext sc)
    {
        sc.User.Ua.GetSp(-GetSpCost());
        sc.User.Ua.GetMp(-GetMpCost());
        if (SkillGroup != "")
            Info.Print($"{sc.User.TrName} 执行 {TrName}");
        StartActivate(sc);
        EndActive(sc);
    }
    public string SkillInfo()
    {
        string text = "";
        if (IsSpellCard())
            text += $"【{TextEx.Tr("符卡")}】";
        text += $"{TextEx.Tr(TrName)}\n";
        switch (EffectType)
        {
            case EffectType.Activate:
                text += $"{TextEx.Tr("主动技能")}\n"; break;
            case EffectType.Passive:
                text += $"{TextEx.Tr("被动技能")}\n"; break;
        }
        if (this is SpellCard spell)
            text += $"{TextEx.Tr("持续时间")}：{Math.Round(spell.GetDuration() / 100):F0} {TextEx.Tr("回合")}\n";
        if (GetCooldown() > 0)
            text += $"{TextEx.Tr("冷却时间")}：{Math.Round(GetCooldown() / 100):F0} {TextEx.Tr("回合")}\n";
        if (GetTargeting() != null)
            text += $"{TextEx.Tr("使用范围")}：{GetTargeting().Range}\n";
        if (GetSpNeed() != 0) { text += $"{TextEx.Tr("SP要求")}：{GetSpNeed()}\n"; }
        if (GetSpCost() > 0)
        {
            text += $"{TextEx.Tr("SP消耗")}：{GetSpCost()}";
            text += "\n";
        }
        if (GetSpCost() < 0) { text += $"{TextEx.Tr("SP累积")}：{-GetSpCost()}\n"; }
        if (GetMpCost() != 0) { text += $"{TextEx.Tr("MP消耗")}：{GetMpCost()}\n"; }
        return text + TextEx.Tr(GetDescription() ?? "");
    }
    public string SkillInfo(int level)
    {
        var skill = Clone();
        skill.Level = level;
        return skill.SkillInfo();
    }
    public static void LoadSkillDeck()
    {
        SkillDeck = SkillLoader.LoadAllSkills();
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
    public virtual void OnLoad(Unit unit)
    {
    }
    public virtual void OffLearn(Unit unit)
    {
    }
    public string EffectTr()
    {
        return TranslationServer.Translate($"s{Name}");
    }
    public virtual bool CanUse(Unit user)
    {
        bool canuse;
        if (CurrentCooldown <= 0 &&
            user.Ua.CurrentSp >= GetSpNeed() &&
            (user.Ua.CurrentSp >= GetSpCost() || user.Ua.MaxSp == 0) &&
            user.Ua.CurrentMp >= GetMpCost() &&
            SkillGroup != "" &&
            EffectType == EffectType.Activate)
            canuse = true;
        else
            canuse = false;
        return user.Ue.CheckSkillUsage(this, canuse);
    }

    public void Use(SkillContext sc)
    {
        //if (!CanUse()) return;
        Activate(sc);
        CurrentCooldown = GetCooldown();
    }

    public virtual void Update(SkillContext context, float delta)
    {
    }
}
//public class Skill
//{
//    public Skill Template { get; set; }
//    
//
//    public Skill(Skill template, int level = 1)
//    {
//        CurrentCooldown = 0;
//        if (template is SpellCard spell)
//        {
//            Duration = spell.GetDuration(level);
//            timedEvents = spell.timedEvents;
//        }
//
//        Level = level;
//    }
//
//    
//}
public abstract class SkillLong : Skill
{
    public virtual float GetDuration() => Duration;
    protected void AddTimedEvent(float time, Action<SkillContext, float> action)
    {
        timedEvents.Add((time, action));
        timedEvents.Sort((a, b) => a.triggerTime.CompareTo(b.triggerTime));
    }
    protected void AddTimedEvent(float[] time, Action<SkillContext, float> action)
    {
        foreach (var t in time)
        {
            AddTimedEvent(t, action);
        }
    }

    protected override void StartActivate(SkillContext sc)
    {
        Skill si = sc.User.Us.GetSkill(Name);
        si.IsActive = true;
        si.TimeElapsed = 0;
        si.User = sc.User;
        ContinueSkills.Add(si);
        OnSkillStart(sc);
    }
    public override void Activate(SkillContext sc)
    {
        sc.User.Ua.GetSp(-GetSpCost());
        sc.User.Ua.GetMp(-GetMpCost());
        if (SkillGroup != "")
            Info.Print($"{sc.User.TrName} 执行 {TrName}");
        StartActivate(sc);
        EndActive(sc);
    }

    public override void Update(SkillContext sc, float delta)
    {
        Skill si = sc.User.Us.GetSkill(Name);
        if (!si.IsActive)
            return;
        float previousTime = si.TimeElapsed;
        si.TimeElapsed += delta;
        // 触发事件，传入 advanceTime
        while (si.eventIndex < timedEvents.Count && si.TimeElapsed >= timedEvents[si.eventIndex].triggerTime)
        {
            var (triggerTime, action) = timedEvents[si.eventIndex];
            float advanceTime = si.TimeElapsed - triggerTime;
            action.Invoke(sc, advanceTime);
            si.eventIndex++;
        }

        OnSkillUpdate(sc, delta);
        if (si.TimeElapsed >= GetDuration())
        {
            OnSkillEnd(sc);
        }
    }
    protected abstract void OnSkillStart(SkillContext sc);
    protected virtual void OnSkillUpdate(SkillContext sc, float delta) { }
    public virtual void OnSkillEnd(SkillContext sc)
    {
        Skill si = sc.User.Us.GetSkill(Name);
        timedEvents.Clear();
        si.eventIndex = 0;
        ContinueSkills.Remove(si);
        si.IsActive = false;
    }
}
public abstract class SpellCard : Skill
{
    public static List<Skill> currentSpellcards = [];
    public virtual float GetDuration() => Duration;
    public float MaxDurability => SpCost * 1.5f;
    public virtual float GetMaxDurability() => GetSpCost() * 1.5f;
    public float CurrentDurability;
    public override bool IsSpellCard() => true;
    
    protected void AddTimedEvent(float time, Action<SkillContext, float> action)
    {
        timedEvents.Add((time, action));
        timedEvents.Sort((a, b) => a.triggerTime.CompareTo(b.triggerTime));
    }
    protected void AddTimedEvent(float[] time, Action<SkillContext, float> action)
    {
        foreach (var t in time)
        {
            AddTimedEvent(t, action);
        }
    }

    protected override void StartActivate(SkillContext sc)
    {
        Skill si = sc.User.Us.GetSkill(Name);
        si.IsActive = true;
        si.TimeElapsed = 0;
        ((SpellCard)si).CurrentDurability = ((SpellCard)si).GetMaxDurability();
        si.User = sc.User;
        currentSpellcards.Add(si);
        sc.User.Us.currentSpellcard = this;
        sc.User.Ua.UpdateSpBar();
        G.I.SpellcardBox.Init(si);
        if (sc.User == Player.PlayerUnit)
            G.I.PlayerStatusBar.UpdateStatusUI();
        OnSpellStart(sc);
    }
    public override void Activate(SkillContext sc)
    {
        sc.User.Ua.GetSp(-GetSpCost());
        sc.User.Ua.GetMp(-GetMpCost());
        StartActivate(sc);
        EndActive(sc);
    }

    public override void Update(SkillContext sc, float delta)
    {
        Skill si = sc.User.Us.GetSkill(Name);
        if (!si.IsActive)
            return;
        float previousTime = si.TimeElapsed;
        si.TimeElapsed += delta;
        // 触发事件，传入 advanceTime
        while (si.eventIndex < timedEvents.Count && si.TimeElapsed >= timedEvents[si.eventIndex].triggerTime)
        {
            var (triggerTime, action) = timedEvents[si.eventIndex];
            float advanceTime = si.TimeElapsed - triggerTime;
            action.Invoke(sc, advanceTime);
            si.eventIndex++;
        }

        OnSpellUpdate(sc, delta);
        if (si.TimeElapsed >= GetDuration())
        {
            OnSpellEnd(sc);
        }
    }
    protected abstract void OnSpellStart(SkillContext sc);
    protected virtual void OnSpellUpdate(SkillContext sc, float delta) { }
    public virtual void OnSpellEnd(SkillContext sc)
    {
        Skill si = sc.User.Us.GetSkill(Name);
        G.I.SpellcardBox.Remove(si);
        timedEvents.Clear();
        si.eventIndex = 0;
        currentSpellcards.Remove(si);
        si.IsActive = false;
        sc.User.Us.currentSpellcard = null;
        sc.User.Ua.UpdateSpBar();
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


