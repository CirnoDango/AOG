using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Xml.Linq;
public class Save
{
    public string SceneName { get; set; }
    public int StageNumber { get; set; }
    public EnemyData PlayerData { get; set; }
    public float MaxMemory { get; set; }
    public int UaPoint { get; set; }
    public int SkillPoint { get; set; }
    public int TalentPoint { get; set; }
    public DamageTypeSet PlayerDamageTypeSet { get; set; }
    public List<int> skilltreehexXs = [];
    public List<int> skilltreehexYs = [];
    public void CreateSaveFile(string sceneName, int stageNumber, string path)
    {
        SceneName = sceneName;
        StageNumber = stageNumber;
        var player = Player.PlayerUnit;
        // 1.背包数据
        List<Dictionary<string, object>> inventory = [];
        List<Dictionary<string, object>> equipment = [];
        Dictionary<string, float> memory = [];
        Dictionary<string, float> Skills = [];
        foreach (Item i in player.Inventory.Items.ToList())
        {
            Dictionary<string, object> itemValue = [];
            itemValue.Add("Amount", 1);
            itemValue.Add("Name", i.Name);
            i.GetParam();
            itemValue.Add("Parameters", i.Params);
            //player.Inventory.RemoveItem(i);
            inventory.Add(itemValue);
        }
        
        // 2.装备数据
        player.Inventory.MaxWeight *= 2;
        foreach(Item i in player.Equipment.EquippedItems.ToList())
        {
            Dictionary<string, object> itemValue = [];
            itemValue.Add("Amount", 1);
            itemValue.Add("Name", i.Name);
            i.GetParam();
            itemValue.Add("Parameters", i.Params);
            equipment.Add(itemValue);
            //player.Equipment.Unequip(i, player);
        }
        // 2a.记忆数据
        foreach (Item i in player.Memorys.EquippedItems.ToList())
        {
            if(memory.TryGetValue(i.Name, out float n))
                memory[i.Name]++;
            else
            {
                memory.Add(i.Name, 1);
            }
        }
        MaxMemory = player.Memorys.MaxEquipWeight;
        // 3.技能数据
        foreach (var s in player.Status.ToList())
        {
            s.OnQuit(player);
        }
        foreach(var si in player.Us.skills.ToList())
        {
            if(si.skill.SkillGroup == "" ||
               si.skill.SkillGroup == "Item")
                continue;
            Skills.Add(si.skill.Name, si.skill.Level);
            //si.skill.OffLearn(player);
        }
        // 4.技能树格子
        var hexs = G.I.SkillTreeBox.hexs
            .Where(x=>x.hexStatus == HexStatus.expert).ToList();
        foreach (var hex in hexs)
        {
            skilltreehexXs.Add(hex.x);
            skilltreehexYs.Add(hex.y);
        }
        // 5.伤害属性表
        PlayerDamageTypeSet = player.Ua.DamageTypeSet;
        // 5a.点数
        UaPoint = player.Ua.uaPoint;
        SkillPoint = player.Ua.skillPoint;
        TalentPoint = G.I.Player.TalentPoint;
        // 6.能力属性
        PlayerData = new EnemyData()
        {
            Name = player.Name,
            Tags = player.symbol,
            HP = player.Ua.MaxHp,
            MP = player.Ua.MaxMp,
            SP = player.Ua.MaxSp,
            Str = player.Ua.Str + 10,
            Dex = player.Ua.Dex + 10,
            Con = player.Ua.Con + 10,
            Spi = player.Ua.Spi + 10,
            Mag = player.Ua.Mag + 10,
            Cun = player.Ua.Cun + 10,
            Skills = Skills,
            SkillGroups = Player.skillTrees.Keys.ToList(),
            Inventory = inventory,
            Equipment = equipment,
            Memory = memory
        };
        // 7.保存json文件
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            Converters = { new StringEnumConverter() }
        };

        string json = JsonConvert.SerializeObject(this, settings);

