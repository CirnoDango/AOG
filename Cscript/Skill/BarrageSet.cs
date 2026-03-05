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
        chestValue = 0;
    }

    public override void ApplyParameters(Dictionary<string, object> parameters)
    {
        parameters = MathEx.ConvertLong(parameters);
        base.ApplyParameters(parameters);
        if (Params.TryGetValue("barrage", out var val))
        {
            B = (Barrage)(Dictionary<string, object>)val;
        }
        if (Params.TryGetValue("colorR", out var r))
        {
            Texture = ImageEx.ColorizeTexture(Texture, new Color((float)r, (float)Params["colorG"], (float)Params["colorB"]));
        }
        else
        {
            Params.Add("colorR", GD.Randf());
            Params.Add("colorG", GD.Randf());
            Params.Add("colorB", GD.Randf());
            Texture = ImageEx.ColorizeTexture(Texture, new Color((float)Params["colorR"], (float)Params["colorG"], (float)Params["colorB"]));
        } 
        Skill = new Instance(this);
    }
    public override void GetParam()
    {
        Params["barrage"] = Barrage.GetParam(B);
    }

    public class Instance : Skill, ISkill
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
        public override float GetCooldown()
        {
            if (_BarrageSet.Components.Count > 0)
                return _BarrageSet.Components.Where(bc => bc != null).Select(bc => bc.CoolDown).Sum();
            else return 0;
        }
        public override float GetSpCost()
        {
            if (_BarrageSet.Components.Count > 0)
                return _BarrageSet.Components.Where(bc => bc != null).Select(bc => bc.SpCost).Sum();
            else return 0;
        }
        public override TargetType GetTargeting()
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
