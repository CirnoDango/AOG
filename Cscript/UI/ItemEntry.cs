using Godot;
using System;

public partial class ItemEntry : HBoxContainer
{
    public ItemInstance RepresentedItem;
    public Action<ItemInstance> OnClick; // 外部绑定处理逻辑
    private Color _originalModulate = Colors.White; // 初始颜色

    public override void _Ready()
    {
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }

    public void Setup(ItemInstance item, Action<ItemInstance> clickCallback)
    {
        RepresentedItem = item;
        OnClick = clickCallback;

        // 设置图标、文字
        GetNode<TextureRect>("TextureRect").Texture = item.Texture;
        GetNode<Label>("Label").Text = TextEx.TrN($"[{item.Weight:F1}] {item.TrName} : {item.Description}");
        GetNode<TextureRect>("TextureRect/Light").Hide();
        // 点击交互
        this.GuiInput += (InputEvent @event) => {
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


