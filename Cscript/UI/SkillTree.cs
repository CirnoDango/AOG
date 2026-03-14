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
    private List<string> skillNames = []; // 记录每个按钮对应的技能名
    public string name;
    public int MaxLevel
    {
        get
        {
            if (Player.skillTrees.TryGetValue(name, out var skillTree) && skillTree == HexStatus.expert)
                return 4;
            return 3;
        }
    }
    public void Init(string skilltree)
    {
        skillNames.Clear();
        name = skilltree;
        title.Text = TextEx.Tr($"{TextEx.Tr("技能组")} ：{skilltree}");

        int index = 0;
        foreach (var skill in Skill.SkillDeck)
        {
            if (skill.SkillGroup != skilltree) continue;
            if (index >= skillButtons.Length) break;

            var skillName = skill.Name;
            skillNames.Add(skillName);
            tr[index].Texture = GD.Load<Texture2D>($"res://Assets/Skill/{skilltree}_{skillName}.png");
            levelLabels[index].Text = $"{VLevel(skillName)}/{MaxLevel}";

            int buttonIndex = index; // 必须缓存变量以避免闭包错误！
            skillButtons[index].Pressed += () => OnSkillPressed(skillNames[buttonIndex], levelLabels[buttonIndex]);
            skillButtons[index].MouseEntered += () =>
            {
                int level = 0;
                if (Player.PlayerUnit.Us.GetSkill(skillName) != null)
                    level = Player.PlayerUnit.Us.GetSkill(skillName).Level;
                ShowSkillInfo(skill, level);
            };
            index++;
        }
    }

    private void ShowSkillInfo(Skill skill, int level)
    {
        if (MaxLevel == 3)
        {
            G.I.Ua.info.Text = level switch
            {
                1 => TextEx.HighlightChanges(skill.SkillInfo(1), skill.SkillInfo(2)),
                2 => TextEx.HighlightChanges(skill.SkillInfo(2), skill.SkillInfo(3)),
                3 => skill.SkillInfo(3),
                4 => skill.SkillInfo(4),
                _ => skill.SkillInfo(1),
            };
        }
        else
        {
            G.I.Ua.info.Text = level switch
            {
                1 => TextEx.HighlightChanges(skill.SkillInfo(1), skill.SkillInfo(2)),
                2 => TextEx.HighlightChanges(skill.SkillInfo(2), skill.SkillInfo(3)),
                3 => TextEx.HighlightChanges(skill.SkillInfo(3), skill.SkillInfo(4)),
                4 => skill.SkillInfo(4),
                _ => skill.SkillInfo(1),
            };
        }
    }

    public void UpdateTree()
    {
        for (int i = 0; i < skillNames.Count; i++)
        {
            string skillName = skillNames[i];
            var skill = Skill.SkillDeck.Find(s => s.Name == skillName && s.SkillGroup == name);
            if (skill == null) continue;
            // 更新技能等级
            levelLabels[i].Text = $"{VLevel(skillName)}/{MaxLevel}";
        }
    }
    private int VLevel(string skillName)
    {
        var Skill = Player.PlayerUnit.Us.skills.FirstOrDefault(si => si.skill.Name == skillName).skill;
        if (Skill != null)
        {
            return Skill.Level;
        }
        else
            return 0;
    }

    private void OnSkillPressed(string skillName, Label label)
    {
        if (G.I.Player.SkillPoint <= 0)
            return;
        var si = Player.PlayerUnit.Us.GetSkill(skillName);
        if (si != null && si is SpellCard sc && si.IsActive)
        {
            Info.Print(TextEx.Tr("激活中的符卡不允许升级！"));
            return;
        }
        var Skill = Player.PlayerUnit.Us.skills.FirstOrDefault(si => si.skill.Name == skillName).skill;
        if (Skill != null)
        {
            if (Skill.Level >= MaxLevel)
                return;
            Skill.Level += 1; // 升级技能
            UpdateTree();
            G.I.Player.SkillPoint -= 1;
            G.I.SkillPanel.Refresh();
        }
        else
        {
            Player.PlayerUnit.Us.LearnSkill(Skill.SkillDeck.FirstOrDefault(s => s.Name == skillName));
            Skill = Player.PlayerUnit.Us.skills.FirstOrDefault(si => si.skill.Name == skillName).skill;
            UpdateTree();
            G.I.Player.SkillPoint -= 1;
            G.I.SkillPanel.Refresh();
        }
        ShowSkillInfo(Skill, Skill.Level);
        GameEvents.SkillLearned(Player.PlayerUnit, Skill);
    }
}
