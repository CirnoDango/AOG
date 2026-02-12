using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

public partial class BarrageBox : Control, IRegisterToG
{
    [Export]
    public PackedScene barrageSetBox;
    [Export]
    public VBoxContainer BarrageList;
    [Export]
    public FlowContainer BagList;
    [Export]
    public Label equipNumber;
    [Export]
    public Label bagNumber;
    [Export]
    public PackedScene BcEntryScene;
    public TextureButton EquipButton;
    public TextureRect EquipLight;
    public TextureButton ThrowButton;
    public TextureRect ThrowLight;
    public bool EquipState = true;
    public Barrage bag;

    private BarrageComponent draggedItem = null;
    private TextureButton selectButton = null;
    private BarrageSetBox draggedParent = null;
    public override void _Ready()
    {
        // 加载预制体
        EquipButton = GetNode<TextureButton>("EquipButton");
        EquipLight = GetNode<TextureRect>("EquipButton/Light");
        ThrowButton = GetNode<TextureButton>("ThrowButton");
        ThrowLight = GetNode<TextureRect>("ThrowButton/Light");
        // 初始状态
        UpdateButtonStates();

        // 绑定悬停和点击事件
        EquipButton.MouseEntered += () => { if (!EquipState) EquipLight.Modulate = Colors.Gray; };
        EquipButton.MouseExited += () => { if (!EquipState) EquipLight.Modulate = Colors.Transparent; };
        EquipButton.Pressed += () => {
            if (draggedItem == null)
                EquipState = true; UpdateButtonStates();
        };

        ThrowButton.MouseEntered += () => { if (EquipState) ThrowLight.Modulate = Colors.Gray; };
        ThrowButton.MouseExited += () => { if (EquipState) ThrowLight.Modulate = Colors.Transparent; };
        ThrowButton.Pressed += () => {
            if (draggedItem == null)
                EquipState = false; UpdateButtonStates();
        };
    }
    public void OnSlotClicked(BarrageSetBox parent, TextureButton slotButton)
    {
        
        // 获取按钮下的图标节点
        TextureRect itemIcon = null;
        if (slotButton.GetChildCount() > 0)
            itemIcon = (TextureRect)slotButton.GetChild(0);
        // 丢弃
        if (!EquipState && itemIcon != null)
        {
            Item toThrow = parent.Buttons[slotButton];
            if (bag.Components.Contains(toThrow))
            {
                Player.PlayerUnit.Inventory.ThrowItem(toThrow);
                slotButton.RemoveChild(itemIcon);
                slotButton.TooltipText = null;
            }
            return;
        }
        if (draggedItem == null && itemIcon != null)
        {
            // 1. 开始拖动：从原位置取出图标，放到顶层
            selectButton = slotButton;
            draggedItem = parent.Buttons[slotButton];
            parent.Buttons[slotButton] = null;
            int index = parent.Buttons
                .Select((pair, idx) => new { pair.Key, Index = idx })
                .FirstOrDefault(p => p.Key == slotButton)?.Index ?? -1;
            parent.barrage.Components[index] = null;
            draggedParent = parent;
            slotButton.Modulate = Colors.Red;
        }
        else if (draggedItem != null && itemIcon == null)
        {
            // 2. 放下物品：移回目标按钮的图标槽里
            ReturnItem(parent, slotButton);
        }
        else if (draggedItem != null && itemIcon != null)
        {
            if (selectButton != slotButton)
            {
                ExchangeItem(parent, slotButton);
            }
            else
            {
                ReturnItem(draggedParent, selectButton);
            }
        }
    }
    public void ExchangeItem(BarrageSetBox parent, TextureButton slotButton)
    {
        // 3. 拖动物品与另一个物品交换
        // 获取目标物品
        var targetItem = parent.Buttons[slotButton];

        // 从目标槽中取出图标
        TextureRect targetIcon = (TextureRect)slotButton.GetChild(0);
        slotButton.RemoveChild(targetIcon);

        // 交换数据
        parent.Buttons[slotButton] = draggedItem;
        draggedParent.Buttons[selectButton] = targetItem;

        // 更新 barrage.Components
        int targetIndex = parent.Buttons
            .Select((pair, idx) => new { pair.Key, Index = idx })
            .FirstOrDefault(p => p.Key == slotButton)?.Index ?? -1;
        int sourceIndex = draggedParent.Buttons
            .Select((pair, idx) => new { pair.Key, Index = idx })
            .FirstOrDefault(p => p.Key == selectButton)?.Index ?? -1;

        if (targetIndex != -1)
            parent.barrage.Components[targetIndex] = draggedItem;
        if (sourceIndex != -1)
            draggedParent.barrage.Components[sourceIndex] = targetItem;

        // 图像处理：交换图标
        TextureRect draggedIcon = (TextureRect)selectButton.GetChild(0);
        selectButton.RemoveChild(draggedIcon);

        slotButton.AddChild(draggedIcon);
        slotButton.TooltipText = draggedItem.GetDescription();
        draggedIcon.Position = Vector2.Zero;
        draggedIcon.ZIndex = 0;

        selectButton.AddChild(targetIcon);
        selectButton.TooltipText = targetItem.GetDescription();
        targetIcon.Position = Vector2.Zero;
        targetIcon.ZIndex = 0;

        // 颜色恢复
        selectButton.Modulate = Colors.White;

        // 清除拖拽引用
        draggedItem = null;
        selectButton = null;

        // 刷新tag
        BarrageSet bs = (BarrageSet)Player.PlayerUnit.Equipment.EquippedItems.FirstOrDefault(x => x is BarrageSet bs && bs.B == parent.barrage);
        if (bs != null)
            parent.L.Text = $"SpCost:{bs.Skill.GetSpCost(1)}; CD:{bs.Skill.GetCooldown(1) / 100}";
        BarrageSet bsd = (BarrageSet)Player.PlayerUnit.Equipment.EquippedItems.FirstOrDefault(x => x is BarrageSet bs && bs.B == draggedParent.barrage);
        if (bsd != null)
            draggedParent.L.Text = $"SpCost:{bsd.Skill.GetSpCost(1)}; CD:{bsd.Skill.GetCooldown(1) / 100}";
    }
    public void ReturnItem(BarrageSetBox parent, TextureButton slotButton)
    {
        // 背包交互
        if (parent.barrage == bag)
            Player.PlayerUnit.Inventory.Items.Add(draggedItem);
        if (draggedParent.barrage == bag)
        {
            var toremove = Player.PlayerUnit.Inventory.Items.FirstOrDefault(x => x == draggedItem);
            Player.PlayerUnit.Inventory.RemoveItem(toremove);
        }
        // 设置新位置
        parent.Buttons[slotButton] = draggedItem;
        var newComponent = draggedItem;
        int index = parent.Buttons
            .Select((pair, idx) => new { pair.Key, Index = idx })
            .FirstOrDefault(p => p.Key == slotButton)?.Index ?? -1;

        if (index != -1)
            parent.barrage.Components[index] = newComponent;

        // 图像处理
        selectButton.Modulate = Colors.White;
        TextureRect moveIcon = (TextureRect)selectButton.GetChild(0);
        selectButton.RemoveChild(moveIcon);
        selectButton.TooltipText = null;
        slotButton.AddChild(moveIcon);
        slotButton.TooltipText = draggedItem.GetDescription();
        moveIcon.Position = Vector2.Zero;
        moveIcon.ZIndex = 0;

        // 清除引用
        draggedItem = null;
        selectButton = null;
        // 刷新tag
        BarrageSet bs = (BarrageSet)Player.PlayerUnit.Equipment.EquippedItems.FirstOrDefault(x => x is BarrageSet bs && bs.B == parent.barrage);
        if (bs != null)
            parent.L.Text = $"SpCost:{bs.Skill.GetSpCost(1)}; CD:{bs.Skill.GetCooldown(1) / 100}";
        BarrageSet bsd = (BarrageSet)Player.PlayerUnit.Equipment.EquippedItems.FirstOrDefault(x => x is BarrageSet bs && bs.B == draggedParent.barrage);
        if (bsd != null)
            draggedParent.L.Text = $"SpCost:{bsd.Skill.GetSpCost(1)}; CD:{bsd.Skill.GetCooldown(1) / 100}";
    }


