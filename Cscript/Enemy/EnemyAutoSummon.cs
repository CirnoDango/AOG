using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class EnemyAutoSummon
{
    
    public static void Update(int time)
    {
        for(int i = 0; i<time; i++)
        {
            Grid g;
            do
            {
                int x = GD.RandRange(0, Scene.CurrentMap.Width-1);
                int y = GD.RandRange(0, Scene.CurrentMap.Height-1);
                g = Scene.CurrentMap.Grid[x, y];
            }
            while (g == null || !g.IsWalkable || g.unit != null);
            float type = GD.Randf();
            if (type < 0.6)
                Scene.CurrentMap.CreateEnemy(g.Position, "dangoPea");
            else if (type < 1)
                Scene.CurrentMap.CreateEnemy(g.Position, "dangoWater");
            else
                Scene.CurrentMap.CreateEnemy(g.Position, "dangoFist");
        }
    }
}
