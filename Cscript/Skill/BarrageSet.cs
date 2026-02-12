using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class BarrageSet : SkillItem<BarrageSet.Instance>
{
    public Barrage B = new();
    public override float Weight
    {
        get
        {
            return 8f;
        }
    }
    public BarrageSet()
    {
        Name = "BarrageSet";
        chestValue = 0;
    }

    public override void ApplyParameters(Dictionary<string, object> parameters)
    {
        if (parameters.TryGetValue("barrage", out var val))
        {
            B = (Barrage)(Dictionary<string, object>)val;
        }
        Texture = ImageEx.ColorizeTexture(Texture, new Color(GD.Randf(), GD.Randf(), GD.Randf()));
        Skill = new Instance(this);
    }

    public class Instance : Skill, ISkillInstance
    {
        private readonly Barrage _BarrageSet;
        public Instance(BarrageSet parent)
        {
            Name = "BarrageSet";
            SkillGroup = "Item";
            Description = "发射弹幕";
            _BarrageSet = parent.B;
            Texture = parent.Texture;
        }
        public override float GetCooldown(int level)
        {
            if (_BarrageSet.Components.Count > 0)
                return _BarrageSet.Components.Where(bc => bc != null).Select(bc => bc.CoolDown).Sum();
            else return 0;
        }
        public override float GetSpCost(int level)
        {
            if (_BarrageSet.Components.Count > 0)
                return _BarrageSet.Components.Where(bc => bc != null).Select(bc => bc.SpCost).Sum();
            else return 0;
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
            return new Instance((BarrageSet)parent);
        }

        protected override void StartActivate(SkillContext sc)
        {
            _BarrageSet.Execute(sc);
        }
    }
}
