using Godot;
using System;

public partial class ItemEntry : Control
{
    [Export]
    private TextureRect Image;
    [Export]
    private TextureRect Light;
    [Export]
    private Label Label;
    [Export]
    private Control Info;
    [Export]
    private Label InfoLabel;
    public Item RepresentedItem;
    public Action<Item> OnClick; // 外部绑定处理逻辑
    private Color _originalModulate = Colors.White; // 初始颜色

    public override void _Ready()
    {
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }

    public void Setup(Item item, Action<Item> clickCallback)
    {
        RepresentedItem = item;
        OnClick = clickCallback;

        // 设置图标、文字
        Image.Texture = item.Texture;
        Label.Text = TextEx.TrN($"[{item.Weight:F1}] {item.TrName}");
        switch (item.effectLevel)
        {
            case 1:
                Label.Text += "+";
                Label.AddThemeColorOverride("font_color", Colors.LimeGreen);
                break;
            case 2:
                Label.Text += "++";
                Label.AddThemeColorOverride("font_color", Colors.Blue);
                break;
        }
        InfoLabel.Text = TextEx.TrN($"{item.GetDescription()}");
        Info.Hide();
        Light.Hide();
        // 点击交互
        GuiInput += (InputEvent @event) => {
            if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
                OnClick?.Invoke(item);
        };
    }

    private void OnMouseEntered()
    {
        Light.Modulate = new Color(0.6f, 0.6f, 1);
        Info.Show();
        Light.Show();
    }

    private void OnMouseExited()
    {
        Info.Hide();
        Light.Hide();
    }
}


