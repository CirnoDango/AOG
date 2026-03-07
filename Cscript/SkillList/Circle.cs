using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using static MathEx;
using static System.Net.Mime.MediaTypeNames;
interface ICircle
{
    List<Grid> Grids { get; set; }
    Unit Summoner { get; set; }
}
public class EvilSealingCircle : SkillLong, ICircle
{
    public EvilSealingCircle()
    {
        SkillGroup = "Circle";
        SpCost = 3;
        Cooldown = 2000;
        Duration = 500;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 3);
    }
    public List<Grid> Grids { get; set; } = [];
    private List<Sprite2D> Images = [];
    public Unit Summoner { get; set; }
    int[] t0 = [8, 12, 16, 16];
    int[] t1 = [6, 9, 12, 12];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], t1[iLevel]);
    }
    protected override void OnSkillStart(SkillContext sc)
    {
        Grids = [.. sc.User.Up.CurrentGrid.NearGrids(3).Where(x => x.IsWalkable)];
        foreach (Grid grid in Grids)
        {
            Images.Add(ImageEx.CreateGridImage(grid.Position, "res://Assets/GridEffect/EvilSealingCircle.png"));
        }
        Summoner = sc.User;
        AddTimedEvent(Linspace(100, 500, 5), (ctx, advanceTime) =>
        {
            foreach(Grid g in Grids)
            {
                if (g.unit == null || g.unit.Friendness * Summoner.Friendness > 0)
                    continue;
                g.unit.Ua.TakeBulletDamage(new Damage(t0[iLevel], DamageType.barrier), sc.User, this);
                g.unit?.Ua.GetMp(-t1[iLevel]);
            }
        });
        if (sc.Level == 4)
            GameEvents.OnUseSkill += MpDamage;
    }
    public override void OnSkillEnd(SkillContext sc)
    {
        foreach (Sprite2D i in Images)
            i.QueueFree();
        Images.Clear();
        GameEvents.OnUseSkill -= MpDamage;
        base.OnSkillEnd(sc);
    }
    private void MpDamage(Unit unit, SkillContext sc, Skill si)
    {
        if(Grids.Contains(unit.Up.CurrentGrid) && si.GetMpCost() > 0)
        {
            unit.Ua.TakeBulletDamage(new Damage(2 * si.GetMpCost(), DamageType.barrier), Summoner, this);
        }
    }
}
public class CautionaryBorder : SkillLong, ICircle
{
    public CautionaryBorder()
    {
        SkillGroup = "Circle";
        Cooldown = 1600;
        Duration = 500;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 3);
    }
    public List<Grid> Grids { get; set; }= [];
    private List<Unit> units = [];
    private List<Sprite2D> Images = [];
    public Unit Summoner { get; set; }
    private int level;
    int[] t0 = [20, 25, 30, 30];
    int[] t1 = [30, 45, 55, 55];
    public override float GetSpCost()
    {
        if (level == 4)
            return 40;
        return 10;
    }
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], t1[iLevel]);
    }
    protected override void OnSkillStart(SkillContext sc)
    {
        Grids = [.. sc.User.Up.CurrentGrid.NearGrids(3).Where(x => x.IsWalkable)];
        foreach (Grid grid in Grids)
        {
            Images.Add(ImageEx.CreateGridImage(grid.Position, "res://Assets/GridEffect/CautionaryBorder.png"));
        }
        Summoner = sc.User;
        level = sc.Level;
        GameEvents.OnUnitMove += AddEvasion;
    }
    public override void OnSkillEnd(SkillContext sc) 
    {
        Grids.Clear();
        foreach (Unit u in units.ToList())
        {
            GameEvents.UnitMove(u);
        }
        GameEvents.OnUnitMove -= AddEvasion;
        foreach (Sprite2D i in Images)
            i.QueueFree();
        Images.Clear();
        base.OnSkillEnd(sc);
    }
    private void AddEvasion(Unit unit)
    {
        if (!units.Contains(unit) && Grids.Contains(unit.Up.CurrentGrid))
        {
            units.Add(unit);
            unit.Ua.DamageEvasion += t0[iLevel] / 100f;
            unit.Ue.OnGraze.Add(GrazeResist);
        }
        else if(units.Contains(unit) && !Grids.Contains(unit.Up.CurrentGrid))
        {
            units.Remove(unit);
            unit.Ua.DamageEvasion -= t0[iLevel] / 100f;
            unit.Ue.OnGraze.Remove(GrazeResist);
        }
    }
    private Damage GrazeResist(Unit unit, Unit attack, Damage baseGraze)
    {
        return baseGraze * (1 - t1[iLevel] / 100f);
    }
}
public class BindingBorder : SkillLong, ICircle
{
    public BindingBorder()
    {
        SkillGroup = "Circle";
        Cooldown = 2400;
        SpCost = 3;
        Duration = 500;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 3);
    }
    public List<Grid> Grids { get; set; }= [];
    private List<Unit> units = [];
    private List<Sprite2D> Images = [];
    public Unit Summoner { get; set; }
    private int level;
    int[] t0 = [4, 8, 12, 20];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel]);
    }
    protected override void OnSkillStart(SkillContext sc)
    {
        Grids = [.. sc.User.Up.CurrentGrid.NearGrids(3).Where(x => x.IsWalkable)];
        foreach (Grid grid in Grids)
        {
            Images.Add(ImageEx.CreateGridImage(grid.Position, "res://Assets/GridEffect/BindingBorder.png"));
        }
        Summoner = sc.User;
        level = sc.Level;
        GameEvents.OnUnitMove += AddPinned;
        AddTimedEvent(Linspace(100, 500, 5), (ctx, advanceTime) =>
        {
            foreach (Grid g in Grids)
            {
                if (g.unit == null || g.unit.Friendness * Summoner.Friendness >= 0)
                    continue;
                g.unit.Ua.TakeBodyDamage(new Damage(t0[iLevel], DamageType.barrier), sc.User, this);
            }
        });
    }
    public override void OnSkillEnd(SkillContext sc)
    {
        GameEvents.OnUnitMove -= AddPinned;
        foreach (Sprite2D i in Images)
            i.QueueFree();
        Images.Clear();
        base.OnSkillEnd(sc);
    }
    private void AddPinned(Unit unit)
    {
        if (Grids.Contains(unit.Up.CurrentGrid) && unit.Friendness * Summoner.Friendness < 0)
            unit.GetStatus(new Pinned(500));
    }
}
public class PermanentBorder : SkillLong, ICircle
{
    public PermanentBorder()
    {
        SkillGroup = "Circle";
        Cooldown = 4000;
        Duration = 90000000000;
    }
    int[] t0 = [3, 4, 5, 5];
    int[] t1 = [8, 10, 12, 12];
    public List<Grid> Grids { get; set; }= [];
    private List<Unit> units = [];
    private List<Sprite2D> Images = [];
    public Unit Summoner { get; set; }
    private int level;
    private float time;
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], t1[iLevel]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        if (ContinueSkills.FirstOrDefault(x=>x.Name == Name && x.User == sc.User) != null)
        {
            PermanentBorder pb = (PermanentBorder)ContinueSkills.FirstOrDefault(x => x.Name == Name && x.User == sc.User);
            pb.OnSkillEnd(sc);
        }
        base.StartActivate(sc);
    }
    public override TargetType GetTargeting()
    {
        return new TargetType(new TargetRuleSelf(), 1, t0[iLevel]);
    }
    protected override void OnSkillStart(SkillContext sc)
    {

        Grids = [.. sc.User.Up.CurrentGrid.NearGrids(t0[iLevel]).Where(x => x.IsWalkable)];
        foreach (Grid grid in Grids)
        {
            Images.Add(ImageEx.CreateGridImage(grid.Position, "res://Assets/GridEffect/PermanentBorder.png"));
        }
        Summoner = sc.User;
        level = sc.Level;
        time = 0;
        GameEvents.OnUnitMove += AddCun;
        GameEvents.OnSceneQuit += SceneQuit;
    }
    protected override void OnSkillUpdate(SkillContext sc, float delta)
    {
        base.OnSkillUpdate(sc, delta);
        if (Math.Floor(time/100) < Math.Floor((time + delta) / 100))
        {
            foreach (Grid g in Grids)
            {
                if (g.unit == null || g.unit.Friendness * Summoner.Friendness >= 0)
                    continue;
                g.unit.Ua.TakeBulletDamage(new Damage(t1[iLevel], DamageType.spirit), sc.User, this);
            }
        }
        time += delta;
    }
    private void SceneQuit()
    {
        OnSkillEnd(new SkillContext(Summoner));
    }
    public override void OnSkillEnd(SkillContext sc)
    {
        Grids.Clear();
        foreach(Unit u in units.ToList())
        {
            GameEvents.UnitMove(u);
        }
        GameEvents.OnUnitMove -= AddCun;
        GameEvents.OnSceneQuit -= SceneQuit;
        foreach (Sprite2D i in Images)
            i.QueueFree();
        Images.Clear();
        base.OnSkillEnd(sc);
    }
    private void AddCun(Unit unit)
    {
        if (!units.Contains(unit) && Grids.Contains(unit.Up.CurrentGrid))
        {
            units.Add(unit);
            unit.Ua.Cun += 6;
            if (level == 4)
                unit.Ua.Dex += 6;
        }
        else if (units.Contains(unit) && !Grids.Contains(unit.Up.CurrentGrid))
        {
            units.Remove(unit);
            unit.Ua.Cun -= 6;
            if (level == 4)
                unit.Ua.Dex -= 6;
        }
    }
}
public class OmniDragonSlayingCircle : SpellCard
{
    public OmniDragonSlayingCircle()
    {
        SkillGroup = "Circle";
        SpCost = 45;
        Cooldown = 3000;
        Duration = 400;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 4);
    }
    int[] t0 = { 20, 30, 40, 40 };
    private List<Unit> units = [];
    private Unit user;
    private int level;
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel]);
    }
    protected override void OnSpellStart(SkillContext sc)
    {
        user = sc.User;
        level = sc.Level;
        GameEvents.OnUnitMove += Circle;
        AddTimedEvent(Linspace(100, 400, 4), (ctx, advanceTime) =>
        {
            foreach (Grid g in CircleGrids(sc))
            {
                if (g.unit == null || g.unit.Friendness * user.Friendness >= 0)
                    continue;
                g.unit.Ua.TakeBulletDamage(new Damage(t0[iLevel], DamageType.slash), sc.User, this);
            }
        });
    }
    public override void OnSpellEnd(SkillContext sc)
    {
        GameEvents.OnUnitMove -= Circle;
        foreach (Unit u in units.ToList())
        {
            LeaveCircle(u);
        }
        base.OnSpellEnd(sc);
    }
    private List<Grid> CircleGrids(SkillContext sc = null)
    {
        sc ??= new SkillContext(user);
        List<Grid> grids = [];
        foreach (ICircle ic in ContinueSkills.Select(x => x).Where(x => x is ICircle).Where(x=>((ICircle)x).Summoner == sc.User))
        {
            grids.AddRange(ic.Grids);
        }
        return grids;
    }
    private void Circle(Unit unit)
    {
        if (!units.Contains(unit) && unit.Friendness * user.Friendness > 0 && CircleGrids().Contains(unit.Up.CurrentGrid))
        {
            units.Add(unit);
            BodyDamageModify = (user, target, dmg) => dmg * 2;
            unit.Ue.OnDealBodyDamage.Add(BodyDamageModify);
            if (level == 4)
            {
                BulletDamageModify = (user, target, dmg) => dmg * 1.3f;
                unit.Ue.OnDealBulletDamage.Add(BulletDamageModify);
            }
        }
        else if (units.Contains(unit) && !CircleGrids().Contains(unit.Up.CurrentGrid))
        {
            LeaveCircle(unit);
        }
    }
    private void LeaveCircle(Unit unit)
    {
        units.Remove(unit);
        if (BodyDamageModify != null)
            unit.Ue.OnDealBulletDamage.Remove(BodyDamageModify);
        if (level == 4)
            unit.Ue.OnDealBulletDamage.Remove(BulletDamageModify);
    }
    private Func<Unit, Unit, Damage, Damage> BodyDamageModify;
    private Func<Unit, Unit, Damage, Damage> BulletDamageModify;
}