using Godot;
using System;
using static System.Net.Mime.MediaTypeNames;

public partial class CombatAbilityBox : Control
{
    [Export]
    public TextureRect HeadPicture;
    [Export]
    public Label Info1;
    [Export]
    public Label Info2;
    [Export]
    public Label Info3;
    public void Update(Unit unit)
    {
        HeadPicture.Texture =(Texture2D)GD.Load($"res://assets/Portraits/Fix/{unit.Name}.png");
        string text1 = 
$@"{(unit.TrName)} 
HP:{unit.CurrentHp:F0}/{unit.MaxHp:F0}
SP:{unit.CurrentSp:F0}/{unit.MaxSp:F0}
MP:{unit.CurrentMp:F0}/{unit.MaxMp:F0}
当前状态: ";
        foreach (Status s in unit.Status)
        {
            text1 += $"{s.TrName}({(s.Duration / 100):F0} 回合 )；";
        }
        if (unit.Status.Count == 0)
            text1 += " 无 ";
        text1 += $"\n 背包容量 ：{unit.inventory.CurrentWeight:F1}/{unit.inventory.MaxWeight:F1}";
        text1 += $"\n 装备容量 ：{unit.equipment.CurrentEquipWeight:F1}/{unit.equipment.MaxEquipWeight:F1}";
        
        string text2 =
$@" 力量 :{unit.Ua.Str + 10}  敏捷 :{unit.Ua.Dex + 10}  体质 :{unit.Ua.Con + 10} 
 灵力 :{unit.Ua.Spi + 10}  魔力 :{unit.Ua.Mag + 10}  灵巧 :{unit.Ua.Cun + 10}
 体术伤害 :{unit.Ua.DamageBody:F1}%
 弹幕伤害 :{unit.Ua.DamageBullet:F1}%
 整体速度 :{unit.Ua.SpeedGlobal:F1}%
 战斗速度 :{unit.Ua.SpeedCombat:F1}%
 移动速度 :{unit.Ua.SpeedMove:F1}%
";
        string text3 = " 持有技能 ：\n";
        foreach ((SkillInstance si, _) in unit.skills)
        {
            if (si.Template.SkillGroup != "")
                text3 += $" {si.Template.TrName} \n";
        }
        Info1.Text = TextEx.TrN(text1);
        Info2.Text = TextEx.TrN(text2);
        Info3.Text = TextEx.TrN(text3);
    }
}
