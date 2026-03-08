using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;

public partial class Startmenu : Control
{
    [Export] public Button start;
    [Export] public Button load;
    [Export] public Button quit;
    [Export] public Label noSaveNote;
    [Export] public VBoxContainer saveNote;
    [Export] public TextureRect player;
    [Export] public Label info;
    public override void _Ready()
    {
        
        start.Text = Tr("smStart");
        load.Text = Tr("smLoad");
        quit.Text = Tr("smQuit");
        start.Pressed += OnStartPressed;
        load.Pressed += OnLoadPressed;
        quit.Pressed += OnExitPressed;
        Visible = true;
        string path = "sav.sav";
        if (!File.Exists(path))
        {
            noSaveNote.Visible = true;
            load.Disabled = true;
        }


        else
        {
            string json = File.ReadAllText(path);

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                Converters = { new StringEnumConverter() }
            };
            Dictionary<string, int> stageNumbers = new()
            {
                {"Eosd", 9}
            };
            Save save = JsonConvert.DeserializeObject<Save>(json, settings);
            player.Texture = GD.Load<Texture2D>($"res://Assets/Sprites/{save.PlayerData.Name}.png");
            info.Text = $"{save.SceneName},{save.StageNumber + 1}/{stageNumbers[save.SceneName]}";
            saveNote.Visible = true;
        }
    }
    private void OnStartPressed()
    {
        Visible = false;
    }
    private void OnLoadPressed()
    {
        if (!File.Exists("sav.sav"))
            throw new FileNotFoundException($"存档文件不存在: {"sav.sav"}");
        GameData.LoadPath = "sav.sav";
        Save save = Save.LoadSaveFile("sav.sav");
        GameData.SelectedStage = save.SceneName;
        G.I.Mainmenu.Hide();
        foreach (Node child in G.I.Mainmenu.GameRoot.GetChildren())
            child.QueueFree();
        var path = $"res://Nodes/Scene/{GameData.SelectedStage}.tscn";
        var packedScene = GD.Load<PackedScene>(path);
        var instance = packedScene.Instantiate();
        G.I.Mainmenu.GameRoot.AddChild(instance);
        Visible = false;
    }
    private void OnExitPressed()
    {
        GetTree().Quit();
    }
}
