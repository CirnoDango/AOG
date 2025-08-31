using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Scene;
public static class GameTime
{
    public static Queue<Unit> Update()
    {
        float updateTime = 1f;
        if (CurrentMap.Bullets.Count == 0)
            updateTime = 20;
        Queue<Unit> activeUnit = new();
        foreach (Unit u in CurrentMap.WakeUnits)
        {
            if(u.TimeEnergy > -updateTime)
            {
                activeUnit.Enqueue(u);
            }
        }
        //更新unit(time,skill,status,hp,mp),bullet,spellcard
        foreach (Unit u in CurrentMap.WakeUnits)
        {
            u.TimeEnergy += updateTime;
            foreach(SkillInstance skill in u.Us.skills.Select(x => x.skill))
            {
                skill.CurrentCooldown = Math.Max(skill.CurrentCooldown - updateTime, 0);
            }
            foreach(Status status in u.Status.ToList())
            {
                status.Duration -= updateTime;
                if (u == Player.PlayerUnit)
                {
                    status.label.Text = $"{status.Duration / 100:F0}";
                    if (status.Param != -999)
                        status.param.Text = $"{Mathf.Round(status.Param)}";
                }
                if (status.Duration <= 0)
                {
                    status.OnQuit(u);
                }
            }
            u.Ua.HealHp(updateTime * u.Ua.MaxHp / 10000);
            u.Ua.GetMp(updateTime * u.Ua.Mag / 1000);
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
    }
}

public static class GameData
{
    public static string SelectedCharacter = "Reimu";
    public static string SelectedStage = "TutorialStage.tscn";
}