    public void ReturnItem()
    {
        if (selectButton != null)
            ReturnItem(draggedParent, selectButton);
    }
    public void Refresh()
    {
        var unit = Player.PlayerUnit;
        equipNumber.Text = $"Equip: {unit.Equipment.CurrentEquipWeight:F1}/{unit.Equipment.MaxEquipWeight:F1}";
        bagNumber.Text = $"Bag: {unit.Inventory.CurrentWeight:F1}/{unit.Inventory.MaxWeight:F1}";
        foreach (var child in BagList.GetChildren())
            child.QueueFree();
        foreach (var child in BarrageList.GetChildren())
            child.QueueFree();
        int n = 0;
        bag = new(100);
        foreach (var item in unit.Inventory.Items)
        {
            if (item is not BarrageComponent)
                continue;
            bag.Components[n] = (BarrageComponent)item;
            n++;
        }
        foreach(var item in unit.Equipment.EquippedItems)
        {
            if (item is BarrageSet bs)
            {
                var entry = barrageSetBox.Instantiate<BarrageSetBox>();
                entry.Init(bs);
                BarrageList.AddChild(entry);
            }
        }
        var bagbsb = barrageSetBox.Instantiate<BarrageSetBox>();
        bagbsb.Init(bag);
        BagList.AddChild(bagbsb);
    }
    private void UpdateButtonStates()
    {
        if (EquipState)
        {
            EquipLight.Modulate = Colors.LightYellow;
            ThrowLight.Modulate = Colors.Transparent;
        }
        else
        {
            EquipLight.Modulate = Colors.Transparent;
            ThrowLight.Modulate = Colors.LightYellow;
        }
    }

    public void RegisterToG(G g)
    {
        g.BarrageBox = this;
    }
}
