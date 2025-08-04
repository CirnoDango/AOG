using Godot;
using System.Collections.Generic;

public partial class BarrageSetBox : VBoxContainer
{
    public Barrage barrage;
    [Export]
    public FlowContainer FBox;
    public Dictionary<TextureButton, BarrageComponent> Buttons = [];

    public void Init(Barrage b)
    {
        barrage = b;
        for (int i = 0; i < b.MaxComponents; i++)
        {
            var item = b.Components[i];
            var entry = G.I.BarrageBox.BcEntryScene.Instantiate<TextureButton>();
            entry.Pressed += () => G.I.BarrageBox.OnSlotClicked(this, entry);
            if(item != null)
            {
                var icon = new TextureRect()
                {
                    StretchMode = TextureRect.StretchModeEnum.Scale,
                    Size = new Vector2(600, 600),
                };
                if (item is BulletModule bm)
                    icon.Texture = Bullet.GetImage(bm.bulletContext.Color, bm.bulletContext.Shape).Texture;
                else
                    icon.Texture = item.Texture;
                    entry.AddChild(icon);
            }
            FBox.AddChild(entry);
            Buttons.Add(entry, item ?? null);
        }
    }
}
