using Godot;
using System;
public interface IRegisterToG
{
    void RegisterToG(G g);
}
public partial class G : Node
{
    public static G I;
    public Fsm Fsm { get; set; }
    public InventoryBox InventoryBox;
    public MemoryBox MemoryBox;
    public TileMapAllLayer TileMapAllLayer { get;  set; }
    public BarrageBox BarrageBox {  get; set; }
    public Cave Cave{ get;  set;}
    public DialogBox DialogBox{ get;  set;}
    public HighlightManager HighlightManager{ get;  set;}
    public Info Info{ get;  set;}
    public LayerItemDropped LayerItemDropped{ get;  set;}
    public Player Player{ get;  set;}
    public PlayerStatusBar PlayerStatusBar{ get;  set;}
    public SkillBar SkillBar{ get;  set;}
    public SkillPanel SkillPanel { get; set; }
    public Ua Ua { get; set; }
    public SkillTreeBox SkillTreeBox { get; set; }
    public SpellcardBox SpellcardBox { get; set; }
    public AnimationManager AnimationManager { get; set; }
    public Mainmenu Mainmenu { get; set; }
    public override void _Ready()
    {
        I = this;
        TranslationServer.SetLocale("en");
        // 先处理当前已经存在的节点
        foreach (var node in GetTree().Root.GetChildren())
            CheckAndRegisterRecursively(node);

        // 再监听后面新添加的节点
        GetTree().NodeAdded += OnNodeAdded;
    }
    private void CheckAndRegisterRecursively(Node node)
    {
        if (node is IRegisterToG reg)
            reg.RegisterToG(this);

        foreach (var child in node.GetChildren())
            CheckAndRegisterRecursively(child);
    }
    private void OnNodeAdded(Node node)
    {
        if (node is IRegisterToG reg)
            reg.RegisterToG(this);
    }
    public void Reset()
    {
        Fsm = null;
        InventoryBox = null;
        MemoryBox = null;
        TileMapAllLayer = null;
        BarrageBox = null;
        Cave = null;
        DialogBox = null;
        HighlightManager = null;
        Info = null;
        LayerItemDropped = null;
        Player = null;
        PlayerStatusBar = null;
        SkillBar = null;
        SkillPanel = null;
        Ua = null;
        SkillTreeBox = null;
        AnimationManager = null;
        Mainmenu = null;
        SpellcardBox = null;
        //Scene.CurrentMap = null;
        GameTime.Reset();
    }
}
