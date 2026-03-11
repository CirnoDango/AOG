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
    [Export]
    public Label unitInfo;
    private List<Node2D> activeHighlights = [];

    private static bool isTargeting = false;
    public Vector2I showingTileWorldPos;
    private Vector2I lastGrid = new(-999, -999);
    public override void _Input(InputEvent @event)
    {
        if (!isTargeting)
            return;

        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            Vector2 clickPos = GetGlobalMousePosition();
            var si = Skill.CurrentSkill;
            if (Player.PlayerUnit.Up.CheckSkillTarget(si, MapToGrid(clickPos)) == HighlightType.green)
            {
                SkillContext sc = si.GetTargeting().TargetRule.GetSc(
                    Player.PlayerUnit, Scene.CurrentMap.GetGrid(MapToGrid(clickPos)));
                sc.Level = si.Level;
                si.Activate(sc);
            }
            else
            {
                G.I.Fsm.ChangeState(Fsm.PlayerSkillState);
            }
            EndTargeting();

        }
    }
    public void InfoProcess()
    {
        Vector2 mouseWorldPos = GetGlobalMousePosition();
        Vector2I tileWorldPos = MapToGrid(mouseWorldPos);
        if (tileWorldPos != lastGrid)
        {
            OnMouseExit();
            OnMouseEnter(tileWorldPos);
            lastGrid = tileWorldPos;
        }
    }
    public void Process()
    {
        if (!isTargeting || Skill.CurrentSkill == null) { ClearHighlights(); return; }
        Vector2 mouseWorldPos = GetGlobalMousePosition();
        Vector2I tileWorldPos = MapToGrid(mouseWorldPos);
        // 获取鼠标的世界坐标

        if (showingTileWorldPos == tileWorldPos)
            return;
        showingTileWorldPos = tileWorldPos;
        ShowHighlights(MathEx.GetLine(Player.PlayerUnit.Up.Position, tileWorldPos), 
            Player.PlayerUnit.Up.CheckSkillTarget(Skill.CurrentSkill, tileWorldPos), 
            Skill.CurrentSkill.GetTargeting().BombRange);
    }
    public static Vector2I MapToGrid(Vector2 screenPosf)
    {
        Vector2I screenPos = new((int)(screenPosf.X + 8*Setting.rootnodeScale), (int)(screenPosf.Y + 8 * Setting.rootnodeScale));
        Vector2I gridPos = screenPos / (int)(16 * Setting.rootnodeScale);
        return gridPos;
    }
    void OnMouseEnter(Vector2I grid)
    {
        if(Scene.CurrentMap.GetGrid(grid) != null && Scene.CurrentMap.GetGrid(grid).unit != null)
            unitInfo.Text = Scene.CurrentMap.GetGrid(grid).unit.Ua.Description();
        else
            unitInfo.Text = "";
    }

    void OnMouseExit()
    {
        unitInfo.Text = "";
    }
    public void ShowHighlights(List<Vector2I> tilePositions, HighlightType color, int yellowRange = 0)
    {
        ClearHighlights();
        switch (color)
        {
            case HighlightType.red:
                foreach (var pos in tilePositions)
                {
                    if(Player.PlayerUnit.Up.CheckSkillTarget(Skill.CurrentSkill, pos) == HighlightType.red)
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

		instance.Position = pos * Setting.imagePx;
		AddChild(instance);
        activeHighlights.Add(instance);
    }

    public void ClearHighlights()
    {
        foreach (var box in activeHighlights)
            box.QueueFree();

        activeHighlights.Clear();
    }

    public static void StartTargeting(Skill si)
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

