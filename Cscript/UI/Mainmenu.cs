using Godot;
using System;
using System.Xml.Linq;

public partial class Mainmenu : Control
{
    [Export] public OptionButton OptionPlayer;
    [Export] public OptionButton OptionScene;
    [Export] public TextureRect Image;
    [Export] public Button start;
    [Export] public Label Info;
    [Export] public Node GameRoot;

    private string selectedScene;
    private string selectedPlayer;

    public override void _Ready()
    {
        // 内部资源加载
        if (!Game.IsLoaded)
        {
            EnemyLoader.LoadEnemies();
            Skill.LoadSkillDeck();
            ItemLoader.LoadAllItems();
            SpriteManager.LoadSkills();
            Game.IsLoaded = true;
        }
        // 给 OptionButton 绑定信号
        OptionPlayer.ItemSelected += OnPlayerSelected;
        OptionScene.ItemSelected += OnSceneSelected;

        OnPlayerSelected(0);
        OnSceneSelected(0);

        // 开始按钮
        start.Pressed += StartGame;
    }

    private void OnPlayerSelected(long index)
    {
        selectedPlayer = OptionPlayer.GetItemText((int)index);
        Image.Texture = GD.Load<Texture2D>($"res://Assets/Sprites/{selectedPlayer}.png");
    }

    private void OnSceneSelected(long index)
    {
        var s = OptionScene.GetItemText((int)index);
        switch (s)
        {
            case "教程(1)":
                OnPlayerSelected(3);
                OptionPlayer.Selected = 3;
                selectedScene = "tutorial";
                break;
            case "教程(2)":
                OnPlayerSelected(3);
                OptionPlayer.Selected = 3;
                selectedScene = "tutorial2";
                break;
            case "雾之湖":
                selectedScene = "mistylake";
                break;
        }
    }

    private void StartGame()
    {
        // 存储选项
        GameData.SelectedCharacter = selectedPlayer;
        GameData.SelectedStage = selectedScene;

        // 切换场景
        Hide();

        foreach (Node child in GameRoot.GetChildren())
            child.QueueFree();

        var path = $"res://Nodes/Scene/{selectedScene}.tscn";
        var packedScene = GD.Load<PackedScene>(path);
        var instance = packedScene.Instantiate();
        GameRoot.AddChild(instance);
    }
}


