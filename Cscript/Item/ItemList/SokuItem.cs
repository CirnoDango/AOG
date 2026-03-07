using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ItemList
{
    public class SpiritualStrikeTalisman : SkillItem<SpiritualStrikeTalisman.SSpiritualStrikeTalisman>
    {
        public SpiritualStrikeTalisman()
        {
            Weight = 4f;
        }
        public class SSpiritualStrikeTalisman : SkillFromItem<SpiritualStrikeTalisman>
        {
            public SSpiritualStrikeTalisman(SpiritualStrikeTalisman parent) : base(parent)
            {
                Cooldown = 2000;
                Targeting = new TargetType(new TargetRuleSelf(), 1, 4);
            }
            protected override void StartActivate(SkillContext sc)
            {
                foreach(var g in sc.User.Up.CurrentGrid.NearGrids(2))
                {
                    if(g.unit != null && !g.unit.IsFriend(sc.User))
                        g.unit.Up.KnockBack(4, sc);
                } 
                foreach(var b in Scene.CurrentMap.Bullets.ToList())
                    if((b.PositionFloat - sc.User.Up.Position).Length() < 2.5f &&
                        !b.creator.IsFriend(sc.User))
                        b.Destroy();
            }
        }
    }
    public class Stopwatch : SkillItem<Stopwatch.SStopwatch>
    {
        public Stopwatch()
        {
            Weight = 3f;
        }
        public class SStopwatch : SkillFromItem<Stopwatch>
        {
            public SStopwatch(Stopwatch parent) : base(parent)
            {
                Cooldown = 2000;
                Targeting = new TargetType(new TargetRuleGridEmpty(), 1, 6);
            }
            protected override void StartActivate(SkillContext sc)
            {
                sc.User.Up.MoveTo(sc.GridOne);
            }
        }
    }
    public class Hakurouken : SkillItem<Hakurouken.SHakurouken>, IWeapon
    {
        public Hakurouken()
        {
            Weight = 6f;
        }
        public Damage BaseDamage()
        {
            return new Damage(20, DamageType.pierce);
        }
        public float Accurency => 0.8f;
        public float CritRate => 0.15f;
        public Dictionary<string, float> Param => new() { { "str", 0.4f }, { "dex", 0.4f }, { "spi", 0.4f } };
        public class SHakurouken : SkillFromItem<Hakurouken>
        {
            public SHakurouken(Hakurouken parent) : base(parent)
            {
                Cooldown = 2000;
                Targeting = new TargetType(new TargetRuleDash(), 1, 5);
            }
            protected override void StartActivate(SkillContext sc)
            {
                // 复制自attack技能 /////////////
                var weapons = sc.User.Equipment.EquippedItems.Where(x => x is IWeapon).ToList();
                if (weapons.Count > 0)
                {
                    float accK = 0; float damK = 1;
                    foreach (IWeapon weapon in weapons.Cast<IWeapon>())
                    {
                        accK -= 0.2f; damK *= 0.8f;
                    }
                }
                else
                    sc.UnitOne.Ua.CheckBodyHit(new Damage(10 + sc.User.Ua.Str, DamageType.strike), 0.8f, 0, sc.User, this);
                sc.User.Ue.Attack(sc);
                /////////////////////////////////
                List<Grid> grids = sc.User.Up.DashCheck(sc.UnitOne.Up.CurrentGrid);
                grids.RemoveAt(grids.Count - 1);
                foreach (var grid in grids)
                {
                    sc.User.Up.MoveTo(grid);
                }
            }
        }
    }
    public class SacrificialDoll : Item, IEquipable
    {
        public SacrificialDoll()
        {
            Weight = 5f;
        }

        public override void OnLoad(Unit unit)
        {
            unit.Ue.OnGetStatus.Add(CounterStatus);
        }
        public override void OnUnequip(Unit unit)
        {
            unit.Ue.OnGetStatus.Remove(CounterStatus);
        }
        public bool CounterStatus(Status status)
        {
            if(status.Type == StatusType.Negative && GD.Randf() < 0.1)
                return false;
            return true;
        }
    }
    public class Grimoire : Item, IEquipable
    {
        public Grimoire()
        {
            Weight = 5f;
        }
        public override void OnEquip(Unit unit)
        {
            unit.Ua.Mag += 2;
        }
        public override void OnLoad(Unit unit)
        {
            unit.Ue.OnUnitUpdate += MpRecover;
        }
        public override void OnUnequip(Unit unit)
        {
            unit.Ua.Mag -= 2;
            unit.Ue.OnUnitUpdate -= MpRecover;
        }
        public void MpRecover(Unit unit, float time)
        {
            unit.Ua.GetMp(time * unit.Ua.Mag / 2000);
        }
    }
    public class CustomParasol : SkillItem<CustomParasol.SCustomParasol>
    {
        public CustomParasol()
        {
            Weight = 6f;
        }
        public class SCustomParasol : SkillFromItem<CustomParasol>
        {
            public SCustomParasol(CustomParasol parent) : base(parent)
            {
                Cooldown = 2000;
                Targeting = new TargetType(new TargetRuleSelf(), 1, 6);
            }
            protected override void StartActivate(SkillContext sc)
            {
                sc.User.GetStatus(new BulletShield(60, 500));
            }
        }
    }
    public class SoulTorch : Item, IEquipable
    {
        private Unit user;
        public SoulTorch()
        {
            Weight = 3f;
        }
        public override void OnLoad(Unit unit)
        {
            user = unit;
            GameEvents.OnEnemyKilled += HpRecover;
        }
        public override void OnUnequip(Unit unit)
        {
            GameEvents.OnEnemyKilled -= HpRecover;
        }
        public void HpRecover(Unit unit)
        {
            if (!unit.IsFriend(user))
                user.Ua.HealHp(unit.Ua.MaxHp * 0.04f);
        }
    }
    public class LeftHandedFoldingFan : Item, IEquipable, IWeapon
    {
        public float Accurency => 0.7f;

        public float CritRate => 0.05f;
        public Damage BaseDamage()
        {
            return new Damage(5, DamageType.shadow);
        }
        public Dictionary<string, float> Param => new() { { "spi", 0.4f }};

        public LeftHandedFoldingFan()
        {
            Weight = 5f;
        }
        public void OnHit(Unit user, Unit target)
        {
            if (GD.Randf() < 0.3f)
                target.GetStatus(new MagicSeal(300));
        }   
    }
    public class IbukiGourd : Item, IEquipable
    {
        public IbukiGourd()
        {
            Weight = 4f;
        }
        public override void OnLoad(Unit unit)
        {
            unit.Ue.OnUnitUpdate += SpRecover;
        }
        public override void OnUnequip(Unit unit)
        {
            unit.Ue.OnUnitUpdate -= SpRecover;
        }
        public void SpRecover(Unit unit, float time)
        {
            var NaturalSp = Scene.CurrentMap.NaturalSp;
            if (NaturalSp > unit.Ua.CurrentSp)
                unit.Ua.GetSp(0.002f * time * (NaturalSp - unit.Ua.CurrentSp));
        }
    }
    public class TenguFan : Item, IEquipable
    {
        public TenguFan()
        {
            Weight = 2f;
        }
        public override void OnLoad(Unit unit)
        {
            unit.Ua.SpeedMove += 25f;
        }
        public override void OnUnequip(Unit unit)
        {
            unit.Ua.SpeedMove -= 25f;
        }
    }
    public class SpellBreakingDrug : Item, IEquipable, IWeapon
    {
        public float Accurency => 0.7f;
        public float CritRate => 0.05f;
        public Damage BaseDamage()
        {
            return new Damage(5, DamageType.poison);
        }
        public Dictionary<string, float> Param => new() { { "cun", 0.4f } };

        public SpellBreakingDrug()
        {
            Weight = 5f;
        }
        public void OnHit(Unit user, Unit target)
        {
            if (GD.Randf() < 0.3f)
                target.GetStatus(new SpiritSeal(300));
        }
    }
    public class APennySaved : Item, IEquipable
    {
        public APennySaved()
        {
            Weight = 4f;
        }
        public override void OnLoad(Unit unit)
        {
            unit.Ue.OnUseSkill += AttackSp;
        }
        public override void OnUnequip(Unit unit)
        {
            unit.Ue.OnUseSkill -= AttackSp;
        }
        public void AttackSp(Unit unit, SkillContext sc, Skill si)
        {
            if (si.Name == "Attack")
                unit.Ua.GetSp(-5);
        }
    }
    public class DivineRaimentoftheDragonFish : Item, IEquipable
    {
        public DivineRaimentoftheDragonFish()
        {
            Weight = 7f;
        }
        public override void OnLoad(Unit unit)
        {
            unit.Ua.DamageEvasion += 0.05f;
            unit.Ua.BulletGraze += 0.05f;
        }
        public override void OnUnequip(Unit unit)
        {
            unit.Ua.DamageEvasion -= 0.05f;
            unit.Ua.BulletGraze -= 0.05f;
        }
    }
    public class SwordofScarletPerception : Item, IEquipable, IWeapon
    {
        public float Accurency => 0.8f;
        public float CritRate => 0.1f;
        public Damage BaseDamage()
        {
            return new Damage(20, DamageType.fantasy);
        }
        public Dictionary<string, float> Param => new() { { "str", 0.6f }, { "con", 0.6f } };

        public SwordofScarletPerception()
        {
            Weight = 7f;
        }
        public override void OnLoad(Unit unit)
        {
            unit.Ue.OnUseSkill += AttackSp;
        }
        public override void OnUnequip(Unit unit)
        {
            unit.Ue.OnUseSkill -= AttackSp;
        }
        public void AttackSp(Unit unit, SkillContext sc, Skill si)
        {
            if (si != null && si.Name == "Attack")
                unit.Ua.GetHp(-5);
        }
    }
    public class IllnessRecoveryCharm : Item, IEquipable
    {
        public IllnessRecoveryCharm()
        {
            Weight = 4f;
        }
        public override void OnEquip(Unit unit)
        {
            unit.Ua.Con += 2;
        }
        public override void OnLoad(Unit unit)
        {
            unit.Ua.HealRatio += 0.2f;
        }
        public override void OnUnequip(Unit unit)
        {
            unit.Ua.HealRatio -= 0.2f;
            unit.Ua.Con -= 2;
        }
    }
    public class FrozenFrog : SkillItem<FrozenFrog.SFrozenFrog>
    {
        public FrozenFrog()
        {
            Weight = 3f;
        }
        public class SFrozenFrog : SkillFromItem<FrozenFrog>
        {
            public SFrozenFrog(FrozenFrog parent) : base(parent)
            {
                Cooldown = 2000;
                Targeting = new TargetType(new TargetRuleEnemy(), 1, 6);
            }
            protected override void StartActivate(SkillContext sc)
            {
                sc.UnitOne.GetStatus(new Pinned(200));
            }
        }
    }
    public class DragonStar : SkillItem<DragonStar.SDragonStar>
    {
        public DragonStar()
        {
            Weight = 6f;
        }
        public class SDragonStar : SkillFromItem<DragonStar>
        {
            public SDragonStar(DragonStar parent) : base(parent)
            {
                Cooldown = 2000;
                Targeting = new TargetType(new TargetRuleSelf(), 1, 6);
            }
            protected override void StartActivate(SkillContext sc)
            {
                sc.User.GetStatus(new NumberShield(2, 500));
            }
        }
    }
    public class ControlRod : Item, IEquipable
    {
        public ControlRod()
        {
            Weight = 6f;
        }
        public override void OnLoad(Unit unit)
        {
            unit.Ue.OnDealBodyDamage.Add(AddDamage);
            unit.Ue.OnDealBulletDamage.Add(AddDamage);
            unit.Ue.OnTakeBodyDamage.Add(AddDamage);
            unit.Ue.OnTakeBulletDamage.Add(AddDamage);
        }
        public override void OnUnequip(Unit unit)
        {
            unit.Ue.OnDealBodyDamage.Remove(AddDamage);
            unit.Ue.OnDealBulletDamage.Remove(AddDamage);
            unit.Ue.OnTakeBodyDamage.Remove(AddDamage);
            unit.Ue.OnTakeBulletDamage.Remove(AddDamage);
        }
        public Damage AddDamage(Unit user, Unit target, Damage damage)
        {
            return damage * 1.1f;
        }
    }
    public class ThreeHeavenlyDrops : Item, IEquipable
    {
        public ThreeHeavenlyDrops()
        {
            Weight = 1f;
        }
        public override void OnLoad(Unit unit)
        {
            unit.Ue.OnCrit += CritHeal;
        }
        public override void OnUnequip(Unit unit)
        {
            unit.Ue.OnCrit -= CritHeal;
        }
        public void CritHeal(Unit unit)
        {
            if (GD.Randf() < 0.1f)
                unit.Ua.HealHp(unit.Ua.MaxHp * 0.5f);
        }
    }
}
