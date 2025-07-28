using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class SkillBar : FlowContainer, IRegisterToG
{
    
    [Export]
    public FlowContainer skillContainer;
    private Dictionary<SkillInstance, TextureButton> skillButtons = [];
    [Export]
    public PackedScene skillButtonScene;
    [Export]
    public PackedScene spellButtonScene;
    public Dictionary<string, Color> skillBoxColor = new()
    {
        { "Freeze", new Color(0.5f, 0.5f, 0.9f) },
        { "Star", new Color(0.9f, 0.9f, 0.1f) },
        { "Dark", new Color(0.2f, 0.1f, 0.1f) },
    };
    // 学会一个技能，添加按钮
    public void LearnSkill(SkillInstance skill)
    {
        if (skillButtons.ContainsKey(skill))// || skill.Template.EffectType == EffectType.Passive)
            return;

        if (skill == null || skill.Texture == null)
        {
            GD.PrintErr($"{skill.Name}技能或技能贴图为空！");
            return;
        }
        Sprite2D s;
        if (skill.Template is SpellCard)
        {
            s = (Sprite2D)spellButtonScene.Instantiate();
        }
        else
        {
            s = (Sprite2D)skillButtonScene.Instantiate();
        }
        s.Modulate = skillBoxColor.TryGetValue(skill.Template.SkillGroup, out Color value) ? value : new Color(1, 1, 1);


        var btn = new TextureButton
        {
            Name = skill.Name,
            TextureNormal = skill.Texture,
            IgnoreTextureSize = true,
            CustomMinimumSize = new Vector2(60, 60),
            StretchMode = TextureButton.StretchModeEnum.Scale,
            TooltipText = Tr(skill.Template.SkillInfo(Player.PlayerUnit.GetSkill(skill.Name).Level)),
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
        };
        float ratio = 60 / skill.Texture.GetSize().X;
        btn.Scale = new Vector2(ratio, ratio);
        // 冷却时间文本标签
        Label cdLabel = new()
        {
            Text = "", // 初始不显示
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            MouseFilter = Control.MouseFilterEnum.Ignore, // 不遮挡按钮点击
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
    public void UnLearnSkill(SkillInstance skill)
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
            SkillInstance skill = kvp.Key;
            TextureButton btn = kvp.Value;
            var label = (Label)(GodotObject)btn.GetMeta("CdLabel");
            if (skill.CurrentCooldown > 0)
            {
                label.Text = Math.Ceiling(skill.CurrentCooldown / 100).ToString();
                label.Modulate = new Color(1,0,0);
            }
            else
            {
                label.Text = "";
            }
        }
    }

    private static void OnSkillButtonPressed(SkillInstance si)
    {
        var skill = si.Template;
        var skillName = skill.Name;
        var level = si.Level;
        if (si.CurrentCooldown > 0)
        {
            Info.Print("技能正在冷却中！");
            return;
        }
        if (Player.PlayerUnit.CurrentSp < skill.GetSpCost(level) || Player.PlayerUnit.CurrentMp < skill.GetMpCost(level))
        {
            Info.Print("不满足发动条件！");
            return;
        }
        if (skill is SpellCard spell && Player.PlayerUnit.CurrentSp < spell.GetSpNeed(level))
        {
            Info.Print("不满足发动条件！");
            return;
        }
        if (skill.EffectType == EffectType.Passive)
        {
            Info.Print("这是被动技能");
            return;
        }
        if (skill.GetTargeting(level).Type == Target.Self)
        {
            skill.Activate(new SkillContext(Player.PlayerUnit, Player.PlayerUnit.GetSkill(skill.Name).Level), si);
        }
        else
        {
            // 通知技能目标层开始选择目标
            HighlightManager.StartTargeting(si);
            G.I.Fsm.ChangeState(Fsm.playerSkillTargetState);
        } 
    }

    public void RegisterToG(G g)
    {
        g.SkillBar = this;
	}
}

