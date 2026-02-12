using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerStatusBar : VBoxContainer, IRegisterToG
{
    [Export] public CanvasLayer uilayer;

    private Label hpLabel, spLabel, grazeLabel, mpLabel;
    private ColorRect hpBarBack, spBarBack, mpBarBack;
    private ColorRect hpBar, spBar, mpBar;
    public Node StatusRow;
    private const float barMaxWidth = 4000;
    private const float barHeight = 400;
    public Dictionary<Status, Node> StatusImages = [];
    public void Init()
    {
        hpLabel = GetNode<Label>("HpRow/HpValue");
        spLabel = GetNode<Label>("SpRow/SpValue");
        mpLabel = GetNode<Label>("MpRow/MpValue");
        grazeLabel = GetNode<Label>("SpRow/SpValue/GrazePercent");

        hpBar = GetNode<ColorRect>("HpRow/BarBack/Bar");
        spBar = GetNode<ColorRect>("SpRow/BarBack/Bar");
        mpBar = GetNode<ColorRect>("MpRow/BarBack/Bar");

        hpBarBack = GetNode<ColorRect>("HpRow/BarBack");
        spBarBack = GetNode<ColorRect>("SpRow/BarBack");
        mpBarBack = GetNode<ColorRect>("MpRow/BarBack");

        Visible = true;
        StatusRow = GetNode("Status"); 
        UpdateStatusUI();
    }
    public void UpdateStatusUI()
    {
        float hpRatio = Player.PlayerUnit.Ua.CurrentHp / Player.PlayerUnit.Ua.MaxHp;
        float spRatio = Player.PlayerUnit.Ua.CurrentSp / Player.PlayerUnit.Ua.MaxSp;
        float mpRatio = Player.PlayerUnit.Ua.CurrentMp / Player.PlayerUnit.Ua.MaxMp;

        hpBar.Size = new Vector2(barMaxWidth * hpRatio, barHeight);
        spBar.Size = new Vector2(barMaxWidth * spRatio, barHeight);
        mpBar.Size = new Vector2(barMaxWidth * mpRatio, barHeight);
        if (Player.PlayerUnit.Us.currentSpellcard != null)
            spBar.Color = new Color(1, 0.5f, 0);
        else
            spBar.Color = new Color(120f/255, 200f/255, 0);

        hpLabel.Text = $"{(int)Player.PlayerUnit.Ua.CurrentHp}/{(int)Player.PlayerUnit.Ua.MaxHp}";
        spLabel.Text = $"{(int)Player.PlayerUnit.Ua.CurrentSp}/{(int)Player.PlayerUnit.Ua.MaxSp}";
        mpLabel.Text = $"{(int)Player.PlayerUnit.Ua.CurrentMp}/{(int)Player.PlayerUnit.Ua.MaxMp}";
        grazeLabel.Text = $"{(int)(100*UnitAttribute.GrazePercent(Player.PlayerUnit.Ua))}%";
    }

    public void RegisterToG(G g)
    {
        g.PlayerStatusBar = this;   
    }
}

