using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class ImageEx
{
    public static Texture2D ColorizeTexture(Texture2D original, Color tint)
    {
        if (original == null)
            return null;

        // 拷贝原始图像数据
        Image img = original.GetImage();

        for (int x = 0; x < img.GetWidth(); x++)
        {
            for (int y = 0; y < img.GetHeight(); y++)
            {
                Color c = img.GetPixel(x, y);
                // 用颜色相乘，相当于 modulate
                img.SetPixel(x, y, c * tint);
            }
        }

        // 生成新的贴图
        ImageTexture newTex = ImageTexture.CreateFromImage(img);
        return newTex;
    }
}
