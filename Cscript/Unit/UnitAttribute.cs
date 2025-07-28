using Godot;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
public class UnitAttribute
{
    public Unit unit;
    private int _str = 10, _dex = 10, _con = 10, _spi = 10, _mag = 10, _cun = 10;
    public float SpeedGlobal = 100;
    public float SpeedCombat = 100;
    public float SpeedMove = 100;
    public float DamageBody = 100;
    public float DamageBullet = 100;
    public UnitAttribute()
    {
        skillPoint = 0;
        uaPoint = 0;
    }
    private int _skillPoint;
    public int skillPoint
    {
        get { return _skillPoint; }
        set
        {
            if (unit == Player.PlayerUnit)
                G.I.Player.SkillPoint = value;

            _skillPoint = value;
        }
    }

    private int _uaPoint;
    public int uaPoint
    {
        get { return _uaPoint; }
        set
        {
            if (unit == Player.PlayerUnit)
                G.I.Player.UaPoint = value;

            _uaPoint = value;
        }
    }

    public int Str
    {
        get => _str - 10;
        set
        {
            int oldDex = _str - 10;
            int newDex = value;
            DamageBody += (newDex - oldDex) * 2;
            _str = value + 10;
        }
    }

    public int Dex
    {
        get => _dex - 10;
        set
        {
            int oldDex = _dex - 10;
            int newDex = value;
            SpeedGlobal += (newDex - oldDex) * 1;
            _dex = value + 10;
        }
    }

    public int Con
    {
        get => _con - 10;
        set
        {
            int oldDex = _con - 10;
            int newDex = value;
            unit.MaxHp += (newDex - oldDex) * 5;
            _con= value + 10;
        }
    }

    public int Spi
    {
        get => _spi - 10;
        set
        {
            int oldDex = _spi - 10;
            int newDex = value;
            unit.MaxSp += (newDex - oldDex) * 2;
            DamageBullet += (newDex - oldDex) * 1;
            _spi = value + 10;
        }
    }

    public int Mag
    {
        get => _mag - 10;
        set
        {
            int oldDex = _mag - 10;
            int newDex = value;
            unit.MaxMp += (newDex - oldDex) * 1;
            DamageBullet += (newDex - oldDex) * 1;
            _mag = value + 10;
        }
    }

    public int Cun
    {
        get => _cun - 10;
        set
        {
            int oldDex = _cun - 10;
            int newDex = value;
            unit.inventory.MaxWeight += (newDex - oldDex) * 2;
            unit.equipment.MaxEquipWeight += (newDex - oldDex) * 2;
            _cun = value + 10;
        }
    }

    // 可选：一个构造函数初始化所有属性
    public void UnitAtt(int str = 10, int dex = 10, int con = 10, int spi = 10, int mag = 10, int cun = 10)
    {
        //return;
        Str = str - 10;
        Dex = dex - 10;
        Con = con - 10;
        Spi = spi - 10;
        Mag = mag - 10;
        Cun = cun - 10;
    }

    // 可选：一个显示真实数值的方法
    public override string ToString()
    {
        return $"Str: {Str}, Dex: {Dex}, Con: {Con}, Spi: {Spi}, Mag: {Mag}, Cun: {Cun}";
    }
}
public class EnemyData
{
    public string Name { get; set; }
    public List<string> Tags { get; set; }
    public float HP { get; set; }
    public float SP { get; set; }
    public float MP { get; set; }
    public int Str { get; set; }
    public int Dex { get; set; }
    public int Con { get; set; }
    public int Spi { get; set; }
    public int Mag { get; set; }
    public int Cun { get; set; }
    public int Value { get; set; } = 0;
    // 技能名称 → 权重/概率
    public Dictionary<string, float> Skills { get; set; } = [];
    public Dictionary<string, float> Equipment { get; set; } = [];
    public Dictionary<string, float> Inventory { get; set; } = [];
    public Dictionary<string, float> Memory { get; set; } = [];
}
public static class EnemyLoader
{
    public static List<EnemyData> enemyDatas;
    public static void LoadEnemies()
    {
        string jsonPath = "Cscript/Enemy/Enemy.json";
        var file = FileAccess.Open(jsonPath, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr($"无法打开敌人角色库: {jsonPath}");
            return;
        }

        string jsonText = file.GetAsText();
        file.Close();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        enemyDatas = JsonSerializer.Deserialize<List<EnemyData>>(jsonText, options) ?? [];
    }
    public static Unit LoadEnemy(string name)
    {
        EnemyData ed = enemyDatas.FirstOrDefault(x => x.Name == name);
        Unit unit = new()
        {
            MaxHp = ed.HP,
            MaxMp = ed.MP,
            MaxSp = ed.SP,
            Name = name,
            Value = ed.Value,
        };
        unit.Ua = new UnitAttribute
        {
            unit = unit
        };
        unit.equipment = new Equipment(unit);
        unit.Memorys = new MemoryBag(unit);
        unit.inventory = new Inventory(unit);
        unit.Ua.UnitAtt(ed.Str, ed.Dex, ed.Con, ed.Spi, ed.Mag, ed.Cun);
        unit.CurrentHp = unit.MaxHp;
        unit.CurrentMp = unit.MaxMp;
        unit.CurrentSp = unit.MaxSp / 2;
        foreach (var kvp in ed.Skills)
        {
            unit.skills.Add((new SkillInstance(Skill.NameSkill[kvp.Key]), kvp.Value));
        }
        foreach (var kvp in ed.Inventory)
        {
            float n = kvp.Value;
            while (GD.Randf() < n)
            {
                unit.inventory.AddItem(Item.GetItemName(kvp.Key));
                n--;
            }
        }
        foreach (var kvp in ed.Equipment)
        {
            float n = kvp.Value;
            while (GD.Randf() < n)
            {
                ItemInstance i = Item.GetItemName(kvp.Key);
                unit.inventory.AddItem(i);
                unit.equipment.TryEquip(i, unit);
                n--;
            }
        }
        foreach (var kvp in ed.Memory)
        {
            float n = kvp.Value;
            while (GD.Randf() < n)
            {
                ItemInstance i = Item.GetItemName(kvp.Key);
                unit.inventory.AddItem(i);
                unit.Memorys.TryEquip(i, unit);
                n--;
            }
        }
        return unit;
    }
}