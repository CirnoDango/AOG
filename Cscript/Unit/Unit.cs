using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public class Unit
{
    private Grid _currentPosition;
    public Grid CurrentGrid
    {
        get => _currentPosition;
        set
        {
            _currentPosition = value;
            if (_currentPosition != null && sprite != null)
            {
                sprite.Position = _currentPosition.Position * 16; // 16是格子大小
            }
        }
    }
    public Vector2I Position
    {
        get => CurrentGrid != null ? CurrentGrid.Position : new Vector2I(-1, -1);
    }
    public Sprite2D sprite;
    private ColorRect hpBarAhead;
    private ColorRect spBarAhead;

    public float MaxHp = 100;
    public float CurrentHp = 100;
    private float _maxSp = 40;
    public float MaxSp
    {
        get => _maxSp;
        set { if (_maxSp != 0) { _maxSp = value; } }
    }
    public float CurrentSp = 0;
    public float MaxMp = 10;
    public float CurrentMp = 10;
    public float TimeEnergy { get; set; } = 0;
    public float imageSizeFactor = 0.16f;
    public string Name;
    public string TrName => $"u{Name}";
    public List<(SkillInstance skill, float weight)> skills = [];
    public SkillInstance GetSkill(string name) => skills.FirstOrDefault(x => x.skill.Name == name).skill;
    public UnitAi unitAi;
    public UnitAttribute Ua { get; set; }
    public int friendness = -1;
    public SpellCard currentSpellcard;
    public List<Status> Status = [];
    public Dictionary<Status, Node> StatusImages = [];
    public static Action OnPlayerdied;
    private int _vision = 10;
    public int Vision
    {
        get => _vision;
        set
        {
            _vision = value;
            if (this == Player.PlayerUnit)
                RefreshVision();
        }
    }
    public Inventory inventory;
    public Equipment equipment;
    public MemoryBag Memorys;
    public float Value = 10;
    public void InitializeHpSpBar()
    {
        // HP条背景
        var hpBarBack = new ColorRect
        {
            Color = Colors.DarkGray,
            Size = new Vector2(1 / imageSizeFactor, 16 / imageSizeFactor),
            Position = new Vector2(6 / imageSizeFactor, -8 / imageSizeFactor) // 靠右
        };
        sprite.AddChild(hpBarBack);

        // HP条前景（红色）
        hpBarAhead = new ColorRect
        {
            Color = Colors.Red,
            Size = new Vector2(1 / imageSizeFactor, 16 / imageSizeFactor), // 初始满血
            Position = Vector2.Zero
        };
        hpBarBack.AddChild(hpBarAhead);
        if (MaxSp == 0)
            return;
        // SP条背景（左边贴着 HP）
        var spBarBack = new ColorRect
        {
            Color = Colors.DarkGray,
            Size = new Vector2(1 / imageSizeFactor, 16 / imageSizeFactor),
            Position = new Vector2(5 / imageSizeFactor, -8 / imageSizeFactor) // 比 hp 左 1 个单位
        };
        sprite.AddChild(spBarBack);

        // SP条前景（黄色）
        spBarAhead = new ColorRect
        {
            Color = Colors.Yellow,
            Size = new Vector2(1 / imageSizeFactor, 0 / imageSizeFactor), // 初始满
            Position = Vector2.Zero
        };
        spBarBack.AddChild(spBarAhead);
        UpdateHpBar();
        UpdateSpBar();
    }

    public void HealHp(float amount)
    {
        amount *= 1 + 0.02f * Ua.Con;
        GetHp(amount);
    }
    public void GetHp(float amount)
    {
        CurrentHp = Math.Clamp(CurrentHp + amount, 0, MaxHp);
        UpdateHpBar();
        if (this == Player.PlayerUnit)
            G.I.PlayerStatusBar.UpdateStatusUI();
        if (CurrentHp == 0)
            if (this == Player.PlayerUnit)
                OnPlayerdied?.Invoke();
            else
                Scene.CurrentMap.DeleteUnit(this);
    }
    public void GetSp(float amount)
    {
        if (currentSpellcard != null && amount > 0)
            return;
        CurrentSp = Math.Clamp(CurrentSp + amount, 0, MaxSp);
        UpdateSpBar();
        if (this == Player.PlayerUnit)
            G.I.PlayerStatusBar.UpdateStatusUI();
    }
    public void GetMp(float amount)
    {
        CurrentMp = Math.Clamp(CurrentMp + amount, 0, MaxMp);
        if (this == Player.PlayerUnit)
            G.I.PlayerStatusBar.UpdateStatusUI();
    }
    /// <summary>
    /// 造成子弹伤害，返回值为实际造成的伤害
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="user"></param>
    /// <param name="skill"></param>
    /// <returns></returns>
    public float TakeBulletDamage(float amount, Unit user, Skill skill)
    {
        foreach (var status in Status)
            status.OnTakeBulletDamage(this, skill, ref amount);
        amount = GameEvents.TakeBulletDamage(user, this, amount);
        amount *= user.Ua.DamageBullet / 100;
        float rd = 0;
        if (currentSpellcard != null)
        {
            CurrentSp -= amount;
            if (CurrentSp < 0)
            {
                float overflow = -CurrentSp;
                CurrentSp = 0;
                float damage = overflow * 2 + MaxHp / 3;
                GetHp(-damage);
                string template = TranslationServer.Translate("击破符卡");
                string result = string.Format(template, user.TrName, skill.TrName, TrName, currentSpellcard.TrName, $"{damage:F0}");
                Info.Print(result);

                currentSpellcard.OnSpellBreak(new SkillContext(this));
                rd = damage;
            }
            else
            {
                string msg;
                msg = string.Format(
                    TranslationServer.Translate("技能被擦弹_减少SP"),
                    user.TrName, skill.TrName, TrName, amount.ToString("F0")
                );
                Info.Print(msg);
            }
        }
        else
        {
            CurrentSp += amount;
            if (CurrentSp > MaxSp)
            {
                float overflow = CurrentSp - MaxSp;
                CurrentSp = MaxSp;
                GetHp(-overflow);
                string msg = string.Format(TranslationServer.Translate("技能击中_溢出伤害"),
                user.TrName, skill.TrName, TrName, overflow.ToString("F0"));
                Info.Print(msg);
                rd = overflow;
            }
            else       
            {
                string
                msg = string.Format(TranslationServer.Translate("技能被擦弹_增加SP"),
                user.TrName, skill.TrName, TrName, amount.ToString("F0"));
                Info.Print(msg);
            }
        }
        GetSp(0);
        return rd;
    }
    public bool CheckBodyHit(float amount, Unit user, Skill skill)
    {
        float dice = 50 + user.Ua.Dex - Ua.Dex;
        if (GD.Randf() * 100 < dice)
        {
            TakeBodyDamage(amount, user, skill);
            return true;
        }
        else
        {
            string msg = string.Format(TranslationServer.Translate("skill.evaded"), user.TrName, skill.TrName, TrName);
            Info.Print(msg);
            return false;
        }
    }
    public void TakeBodyDamage(float amount, Unit user, Skill skill)
    {
        amount = GameEvents.TakeBodyDamage(user, this, amount);
        foreach (var status in Status)
            status.OnTakeBodyDamage(this, ref amount);
        amount *= user.Ua.DamageBody / 100;
        amount *= Math.Min(1, (float)(1 / Math.Round(Math.Pow((user.Position - Position).Length(), 0.5f))));
        if (amount < 0) { amount = 0; }
        GetHp(-amount);
        string msg = string.Format(TranslationServer.Translate("skill.hit_damage"), user.TrName, skill.TrName, TrName, amount.ToString("F0"));
        Info.Print(msg);
    }
    public void BalanceSp(float amount)
    {
        if (currentSpellcard != null)
            return;
        if (CurrentSp < MaxSp * 0.25f)
            GetSp(Math.Min(amount, MaxSp * 0.25f - CurrentSp));
        else
            GetSp(-Math.Min(amount, CurrentSp - MaxSp * 0.25f));
    }
    public void UpdateHpBar()
    {
        float ratio = (float)CurrentHp / MaxHp;
        hpBarAhead.Size = new Vector2(1 / imageSizeFactor, 16 * ratio / imageSizeFactor);
        hpBarAhead.Position = new Vector2(0, 16 * (1 - ratio) / imageSizeFactor);
    }
    public void UpdateSpBar()
    {
        if (MaxSp == 0)
            return;
        float ratio = (float)CurrentSp / MaxSp;
        spBarAhead.Size = new Vector2(1 / imageSizeFactor, 16 * ratio / imageSizeFactor);
        spBarAhead.Position = new Vector2(0, 16 * (1 - ratio) / imageSizeFactor);
        if (currentSpellcard != null)
            spBarAhead.Color = new Color(1, 0.5f, 0);
        else
            spBarAhead.Color = new Color(1, 1, 0);
    }
    public void MoveTo(Grid grid)
    {
        CurrentGrid?.unit = null;
        CurrentGrid = grid;
        grid.unit = this;
        GameEvents.UnitMoved(this);
        if (this == Player.PlayerUnit)
        {
            RefreshVision();
        }
    }
    /// <summary>
    /// 刷新玩家视野，主体只能传玩家单位！！！
    /// </summary>
    public void RefreshVision()
    {
        List<Grid> oldVisibleGrids = [];
        if (CurrentGrid != null)
            oldVisibleGrids = CurrentGrid.NearGrids(12);
        var newVisibleGrids = GridInVision();

        // 半雾处理：原来能看见但现在看不见的变成 HalfFog
        foreach (var g in oldVisibleGrids)
        {
            if (!newVisibleGrids.Contains(g) && g.TerrainFogOfWar == "Clear")
            {
                MapBuilder.SetLogicMapTerrain(LogicMapLayer.FogOfWar, g, "HalfFog");
                g.unit?.sprite.Visible = false;
            }
                

        }
        // 新增清晰视野
        foreach (var g in CurrentGrid.NearGrids(12))
        {
            if (newVisibleGrids.Contains(g))
            { 
                MapBuilder.SetLogicMapTerrain(LogicMapLayer.FogOfWar, g, "Clear"); 
                g.unit?.sprite.Visible = true;
            }
            if (g.unit != null && g.unit != Player.PlayerUnit && g.unit.GridInVision().Contains(g))
            {
                Scene.CurrentMap.WakeUnits.Add(g.unit);
                g.unit.unitAi.State = AiState.Attack;
            }
        }
    }
    public void LearnSkill(Skill skill)
    {
        SkillInstance si = new(skill);
        AddSkill(si, 1);
        if (Player.PlayerUnit == this)
        {
            G.I.SkillBar.LearnSkill(si);
        }
        skill.OnLearn(this); // 👈 自动触发学习效果
    }
    public void LearnSkill(string Name)
    {
        Skill skill = Skill.NameSkill[Name];
        SkillInstance si = new(skill);
        if (skill.SkillGroup == "Item")
            si.CurrentCooldown = skill.Cooldown;
        AddSkill(si, 1);
        if (Player.PlayerUnit == this)
        {
            G.I.SkillBar.LearnSkill(si);
        }
        skill.OnLearn(this); // 👈 自动触发学习效果
    }
    public void UnLearnSkill(Skill skill)
    {
        var index = skills.FindIndex(entry => entry.skill.Name == skill.Name);
        if (index >= 0)
        {
            if (Player.PlayerUnit == this)
            {
                G.I.SkillBar.UnLearnSkill(Player.PlayerUnit.GetSkill(skill.Name));
            }
            skills.RemoveAt(index);
        }
    }
    public void UnLearnSkill(string skilll)
    {
        Skill skill = Skill.NameSkill[skilll];
        UnLearnSkill(skill);
    }
    public void LearnSkillGroup(string groupName)
    {
        foreach (var skill in Skill.SkillDeck.Where(s => s.SkillGroup == groupName))
        {
            LearnSkill(skill);
        }
    }   
    public HighlightType CheckSkillTarget(SkillInstance skill, Vector2I target)
    {
        // Step 1: 地图边界检查
        if (!Scene.CurrentMap.CheckGrid(target))
            return HighlightType.blue;

        // Step 2: 距离检查
        if (Position.DistanceTo(target) > skill.Targeting.Range)
            return HighlightType.red;

        // Step 3: 获取目标格子和单位（缓存）
        var grid = Scene.CurrentMap.GetGrid(target);
        var unit = grid?.unit;

        // Step 4: 根据技能目标类型判定
        switch (skill.Targeting.Type)
        {
            case Target.Grid:
                if (grid.IsWalkable)
                    return HighlightType.green;
                return HighlightType.blue;

            case Target.Unit:
                return unit != null ? HighlightType.green : HighlightType.blue;

            case Target.Enemy:
            
                if (unit != null && unit != Player.PlayerUnit)
                    return HighlightType.green;
                else
                    return HighlightType.blue;
            case Target.Dash:
                List<Grid> g = DashCheck(grid);
                if (g == null || g.Count == 0 || g[^1].unit == null || g[^1].unit == Player.PlayerUnit)
                    return HighlightType.blue;
                return HighlightType.green;
            case Target.Ray:
                List<Grid> gr = RayCheck(grid);
                if (gr == null || gr.Count == 0)
                    return HighlightType.blue;
                return HighlightType.green;
            default:
                return HighlightType.blue;
        }
    }
    /// <summary>
    /// 获取当前单位在视野内的所有格子
    /// </summary>
    /// <returns></returns>
    public List<Grid> GridInVision()
    {
        List<Grid> grids = [];
        foreach (Grid grid in CurrentGrid.NearGrids(Vision))
        {
            if (grids.Contains(grid))
                continue;
            List<Vector2I> line = MathEx.GetLine(CurrentGrid.Position, grid.Position);
            List<Vector2I> sline = [.. line.Take(line.Count - 1)]; // 去掉最后一个点
            bool isVisible = true;
            foreach (Vector2I pos in sline)
            {
                Grid g = Scene.CurrentMap.GetGrid(pos);
                if (g == null || !g.IsTransparent)
                {
                    isVisible = false;
                    break;
                }
            }
            foreach(Vector2I v in MathEx.NearVectors)
            {
                if (isVisible)
                    break;
                line = MathEx.GetLine(CurrentGrid.Position + v, grid.Position);
                sline = [.. line.Take(line.Count - 1)];
                isVisible = true;
                foreach (Vector2I pos in sline)
                {
                    Grid g = Scene.CurrentMap.GetGrid(pos);
                    if (g == null || !g.IsTransparent)
                    {
                        isVisible = false;
                        break;
                    }
                }
            }
            if (isVisible)
            {
                foreach(var v in line)
                {
                    grids.Add(Scene.CurrentMap.GetGrid(v));
                }
            }
        }
        return grids;
    }
    public void GetStatus(Status status)
    {
        foreach(var s in this.Status)
        {
            if (s.Name == status.Name)
                return;
        }
        Status.Add(status);
        status.OnGet(this); // 👈 自动触发获得效果
    }
    public string Description()
    {
        string text = $@"{Name}
HP:{CurrentHp:F0}/{MaxHp:F0}
SP:{CurrentSp:F0}/{MaxSp:F0}
MP:{CurrentMp:F0}/{MaxMp:F0}
当前状态:";
        foreach(Status s in Status)
        {
            text += $"{s.Name}({(s.Duration / 100):F0}回合)；";
        }
        if (Status.Count == 0)
            text += "无";
        text += "\n持有技能：\n";
        foreach((SkillInstance si, _) in skills)
        {
            if (si.Template.SkillGroup != "")
                text += $"{si.Template.Name}\n";
        }
        return text;
    }
    /// <summary>
    /// 获取当前单位向某个目标格子冲刺的路径列表
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public List<Grid> DashCheck(Grid target)
    {
        // 获取从当前位置到目标格子的直线路径（包含起点和终点）
        List<Vector2I> line = MathEx.GetLine(CurrentGrid.Position, target.Position);
        List<Grid> lastWalkable = [];
        line.RemoveAt(0); // 移除起点

        foreach (Vector2I pos in line)
        {
            Grid grid = Scene.CurrentMap.GetGrid(pos);
            if (grid == null || !grid.IsWalkable)
                return null; // 路径被阻挡，无法冲刺

            if (grid.unit != null)
            {
                lastWalkable.Add(grid);
                return lastWalkable; // 前方有单位，停在前一个格子
            }
            lastWalkable.Add(grid); // 更新可到达的最后一个格子
        }
        return lastWalkable; // 路径畅通，返回终点
    }
    /// <summary>
    /// 获取当前单位向某个目标格子的激光穿透路径（遇到不透明格子停止）
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public List<Grid> RayCheck(Grid target)
    {
        List<Vector2I> line = MathEx.GetLine(CurrentGrid.Position, target.Position);
        List<Grid> path = [];
        line.RemoveAt(0); // 移除起点

        foreach (Vector2I pos in line)
        {
            Grid grid = Scene.CurrentMap.GetGrid(pos);
            if (grid == null)
                break; // 超出地图边界
            path.Add(grid);
            if (!grid.IsTransparent)
                break; // 激光被阻挡，停止
        }
        return path;
    }
    public void AddSkill(SkillInstance newSkill, float weight)
    {
        var index = skills.FindIndex(entry => entry.skill.Name == newSkill.Name);
        if (index >= 0 && this != Player.PlayerUnit)
            skills[index] = (newSkill, weight);  // 替换
        else
            skills.Add((newSkill, weight));      // 添加
    }

}

