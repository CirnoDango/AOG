using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class SkillBar : FlowContainer, IRegisterToG
{
    
    [Export]
    public FlowContainer skillContainer;
    private Dictionary<Skill, TextureButton> skillButtons = [];
    [Export]
    public PackedScene skillButtonScene;
    [Export]
    public PackedScene spellButtonScene;
    [Export]
    public Label skillInfo;
    public static Dictionary<string, Color> skillBoxColor = new()
    {
        { "YinyangBall", Colors.Crimson},
        { "Circle", Colors.MediumSpringGreen },
        { "Star", Colors.Yellow },
        { "Dark", Colors.Maroon },
        { "Freeze", Colors.LightSkyBlue },
        { "Fist", Colors.MediumVioletRed},
        { "FireElement", Colors.OrangeRed },
        { "Time", Colors.Silver },
        { "Blood", Colors.DarkRed },
        { "Winter", Colors.SteelBlue },
    };
    // 学会一个技能，添加按钮
    public void LearnSkill(Skill skill)
    {
        if (skillButtons.ContainsKey(skill))
            return;

        if (skill == null || skill.Texture == null)
        {
            GD.PrintErr($"{skill.Name}技能或技能贴图为空！");
            return;
        }
        Sprite2D s;
        if (skill is SpellCard)
        {
            s = (Sprite2D)spellButtonScene.Instantiate();
        }
        else
        {
            s = (Sprite2D)skillButtonScene.Instantiate();
        }
        s.Modulate = skillBoxColor.TryGetValue(skill.SkillGroup, out Color value) ? value : new Color(1, 1, 1);


        var btn = new SkillButton
        {
            Name = skill.Name,
            TextureNormal = skill.Texture,
            IgnoreTextureSize = true,
            CustomMinimumSize = new Vector2(60, 60),
            StretchMode = TextureButton.StretchModeEnum.Scale,
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            FocusMode = FocusModeEnum.None,
            Skill = skill
        };
        btn.MouseEntered += () => { OnMouseEntered(skill); };
        btn.MouseExited += OnMouseExited;
        float ratio = 60 / skill.Texture.GetSize().X;
        btn.Scale = new Vector2(ratio, ratio);
        // 冷却时间文本标签
        Label cdLabel = new()
        {
            Text = "", // 初始不显示
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            MouseFilter = MouseFilterEnum.Ignore, // 不遮挡按钮点击
        };
        btn.AddChild(cdLabel);
        cdLabel.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); // 占满整个按钮
        cdLabel.AddThemeFontSizeOverride("font_size", 40);
        // 你可能想记住 label 引用，可以扩展 skillButtons 成结构体或字典
        btn.SetMeta("CdLabel", cdLabel);

        btn.Pressed += () => OnSkillButtonPressed(skill);
        btn.AddChild(s);
        skillContainer.AddChild(btn);
        skillButtons[skill] = btn;
    }
    public void UnLearnSkill(Skill skill)
    {
        if (skillButtons.TryGetValue(skill, out TextureButton btn))
        {
            skillContainer.RemoveChild(btn);
            btn.QueueFree();
            skillButtons.Remove(skill);
        }
    }
    public void UpdateSkillCooldowns()
    {
        foreach (var kvp in skillButtons)
        {
            Skill skill = kvp.Key;
            TextureButton btn = kvp.Value;
            var label = (Label)(GodotObject)btn.GetMeta("CdLabel");
            if (skill.CurrentCooldown > 0)
            {
                label.Text = Math.Ceiling(skill.CurrentCooldown / 100).ToString();
                label.Modulate = new Color(0, 1, 0);
            }
            else
            {
                label.Text = "";
            }
        }
    }

    private static void OnSkillButtonPressed(Skill si)
    {
        if (G.I.Fsm.currentState is not PlayerSkillState)
            return;
        var skill = si;
        var level = si.Level;
        if (!si.CanUse(Player.PlayerUnit))
        {
            Info.Print("不满足发动条件");
            return;
        }
        if (!skill.GetTargeting().TargetRule.MapClick)
        {
            skill.Activate(new SkillContext(Player.PlayerUnit, Player.PlayerUnit.Us.GetSkill(skill.Name).Level));
        }
        else
        {
            // 通知技能目标层开始选择目标
            HighlightManager.StartTargeting(si);
            G.I.Fsm.ChangeState(Fsm.PlayerSkillTargetState);
        } 
    }
    private void OnMouseEntered(Skill skill)
    {
        skillInfo.Text = skill.SkillInfo();
    }

    // 鼠标移出按钮时
    private void OnMouseExited()
    {
        skillInfo.Text = "";
    }
    public void RegisterToG(G g)
    {
        g.SkillBar = this;
	}
}

public partial class SkillButton : TextureButton
{
    public Skill Skill;
}