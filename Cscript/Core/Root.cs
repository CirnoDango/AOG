using Godot;

public partial class Root : Node2D
{
    public static Node rootnode;
    [Export]
    public Camera2D camera;
    public override void _Ready()
    {
        rootnode = GetNode("Main");
        Scale = new Vector2(Setting.rootnodeScale, Setting.rootnodeScale);
    }
}
