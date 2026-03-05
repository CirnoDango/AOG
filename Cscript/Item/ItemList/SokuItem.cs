using System.Collections.Generic;
using System.Xml.Linq;

namespace ItemList
{
    public class HealPotion : SkillItem<HealPotion.Instance>
    {
        public int HpRecoverPercent { get; private set; } = 40;
        public HealPotion()
        {
            Weight = 8f;
        }

        public class Instance : Skill, ISkill
        {
            private readonly int _hp;
            public Instance(HealPotion parent)
            {
                _hp = parent.HpRecoverPercent;
                Texture = parent.Texture;
                Name = "HealPotion";
                SkillGroup = "Item";
                Description = $"恢复{_hp}%HP";
                Cooldown = 2000;
                Targeting = new TargetType(Target.Self);
            }
            public Skill GetSkill(SkillItem parent)
            {
                return new Instance((HealPotion)parent);
            }

            protected override void StartActivate(SkillContext sc)
            {
                sc.User.Ua.GetHp(sc.User.Ua.MaxHp * _hp / 100f);
            }
        }
    }

    public class Axe : Item, IEquipable, IWeapon
    {
        public Axe()
        {
            Weight = 4f;
        }
        public Damage Damage(Unit unit)
        {
            return new Damage(unit.Ua.Str, DamageType.slash);
        }
        public float Accurency => 0.8f;
        public float CritRate => 0.05f;
        public Dictionary<string, float> Param => new() { { "str", 0.4f }, { "dex", 0.4f } };
    }
}
