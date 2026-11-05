using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LineState { Idle, Moving, Done }

public class LineController : MonoBehaviour
{
    public LineState State { get; private set; } = LineState.Idle;
    public SplinePath Path { get; private set; }
    public int MovingEnd { get; private set; } = -1;
    public float Dist { get; private set; }
    public Color BaseColor { get; private set; }
    public event Action<LineController> Completed;

    float speed, width, arrowSize;
    LineRenderer lr;
    readonly ArrowHead[] heads = new ArrowHead[2];
    readonly List<Vector3> buf = new List<Vector3>();
    Coroutine hintRoutine, flashRoutine;

    public Vector2 HeadPosition(int end) => heads[end].transform.position;
    public Vector2 PointFrom(int end, float d) => end == 0 ? Path.GetPoint(d) : Path.GetPoint(Path.Length - d);

    public void Init(LineData data, float speed, float width, float arrowSize)
    {
        this.speed = speed; this.width = width; this.arrowSize = arrowSize;
        BaseColor = data.color;
        Path = new SplinePath(data.points);

        lr = gameObject.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = BaseColor;
        lr.widthMultiplier = width;
        lr.numCapVertices = 4;
        lr.numCornerVertices = 4;
        lr.sortingOrder = 0;

        for (int i = 0; i < 2; i++)
        {
            var go = new GameObject("Head" + i);
            go.transform.SetParent(transform);
            go.transform.localScale = Vector3.one * arrowSize;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Triangle();
            sr.sortingOrder = 2;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.8f;
            var h = go.AddComponent<ArrowHead>();
            h.line = this; h.endIndex = i; h.sr = sr; h.col = col;
            heads[i] = h;
        }
        ResetLine(false);
    }

    void SetHead(int i, float d)
    {
        var t = heads[i].transform;
        t.position = PointFrom(i, d);
        Vector2 dir = Path.GetDir(i == 0 ? d : Path.Length - d) * (i == 0 ? 1f : -1f);
        t.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
    }

    void Draw()
    {
        float a = 0f, b = Path.Length;
        if (State == LineState.Moving)
        {
            if (MovingEnd == 0) a = Dist; else b = Path.Length - Dist;
        }
        Path.GetRange(a, b, buf);
        lr.positionCount = buf.Count;
        for (int i = 0; i < buf.Count; i++) lr.SetPosition(i, buf[i]);
    }

    void Update()
    {
        if (State != LineState.Moving) return;
        Dist += speed * Time.deltaTime;
        if (Dist >= Path.Length) { Complete(); return; }
        SetHead(MovingEnd, Dist);
        Draw();
    }

    public void StartMove(int end)
    {
        if (State != LineState.Idle) return;
        StopHint();
        if (flashRoutine != null) { StopCoroutine(flashRoutine); flashRoutine = null; }
        State = LineState.Moving; MovingEnd = end; Dist = 0f;
        heads[end].SetActive(true);
        heads[1 - end].SetActive(false); 
    }

    void Complete()
    {
        State = LineState.Done;
        StopHint();
        lr.enabled = false;
        SetHead(MovingEnd, Path.Length);
        foreach (var h in heads) h.col.enabled = false;
        Completed?.Invoke(this);
        StartCoroutine(Pop());
    }

    IEnumerator Pop()
    {
        float t = 0f; const float T = 0.25f;
        while (t < T)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / T);
            float sc = k < 0.4f ? 1f + k : 1.4f * (1f - (k - 0.4f) / 0.6f);
            foreach (var h in heads) h.transform.localScale = Vector3.one * arrowSize * Mathf.Max(0f, sc);
            yield return null;
        }
        Destroy(gameObject);
    }

    public void ResetLine(bool flash = true)
    {
        StopHint();
        State = LineState.Idle; MovingEnd = -1; Dist = 0f;
        for (int i = 0; i < 2; i++) { heads[i].SetActive(true); SetHead(i, 0f); }
        Draw();
        if (flash) { if (flashRoutine != null) StopCoroutine(flashRoutine); flashRoutine = StartCoroutine(Flash()); }
    }

    IEnumerator Flash()
    {
        float t = 0f;
        while (t < 0.4f)
        {
            t += Time.unscaledDeltaTime;
            bool on = ((int)(t / 0.08f)) % 2 == 0;
            foreach (var h in heads) h.sr.color = on ? Color.white : BaseColor;
            yield return null;
        }
        foreach (var h in heads) h.SetActive(true);
        flashRoutine = null;
    }


    public void ShowHint(List<int> ends)
    {
        StopHint();
        hintRoutine = StartCoroutine(Hint(ends));
    }

    IEnumerator Hint(List<int> ends)
    {
        float t = 0f;
        while (true)
        {
            t += Time.deltaTime;
            float p = (Mathf.Sin(t * 10f) + 1f) * 0.5f;
            lr.widthMultiplier = width * (1f + 1.2f * p);
            foreach (int e in ends) heads[e].transform.localScale = Vector3.one * arrowSize * (1f + 0.6f * p);
            yield return null;
        }
    }

    public void StopHint()
    {
        if (hintRoutine != null) { StopCoroutine(hintRoutine); hintRoutine = null; }
        if (lr) lr.widthMultiplier = width;
        for (int i = 0; i < 2; i++) if (heads[i]) heads[i].transform.localScale = Vector3.one * arrowSize;
    }
}
