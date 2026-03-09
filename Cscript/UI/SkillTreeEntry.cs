using Godot;
using System;
using System.Linq;

public partial class SkillTreeEntry : TextureRect
{
    [Export]
    public TextureRect background;
    [Export]
    public TextureButton image;
    public string name;
    public void SetImage(string skillTreeName)
    {
        image.TextureNormal = Skill.SkillDeck.FirstOrDefault(x => x.SkillGroup == skillTreeName && x is SpellCard).Texture;
        name = skillTreeName;
        image.MouseEntered += MouseEnter;
    }

    public void MouseEnter()
    {
        if (G.I.Fsm.currentState != Fsm.SkillTreeState) return;
        if (name == "")
            G.I.SkillTreeBox.pointText.Text = $"{Tr("剩余天赋点")}:{G.I.Player.TalentPoint}";
        else
            G.I.SkillTreeBox.pointText.Text = $"{Tr("剩余天赋点")}:{G.I.Player.TalentPoint}\n{Tr("技能树")}：{TextEx.Tr($"sg{name}")}";
    }
}
