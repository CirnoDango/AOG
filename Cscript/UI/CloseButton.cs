using Godot;
using System;

public partial class CloseButton : TextureButton
{
    public override void _Pressed()
    {
        G.I.Fsm.ChangeState(Fsm.PlayerSkillState);
    }
}
