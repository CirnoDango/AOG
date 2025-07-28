using System;
using System.Collections.Generic;

public class BarrageBox : SkillItem
{
    public Barrage barrage;
    public BarrageBox()
    {
        Name = "BarrageBox";
        Weight = 2f;
        Description = "弹幕盒子";
    }

    public override void ApplyParameters(Dictionary<string, object> parameters)
    {
        Skill = new Instance(this);
    }
    private class Instance : Skill, ISkillInstance
    {
        private readonly Barrage _barrageBox;
        public Instance(BarrageBox parent)
        {
            Name = "BarrageBox";
            SkillGroup = "Item";
            Description = "发射弹幕";
            Cooldown = 2000;
            Targeting = new TargetType(Target.Grid, 1, 8);
            _barrageBox = parent.barrage;
            Texture = GetTemplate(Name).Texture;
        }

        public Skill GetSkill(SkillItem parent)
        {
            return new Instance((BarrageBox)parent);
        }

        protected override void StartActivate(SkillContext sc)
        {
            _barrageBox.Activate(sc);
        }
    }
}
