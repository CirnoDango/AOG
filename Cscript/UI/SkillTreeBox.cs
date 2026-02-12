using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class SkillTreeBox : Control, IRegisterToG
{
    [Export]
    public PackedScene st;
    [Export]
    public Label pointText;

    public List<Hex> hexs = [];
    public int map_stage = 0;
    public int hmx = -1;
    public int hnx = 0;
    public int hmy = -1;
    public int hny = 0;
    public int hmz = -1;
    public int hnz = 0;

    public override void _Ready()
    {
        SummonHexs(4, -4, 4, -4, 4, -4);
        SetHex(2, 1, "YinyangBall");
        SetHex(0, 0, "Dark");
        SetHex(3, -2, "Fist");
        SetHex(-1, 4, "Freeze");
        SetHex(-2, 2, "Star");
        SetHex(-3, 4, "FireElement");
        SetHex(-3, -1, "Time");
        SetHex(-1, -1, "Blood");
    }

    public void SummonHex(int x, int y)
    {
        Hex h = new()
        {
            x = x,
            y = y,
            z = 0 - x - y,
            ste = st.Instantiate<SkillTreeEntry>()
        };
        h.ste.Position = new Vector2(1500/2 * x + 9600, 1730/2 * y + 867f/2 * x + 5400);
        AddChild(h.ste);
        hexs.Add(h);
    }
    public void SummonHexs(int mx, int nx, int my, int ny, int mz, int nz)
    {
        for (int ix = nx; ix < mx + 1; ix++)
        {
            for (int iy = ny; iy < my + 1; iy++)
            {
                int iz = -ix - iy;
                if (iz >= nz && iz <= mz)
                {
                    SummonHex(ix, iy);
                    SetHex(ix, iy);
                }
            }
        }
        hmx = mx;
        hnx = nx;
        hmy = my;
        hny = ny;
        hmz = mz;
        hnz = nz;
    }
    public void SetHex(int x, int y, string skillTreeName = "")
    {
        Hex h = hexs.FirstOrDefault(h => h.x == x && h.y == y);
        if (skillTreeName != "")
            h.ste.SetImage(skillTreeName);
        h.skillTreeName = skillTreeName;
        h.ste.image.Pressed += () =>
        {
            if (h.hexStatus == HexStatus.learnt && G.I.Player.TalentPoint > 0)
            {
                G.I.Player.TalentPoint--;
                Expert(h);
            }
            Refresh();
        };
    }
    public void Expert(int x, int y)
    {
        Hex h = hexs.FirstOrDefault(h => h.x == x && h.y == y);
        if (h != null)
            Expert(h);
    }
    public void Expert(Hex h)
    {
        foreach (Hex hex in h.Distance(hexs, 1))
        {
            if (hex.hexStatus == HexStatus.unlearnt)
            {
                hex.hexStatus = HexStatus.learnt;
                if (hex.skillTreeName != null && hex.skillTreeName != "")
                {
                    G.I.SkillPanel.Add(hex.skillTreeName);
                    Player.skillTrees.Add(hex.skillTreeName, HexStatus.learnt);
                }
            }
        }
        h.hexStatus = HexStatus.expert;
        Player.skillTrees[h.skillTreeName] = HexStatus.expert;
    }
    public void Expert(string name)
    {
        G.I.SkillPanel.Add(name);
        Hex h = hexs.FirstOrDefault(h => h.skillTreeName == name);
        if (h != null)
            Expert(h);
    }
    public void Refresh()
    {
        foreach (Hex hex in hexs)
        {
            switch (hex.hexStatus)
            {
                case HexStatus.unlearnt:
                    hex.ste.background.Modulate = Colors.DarkGray; break;
                case HexStatus.learnt:
                    hex.ste.background.Modulate = Colors.CadetBlue; break;
                case HexStatus.expert:
                    hex.ste.background.Modulate = Colors.Gold; break;
            }
        }
        if (G.I.Fsm.currentState != Fsm.SkillTreeState) return;
        pointText.Text = $"剩余天赋点:{G.I.Player.TalentPoint}";
    }
    public void RegisterToG(G g)
    {
        g.SkillTreeBox = this;
    }
}
public class Hex
{
    public int x;
    public int y;
    public int z;
    public string skillTreeName;
    public HexStatus hexStatus = HexStatus.unlearnt;
    public SkillTreeEntry ste;
    
    public List<Hex> Hexs(List<Hex> hexs, int mx, int nx, int my, int ny, int mz, int nz)
    {
        List<Hex> listhex = [];
        foreach (Hex h in hexs)
        {
            if ((h.x - x) >= nx && (h.y - y) >= ny && (h.z - z) >= nz && (h.x - x) <= mx && (h.y - y) <= my && (h.z - z) <= mz)
            {
                listhex.Add(h);
            }
        }
        return listhex;
    }
    public List<Hex> Distance(List<Hex> hexs, params int[] d)
    {
        List<Hex> listhex = [];
        foreach (int dd in d)
        {
            foreach (Hex h in hexs)
            {
                int dis = Mathf.Max(Mathf.Max(Mathf.Abs(h.x - x), Mathf.Abs(h.y - y)), Mathf.Abs(h.z - z));
                if (dis == dd)
                {
                    listhex.Add(h);
                }
            }
        }
        return listhex;
    }
}
public enum HexStatus
{
    unlearnt, learnt, expert
}