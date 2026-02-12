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
        HeadPicture.Texture =(Texture2D)GD.Load($"res://Assets/Portraits/Fix/{unit.Name}.png");
        string text1 = 
        $@"{(unit.TrName)} 
HP:{unit.Ua.CurrentHp:F0}/{unit.Ua.MaxHp:F0}
SP:{unit.Ua.CurrentSp:F0}/{unit.Ua.MaxSp:F0}
MP:{unit.Ua.CurrentMp:F0}/{unit.Ua.MaxMp:F0}
当前状态: ";
        foreach (Status s in unit.Status)
        {
            text1 += $"{s.TrName}({(s.Duration / 100):F0} 回合 )；";
        }
        if (unit.Status.Count == 0)
            text1 += " 无 ";
        text1 += $"\n 背包容量 ：{unit.Inventory.CurrentWeight:F1}/{unit.Inventory.MaxWeight:F1}";
        text1 += $"\n 装备容量 ：{unit.Equipment.CurrentEquipWeight:F1}/{unit.Equipment.MaxEquipWeight:F1}";
        
        string text2 =
        $"力量 :{unit.Ua.Str + 10}  敏捷 :{unit.Ua.Dex + 10}  体质 :{unit.Ua.Con + 10}\n"+
        $"灵力 :{unit.Ua.Spi + 10}  魔力 :{unit.Ua.Mag + 10}  灵巧 :{unit.Ua.Cun + 10}\n" +
        $"体术命中 :{unit.Ua.BodyDamageAccuracy * 100:F1}  弹幕命中 :{unit.Ua.BulletDamageAccuracy * 100:F1}\n" +
        $"闪避 :{unit.Ua.DamageEvasion * 100:F1}  擦弹 :{unit.Ua.BulletGraze * 100:F1}\n" +
        $"体术伤害 :{unit.Ua.DamageBody:F1}%\n" +
        $"弹幕伤害 :{unit.Ua.DamageBullet:F1}%\n" +
        $"整体速度 :{unit.Ua.SpeedGlobal:F1}%\n" +
        $"战斗速度 :{unit.Ua.SpeedCombat:F1}%\n" +
        $"移动速度 :{unit.Ua.SpeedMove:F1}%\n";

        string text3 = " 持有技能 ：\n";
        foreach ((SkillInstance si, _) in unit.Us.skills)
        {
            if (si.Template.SkillGroup != "")
                text3 += $" {si.Template.TrName} \n";
        }
        Info1.Text = text1;
        Info2.Text = text2;
        Info3.Text = TextEx.TrN(text3);
    }
}
