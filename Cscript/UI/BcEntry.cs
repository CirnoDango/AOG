using Godot;
using System;

public partial class BcEntry : FlowContainer
{
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
        GetNode<TextureRect>("TextureRect").Texture = item.Texture;
        GetNode<Label>("Label").Text = TextEx.TrN($"[{item.Weight:F1}] {item.TrName} : {item.GetDescription()}");
        GetNode<TextureRect>("TextureRect/Light").Hide();
        // 点击交互
        GuiInput += @event =>
        {
            if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
                OnClick?.Invoke(item);
        };
    }

    private void OnMouseEntered()
    {
        GetNode<TextureRect>("TextureRect/Light").Modulate = new Color(0.6f, 0.6f, 1);
        GetNode<TextureRect>("TextureRect/Light").Show();
    }

    private void OnMouseExited()
    {
        GetNode<TextureRect>("TextureRect/Light").Hide();
    }
}


