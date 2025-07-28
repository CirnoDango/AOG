using Godot;
using System;
using System.ComponentModel;

public partial class SkillPanel : VFlowContainer,IRegisterToG
{
    [Export]
    public Label Point;
    [Export]
    public PackedScene SkillTree;
    [Export]
    public Label info;
    public void Refresh()
    {
        Point.Text = TextEx.TrN($"剩余技能点 :{G.I.Player.SkillPoint}");
    }
    public void Add(string skilltree)
    {
        SkillTree tree = (SkillTree)SkillTree.Instantiate();
        tree.Init(skilltree);
        AddChild(tree);
    }

    public void RegisterToG(G g)
    {
        g.SkillPanel = this;
    }
}
