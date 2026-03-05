using Godot;
using System;

public partial class SpellcardBar : Control
{
    [Export] public ColorRect ScDuraBar;
    [Export] public Label ScDuraText;
    [Export] public Label ScName;
    [Export] public Label ScDuration;
    public Skill sc;
    public void Init(Skill si)
    {
        sc = si;
        ScName.Text = $"{si.User.Name} {si.Name}";
        if (si.User != Player.PlayerUnit)
            ScDuraBar.Color = Colors.DodgerBlue;
    }
    public void Refresh()
    {
        ScDuraBar.Size = new Vector2(((SpellCard)sc).CurrentDurability / ((SpellCard)sc).GetMaxDurability() * 8000, 200);
        ScDuraText.Text = $"{((SpellCard)sc).CurrentDurability:F0}/{((SpellCard)sc).GetMaxDurability():F0}";
        ScDuration.Text = ((int)((sc.Duration - sc.TimeElapsed) / 100)).ToString();
    }
}
