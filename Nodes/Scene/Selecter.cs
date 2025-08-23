using Godot;
using System;

public partial class Selecter : OptionButton
{
    public override void _Ready()
    {
        GetPopup().AddThemeFontSizeOverride("font_size", 60);
    }

}
