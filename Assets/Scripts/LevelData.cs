using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LineData
{
    public Color color = Color.white;
    public Vector2[] points;   
}

[CreateAssetMenu(menuName = "Arrows/Level")]
public class LevelData : ScriptableObject
{
    public int lives = 3;
    public List<LineData> lines = new List<LineData>();
}
