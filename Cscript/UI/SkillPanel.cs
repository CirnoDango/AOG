using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;

public partial class SkillPanel : VFlowContainer, IRegisterToG
{
    [Export]
    public Label Point;
    [Export]
    public PackedScene SkillTree;
    [Export]
    public Label info;
    public List<SkillTree> skillTrees = [];
    public void Refresh()
    {
        Point.Text = TextEx.TrN($"剩余技能点 :{G.I.Player.SkillPoint}");
        foreach (var skillTree in skillTrees)
        {
            skillTree.UpdateTree();
        }
    }
    public void Add(string skilltree)
    {
        SkillTree tree = (SkillTree)SkillTree.Instantiate();
        tree.Init(skilltree);
        skillTrees.Add(tree);
        AddChild(tree);
    }

    public void RegisterToG(G g)
    {
        g.SkillPanel = this;
    }
}
