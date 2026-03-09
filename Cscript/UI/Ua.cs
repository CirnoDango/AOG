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
        
        buttonHints = new Dictionary<Button, string>
        {
            { str, Tr("estr") },
            { dex, Tr("edex") },
            { con, Tr("econ") },
            { spi, Tr("espi") },
            { mag, Tr("emag") },
            { cun, Tr("ecun") },
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
        Point.Text = $"{Tr("剩余属性点")}:{G.I.Player.UaPoint}";
        str.Text = $"{Tr("力量")}:{Player.PlayerUnit.Ua.Str + 10}";
        dex.Text = $"{Tr("敏捷")}:{Player.PlayerUnit.Ua.Dex + 10}";
        con.Text = $"{Tr("体质")}:{Player.PlayerUnit.Ua.Con + 10}";
        spi.Text = $"{Tr("灵力")}:{Player.PlayerUnit.Ua.Spi + 10}";
        mag.Text = $"{Tr("魔力")}:{Player.PlayerUnit.Ua.Mag + 10}";
        cun.Text = $"{Tr("灵巧")}:{Player.PlayerUnit.Ua.Cun + 10}";
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