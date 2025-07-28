using Godot;
using System;

public partial class MemoryBox : Control,IRegisterToG
{
    [Export]
    public VBoxContainer EquipList;
    [Export]
    public VBoxContainer BagList;
    [Export]
    public Label equipnumber;
    [Export]
    public Label bagnumber;

    public PackedScene ItemEntryScene;

    public TextureButton EquipButton;
    public TextureRect EquipLight;
    public TextureButton ThrowButton;
    public TextureRect ThrowLight;
    public bool EquipState = true;
    public override void _Ready()
    {
        // 加载预制体
        ItemEntryScene = GD.Load<PackedScene>("res://assets/Item/ItemEntry.tscn");
        EquipButton = GetNode<TextureButton>("EquipButton");
        EquipLight = GetNode<TextureRect>("EquipButton/Light");
        ThrowButton = GetNode<TextureButton>("ThrowButton");
        ThrowLight = GetNode<TextureRect>("ThrowButton/Light");
        // 初始状态
        UpdateButtonStates();

        // 绑定悬停和点击事件
        EquipButton.MouseEntered += () => { if (!EquipState) EquipLight.Modulate = Colors.Gray; };
        EquipButton.MouseExited += () => { if (!EquipState) EquipLight.Modulate = Colors.White; };
        EquipButton.Pressed += () => {
            EquipState = true;
            UpdateButtonStates();
        };

        ThrowButton.MouseEntered += () => { if (EquipState) ThrowLight.Modulate = Colors.Gray; };
        ThrowButton.MouseExited += () => { if (EquipState) ThrowLight.Modulate = Colors.White; };
        ThrowButton.Pressed += () => {
            EquipState = false;
            UpdateButtonStates();
        };
    }
    private void UpdateButtonStates()
    {
        if (EquipState)
        {
            EquipLight.Modulate = Colors.LightYellow; // 常亮天蓝光
            ThrowLight.Modulate = Colors.White;   // 关闭 Throw 光
        }
        else
        {
            EquipLight.Modulate = Colors.White;   // 关闭 Equip 光
            ThrowLight.Modulate = Colors.LightYellow;  // 常亮天蓝光
        }
    }

    public void Refresh(Unit unit)
    {
        equipnumber.Text = $"Memory: {unit.Memorys.CurrentEquipWeight:F1}/{unit.Memorys.MaxEquipWeight:F1}";
        bagnumber.Text = $"Bag: {unit.inventory.CurrentWeight:F1}/{unit.inventory.MaxWeight:F1}";
        foreach (var child in EquipList.GetChildren())
            child.QueueFree();

        foreach (var child in BagList.GetChildren())
            child.QueueFree();


        foreach (var item in unit.Memorys.EquippedItems)
        {
            var entry = ItemEntryScene.Instantiate<ItemEntry>();
            entry.Setup(item, (clickedItem) => { });
            EquipList.AddChild(entry);
        }

        foreach (var item in unit.inventory.Items)
        {
            if (item.Template is not Memory)
                continue;
            var entry = ItemEntryScene.Instantiate<ItemEntry>();
            entry.Setup(item, (clickedItem) => {
                if (item.CanEquip)
                {
                    if (EquipState)
                    {
                        if (item.CanEquip)
                        {
                            if (unit.Memorys.TryEquip(item, unit))
                            {
                                unit.inventory.RemoveItem(clickedItem);
                                Refresh(unit);
                            }
                        }
                    }
                    else
                    {
                        unit.inventory.ThrowItem(clickedItem);
                    }
                }
            });
            BagList.AddChild(entry);
        }
    }

    public void RegisterToG(G g)
    {
        g.MemoryBox = this;
    }
}

