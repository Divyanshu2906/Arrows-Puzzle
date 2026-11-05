using UnityEngine;

public class ArrowHead : MonoBehaviour
{
    [HideInInspector] public LineController line;
    [HideInInspector] public int endIndex;
    [HideInInspector] public SpriteRenderer sr;
    [HideInInspector] public CircleCollider2D col;

    public void SetActive(bool on)
    {
        col.enabled = on;
        Color c = line.BaseColor;
        c.a = on ? 1f : 0.3f;
        sr.color = c;
    }
}
