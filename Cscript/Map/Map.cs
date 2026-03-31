using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
public class Grid : IInteractable
{
    public Vector2I Position;
    public bool IsWalkable;
    public bool IsTransparent;
    public bool IsEmpty => IsWalkable && unit == null && TerrainStand == "";
    private string _terrainBaseGround = "Grass";
    public string TerrainBaseGround
    {
        get => _terrainBaseGround;
        set
        {
            _terrainBaseGround = value;
            RefreshGridVisio();
        }
    }

    private void RefreshGridVisio()
    {
        switch (_terrainBaseGround)
        {
            case "Road":
            case "Grass":
            case "Floor":
            default:
                IsWalkable = true;
                IsTransparent = true;
                break;
            case "Wall":
            case "ScarletWall":
            case "Stone":
                IsWalkable = false;
                IsTransparent = false;
                break;
            case "Water":
                IsWalkable = false;
                IsTransparent = true;
                break;
        }
        switch (_terrainStand)
        {
            case "DoorClosed":
            case "Forest":
                IsWalkable = false;
                IsTransparent = false;
                break;
            default:
                break;
        }
    }

    private string _terrainStand = "";
    public string TerrainStand
    {
        get => _terrainStand;
        set
        {
            _terrainStand = value;
            RefreshGridVisio();
        }
    }
    public string TerrainFogOfWar = "";
    public Grid(Vector2I position, string tBaseGround, string tStand = "", string tFogOfWar = "Fog")
    {
        Position = position;
        TerrainBaseGround = tBaseGround;
        TerrainStand = tStand;
        TerrainFogOfWar = tFogOfWar;
    }
    public Unit unit;
    public List<IInteractable> InteractableObjects = [];
    /// <summary>
    /// 返回当前地图中该格子的周围格子,如果地图未加载,需改用map类下的同名方法
    /// </summary>
    /// <param name="radius"></param>
    /// <returns></returns>
    public List<Grid> NearGrids(int radius)
    {
        List<Grid> tiles = [];
        float rSquared = (radius + 0.5f) * (radius + 0.5f);

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                Vector2I pos = new(Position.X + dx, Position.Y + dy);
                int distSquared = dx * dx + dy * dy;

                if (distSquared <= rSquared && Scene.CurrentMap.CheckGrid(pos))
                    tiles.Add(Scene.CurrentMap.GetGrid(pos));
            }
        }
        return tiles;
    }

    void IInteractable.Interact(Unit u)
    {
        if (IsWalkable)
        {
            if (unit != null)
            {
                Skill.NameSkill["Attack"].Activate(new SkillContext(u, unit));
            }
            else if (u.Ue.CheckMoveUsage(true))
            {
                Skill.NameSkill["Move"].Activate(new SkillContext(u, this));
            }
            else
                Info.Print("你无法移动");
        }
        else
        {
            if (TerrainStand == "DoorClosed")
            {
                MapBuilder.SetLogicMapTerrain(LogicMapLayer.Stand, this, "DoorOpen");
                IsWalkable = true;
                IsTransparent = true;
                Player.PlayerUnit.Up.RefreshVision();
            }
        }
    }
}

