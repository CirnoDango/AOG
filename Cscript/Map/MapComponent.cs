using System.Collections.Generic;

public class Chest : IInteractable
{
    public List<ItemInstance> items = [];
    public Grid position;
    public bool open = false;
    public Chest(Grid grid)
    {
        MapBuilder.SetLogicMapTerrain(LogicMapLayer.Stand, grid, "ChestClosed");
        position = grid;
        position.InteractableObjects.Add(this);
    }
    public void Interact(Unit unit)
    {
        if (open)
        {
            Info.Print("宝箱里空空如也");
        }
        else
        {
            foreach (ItemInstance item in items)
            {
                ItemInstance.SummonItem(position.Position, item);
            }
            MapBuilder.SetLogicMapTerrain(LogicMapLayer.Stand, position, "ChestOpen");
            position.InteractableObjects.Remove(this);
            items.Clear();
            Info.Print("宝箱掉落了一些物品!");
        }
    }
}
