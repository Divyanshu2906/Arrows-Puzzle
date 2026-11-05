using UnityEngine;

public static class SpriteFactory
{
    static Sprite tri, heart;
    const int N = 64;

    public static Sprite Triangle()
    {
        if (tri) return tri;
        var px = new Color32[N * N];
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float half = (60f - y) / 56f * 30f;
                bool inside = y >= 4 && y <= 60 && Mathf.Abs(x - 31.5f) <= half;
                px[y * N + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
            }
        return tri = Make(px);
    }

    public static Sprite Heart()
    {
        if (heart) return heart;
        var px = new Color32[N * N];
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float u = (x - 31.5f) / 24f, v = (y - 28f) / 24f;
                float a = u * u + v * v - 1f;
                bool inside = a * a * a - u * u * v * v * v <= 0f;
                px[y * N + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
            }
        return heart = Make(px);
    }

    static Sprite Make(Color32[] px)
    {
        var tex = new Texture2D(N, N, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), N);
    }
}