public class Map
{
    public string Name;
    public Grid[,] Grid;
    public int Width;
    public int Height;
    public int Size => Width * Height;
    public List<Unit> Units = [];
    public HashSet<Unit> WakeUnits = [];
    public Action AfterEnter;
    public List<Bullet> Bullets = [];
    public Vector2I Entrance;
    public Vector2I Exit;
    public Map MapGoto;
    public Unit ActiveUnit;
    /// <summary>
    /// 权重单位：每100格生成数量
    /// </summary>
    public Dictionary<string, float> EnemySummonValue;
    public bool IsPrebuild = false;
    public int NaturalSp = 20;
    public Map(int width, int height)
    {
        Width = width;
        Height = height;
        Grid = new Grid[width, height];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Grid[x, y] = new Grid(new Vector2I(x, y), "Grass");
            }
        }
    }
    public Map(string name)
    {
        IsPrebuild = true;
        Name = name;
    }
    public Grid GetGrid(Vector2I v)
    {
        int x = v.X; int y = v.Y;
        if (!CheckGrid(v))
            return null; // 越界返回 null
        return Grid[x, y];
    }
    public bool CheckGrid(Vector2I v)
    {
        return CheckGrid(v.X, v.Y);
    }
    public bool CheckGrid(int x, int y)
    {
        if (x < 0 || x >= Grid.GetLength(0) || y < 0 || y >= Grid.GetLength(1))
            return false;
        return true;
    }
    public bool CheckWalkable(Vector2I v)
    {
        var grid = GetGrid(v);
        if (grid == null)
            return false; // 越界返回 false
        return grid.IsWalkable && grid.unit == null;
    }
    public Grid RandomEmptyGrid()
    {
        int i = 0;
        do
        {
            i++;
            int x = GD.RandRange(0, Width - 1);
            int y = GD.RandRange(0, Height - 1);
            Vector2I v = new(x, y);
            if (GetGrid(v).IsEmpty)
            {
                return GetGrid(new Vector2I(x, y));
            }
        } while (i < 1000);
        return null; // 如果找不到空格子，返回默认位置
    }
    public List<Grid> NearGrids(Grid grid, int radius)
    {
        List<Grid> tiles = [];
        float rSquared = (radius + 0.5f) * (radius + 0.5f);

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                Vector2I pos = new(grid.Position.X + dx, grid.Position.Y + dy);
                int distSquared = dx * dx + dy * dy;

                if (distSquared <= rSquared && CheckGrid(pos))
                    tiles.Add(GetGrid(pos));
            }
        }
        return tiles;
    }
    public void SetExit(Grid exit = null)
    {
        exit ??= GetGrid(MapGenerator.FloodFindFarthest(this, Entrance));
        Exit = exit.Position;
        exit.TerrainBaseGround = "Stair";
        exit.TerrainStand = "";
    }
    public void FindExit()
    {
        Grid exit = GetGrid(MapGenerator.FloodFindFarthest(this, Entrance));
        Exit = exit.Position;
    }
    public Unit CreateEnemy(Vector2I position, string name, UnitEgo ego = UnitEgo.random, float memoryValue = -1, bool jsonImport = true)
    {
        Unit unit = EnemyLoader.LoadEnemy(name, jsonImport);
        // 分配Ego
        if (ego == UnitEgo.random)
        {
            float p = GD.Randf();
            if (p < 1 - Scene.eliteProp - Scene.greatProp)
                unit.Ego = UnitEgo.normal;
            else if (p < 1 - Scene.greatProp)
                unit.Ego = UnitEgo.elite;
            else
                unit.Ego = UnitEgo.great;
        }
        else
            unit.Ego = ego;
        // 初始化数据,图像
        SpriteManager.LoadEnemy(unit);
        unit.Up.CurrentGrid = GetGrid(position);
        unit.Up.CurrentGrid.unit = unit;
        Root.rootnode.AddChild(unit.Up.sprite);
        unit.Up.sprite.Position = Setting.imagePx * unit.Up.Position;
        Units.Add(unit);
        // 初始化事件：Ue
        unit.Ue.OnUnitUpdate += (unit, updateTime) => unit.Ua.HealHp(updateTime * unit.Ua.MaxHp / 10000);
        unit.Ue.OnUnitUpdate += (unit, updateTime) => unit.Ua.GetSp(0.002f * updateTime * (NaturalSp - unit.Ua.CurrentSp));
        unit.Ue.OnUnitUpdate += (unit, updateTime) => unit.Ua.GetMp(updateTime * unit.Ua.Mag / 1000);
        // 初始化技能,Ai
        unit.Friendness = -1;
        unit.UnitAi = new UnitAi(unit);
        unit.UnitAi.FindTarget();
        foreach (var gskill in Skill.SkillDeck)
        {
            if (gskill.SkillGroup == "")
                unit.Us.LearnSkill(gskill);
        }
        // 分配EgoMemory与MemoryValue
        switch (unit.Ego)
        {
            case UnitEgo.normal:
                unit.MemoryValue += 1.5f * Setting.chaos;
                break;
            case UnitEgo.elite:
                unit.MemoryValue += 5 * Setting.chaos;
                unit.Memorys.TryEquip(Item.CreateItem("mElite"), unit);
                break;
            case UnitEgo.great:
                unit.MemoryValue += 8 * Setting.chaos;
                unit.Memorys.TryEquip(Item.CreateItem("mGreat"), unit);
                break;
            case UnitEgo.boss:
                unit.MemoryValue += 12 * Setting.chaos;
                unit.Memorys.TryEquip(Item.CreateItem("mBoss"), unit);
                break;
            case UnitEgo.eliteBoss:
                unit.MemoryValue += 16 * Setting.chaos;
                unit.Memorys.TryEquip(Item.CreateItem("mEliteBoss"), unit);
                break;
        }
        if(memoryValue != -1)
            unit.MemoryValue = memoryValue;
        // 随机获得 Memory
        {
            unit.Memorys.MaxEquipWeight = unit.MemoryValue;
            // 总容量
            float capacity = unit.MemoryValue;
            float currentWeight = 0;
            int attempts = 0;

            // 提前构建候选 Memory 列表
            List<Item> memories = [.. Item.ItemDeck
        .OfType<Memory>()
        .Where(x => x.Group != "Ego")
        .Cast<Item>()];

            // 随机尝试直到满足容量或达到最大尝试次数
            while (attempts < 40 && currentWeight < capacity)
            {
                attempts++;
                var it = memories[GD.RandRange(0, memories.Count - 1)];

                if (currentWeight + it.Weight <= capacity)
                {
                    unit.Memorys.TryEquip(Item.CreateItem(it.Name, 0.6f), unit);
                    currentWeight += it.Weight;
                }
            }
        }
        // 升级技能
        for(int i = 0; i < unit.Ua.skillPoint; i++)
        {
            List<Skill> skills = [.. unit.Us.skills
                .Select(x => x.skill)
                .Where(x => x.SkillGroup != "" && x is not SkillFromItem<SkillItem> && x.Level < 4)];
            if (skills.Count == 0) break;
            var sn = skills[GD.RandRange(0, skills.Count - 1)].Name;
            unit.Us.skills.FirstOrDefault(si => si.skill.Name == sn).skill.Level++;
        }

        // 计算优势距离
        float bestScore = int.MinValue;
        // 1. 计算优势距离（1~32）以最大化技能收益
        for (int dist = 1; dist <= 32; dist++)
        {
            double score = 0;
            foreach (var lvp in unit.Us.skills)
            {
                var skill = lvp.skill;
                if (skill.EffectType == EffectType.Passive || skill.GetTargeting().Range == -1)
                    continue;
                float weight = lvp.weight;
                int skillDistance = skill.GetTargeting().Range;
                if (skillDistance == 0) { continue; }
                double diff = Math.Abs(dist - skillDistance) + 0.3;
                score += weight / (diff * diff);
            }

            if (score > bestScore)
            {
                bestScore = (float)score;
                unit.UnitAi.bestDistance = dist;
            }
        }
        unit.UnitAi.bestDistance /= 2;
        // 随机分配UaPoint
        int[] attributes = new int[6]; // 用来存储分配的值
        for (int i = 0; i < unit.Ua.uaPoint; i++)
        {
            int index = GD.RandRange(0, 5); // 0到5之间选择一个属性
            attributes[index]++;
        }
        unit.Ua.Str += attributes[0];
        unit.Ua.Dex += attributes[1];
        unit.Ua.Con += attributes[2];
        unit.Ua.Spi += attributes[3];
        unit.Ua.Mag += attributes[4];
        unit.Ua.Cun += attributes[5];
        // 回满hpspmp
        unit.Ua.CurrentHp = unit.Ua.MaxHp;
        unit.Ua.CurrentSp = unit.Ua.MaxSp;
        unit.Ua.CurrentMp = unit.Ua.MaxMp;
        unit.Ua.InitializeHpSpBar();
        unit.TimeEnergy = GD.RandRange(-100, 0) - 20;
        return unit;
    }
    public Unit CreateFriend(Unit master, string name, UnitEgo ego = UnitEgo.random, float memoryValue = -1, bool jsonImport = true)
    {
        for(int d = 1; d <= 4; d++)
        {
            foreach(var g in master.Up.CurrentGrid.NearGrids(d))
            {
                if (g.IsEmpty)
                {
                    Unit unit = CreateEnemy(g.Position, name, ego, memoryValue, jsonImport);
                    unit.Friendness = master.Friendness;
                    unit.UnitAi = new(unit)
                    {
                        master = master,
                        State = AiState.Follow
                    };
                    unit.UnitAi.FindTarget();
                    return unit;
                }
            }
        }
        return null;
    }
    public void DeleteUnit(Unit unit)
    {
        if (unit == Player.PlayerUnit)
        {
            // 实际玩家死亡效果由scene脚本负责
            return;
        }
        unit.dead = true;
        unit.Ue.Killed();
        unit.Up.CurrentGrid.unit = null;
        Info.Print($"{unit.TrName} 被退治了");
        unit.Up.sprite.QueueFree();
        Units.Remove(unit);
        WakeUnits.Remove(unit);
        foreach (var si in SpellCard.currentSpellcards.Where(x => x.User == unit).ToList())
        {
            SpellCard.currentSpellcards.Remove(si);
        }
        Tutorial.enemydead++;
        GameEvents.EnemyKilled(unit);
        if (unit.Ego != UnitEgo.normal)
        {
            foreach (Item i in unit.Equipment.EquippedItems.ToList())
            {
                unit.Equipment.Unequip(i, unit);
            }
            foreach (Item i in unit.Memorys.EquippedItems.ToList())
            {
                if (i is Memory m && m.Group == "Ego")
                    continue;
                unit.Memorys.Unequip(i, unit);
            }
        }
        foreach (Item i in unit.Inventory.Items.ToList())
        {
            if (GD.Randf() < i.salvage)
                unit.Inventory.ThrowItem(i);
        }
    }
    public void SummonEnemy()
    {
        if (EnemySummonValue == null || EnemySummonValue.Count == 0)
            return; // 没有敌人召唤值，直接返回
        var totalWeight = EnemySummonValue.Values.Sum();
        float groupValue = 2;
        for (float i = 0; i < totalWeight * Size / 100; i+=groupValue)
        {
            float rand = (float)GD.RandRange(0, totalWeight);
            float cumulative = 0;
            foreach (var kvp in EnemySummonValue)
            {
                cumulative += kvp.Value;
                if (rand <= cumulative)
                {
                    GroupSummon(RandomEmptyGrid().Position, kvp.Key, 3, groupValue - 1);
                    break;
                }
            }
        }
        void GroupSummon(Vector2I position, string name, int searchDist = 3, float expectExtraNumber = 1)
        {
            CreateEnemy(position, name);
            while (GD.Randf() < expectExtraNumber / (expectExtraNumber + 1))
            {
                var lg = Scene.CurrentMap.NearGrids(GetGrid(position), searchDist);
                List<Grid> lga = [.. lg.Where(x => x.IsWalkable && x.unit == null)];
                if (lga.Count == 0) return; // 没有可用格子了
                int index = GD.RandRange(0, lga.Count - 1);
                CreateEnemy(lga[index].Position, name);
            }
        }
    }
    public void SummonChest(float value, int number, Grid grid = null)
    {
        AfterEnter += Instance;

        void Instance()
        {
            // 提前准备“可抽取物品池”
            var itemPool = Item.ItemDeck
                .Where(x => x is not BarrageComponent && x is not Memory && x.chestValue > 0)
                .ToList();

            for (int i = 0; i < number; i++)
            {
                Chest c = new(grid ?? RandomEmptyGrid());
                int j = 0;
                float v = 0;

                do
                {
                    j++;
                    Item baseItem = Item.GetWeightedRandomItem(itemPool);
                    if (baseItem == null)
                        break;
                    // 实例化
                    Item it = Item.CreateItem(baseItem.Name);
                    // 随机参数 & Ego
                    Item item = it.RandomSummonParam();
                    item.effectLevel = 0;//GD.RandRange(0, 2);
                    if (item is Memory)
                        item.effectLevel = 0;
                    //item.AddRandomEgo(3 * item.effectLevel);
                    item.ApplyParameters([]);
                    it = item;
                    // 价值判断
                    if (v + it.Weight - 1 < value)
                    {
                        if (it is Memory m && m.Group != "")
                            continue;
                        c.items.Add(it);
                        v += it.Weight;
                    }

                } while (j < 40 && v < value);
            }
        }
    }

}
public static class Scene
{
    public static Map CurrentMap { get; set; } = new(2, 2);
    public static float eliteProp = 0.04f;
    public static float greatProp = 0.01f;
    public static void Enter(Map map)
    {
        if (map.IsPrebuild)
        {
            CurrentMap = G.I.TileMapAllLayer.InitImportMap(map);
        }
        else
        {
            G.I.TileMapAllLayer.InitGeneralMap();
        }
        CurrentMap = map;
        CurrentMap.Units.Clear();
        CurrentMap.WakeUnits.Clear();
        MapBuilder.BuildFogOfWar(map);
        map.Units.Add(Player.PlayerUnit);
        map.WakeUnits.Add(Player.PlayerUnit);
        Player.PlayerUnit.Up.MoveTo(map.GetGrid(map.Entrance));
        if (!map.IsPrebuild)
            MapBuilder.BuildTileMapFromLogic(CurrentMap);
        CurrentMap.SummonEnemy();
        Player.PlayerUnit.Up.MoveTo(map.GetGrid(map.Entrance));//激活敌人单位
        CurrentMap.AfterEnter?.Invoke();
        Info.Print("ifNewMapMemory");
        Player.PlayerUnit.Memorys.MaxEquipWeight += 5;
        Player.PlayerUnit.Ua.CurrentSp = CurrentMap.NaturalSp;
        eliteProp += 0.006f;
        greatProp += 0.003f;
        GameEvents.MapEnter();
        G.I.PlayerStatusBar.Init();
        Player.PlayerUnit.Ua.HealHp(Player.PlayerUnit.Ua.MaxHp);
    }

    public static void LeaveAndGo()
    {
        Quit();
        Enter(CurrentMap.MapGoto);
    }

    public static void Quit()
    {
        GameEvents.SceneQuit();
        foreach (var child in G.I.LayerItemDropped.GetChildren())
            child?.QueueFree();
        foreach (var u in CurrentMap.Units)
            if (u != Player.PlayerUnit)
                u.Up.sprite.QueueFree();
        CurrentMap.Units.Clear();
        CurrentMap.WakeUnits.Clear();
        foreach (var b in CurrentMap.Bullets)
            b.image.QueueFree();
        CurrentMap.Bullets.Clear();
        SpellCard.currentSpellcards.Clear();
    }
}



