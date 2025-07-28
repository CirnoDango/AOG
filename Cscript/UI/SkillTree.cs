using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SkillTree : Control
{
    [Export] public Label title;
    [Export] public TextureButton[] skillButtons = new TextureButton[5];
    [Export] public Label[] levelLabels = new Label[5];
    [Export] public TextureRect[] tr = new TextureRect[5];
    private List<string> skillNames = new(); // 记录每个按钮对应的技能名

    public void Init(string skilltree)
    {
        skillNames.Clear();
        title.Text = TextEx.TrN($"技能组 ： {skilltree}");

        int index = 0;
        foreach (var skill in Skill.SkillDeck)
        {
            if (skill.SkillGroup != skilltree) continue;
            if (index >= skillButtons.Length) break;

            var skillName = skill.Name;
            skillNames.Add(skillName);
            tr[index].Texture = GD.Load<Texture2D>($"res://assets/Skill/{skilltree}_{skillName}.png");
            levelLabels[index].Text = $"{VLevel(skillName)}/3";

            int buttonIndex = index; // 必须缓存变量以避免闭包错误！
            skillButtons[index].Pressed += () => OnSkillPressed(skillNames[buttonIndex], levelLabels[buttonIndex]);
            skillButtons[index].MouseEntered += () =>
            {
                int level = 0;
                if(Player.PlayerUnit.GetSkill(skillName) != null)
                    level = Player.PlayerUnit.GetSkill(skillName).Level;
                G.I.Ua.info.Text = level switch
                {
                    1 => TextEx.HighlightChanges(skill.SkillInfo(1), skill.SkillInfo(2)),
                    2 => TextEx.HighlightChanges(skill.SkillInfo(2), skill.SkillInfo(3)),
                    3 => TextEx.HighlightChanges(skill.SkillInfo(3), skill.SkillInfo(4)),
                    4 => skill.SkillInfo(4),
                    _ => skill.SkillInfo(1),
                };
            };


            index++;
        }
    }

    private int VLevel(string skillName)
    {
        var skillInstance = Player.PlayerUnit.skills.FirstOrDefault(si => si.skill.Name == skillName).skill;
        if (skillInstance != null)
        {
            return skillInstance.Level;
        }
        else
            return 0;
    }

    private void OnSkillPressed(string skillName, Label label)
    {
        if (G.I.Player.SkillPoint <= 0)
            return;
        var si = Player.PlayerUnit.GetSkill(skillName);
        if (si != null && si.Template is SpellCard sc && si.IsActive)
        {
            Info.Print(TextEx.TrN("激活中的符卡不允许升级！"));
            return;
        }
        var skillInstance = Player.PlayerUnit.skills.FirstOrDefault(si => si.skill.Name == skillName).skill;
        if (skillInstance != null)
        {
            if (skillInstance.Level >= 3)
                return;
            skillInstance.Level += 1; // 升级技能
            label.Text = $"{skillInstance.Level}/3";
            G.I.Player.SkillPoint -= 1;
            (G.I.SkillPanel).Refresh();
        }
        else
        {
            Player.PlayerUnit.LearnSkill(Skill.SkillDeck.FirstOrDefault(s => s.Name == skillName));
            skillInstance = Player.PlayerUnit.skills.FirstOrDefault(si => si.skill.Name == skillName).skill;
            label.Text = $"{skillInstance.Level}/3";
            G.I.Player.SkillPoint -= 1;
            (G.I.SkillPanel).Refresh();
        }
    }
}
