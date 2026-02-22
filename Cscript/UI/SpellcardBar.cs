using Godot;
using System;

public partial class SpellcardBar : Control
{
    [Export] public ColorRect ScDuraBar;
    [Export] public Label ScDuraText;
    [Export] public Label ScName;
    [Export] public Label ScDuration;
    public SkillInstance sc;
    public void Init(SkillInstance si)
    {
        sc = si;
        ScName.Text = $"{si.User.Name} {si.Name}";
        if (si.User != Player.PlayerUnit)
            ScDuraBar.Color = Colors.DodgerBlue;
    }
    public void Refresh()
    {
        ScDuraBar.Size = new Vector2(((SpellCard)sc.Template).CurrentDurability / ((SpellCard)sc.Template).GetMaxDurability(sc.Level) * 8000, 200);
        ScDuraText.Text = $"{((SpellCard)sc.Template).CurrentDurability:F0}/{((SpellCard)sc.Template).GetMaxDurability(sc.Level):F0}";
        ScDuration.Text = ((int)((sc.Duration - sc.TimeElapsed) / 100)).ToString();
    }
}
