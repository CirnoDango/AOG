using Godot;
using System;
using System.Collections.Generic;

public partial class BarrageBar : Node
{
    public Label Label { get; set; }
    public HBoxContainer HBox { get; set; }
    public List<TextureButton> Buttons { get; set; }
}
