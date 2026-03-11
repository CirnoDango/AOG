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
    [Export] Label tResume;
    [Export] Label tRestart;
    [Export] Label tDebug;
    [Export] Label tExit;
    [Export] Label tBack;
    public override void _Ready()
    {
        ResumeButton.Pressed += OnResumePressed;
        DebugButton.Pressed += OnDebugPressed;
        RestartButton.Pressed += OnRestartPressed;
        ExitButton.Pressed += OnExitPressed;
        Hide();
        tResume.Text = Tr("smContinue");
        tRestart.Text = Tr("smRestart");
        tDebug.Text = Tr("smOptions");
        tExit.Text = Tr("smQuit");
        tBack.Text = Tr("smBack");
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            var key = keyEvent.Keycode;
            if (key == Key.Escape)
            {
                Show();
                DebugMenu.Visible = false;
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
        G.I.SettingMenu.Visible = true;
        //DebugMenu.Visible = !DebugMenu.Visible;
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
