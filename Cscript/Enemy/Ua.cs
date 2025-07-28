using Godot;
using System;
using System.Collections.Generic;
public partial class Ua : VBoxContainer, IRegisterToG
{
    [Export]
    public Label Point;
    [Export]
    public Button str;
    [Export]
    public Button dex;
    [Export]
    public Button con;
    [Export]
    public Button spi;
    [Export]
    public Button mag;
    [Export]
    public Button cun;
    [Export]
    public RichTextLabel info;
    private Dictionary<Button, string> buttonHints;

    public override void _Ready()
    {
        // 初始化字典
        buttonHints = new Dictionary<Button, string>
        {
            { str, "+1力量：+2%体术伤害，+1普攻伤害" },
            { dex, "+1敏捷：+1%整体速度，+1%体术命中，+1%体术闪避" },
            { con, "+1体质：+2%治疗系数，+5MaxHp" },
            { spi, "+1灵力：+1%弹幕伤害，+2MaxSp" },
            { mag, "+1魔力：+1%弹幕伤害，+1MaxMp，+0.1Mp回复" },
            { cun, "+1灵巧：+1装备负重，+1背包负重" }
        };

        // 为每个按钮注册事件
        foreach (var pair in buttonHints)
        {
            Button button = pair.Key;
            button.MouseEntered += () => info.Text = buttonHints[button];
            button.MouseExited += () => info.Text = "";
        }
    }

    public void Refresh()
    {
        Point.Text = $"剩余属性点:{G.I.Player.UaPoint}";
        str.Text = $"力量:{Player.PlayerUnit.Ua.Str + 10}";
        dex.Text = $"敏捷:{Player.PlayerUnit.Ua.Dex + 10}";
        con.Text = $"体质:{Player.PlayerUnit.Ua.Con + 10}";
        spi.Text = $"灵力:{Player.PlayerUnit.Ua.Spi + 10}";
        mag.Text = $"魔力:{Player.PlayerUnit.Ua.Mag + 10}";
        cun.Text = $"灵巧:{Player.PlayerUnit.Ua.Cun + 10}";
    }
    public void RegisterToG(G g)
    {
        g.Ua = this;
        str.Pressed += () => { if (G.I.Player.UaPoint > 0) { Player.PlayerUnit.Ua.Str += 1; G.I.Player.UaPoint -= 1; Refresh(); } };
        dex.Pressed += () => { if (G.I.Player.UaPoint > 0) { Player.PlayerUnit.Ua.Dex += 1; G.I.Player.UaPoint -= 1; Refresh(); } };
        con.Pressed += () => { if (G.I.Player.UaPoint > 0) { Player.PlayerUnit.Ua.Con += 1; G.I.Player.UaPoint -= 1; Refresh(); } };
        spi.Pressed += () => { if (G.I.Player.UaPoint > 0) { Player.PlayerUnit.Ua.Spi += 1; G.I.Player.UaPoint -= 1; Refresh(); } };
        mag.Pressed += () => { if (G.I.Player.UaPoint > 0) { Player.PlayerUnit.Ua.Mag += 1; G.I.Player.UaPoint -= 1; Refresh(); } };
        cun.Pressed += () => { if (G.I.Player.UaPoint > 0) { Player.PlayerUnit.Ua.Cun += 1; G.I.Player.UaPoint -= 1; Refresh(); } };
    }
}