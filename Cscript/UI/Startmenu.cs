using Godot;
using System;

public partial class Startmenu : Control
{
    [Export] public Button start;
    [Export] public Button quit;
    public override void _Ready()
    {
        start.Pressed += OnStartPressed;
        quit.Pressed += OnExitPressed;
        //Visible = true;
    }
    private void OnStartPressed()
    {
        Visible = false;
    }
    private void OnExitPressed()
    {
        GetTree().Quit();
    }
}
