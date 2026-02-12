using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

public partial class SpriteManager : Node
{
    
    public static void LoadEnemy(Unit unit)
    {
        string name = unit.Name;
        var texture = GD.Load<Texture2D>($"res://Assets/Sprites/{name}.png");
        var sprite = new Sprite2D { Texture = texture };
        Vector2 originalSize = texture.GetSize(); // 原图尺寸
        Vector2 targetSize = new Vector2(15, 15); // 显示目标尺寸
        sprite.Scale = targetSize * Setting.imagePx / 16 / originalSize;
        unit.Ua.imageSizeFactor = sprite.Scale.Y;
        var tip_area = new DynamicTooltipPanel
        {
            TooltipSource = unit.Ua.Description,
            Size = sprite.Texture.GetSize(), // sprite.Scale,
            Modulate = new Color(1, 1, 1, 0)
        };
        sprite.AddChild(tip_area);
        tip_area.GlobalPosition += new Vector2I(-8, -8)/sprite.Scale;
        unit.Up.sprite = sprite;
    }
    public static void LoadSkills()
    {
        foreach (var skill in Skill.SkillDeck)
        {
            if (skill.SkillGroup == null || skill.SkillGroup == "" || skill.SkillGroup == "General"
                || skill.SkillGroup == "Private")
                continue;
            Texture2D texture;
            if (skill.SkillGroup == "Item")
                texture = GD.Load<Texture2D>($"res://Assets/Item/{skill.Name}.png");
            else
                texture = GD.Load<Texture2D>($"res://Assets/Skill/{skill.SkillGroup}_{skill.Name}.png");
            skill.Texture = texture;
        }
    }
}

public partial class DynamicTooltipPanel : Panel
{
    public Func<string> TooltipSource;

    public override string _GetTooltip(Vector2 atPosition)
    {
        return TooltipSource != null ? TooltipSource() : string.Empty;
    }  
}