        File.WriteAllText(path, json);
    }
    public static Save LoadSaveFile(string path)
    {
        // 1.读取json文件
        if (!File.Exists(path))
            throw new FileNotFoundException($"存档文件不存在: {path}");

        try
        {
            string json = File.ReadAllText(path);

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                Converters = { new StringEnumConverter() }
            };

            Save save = JsonConvert.DeserializeObject<Save>(json, settings);

            return save;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"读取存档失败: {ex.Message}");
            return null;
        }
    }
    /// <summary>
    /// 读档时替换Player.Init
    /// </summary>
    /// <param name="save"></param>
    public static void LoadPlayerInit(Save save, float zoom)
    {
        // 1.能力属性
        EnemyData ed = save.PlayerData;
        Unit unit = new()
        {
            Name = ed.Name,
            symbol = ed.Tags,
            MemoryValue = ed.Value,
        };
        Player.PlayerUnit = unit;
        unit.Equipment = new Equipment(unit);
        unit.Memorys = new MemoryBag(unit);
        unit.Inventory = new Inventory(unit);
        unit.Ua.UnitAtt(ed.Str, ed.Dex, ed.Con, ed.Spi, ed.Mag, ed.Cun);
        unit.Ua.MaxHp = ed.HP;
        unit.Ua.MaxMp = ed.MP;
        unit.Ua.MaxSp = ed.SP;
        unit.Ua.CurrentHp = unit.Ua.MaxHp;
        unit.Ua.CurrentMp = unit.Ua.MaxMp;
        unit.Ua.CurrentSp = unit.Ua.MaxSp / 2;
        // 3.伤害属性表
        unit.Ua.DamageTypeSet = save.PlayerDamageTypeSet;
        // 4.技能树格子
        for(int i = 0; i < save.skilltreehexXs.Count; i++)
        {
            G.I.SkillTreeBox.Expert(save.skilltreehexXs[i], save.skilltreehexYs[i]);
        }
        // 6.物品
        foreach (var dict in ed.Inventory)
        {
            float n = Convert.ToSingle(dict["Amount"]);
            Item i;
            if (dict.TryGetValue("Parameters", out object value))
                i = Item.CreateItem((string)dict["Name"], (Dictionary<string, object>)value);
            else
                i = Item.CreateItem((string)dict["Name"]);
            unit.Inventory.AddItem(i);
        }

        foreach (var dict in ed.Equipment)
        {
            float n = Convert.ToSingle(dict["Amount"]);
            Item i;
            if (dict.TryGetValue("Parameters", out object value))
                i = Item.CreateItem((string)dict["Name"], (Dictionary<string, object>)value);
            else
                i = Item.CreateItem((string)dict["Name"]);
            unit.Equipment.TryEquip(i, unit);
        }
        foreach (var kvp in ed.Memory)
        {
            int n = (int)(kvp.Value + 0.01f);
            do
            {
                Item i = Item.CreateItem(kvp.Key);
                unit.Memorys.EquippedItems.Add(i);
                n--;
            } while (n > 0);
            
        }
        // x.Init处理
        foreach (var sg in save.PlayerData.SkillGroups)
        {
            G.I.SkillTreeBox.Expert(sg);
        }
        SpriteManager.LoadEnemy(unit);
        Root.rootnode.AddChild(unit.Up.sprite);
        unit.Up.sprite.Position = Setting.imagePx * unit.Up.Position;
        unit.Ua.InitializeHpSpBar();
        unit.Ue.OnUnitUpdate += (unit, updateTime) => unit.Ua.HealHp(updateTime * unit.Ua.MaxHp / 10000);
        unit.Ue.OnUnitUpdate += (unit, updateTime) => unit.Ua.GetMp(updateTime * unit.Ua.Mag / 1000);
        unit.TimeEnergy = 0;
        unit.UnitAi = null;
        zoom /= (Setting.imagePx / 16);
        Player.playerCamera = new Camera2D
        {
            Position = Vector2.Zero,
            Zoom = new Vector2(zoom, zoom)
        };
        unit.Up.sprite.AddChild(Player.playerCamera);
        Player.playerCamera.MakeCurrent();
        Player.playerCamera.Position = new Vector2I(Setting.imagePx / 2, Setting.imagePx / 2);
        G.I.PlayerStatusBar.Init();
    }
}