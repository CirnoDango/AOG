using Godot;

public partial class ItemDropped : Node2D
{
    public ItemInstance RepresentedItem;

    public void SetItem(ItemInstance item)
    {
        RepresentedItem = item;
        GetNode<TextureRect>("TextureRect").Texture = item.Texture;
    }
}

