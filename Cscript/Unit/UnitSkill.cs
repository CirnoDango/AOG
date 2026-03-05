using System.Collections.Generic;
using System.Linq;

public class UnitSkill(Unit unit)
{
    private Unit _parent = unit;
    public SpellCard currentSpellcard;
    public List<(Skill skill, float weight)> skills = [];
    public Skill GetSkill(string name) => skills.FirstOrDefault(x => x.skill.Name == name).skill;
    public void LearnSkill(Skill skill, bool load = false)
    {
        Skill si = skill.Clone();
        if (skill.SkillGroup == "Item" && _parent == Player.PlayerUnit)
            si.CurrentCooldown = skill.Cooldown;
        AddSkill(si);
        if (Player.PlayerUnit == _parent)
        {
            G.I.SkillBar.LearnSkill(si);
        }
        if (!load)
            skill.OnLearn(_parent);
        skill.OnLoad(_parent);
    }
    public void LearnSkill(string Name)
    {
        Skill skill = Skill.NameSkill[Name];
        Skill si = skill.Clone();
        if (skill.SkillGroup == "Item")
            si.CurrentCooldown = skill.Cooldown;
        AddSkill(si);
        if (Player.PlayerUnit == _parent)
        {
            G.I.SkillBar.LearnSkill(si);
        }
        skill.OnLearn(_parent);
    }
    public void UnLearnSkill(Skill skill)
    {
        var s = skills.Select(x => x.skill).Where(x => x == skill).FirstOrDefault();
        var index = skills.FindIndex(entry => entry.skill == s);
        if (index >= 0)
        {
            if (Player.PlayerUnit == _parent)
            {
                G.I.SkillBar.UnLearnSkill(skills.Select(x => x.skill).ToList()[index]);
            }
            skills.RemoveAt(index);
        }
    }
    public void UnLearnSkill(string skilll)
    {
        Skill skill = Skill.NameSkill[skilll];
        UnLearnSkill(skill);
    }
    public void AddSkill(Skill newSkill, float weight = 10)
    {
        skills.Add((newSkill, weight));
    }
    public void LearnSkillGroup(string groupName)
    {
        foreach (var skill in Skill.SkillDeck.Where(s => s.SkillGroup == groupName))
        {
            LearnSkill(skill);
        }
    }
}
