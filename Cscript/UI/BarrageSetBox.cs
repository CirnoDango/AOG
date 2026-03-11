using Godot;
using System.Collections.Generic;

public partial class BarrageSetBox : VBoxContainer
{
    public Barrage barrage;
    [Export]
    public FlowContainer FBox;
    [Export]
    public TextureRect Image;
    [Export]
    public Label L;
    public Dictionary<TextureButton, BarrageComponent> Buttons = [];

    public void Init(BarrageSet bs)
    {
        Image.Texture = bs.Texture;
        L.Text = $"SpCost:{bs.Skill.GetSpCost()} ; CD: {bs.Skill.GetCooldown() / 100}";
        Init(bs.B);
    }
    public void Init(Barrage b)
    {
        barrage = b;
        for (int i = 0; i < b.MaxComponents; i++)
        {
            var item = b.Components[i];
            var entry = G.I.BarrageBox.BcEntryScene.Instantiate<TextureButton>();
            entry.MouseEntered += () => { if (item != null) G.I.BarrageBox.info.Text = item.GetDescription(); };
            entry.MouseExited += () => G.I.BarrageBox.info.Text = "";
            entry.Pressed += () => G.I.BarrageBox.OnSlotClicked(this, entry);
            if (item != null)
            {
                var icon = new TextureRect()
                {
                    StretchMode = TextureRect.StretchModeEnum.Scale,
                    Size = new Vector2(600, 600),
                };
                icon.Texture = item.Texture; 
                entry.AddChild(icon);
            }
            FBox.AddChild(entry);
            Buttons.Add(entry, item ?? null);
        }
    }
}
