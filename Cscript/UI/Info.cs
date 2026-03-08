using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection.Emit;
using System.Reflection.Metadata;

public partial class Info : RichTextLabel, IRegisterToG
{
	private List<string> lines = new();   // 存储所有行
	public string bbcodeText = "";

	public static void Print(string text)
	{
		string translated = TextEx.Tr(text);

		// 存入行
		var info = G.I.Info;
		info.lines.Add(translated);

		// 如果超过 50 行，清掉最前面
		if (info.lines.Count > 50)
		{
			info.lines.RemoveAt(0);
		}

		// 拼接文本
		info.bbcodeText = string.Join("\n", info.lines);

		// 显示到 Info 面板
		info.ParseBbcode(info.bbcodeText);

		// 控制台原文打印（可保留）
		GD.Print(text);
	}

	public void RegisterToG(G g)
	{
		g.Info = this;
	}
}
