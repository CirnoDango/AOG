using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
public static class EnemyLoader
{
    public static List<EnemyData> enemyDatas;
    public static void LoadEnemies()
    {
        string jsonPath = "Data/Enemy.json";
        var file = FileAccess.Open(jsonPath, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr($"无法打开敌人角色库: {jsonPath}");
            return;
        }

        string jsonText = file.GetAsText();
        file.Close();

        enemyDatas = JsonConvert.DeserializeObject<List<EnemyData>>(jsonText);
        foreach (var ed in enemyDatas)
        {
            CleanupEnemyData(ed);
        }

    }
    static object ConvertJToken(object token)
    {
        if (token is JObject jObj)
        {
            // JObject -> Dictionary<string, object>，同时递归处理内部
            return jObj.ToObject<Dictionary<string, object>>()
                       .ToDictionary(k => k.Key, v => ConvertJToken(v.Value));
        }
        if (token is JArray jArr)
        {
            // JArray -> List<object>
            return jArr.Select(x => ConvertJToken(x)).ToList();
        }
        if (token is IDictionary<string, object> dict)
        {
            // 针对已是 Dictionary<string, object>，递归处理其中的值
            return dict.ToDictionary(k => k.Key, v => ConvertJToken(v.Value));
        }
        if (token is IList list)
        {
            // 这个分支防止List<object>内还有JObject或JArray
            var newList = new List<object>();
            foreach (var item in list)
            {
                newList.Add(ConvertJToken(item));
            }
            return newList;
        }
        if (token is long l)
            return (int)l;

        if (token is JValue jVal)
        {
            return jVal.Value;
        }
        return token;
    }

    static void CleanupEnemyData(EnemyData ed)
    {
        var props = typeof(EnemyData)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(List<Dictionary<string, object>>));

        foreach (var prop in props)
        {
            var list = prop.GetValue(ed) as List<Dictionary<string, object>>;
            if (list == null) continue;

            var cleaned = list
                .Select(dict => ConvertJToken(dict) as Dictionary<string, object>)
                .ToList();

            prop.SetValue(ed, cleaned);
        }
    }

    public static Unit LoadEnemy(string name, bool jsonImport = true)
    {
        EnemyData ed = enemyDatas.FirstOrDefault(x => x.Name == name);
        Unit unit = new()
        {
            Name = name,
            MemoryValue = ed.Value,
        };
        unit.Ua = new UnitAttribute(unit)
        {
            MaxHp = ed.HP, 
            MaxMp = ed.MP,
            MaxSp = ed.SP,
        };
        unit.Equipment = new Equipment(unit);
        unit.Memorys = new MemoryBag(unit);
        unit.Inventory = new Inventory(unit);
        unit.Ua.UnitAtt(ed.Str, ed.Dex, ed.Con, ed.Spi, ed.Mag, ed.Cun);
        unit.Ua.CurrentHp = unit.Ua.MaxHp;
        unit.Ua.CurrentMp = unit.Ua.MaxMp;
        unit.Ua.CurrentSp = unit.Ua.MaxSp / 2;
        if (jsonImport)
            ImportJsonData(ed, unit);
        return unit;

        static void ImportJsonData(EnemyData ed, Unit unit)
        {
            foreach (var kvp in ed.Skills)
            {
                unit.Us.skills.Add((new SkillInstance(Skill.NameSkill[kvp.Key]), kvp.Value));
            }
            foreach (var sg in ed.SkillGroups)
            {
                unit.Us.LearnSkillGroup(sg);
            }
            foreach (var dict in ed.Inventory)
            {
                float n = Convert.ToSingle(dict["Amount"]);

                while (GD.Randf() < n)
                {
                    Item i = Item.CreateItem((string)dict["Name"]);

                    if (dict.TryGetValue("Parameters", out object value))
                    {
                        i.ApplyParameters(value as Dictionary<string, object>);
                    }
                    unit.Inventory.AddItem(i);
                    n--;
                }
            }
            foreach (var dict in ed.Equipment)
            {
                float n = Convert.ToSingle(dict["Amount"]);

                while (GD.Randf() < n)
                {
                    Item i = Item.CreateItem((string)dict["Name"]);

                    if (dict.TryGetValue("Parameters", out object value))
                    {
                        i.ApplyParameters(value as Dictionary<string, object>);
                    }
                    unit.Equipment.TryEquip(i, unit);
                    n--;
                }
            }

            foreach (var kvp in ed.Memory)
            {
                float n = kvp.Value;
                while (GD.Randf() < n)
                {
                    Item i = Item.CreateItem(kvp.Key);
                    unit.Memorys.TryEquip(i, unit);
                    n--;
                }
            }
        }
    }
    public static Unit LoadPlayer(string name)
    {
        EnemyData ed = enemyDatas.FirstOrDefault(x => x.Name == name);
        Unit unit = new()
        {
            Name = name,
            MemoryValue = ed.Value,
        };
        unit.Ua = new UnitAttribute(unit)
        {
            MaxHp = ed.HP,
            MaxMp = ed.MP,
            MaxSp = ed.SP,
        };
        unit.Equipment = new Equipment(unit);
        unit.Memorys = new MemoryBag(unit);
        unit.Inventory = new Inventory(unit);
        unit.Ua.UnitAtt(ed.Str, ed.Dex, ed.Con, ed.Spi, ed.Mag, ed.Cun);
        unit.Ua.CurrentHp = unit.Ua.MaxHp;
        unit.Ua.CurrentMp = unit.Ua.MaxMp;
        unit.Ua.CurrentSp = unit.Ua.MaxSp / 2;
        return unit;
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
    public List<string> SkillGroups { get; set; } = [];
    public Dictionary<string, float> Skills { get; set; } = [];
    public List<Dictionary<string, object>> Equipment { get; set; } = [];
    public List<Dictionary<string, object>> Inventory { get; set; } = [];
    public Dictionary<string, float> Memory { get; set; } = [];
}