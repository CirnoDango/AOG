using Godot;
using System;
using static Godot.DisplayServer;

public partial class PauseMenu : ColorRect
{
    [Export]
    public Button ResumeButton;
    [Export]
    public Button DebugButton;
    [Export]
    public Button RestartButton;
    [Export]
    public Button ExitButton;
    [Export]
    public Control DebugMenu;
    public override void _Ready()
    {
        ResumeButton.Pressed += OnResumePressed;
        DebugButton.Pressed += OnDebugPressed;
        RestartButton.Pressed += OnRestartPressed;
        ExitButton.Pressed += OnExitPressed;
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
    private void OnResumePressed()
    {
        Hide();
        G.I.Fsm.ChangeState(Fsm.UpdateState);
    }
    private void OnDebugPressed()
    {
        DebugMenu.Visible = !DebugMenu.Visible;
    }
    private void OnRestartPressed()
    {
        Scene.Quit();
        G.I.Reset();
        var scenePath = GetTree().CurrentScene.SceneFilePath;
        GetTree().ChangeSceneToFile(scenePath);
    }
    private void OnExitPressed()
    {
        GetTree().Quit();
    }
}
