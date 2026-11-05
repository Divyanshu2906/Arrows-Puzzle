using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Levels (drag LevelData assets here)")]
    public LevelData[] levels;
    public int levelIndex = 0;

    [Header("Tuning")]
    public float arrowSpeed = 4f;
    public float lineWidth = 0.09f;
    public float arrowSize = 0.4f;
    public float hitDistance = 0.35f;

    readonly List<LineController> lines = new List<LineController>();
    Camera cam;
    GameUI ui;
    Bounds bounds;
    Vector3 camBase;
    int lives, remaining;
    bool over, shaking;

    void Start()
    {
        Time.timeScale = 1f;
        cam = Camera.main;
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.white;

        var lvl = levels[levelIndex];
        lives = lvl.lives;
        bool first = true;
        foreach (var d in lvl.lines)
        {
            var go = new GameObject("Line");
            go.transform.SetParent(transform);
            var lc = go.AddComponent<LineController>();
            lc.Init(d, arrowSpeed, lineWidth, arrowSize);
            lc.Completed += OnLineCompleted;
            lines.Add(lc);
            foreach (var p in d.points)
            {
                if (first) { bounds = new Bounds((Vector3)p, Vector3.zero); first = false; }
                else bounds.Encapsulate((Vector3)p);
            }
        }
        remaining = lines.Count;

        ui = new GameObject("UI").AddComponent<GameUI>();
        ui.Build(lives, Retry, ShowHint);
        FitCamera();
    }

    void FitCamera()
    {
        
        float halfH = bounds.size.y * 0.5f / 0.76f;
        float halfW = (bounds.size.x + 1f) * 0.5f / cam.aspect;
        cam.orthographicSize = Mathf.Max(halfH, halfW);
        camBase = new Vector3(bounds.center.x, bounds.center.y, -10f);
        if (!shaking) cam.transform.position = camBase;
    }

    void Update()
    {
        if (over) return;
        FitCamera();
        if (Input.GetMouseButtonDown(0)) HandleTap();
        CheckCollisions();
    }

    void HandleTap()
    {
        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;
        Vector2 p = cam.ScreenToWorldPoint(Input.mousePosition);
        Physics2D.SyncTransforms();
        ArrowHead best = null; float bd = float.MaxValue;
        foreach (var c in Physics2D.OverlapCircleAll(p, 0.15f))
        {
            var h = c.GetComponent<ArrowHead>();
            if (h == null || h.line.State != LineState.Idle) continue;
            float d = Vector2.Distance(p, h.transform.position);
            if (d < bd) { bd = d; best = h; }
        }
        if (best != null)
        {
            foreach (var l in lines) l.StopHint();
            best.line.StartMove(best.endIndex);
        }
    }

    void CheckCollisions()
    {
        for (int i = 0; i < lines.Count; i++)
            for (int j = i + 1; j < lines.Count; j++)
            {
                var a = lines[i]; var b = lines[j];
                if (a.State != LineState.Moving || b.State != LineState.Moving) continue;
                if (Vector2.Distance(a.HeadPosition(a.MovingEnd), b.HeadPosition(b.MovingEnd)) < hitDistance)
                {
                    a.ResetLine(); b.ResetLine();
                    LoseLife();
                    if (over) return;
                }
            }
    }

    void LoseLife()
    {
        lives--;
        ui.SetLives(lives);
        StartCoroutine(Shake());
        if (lives <= 0)
        {
            over = true;
            Time.timeScale = 0f;
            ui.ShowPopup("You Lost");
        }
    }

    IEnumerator Shake()
    {
        shaking = true;
        float t = 0f;
        while (t < 0.25f)
        {
            t += Time.unscaledDeltaTime;
            cam.transform.position = camBase + (Vector3)(Random.insideUnitCircle * 0.15f);
            yield return null;
        }
        shaking = false;
        cam.transform.position = camBase;
    }

    void OnLineCompleted(LineController l)
    {
        lines.Remove(l);
        remaining--;
        if (remaining <= 0) { over = true; StartCoroutine(WinRoutine()); }
    }

    IEnumerator WinRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        ui.ShowPopup("You Won");
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ShowHint()
    {
        if (over) return;
        foreach (var l in lines) l.StopHint();

        var pool = new List<(LineController line, List<int> ends)>();
        foreach (var l in lines)
        {
            if (l.State != LineState.Idle) continue;
            var ends = new List<int>();
            for (int e = 0; e < 2; e++) if (IsLegal(l, e)) ends.Add(e);
            if (ends.Count > 0) pool.Add((l, ends));
        }
        if (pool.Count == 0) return;
        var pick = pool[Random.Range(0, pool.Count)];
        pick.line.ShowHint(pick.ends);
    }

    bool IsLegal(LineController c, int end)
    {
        float T = c.Path.Length / arrowSpeed;
        for (float t = 0f; t <= T; t += 0.02f)
        {
            Vector2 p = c.PointFrom(end, t * arrowSpeed);
            foreach (var m in lines)
            {
                if (m == c || m.State != LineState.Moving) continue;
                float md = m.Dist + t * arrowSpeed;
                if (md >= m.Path.Length) continue;
                if (Vector2.Distance(p, m.PointFrom(m.MovingEnd, md)) < hitDistance) return false;
            }
        }
        return true;
    }
}
