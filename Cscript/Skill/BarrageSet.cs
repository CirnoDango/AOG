using System;
using System.Collections.Generic;
using System.Linq;

public class BarrageSet : SkillItem
{
    public Barrage B = new();
    public override float Weight
    {
        get
        {
            if (B.Components == null || B.Components.Count == 0)
                return 2f;
            else
                return 2f + B.Components.Where(a => a != null).Sum(a => a.Weight);
        }
    }
    public BarrageSet()
    {
        Name = "BarrageSet";
        Description = "弹幕盒子";
    }

    public override void ApplyParameters(Dictionary<string, object> parameters)
    {
        if (parameters.TryGetValue("barrage", out var val))
        {
            B = (Barrage)val;
        }
        Skill = new Instance(B);
    }

    private class Instance : Skill, ISkillInstance
    {
        private readonly Barrage _BarrageSet;
        public Instance(Barrage parent)
        {
            Name = "BarrageSet";
            SkillGroup = "Item";
            Description = "发射弹幕";
            Cooldown = 2000;
            Targeting = new TargetType(Target.Grid, 1, 8);
            _BarrageSet = parent;
            Texture = GetTemplate(Name).Texture;
        }

        public Skill GetSkill(SkillItem parent)
        {
            return new Instance(_BarrageSet);
        }

        protected override void StartActivate(SkillContext sc)
        {
            _BarrageSet.Execute(sc);
        }
    }
}
