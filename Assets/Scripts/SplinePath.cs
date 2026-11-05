using System.Collections.Generic;
using UnityEngine;

public class SplinePath
{
    readonly List<Vector2> pts = new List<Vector2>();
    readonly List<float> cum = new List<float>();
    public float Length { get; private set; }

    public SplinePath(Vector2[] ctrl, int samplesPerSegment = 16)
    {
        var p = new List<Vector2>(ctrl);
        p.Insert(0, ctrl[0] * 2f - ctrl[1]);
        p.Add(ctrl[ctrl.Length - 1] * 2f - ctrl[ctrl.Length - 2]);

        for (int i = 1; i < p.Count - 2; i++)
            for (int s = 0; s < samplesPerSegment; s++)
                pts.Add(CatmullRom(p[i - 1], p[i], p[i + 1], p[i + 2], s / (float)samplesPerSegment));
        pts.Add(ctrl[ctrl.Length - 1]);

        cum.Add(0f);
        for (int i = 1; i < pts.Count; i++)
            cum.Add(cum[i - 1] + Vector2.Distance(pts[i - 1], pts[i]));
        Length = cum[cum.Count - 1];
    }

    static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float t2 = t * t, t3 = t2 * t;
        return 0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }

    public Vector2 GetPoint(float d)
    {
        d = Mathf.Clamp(d, 0f, Length);
        int i = cum.BinarySearch(d);
        if (i < 0) i = ~i;
        if (i <= 0) return pts[0];
        if (i >= pts.Count) return pts[pts.Count - 1];
        float seg = cum[i] - cum[i - 1];
        float t = seg > 0.00001f ? (d - cum[i - 1]) / seg : 0f;
        return Vector2.Lerp(pts[i - 1], pts[i], t);
    }

    public Vector2 GetDir(float d)
    {
        Vector2 v = GetPoint(d + 0.05f) - GetPoint(d - 0.05f);
        return v.sqrMagnitude < 1e-8f ? Vector2.up : v.normalized;
    }

    public void GetRange(float a, float b, List<Vector3> output)
    {
        output.Clear();
        output.Add(GetPoint(a));
        for (int i = 0; i < pts.Count; i++)
            if (cum[i] > a && cum[i] < b) output.Add(pts[i]);
        output.Add(GetPoint(b));
    }
}
