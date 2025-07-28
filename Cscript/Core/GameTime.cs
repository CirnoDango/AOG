using Godot;
using System;
using System.Linq;
using static Scene;
public static class GameTime
{
    public static float timePerFrame = 100;
    public static float totalTime = 0;
    public static int TotalTurn { get => (int)(totalTime / 100); }
    private static int _currentTurn = 0;
    public static Unit Update(Unit unit, float time)
    {
        //确定更新时间
        if (CurrentMap.Bullets.Count > 0 || SpellCard.currentSpellcards.Count > 0)
        {
            timePerFrame = 1.2f;
        }
        else
        {
            timePerFrame = 100;
        }
        if (unit != null)
            unit.TimeEnergy -= time;
        float updateTime = timePerFrame;
        Unit activeUnit = null;
        foreach (Unit u in CurrentMap.WakeUnits)
        {
            if(-u.TimeEnergy < updateTime)
            {
                updateTime = -u.TimeEnergy;
                activeUnit = u;
            }
        }
        //打印确定好的更新时间
        totalTime += updateTime;
        if (TotalTurn > _currentTurn)
        {
            _currentTurn = TotalTurn;
            //GD.Print($"======Turn {TotalTurn}======");
        }
        //更新unit(time,skill,status,mp),bullet,spellcard
        foreach (Unit u in CurrentMap.WakeUnits)
        {
            u.TimeEnergy += updateTime;
            foreach(SkillInstance skill in u.skills.Select(x => x.skill))
            {
                skill.CurrentCooldown = Math.Max(skill.CurrentCooldown - updateTime, 0);
            }
            foreach(Status status in u.Status.ToList())
            {
                status.Duration -= updateTime;
                if(u == Player.PlayerUnit)
                {
                    G.I.PlayerStatusBar.StatusImages[status].GetChildren().OfType<Label>().FirstOrDefault()
                        .Text = $"{status.Duration / 100:F0}";
                }
                if (status.Duration <= 0)
                {
                    status.OnQuit(u);
                }
            }
            u.CurrentMp = Math.Min(u.CurrentMp + updateTime * u.Ua.Mag/1000, u.MaxMp);
        }
        foreach (Bullet b in CurrentMap.Bullets.ToList())
        {
            b.Update(updateTime);
        }
        
        foreach (SkillInstance si in SpellCard.currentSpellcards.ToList())
        {
            si.Update(new SkillContext(si.User), updateTime);
        }
        return activeUnit;
    }
    public static void Reset()
    {
        totalTime = 0;
        _currentTurn = 0;
    }
}

public static class GameData
{
    public static string SelectedCharacter = "Reimu";
    public static string SelectedStage = "TutorialStage.tscn";
}


