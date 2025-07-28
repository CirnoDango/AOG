using System;
using System.Collections.Generic;
using System.Text;

public static class TextEx
{
    public static string HighlightChanges(string oldDesc, string newDesc)
    {
        var oldWords = Tokenize(oldDesc);
        var newWords = Tokenize(newDesc);

        var sb = new StringBuilder();
        int i = 0, j = 0;

        while (i < oldWords.Count && j < newWords.Count)
        {
            if (oldWords[i] == newWords[j])
            {
                sb.Append(newWords[j]);
                i++;
                j++;
            }
            else
            {
                // 找到变化起点后，尝试找下一个匹配的稳定点
                int nextOld = i + 1, nextNew = j + 1;
                bool found = false;
                while (nextOld < oldWords.Count && nextNew < newWords.Count)
                {
                    if (oldWords[nextOld] == newWords[nextNew])
                    {
                        found = true;
                        break;
                    }
                    nextOld++;
                    nextNew++;
                }

                if (!found)
                {
                    // 没找到稳定点，认为剩下的全是新内容
                    sb.Append("[color=green]");
                    for (int k = j; k < newWords.Count; k++)
                        sb.Append(newWords[k]);
                    sb.Append("[/color]");
                    return sb.ToString();
                }
                else
                {
                    while (i < nextOld)
                    {
                        sb.Append(oldWords[j]);
                        i++;
                    }
                    sb.Append("→");
                    // 添加变化内容
                    sb.Append("[color=green]");
                    while (j < nextNew)
                    {
                        sb.Append(newWords[j]);
                        j++;
                    }
                    sb.Append("[/color]");
                    i = nextOld;
                    j = nextNew;
                }
            }
        }

        // 处理新增部分
        while (j < newWords.Count)
        {
            sb.Append("[color=green]");
            sb.Append(newWords[j]);
            sb.Append("[/color]");
            j++;
        }

        return sb.ToString();
    }

    private static List<string> Tokenize(string input)
    {
        var list = new List<string>();
        var word = new StringBuilder();
        foreach (char c in input)
        {
            if (char.IsDigit(c) || c == '%')
            {
                word.Append(c);
            }
            else
            {
                if (word.Length > 0)
                {
                    list.Add(word.ToString());
                    word.Clear();
                }
                list.Add(c.ToString()); // keep punctuation as separate tokens
            }
        }
        if (word.Length > 0)
            list.Add(word.ToString());
        return list;
    }
    public static string Tr(string text)
    {
        // 按空格分割
        var parts = text.Split(' ');

        // 对每段做翻译
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i] = G.I.Info.Tr(parts[i]);
        }

        // 用空格拼接回完整字符串
        string translated = string.Join(" ", parts);
        return translated;
    }

    public static string TrN(string text)
    {
        // 按空格分割
        var parts = text.Split(' ');

        // 对每段做翻译
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i] = G.I.Info.Tr(parts[i]);
        }

        // 用空格拼接回完整字符串
        string translated = string.Join("", parts);
        return translated;
    }
}
