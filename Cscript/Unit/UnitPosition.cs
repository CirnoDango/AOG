using Godot;
using System.Collections.Generic;
using System.Linq;

public class UnitPosition(Unit unit)
{
    private Unit _parent = unit;
    private Grid _currentPosition;
    public Grid CurrentGrid
    {
        get => _currentPosition;
        set
        {
            _currentPosition = value;
            if (_currentPosition != null && sprite != null)
            {
                sprite.Position = _currentPosition.Position * Setting.imagePx;
            }
        }
    }
    public Vector2I Position
    {
        get => CurrentGrid != null ? CurrentGrid.Position : new Vector2I(-1, -1);
    }
    public Sprite2D sprite { get; set; }
    private int _vision = 12;
    public int Vision
    {
        get => _vision;
        set
        {
            _vision = value;
            if (_parent == Player.PlayerUnit)
                RefreshVision();
        }
    }

    public void MoveTo(Grid grid)
    {
        if (_parent.dead)
            return;
        CurrentGrid?.unit = null;
        CurrentGrid = grid;
        grid.unit = _parent;
        _parent.Ue.UnitMove();
        GameEvents.UnitMove(_parent);
        if (_parent == Player.PlayerUnit)
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
            oldVisibleGrids = CurrentGrid.NearGrids(14);
        var newVisibleGrids = GridInVision();

        // 半雾处理：原来能看见但现在看不见的变成 HalfFog
        foreach (var g in oldVisibleGrids)
        {
            if (!newVisibleGrids.Contains(g) && g.TerrainFogOfWar == "Clear")
            {
                MapBuilder.SetLogicMapTerrain(LogicMapLayer.FogOfWar, g, "HalfFog");
                g.unit?.Up.sprite.Visible = false;
            }
        }
        // 新增清晰视野
        foreach (var g in CurrentGrid.NearGrids(14))
        {
            if (newVisibleGrids.Contains(g))
            {
                MapBuilder.SetLogicMapTerrain(LogicMapLayer.FogOfWar, g, "Clear");
                g.unit?.Up.sprite.Visible = true;
            }
            if (g.unit != null && g.unit != Player.PlayerUnit && g.unit.Up.GridInVision().Contains(CurrentGrid))
            {
                Scene.CurrentMap.WakeUnits.Add(g.unit);
                g.unit.UnitAi.State = AiState.Attack;
            }
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
            foreach (Vector2I v in MathEx.NearVectors)
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
                foreach (var v in line)
                {
                    grids.Add(Scene.CurrentMap.GetGrid(v));
                }
            }
        }
        return [.. grids];
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

    public bool RandomEnemyInVision(out Unit unit)
    {
        var lag = GridInVision().Where(g => g.unit != null && g.unit.Friendness * _parent.Friendness < 0).ToArray();
        if (lag.Length > 0)
        {
            unit = lag[GD.RandRange(0, lag.Length - 1)].unit;
            return true;
        }
        unit = null;
        return false;
    }
    public HighlightType CheckSkillTarget(Skill skill, Vector2I target)
    {
        // Step 1: 地图边界检查
        if (!Scene.CurrentMap.CheckGrid(target))
            return HighlightType.blue;

        // Step 2: 距离检查
        if (Position.DistanceTo(target) > (skill.GetTargeting().Range+0.5f))
            return HighlightType.red;

        // Step 3: 获取目标格子和单位（缓存）
        var grid = Scene.CurrentMap.GetGrid(target);
        var unit = grid?.unit;

        // Step 4: 根据技能目标类型判定
        switch (skill.GetTargeting().Type)
        {
            case Target.Grid:
                if (grid.IsWalkable)
                    return HighlightType.green;
                return HighlightType.blue;

            case Target.Unit:
                return unit != null ? HighlightType.green : HighlightType.blue;

            case Target.Enemy:
                if (unit != null && unit.Friendness * _parent.Friendness < 0)
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
            case Target.Self:
                return HighlightType.green;
            default:
                return HighlightType.blue;
        }
    }
    public void KnockBack(float distance, SkillContext sc)
    {
        Vector2I Going = (Vector2I)(Position
                       + distance * ((Vector2)(Position - sc.User.Up.Position)).Normalized()
                       + new Vector2(0.5f, 0.5f));
        List<Vector2I> gs = MathEx.GetLine(sc.User.Up.Position, Going);
        gs.RemoveAt(0);
        foreach (var grid in gs)
        {
            if (Scene.CurrentMap.GetGrid(grid) != null &&
                Scene.CurrentMap.GetGrid(grid).IsWalkable)
            {
                MoveTo(Scene.CurrentMap.GetGrid(grid));
            }
            else
            {
                break;
            }
        }
    }
}
