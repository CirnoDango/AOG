using Godot;
using System;

public partial class LayerItemDropped : Node2D, IRegisterToG
{
    [Export]
    public PackedScene droppedScene;
    public static void SummonItem(Vector2I position, ItemInstance item)
    {
        var instance = G.I.LayerItemDropped.droppedScene.Instantiate<ItemDropped>();
        instance.Position = position * 16;
        item.Sprite = instance; // 关联 Sprite
        item.Position = position; // 设置位置
        instance.SetItem(item); // 设置 Sprite + 数据
        G.I.LayerItemDropped.AddChild(instance);
        Scene.CurrentMap.GetGrid(position).InteractableObjects.Add(item);
    }
    public static void DeleteItem(ItemInstance item)
    {
        Scene.CurrentMap.GetGrid(item.Position).InteractableObjects.Remove(item);
        item.Position = Vector2I.Left;
        item.Sprite.QueueFree(); // 删除 Sprite
    }

    public void RegisterToG(G g)
    {
        g.LayerItemDropped = this;
    }
}
