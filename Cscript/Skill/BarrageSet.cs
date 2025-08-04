using System;
using System.Collections.Generic;

public class BarrageSet : SkillItem
{
    public Barrage barrage;
    public BarrageSet()
    {
        Name = "BarrageSet";
        Weight = 2f;
        Description = "弹幕盒子";
    }

    public override void ApplyParameters(Dictionary<string, object> parameters)
    {
        if (parameters.TryGetValue("barrage", out var val))
        {
            barrage = (Barrage)val;
        }
        Skill = new Instance(barrage);
    }

    public override object RandomSummonParam()
    {
        return new BarrageSet();
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
