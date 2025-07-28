using Godot;
using System;
using System.Drawing;
using System.Reflection.Emit;
using System.Reflection.Metadata;

public partial class Info : RichTextLabel, IRegisterToG
{
    
    public string bbcodeText = "";
    public static void Print(string text)
    {
        string translated = TextEx.Tr(text);

        // 显示到 Info 面板
        G.I.Info.bbcodeText += translated + "\n";
        G.I.Info.ParseBbcode(G.I.Info.bbcodeText);

        // 控制台原文打印（可保留）
        GD.Print(text);
    }

    

    public void RegisterToG(G g)
    {
        g.Info = this;
    }
}
