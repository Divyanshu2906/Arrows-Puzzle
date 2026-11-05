using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    Image[] hearts;
    GameObject popup;
    Text popupText;
    Font font;

    public void Build(int lives, Action onRetry, Action onHint)
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var sc = gameObject.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1080, 1920);
        sc.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        hearts = new Image[lives];
        for (int i = 0; i < lives; i++)
        {
            var rt = NewRect("Heart" + i, transform, new Vector2(0.5f, 1f), new Vector2((i - (lives - 1) / 2f) * 120f, -110f), new Vector2(100, 100));
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = SpriteFactory.Heart();
            hearts[i] = img;
        }
        SetLives(lives);

        MakeButton("HintButton", transform, new Vector2(0.5f, 0f), new Vector2(0, 120), new Vector2(360, 120), "HINT", new Color(0.2f, 0.5f, 0.95f), onHint);

        popup = new GameObject("Popup", typeof(RectTransform), typeof(Image));
        popup.transform.SetParent(transform, false);
        var prt = popup.GetComponent<RectTransform>();
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
        prt.offsetMin = prt.offsetMax = Vector2.zero;
        popup.GetComponent<Image>().color = new Color(0, 0, 0, 0.75f);
        popupText = MakeText("Title", popup.transform, new Vector2(0.5f, 0.5f), new Vector2(0, 150), new Vector2(900, 200), "", 120, Color.white);
        MakeButton("RetryButton", popup.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -100), new Vector2(420, 140), "RETRY", new Color(0.2f, 0.7f, 0.3f), onRetry);
        popup.SetActive(false);
    }

    public void SetLives(int n)
    {
        for (int i = 0; i < hearts.Length; i++)
            hearts[i].color = i < n ? Color.red : new Color(0.6f, 0.6f, 0.6f, 0.5f);
    }

    public void ShowPopup(string msg)
    {
        popupText.text = msg;
        popup.SetActive(true);
    }

    static RectTransform NewRect(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    Text MakeText(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, string s, int fontSize, Color c)
    {
        var rt = NewRect(name, parent, anchor, pos, size);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = font; t.text = s; t.fontSize = fontSize; t.color = c;
        t.alignment = TextAnchor.MiddleCenter;
        return t;
    }

    void MakeButton(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, string label, Color col, Action onClick)
    {
        var rt = NewRect(name, parent, anchor, pos, size);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = col;
        var b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        b.onClick.AddListener(() => onClick());
        MakeText("Label", rt, new Vector2(0.5f, 0.5f), Vector2.zero, size, label, 56, Color.white);
    }
}
