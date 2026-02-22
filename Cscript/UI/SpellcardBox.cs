using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class SpellcardBox : VBoxContainer, IRegisterToG
{
    [Export] public PackedScene SpellcardBar;
    public List<SpellcardBar> scs = [];

    public void Init(SkillInstance si)
    {
        SpellcardBar sb = (SpellcardBar)SpellcardBar.Instantiate();
        sb.Init(si);
        scs.Add(sb);
        AddChild(sb);
    }
    public void Remove(SkillInstance si)
    {
        SpellcardBar sb = scs.FirstOrDefault(x => x.sc == si);
        scs.Remove(sb);
        sb.QueueFree();

    }
    public void RegisterToG(G g)
    {
        g.SpellcardBox = this;
    }
}
