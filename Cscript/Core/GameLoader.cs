using Godot;
using GodotPlugins.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class GameLoader : Node
{
    public static Node rootnode;
    [Export]
    public Camera2D camera;
    public override void _Ready()
    {
        rootnode = GetNode("Main");
    }
}
