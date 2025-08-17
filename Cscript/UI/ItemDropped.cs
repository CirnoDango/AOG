using Godot;

public partial class ItemDropped : Node2D
{
    public Item RepresentedItem;

    public void SetItem(Item item)
    {
        RepresentedItem = item;
        GetNode<TextureRect>("TextureRect").Texture = item.Texture;
    }
}

