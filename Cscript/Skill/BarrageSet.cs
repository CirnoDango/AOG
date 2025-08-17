using Godot;
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
            B = (Barrage)(Dictionary<string, object>)val;
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
            Cooldown = 300;
            _BarrageSet = parent;
            Texture = GetTemplate(Name).Texture;
        }
        public override TargetType GetTargeting(int level = 0)
        {
            float maxD = _BarrageSet.Components.OfType<BulletModule>()      
                .Where(bm => bm != null)                                    
                .Select(bm => bm.bulletContext?.MaxDistance ?? 0)           
                .DefaultIfEmpty(0)                                          
                .Max();

            return new TargetType(Target.Grid, 1, (int)Mathf.Round(maxD));
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
