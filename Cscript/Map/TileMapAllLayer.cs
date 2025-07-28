using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.Emit;

public partial class TileMapAllLayer : Node, IRegisterToG
{
    [Export]
    public TileMapLayer BaseGround { get; set; }
    [Export]
    public TileMapLayer Stand { get; set; }
    [Export]
    public TileMapLayer FogOfWar { get; set; }
    public void RegisterToG(G g)
    {
        g.TileMapAllLayer = this;
    }
}
