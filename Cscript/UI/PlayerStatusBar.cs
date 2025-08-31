using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerStatusBar : VBoxContainer, IRegisterToG
{
    [Export] public CanvasLayer uilayer;

    private Label hpLabel, spLabel, mpLabel;
    private ColorRect hpBarBack, spBarBack, mpBarBack;
    private ColorRect hpBar, spBar, mpBar;
    public Node StatusRow;
    private const float barMaxWidth = 4000; // 对应编辑器中背景条宽度
    private const float barHeight = 400;
    public Dictionary<Status, Node> StatusImages = [];
    public void Init()
    {
        hpLabel = GetNode<Label>("HpRow/Value");
        spLabel = GetNode<Label>("SpRow/Value");
        mpLabel = GetNode<Label>("MpRow/Value");

        hpBar = GetNode<ColorRect>("HpRow/Bar");
        spBar = GetNode<ColorRect>("SpRow/Bar");
        mpBar = GetNode<ColorRect>("MpRow/Bar");

        hpBarBack = GetNode<ColorRect>("HpRow/Bar/BarBack");
        spBarBack = GetNode<ColorRect>("SpRow/Bar/BarBack");
        mpBarBack = GetNode<ColorRect>("MpRow/Bar/BarBack");

        Visible = true;
        StatusRow = GetNode("Status"); 
        UpdateStatusUI();
    }
    public void UpdateStatusUI()
    {
        float hpRatio = Player.PlayerUnit.Ua.CurrentHp / Player.PlayerUnit.Ua.MaxHp;
        float spRatio = Player.PlayerUnit.Ua.CurrentSp / Player.PlayerUnit.Ua.MaxSp;
        float mpRatio = Player.PlayerUnit.Ua.CurrentMp / Player.PlayerUnit.Ua.MaxMp;

        hpBar.CustomMinimumSize = new Vector2(barMaxWidth * hpRatio, barHeight);
        spBar.CustomMinimumSize = new Vector2(barMaxWidth * spRatio, barHeight);
        mpBar.CustomMinimumSize = new Vector2(barMaxWidth * mpRatio, barHeight);
        if (Player.PlayerUnit.Us.currentSpellcard != null)
            spBar.Color = new Color(1, 0.5f, 0);
        else
            spBar.Color = new Color(1, 1, 0);

        hpLabel.Text = $"{(int)Player.PlayerUnit.Ua.CurrentHp}/{(int)Player.PlayerUnit.Ua.MaxHp}";
        spLabel.Text = $"{(int)Player.PlayerUnit.Ua.CurrentSp}/{(int)Player.PlayerUnit.Ua.MaxSp}";
        mpLabel.Text = $"{(int)Player.PlayerUnit.Ua.CurrentMp}/{(int)Player.PlayerUnit.Ua.MaxMp}";
    }

    public void RegisterToG(G g)
    {
        g.PlayerStatusBar = this;   
    }
}

