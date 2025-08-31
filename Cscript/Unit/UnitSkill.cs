using System.Collections.Generic;
using System.Linq;

public class UnitSkill(Unit unit)
{
    private Unit _parent = unit;
    public SpellCard currentSpellcard;
    public List<(SkillInstance skill, float weight)> skills = [];
    public SkillInstance GetSkill(string name) => skills.FirstOrDefault(x => x.skill.Name == name).skill;
    public void LearnSkill(Skill skill)
    {
        SkillInstance si = new(skill);
        if (skill.SkillGroup == "Item" && _parent == Player.PlayerUnit)
            si.CurrentCooldown = skill.Cooldown;
        AddSkill(si);
        if (Player.PlayerUnit == _parent)
        {
            G.I.SkillBar.LearnSkill(si);
        }
        skill.OnLearn(_parent); // 👈 自动触发学习效果
    }
    public void LearnSkill(string Name)
    {
        Skill skill = Skill.NameSkill[Name];
        SkillInstance si = new(skill);
        if (skill.SkillGroup == "Item")
            si.CurrentCooldown = skill.Cooldown;
        AddSkill(si);
        if (Player.PlayerUnit == _parent)
        {
            G.I.SkillBar.LearnSkill(si);
        }
        skill.OnLearn(_parent); // 👈 自动触发学习效果
    }
    public void UnLearnSkill(Skill skill)
    {
        var index = skills.FindIndex(entry => entry.skill.Name == skill.Name);
        if (index >= 0)
        {
            if (Player.PlayerUnit == _parent)
            {
                G.I.SkillBar.UnLearnSkill(Player.PlayerUnit.Us.GetSkill(skill.Name));
            }
            skills.RemoveAt(index);
        }
    }
    public void UnLearnSkill(string skilll)
    {
        Skill skill = Skill.NameSkill[skilll];
        UnLearnSkill(skill);
    }
    public void AddSkill(SkillInstance newSkill, float weight = 10)
    {
        var index = skills.FindIndex(entry => entry.skill.Name == newSkill.Name);
        if (index >= 0 && _parent != Player.PlayerUnit)
            skills[index] = (newSkill, weight);  // 替换
        else
            skills.Add((newSkill, weight));      // 添加
    }
    public void LearnSkillGroup(string groupName)
    {
        foreach (var skill in Skill.SkillDeck.Where(s => s.SkillGroup == groupName))
        {
            LearnSkill(skill);
        }
    }
    
}
