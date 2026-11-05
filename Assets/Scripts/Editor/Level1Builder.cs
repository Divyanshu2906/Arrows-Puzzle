using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public static class Level1Builder
{
    [MenuItem("Arrows/Create Level 1")]
    public static void Create()
    {
        var lvl = ScriptableObject.CreateInstance<LevelData>();
        lvl.lives = 3;
        lvl.lines = new List<LineData>();

        Color pink = new Color32(245, 95, 225, 255);
        Color blue = new Color32(30, 123, 240, 255);
        Color green = new Color32(43, 214, 43, 255);
        Color red = new Color32(240, 16, 16, 255);
        Color orange = new Color32(210, 120, 77, 255);
        Color purple = new Color32(138, 21, 238, 255);

        Add(lvl, pink, 118,123, 100,100, 95,68, 112,48, 142,47, 165,62, 163,92, 140,118);
        Add(lvl, pink, 97,150, 68,153, 57,178, 72,198, 98,198, 120,170);
        Add(lvl, pink, 155,146, 195,140, 228,158, 220,190, 195,210, 168,192, 148,170);
        Add(lvl, blue, 138,205, 137,245, 148,285, 165,335, 200,375, 237,410);
        Add(lvl, green, 165,238, 172,208, 205,190, 237,186, 225,235, 212,266, 170,260);
        Add(lvl, red, 140,258, 165,252, 195,230);
        Add(lvl, blue, 247,322, 242,290, 255,240, 300,210, 340,218, 358,255, 354,300, 335,342);
        Add(lvl, orange, 332,298, 355,277, 400,265, 435,280, 442,312);
        Add(lvl, green, 440,336, 425,380, 385,418, 350,432);
        Add(lvl, purple, 345,433, 300,455, 245,483);
        Add(lvl, orange, 242,420, 262,450, 272,485);
        Add(lvl, red, 225,488, 218,512, 242,522, 265,508);
        Add(lvl, green, 130,325, 95,325, 80,360, 80,410, 130,392, 150,362, 140,345);
        Add(lvl, red, 112,360, 130,340, 163,320);

        if (!AssetDatabase.IsValidFolder("Assets/Levels")) AssetDatabase.CreateFolder("Assets", "Levels");
        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Levels/Level1.asset");
        AssetDatabase.CreateAsset(lvl, path);
        AssetDatabase.SaveAssets();
        Selection.activeObject = lvl;
    }

    static void Add(LevelData l, Color c, params float[] xy)
    {
        var pts = new Vector2[xy.Length / 2];
        for (int i = 0; i < pts.Length; i++)
            pts[i] = new Vector2((xy[2 * i] - 249f) / 50f, (283f - xy[2 * i + 1]) / 50f);
        l.lines.Add(new LineData { color = c, points = pts });
    }
}
