using Godot;
using System;
using static Godot.DisplayServer;

public partial class DebugMenu : ColorRect
{
    [Export]
    public Button Debug1;
    [Export]
    public Button Debug2;
    [Export]
    public Button Debug3;
    [Export]
    public Button Debug4;
    public override void _Ready()
    {
        Debug1.Pressed += OnDebug1Pressed;
        Debug2.Pressed += OnDebug2Pressed;
        Debug3.Pressed += OnDebug3Pressed;
        Debug4.Pressed += OnDebug4Pressed;
        Hide();
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            var key = keyEvent.Keycode;
            if (key == Key.Escape)
            {
                Show();
                G.I.Fsm.ChangeState(Fsm.TalkState);
            }
        }
    }
    private void OnDebug1Pressed()
    { 
        Player.playerCamera.Zoom /= 3;
        G.I.TileMapAllLayer.oFogOfWar.Visible = false;
    }
    private void OnDebug2Pressed()
    {
        Player.PlayerUnit.Ua.MaxHp += 100000;
        Player.PlayerUnit.Ua.CurrentHp = Player.PlayerUnit.Ua.MaxHp;
    }
    private void OnDebug3Pressed()
    {
        G.I.Player.TalentPoint = 100;
        G.I.Player.SkillPoint = 100;
        G.I.Player.UaPoint = 100;
        Player.PlayerUnit.Ua.Cun += 100;
        Player.PlayerUnit.Ua.Spi += 100;
        Player.PlayerUnit.Ua.Mag += 100;
    }
    private void OnDebug4Pressed()
    {
        Scene.LeaveAndGo();
    }
}
