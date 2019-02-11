using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public static class LoadResources
{
    public static Dictionary<string, Texture2D> textureLibrary = new Dictionary<string, Texture2D>();

    // Start is called before the first frame update
    public static Texture2D GetTexture(string name)
    {
        if (!textureLibrary.ContainsKey(name))
        {
            var tex = LoadTexture(name);
            if(!tex)
                return null;
            textureLibrary[name] = tex;
        }
        if (!textureLibrary[name])
        {
            var tex = LoadTexture(name);
            if (!tex)
                return null;
            textureLibrary[name] = tex;
        }
        return textureLibrary[name];
    }

    public static Texture2D GetTexture(string name, Color color)
    {
        var key = name + color.GetHashCode();
        if (!textureLibrary.ContainsKey(key))
        {
            var tex = GetTexture(name);
            if (!tex)
                return null;
            textureLibrary[key] = PaintColor(tex,color);
        }
        if (!textureLibrary[key])
        {
            var tex = GetTexture(name);
            if (!tex)
                return null;
            textureLibrary[key] = PaintColor(tex, color);
        }
        return textureLibrary[key];
    }

    // Update is called once per frame
    public static Texture2D LoadTexture(string name)
    {
        return Resources.Load<Texture2D>(name);
    }

    public static GUIStyle GetNormalStyle(string name)
    {
        var style = new GUIStyle();
        style.normal.background = GetTexture(name);
        style.border = new RectOffset(32, 32, 32, 32);
        style.padding = new RectOffset(16, 16, 4, 16);
        return style;
    }
    public static GUIStyle GetHiglightedStyle(string name)
    {
        var style = new GUIStyle();
        style.normal.background = GetTexture(name);
        style.border = new RectOffset(32, 32, 32, 32);
        return style;
    }

    private static Texture2D PaintColor(Texture2D tex, Color color)
    {
        int pixCount = tex.width * tex.height;

        var tintedTex = new Texture2D(tex.width, tex.height);
        tintedTex.alphaIsTransparency = true;

        var newPixels = new Color[pixCount];
        var pixels = tex.GetPixels();

        for (int i = 0; i < pixCount; ++i)
        {
            newPixels[i] = color;
            newPixels[i].a = pixels[i].a;
        }

        tintedTex.SetPixels(newPixels);
        tintedTex.Apply();

        return tintedTex;
    }
}
