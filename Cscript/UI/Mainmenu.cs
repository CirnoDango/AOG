using Godot;
using System;

public partial class Mainmenu : Control
{
    [Export]
    public Node GameRoot;
    private string selectedStage = "tutorial";  // 默认选中教程关
    private string selectedCharacter
    {
        get => GameData.SelectedCharacter;
        set
        {
            GameData.SelectedCharacter = value;
            label.Text = "选中自机：" + value; // 更新标签显示
        }
    }
    [Export]
    public Label label;

    public override void _Ready()
    {
        //Visible = true;
        GetNode<Button>("tutorial").Pressed += () => { selectedStage = "tutorial"; StartGame(); };
        GetNode<Button>("cave").Pressed += () => {selectedStage = "cave"; StartGame(); };
        GetNode<TextureButton>("cirno").Pressed += () => selectedCharacter = "cirno";
        GetNode<TextureButton>("marisa").Pressed += () => selectedCharacter = "marisa";
        GetNode<TextureButton>("rumia").Pressed += () => selectedCharacter = "rumia";
        selectedCharacter = "cirno";
    }

    private void OnCharacterSelected(long index)
    {
        selectedCharacter = GetNode<OptionButton>("CharacterSelect").GetItemText((int)index);
    }

    private void StartGame()
    {
        // 存储选项（可选）
        GameData.SelectedCharacter = selectedCharacter;
        GameData.SelectedStage = selectedStage;

        // 切换场景
        Hide();

        foreach (Node child in GameRoot.GetChildren())
            child.QueueFree();

        var path = $"res://nodes/{selectedStage}.tscn";
        var packedScene = GD.Load<PackedScene>(path);
        var instance = packedScene.Instantiate();
        GameRoot.AddChild(instance);
    }
}


