using Godot;
using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

public partial class Mainmenu : Control, IRegisterToG
{
	[Export] public OptionButton OptionPlayer;
	[Export] public OptionButton OptionScene;
	[Export] public TextureRect Image;
	[Export] public Button start;
	[Export] public Label Info;
	[Export] public Node GameRoot;
	[Export] private Label choosePlayer;
	[Export] private Label chooseScene;
	private string selectedScene;
	private string selectedPlayer;

	public void RegisterToG(G g)
	{
		g.Mainmenu = this;
	}

	public override void _Ready()
	{
		TranslationServer.SetLocale("en");
		choosePlayer.Text = Tr("smChoosePlayer");
		chooseScene.Text = Tr("smChooseScene");
		start.Text = Tr("smStart");
		// 内部资源加载
		if (!Game.IsLoaded)
		{
			EnemyLoader.LoadEnemies();
			Skill.LoadSkillDeck();
			ItemLoader.LoadAllItems();
			ItemEffect.LoadAllItemEffects();
			SpriteManager.LoadSkills();
			Game.IsLoaded = true;
		}
		// 给 OptionButton 绑定信号
		OptionPlayer.ItemSelected += OnPlayerSelected;
		OptionScene.ItemSelected += OnSceneSelected;

		OnPlayerSelected(3);
		OnSceneSelected(0);

		// 开始按钮
		Visible = true;
		start.Pressed += StartGame;
		OnSceneSelected(OptionScene.Selected);
	}

	private void OnPlayerSelected(long index)
	{
		selectedPlayer = OptionPlayer.GetItemText((int)index);
		Image.Texture = GD.Load<Texture2D>($"res://Assets/Sprites/{selectedPlayer}.png");
		var ed = EnemyLoader.enemyDatas.Where(x => x.Name == selectedPlayer).FirstOrDefault();
		Info.Text = TextEx.Tr($"u{ed.Name} \nHP:{ed.HP}  SP:{ed.SP}  MP:{ed.MP}\n" +
			$"{Tr("力量")}:{ed.Str} {Tr("敏捷")}:{ed.Dex} {Tr("体质")}:{ed.Con}\n" +
			$"{Tr("灵力")}:{ed.Spi} {Tr("魔力")}:{ed.Mag} {Tr("灵巧")}:{ed.Cun}\n" +
			$"{Tr("smInitsg")}: sg{ed.SkillGroups.FirstOrDefault()}");
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
			case "红魔馆":
				selectedScene = "Eosd";
				break;
			case "测试场景":
				selectedScene = "CaveD";
				break;
			default:
				selectedScene = s;
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
