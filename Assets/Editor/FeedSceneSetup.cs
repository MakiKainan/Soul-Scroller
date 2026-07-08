using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class FeedSceneSetup
{
    private const float CardHeight = 1920f;
    private const int CardCount = 50;

    private const string ScenePath = "Assets/Scenes/SampleScene.unity";

    [MenuItem("Tools/Setup Feed Scene")]
    public static void Setup()
    {
        EditorSceneManager.OpenScene(ScenePath);

        EnsureEventSystem();

        var gameManagerGO = new GameObject("GameManager");
        var gameManager = gameManagerGO.AddComponent<GameManager>();

        var canvasGO = new GameObject("FeedCanvas", typeof(RectTransform));
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasGO.AddComponent<GraphicRaycaster>();

        RectTransform content = BuildFeed(canvasGO.transform, out GameObject feedGO);
        GameObject cardPrefab = BuildCardPrefab();

        var spawner = content.gameObject.AddComponent<FeedSpawner>();
        var spawnerSO = new SerializedObject(spawner);
        spawnerSO.FindProperty("cardPrefab").objectReferenceValue = cardPrefab.GetComponent<RectTransform>();
        spawnerSO.FindProperty("content").objectReferenceValue = content;
        spawnerSO.FindProperty("cardCount").intValue = CardCount;
        spawnerSO.ApplyModifiedPropertiesWithoutUndo();

        var scroller = feedGO.AddComponent<FeedScroller>();
        var scrollerSO = new SerializedObject(scroller);
        scrollerSO.FindProperty("cardHeight").floatValue = CardHeight;
        scrollerSO.FindProperty("snapSpeed").floatValue = 10f;
        scrollerSO.ApplyModifiedPropertiesWithoutUndo();

        BuildMoneyLabel(canvasGO.transform);
        BuildResetButton(canvasGO.transform, gameManager);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("Feed scene setup complete.");
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>();
    }

    private static RectTransform BuildFeed(Transform parent, out GameObject feedGO)
    {
        feedGO = new GameObject("Feed", typeof(RectTransform));
        feedGO.transform.SetParent(parent, false);
        StretchFull(feedGO.GetComponent<RectTransform>());

        var scrollRect = feedGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.inertia = false;

        var viewportGO = new GameObject("Viewport", typeof(RectTransform));
        viewportGO.transform.SetParent(feedGO.transform, false);
        var viewportRT = viewportGO.GetComponent<RectTransform>();
        StretchFull(viewportRT);
        viewportGO.AddComponent<Image>().color = Color.white;
        viewportGO.AddComponent<RectMask2D>();

        var contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(viewportGO.transform, false);
        var contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.anchoredPosition = Vector2.zero;
        contentRT.sizeDelta = Vector2.zero;

        var layoutGroup = contentGO.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true; // must be true or LayoutElement.preferredHeight is ignored and cards collapse to 0
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.spacing = 0;

        contentGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRT;
        scrollRect.content = contentRT;

        return contentRT;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static GameObject BuildCardPrefab()
    {
        var cardGO = new GameObject("ColorCard", typeof(RectTransform));
        cardGO.AddComponent<Image>();
        cardGO.AddComponent<LayoutElement>().preferredHeight = CardHeight;

        if (!Directory.Exists("Assets/Prefabs"))
        {
            Directory.CreateDirectory("Assets/Prefabs");
            AssetDatabase.Refresh();
        }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(cardGO, "Assets/Prefabs/ColorCard.prefab");
        Object.DestroyImmediate(cardGO);
        return prefab;
    }

    private static void BuildMoneyLabel(Transform parent)
    {
        var labelGO = new GameObject("MoneyLabel", typeof(RectTransform));
        labelGO.transform.SetParent(parent, false);
        var rt = labelGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0, -60);
        rt.sizeDelta = new Vector2(600, 120);

        var text = labelGO.AddComponent<TextMeshProUGUI>();
        text.text = "$0";
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 72;

        labelGO.AddComponent<MoneyLabel>();
    }

    private static void BuildResetButton(Transform parent, GameManager gameManager)
    {
        var buttonGO = new GameObject("ResetButton", typeof(RectTransform));
        buttonGO.transform.SetParent(parent, false);
        var rt = buttonGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, 60);
        rt.sizeDelta = new Vector2(400, 120);

        buttonGO.AddComponent<Image>().color = Color.white;
        var button = buttonGO.AddComponent<Button>();

        var labelGO = new GameObject("Label", typeof(RectTransform));
        labelGO.transform.SetParent(buttonGO.transform, false);
        StretchFull(labelGO.GetComponent<RectTransform>());
        var text = labelGO.AddComponent<TextMeshProUGUI>();
        text.text = "Reset";
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;
        text.fontSize = 48;

        UnityEventTools.AddPersistentListener(button.onClick, gameManager.ResetMoney);
    }
}
