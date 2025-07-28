using Godot;
using System;
using System.Collections.Generic;
public enum HighlightType
{
    red, green, blue, yellow
}
public partial class HighlightManager : Node2D, IRegisterToG
{
    [Export]
    public PackedScene highlightBlue;
    [Export]
    public PackedScene highlightGreen;
    [Export]
    public PackedScene highlightRed;
    [Export]
    public PackedScene highlightYellow;

    private List<Node2D> activeHighlights = [];

    private static bool isTargeting = false;
    public Vector2I showingTileWorldPos;
    public override void _Input(InputEvent @event)
    {
        if (!isTargeting)
            return;

        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            Vector2 clickPos = GetGlobalMousePosition();
            var si = Player.PlayerUnit.GetSkill(Skill.CurrentSkill.Name);
            if (Player.PlayerUnit.CheckSkillTarget(si, MapToGrid(clickPos)) == HighlightType.green)
            {
                
                switch (si.Targeting.Type)
                {
                    case Target.Dash:
                        Skill.CurrentSkill.Activate(new SkillContext(
                            Player.PlayerUnit, 
                            Player.PlayerUnit.DashCheck(Scene.CurrentMap.GetGrid(MapToGrid(clickPos)))[^1].unit,
                            si.Level));
                        break;
                    case Target.Grid:
                        Skill.CurrentSkill.Activate(new SkillContext(
                            Player.PlayerUnit, 
                            Scene.CurrentMap.GetGrid(MapToGrid(clickPos)),
                            si.Level));
                        break;
                    case Target.Ray:
                        Skill.CurrentSkill.Activate(new SkillContext(
                            Player.PlayerUnit, 
                            Player.PlayerUnit.RayCheck(Scene.CurrentMap.GetGrid(MapToGrid(clickPos))),
                            si.Level));
                        break;
                    default:
                        Skill.CurrentSkill.Activate(new SkillContext(
                            Player.PlayerUnit, 
                            Scene.CurrentMap.GetGrid(MapToGrid(clickPos)).unit,
                            si.Level));
                        break;
                }
            }
            else
            {
                G.I.Fsm.ChangeState(Fsm.playerSkillState);
            }
            EndTargeting();

        }
    }
    public void Process()
    {
        if (!isTargeting || Skill.CurrentSkill == null) { ClearHighlights(); return; }
        // 获取鼠标的世界坐标
        Vector2 mouseWorldPos = GetGlobalMousePosition();
        Vector2I tileWorldPos = MapToGrid(mouseWorldPos);
        if (showingTileWorldPos == tileWorldPos)
            return;
        showingTileWorldPos = tileWorldPos;
        ShowHighlights(MathEx.GetLine(Player.PlayerUnit.Position, tileWorldPos), Player.PlayerUnit.CheckSkillTarget(Skill.CurrentSkill, tileWorldPos), Skill.CurrentSkill.Targeting.BombRange);
    }
    public static Vector2I MapToGrid(Vector2 screenPosf)
    {
        Vector2I screenPos = new((int)screenPosf.X + 8, (int)screenPosf.Y + 8);
        Vector2I gridPos = screenPos / 16;
        return gridPos;
    }
    public void ShowHighlights(List<Vector2I> tilePositions, HighlightType color, int yellowRange = 0)
    {
        ClearHighlights();
        switch (color)
        {
            case HighlightType.red:
                foreach (var pos in tilePositions)
                {
                    if(Player.PlayerUnit.CheckSkillTarget(Skill.CurrentSkill, pos) == HighlightType.red)
                    {
                        GridHighlight(pos, HighlightType.red);
                    }
                    else
                    {
                        GridHighlight(pos, HighlightType.blue);
                    }
                }
                break;
            case HighlightType.blue:
                foreach (var pos in tilePositions)
                {
                    GridHighlight(pos, HighlightType.blue);
                }
                break;
            case HighlightType.green:
                foreach (var pos in tilePositions.GetRange(0,tilePositions.Count-1))
                {
                    GridHighlight(pos, HighlightType.blue);
                }
                GridHighlight(tilePositions[tilePositions.Count-1], HighlightType.green);
                break;
        }
        if (yellowRange > 0)
        {
            if (!Scene.CurrentMap.CheckGrid(tilePositions[^1]))
                return;
            foreach (var grid in Scene.CurrentMap.GetGrid(tilePositions[^1]).NearGrids(yellowRange))
            {
                GridHighlight(grid.Position, HighlightType.yellow);
            }
        }
    }
    private void GridHighlight(Vector2I pos, HighlightType color)
    {
        if (!Scene.CurrentMap.CheckGrid(pos))
            return;
        Node2D instance = new();
        switch (color)
        {
            case HighlightType.red:
                instance = (Node2D)highlightRed.Instantiate();
                break;
            case HighlightType.green:
                instance = (Node2D)highlightGreen.Instantiate();
                break;
            case HighlightType.blue:
                instance = (Node2D)highlightBlue.Instantiate();
                break;
            case HighlightType.yellow:
                instance = (Node2D)highlightYellow.Instantiate();
                break;
        }

		instance.Position = pos * 16; // 假设 tile 是 16x16
		AddChild(instance);
        activeHighlights.Add(instance);
    }

    public void ClearHighlights()
    {
        foreach (var box in activeHighlights)
            box.QueueFree();

        activeHighlights.Clear();
    }

    public static void StartTargeting(SkillInstance si)
    {
        Skill.CurrentSkill = si;
        isTargeting = true;
        G.I.HighlightManager.Visible = true;
    }

    private static void EndTargeting()
    {
        Skill.CurrentSkill = null;
        isTargeting = false;
        G.I.HighlightManager.Visible = false;
    }

    public void RegisterToG(G g)
    {
        g.HighlightManager = this;
	}
}

