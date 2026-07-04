# Scroll Feed Prototype Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A portrait-locked Unity scene where swiping through a vertical feed of solid-color cards snaps to each card and mints money, with a button to reset the money counter.

**Architecture:** Unity UI `ScrollRect` (Canvas → ScrollRect → Viewport → Content, with a `Vertical Layout Group` + `Content Size Fitter`) holds ~50 pre-generated full-screen color cards. A `FeedScroller` script snaps the content to the nearest card on drag-release and reports each settled-index change to a `GameManager` singleton, which tracks money as a plain `double` and exposes an `AddMoney`/`ResetMoney` API with a change event. UI binds to that event. Because the executing agent has no way to click through the Unity Editor GUI, the entire scene hierarchy (Canvas, ScrollRect, prefab, component wiring) is built by one idempotent Editor automation script (`Assets/Editor/FeedSceneSetup.cs`) run headlessly via Unity batchmode, instead of manual Inspector steps.

**Tech Stack:** Unity 6000.4.5f1, URP 2D template, Unity UI (uGUI + TextMeshPro), new Input System (`activeInputHandler: 1` — EventSystem needs `InputSystemUIInputModule`, not the legacy `StandaloneInputModule`), Unity Test Framework 1.6.0 (EditMode).

## Global Constraints

- Target resolution: 1080×1920 (portrait). Card height is fixed at 1920 to match — no responsive/multi-resolution handling this phase.
- Money is a plain `double` this phase (per spec) — `ponytail:` comment marks the future BigDouble swap when upgrades land.
- Feed is a fixed set of ~50 pre-generated cards, not infinite/pooled.
- Reset button zeroes money only; it does not touch feed/scroll position.
- VCS is Plastic SCM, not git (`.plastic/`) — **omit all `git add`/`git commit` steps**. There is no scripted checkpoint step available in this environment; treat file saves as the unit of progress.
- Unity Editor for this project: `C:\Program Files\Unity\Hub\Editor\6000.4.5f1\Editor\Unity.exe` (version-matched to `ProjectSettings/ProjectVersion.txt`).
- Project root: `C:\Users\Kevin Sukias\SoulScroll`.
- The executing agent cannot drive the Editor's Game view or Play mode (no GUI automation available). Anything requiring a human to watch/drag/click in Play mode is called out explicitly as **user verification**, not agent verification. Everything else (file edits, compilation, EditMode tests, headless scene construction) is agent-verifiable.

---

### Task 1: Portrait resolution project settings

**Files:**
- Modify: `ProjectSettings/ProjectSettings.asset`

**Interfaces:**
- Produces: standalone player defaults to a 1080×1920 window; portrait orientation flag set for any future mobile build target.

- [x] **Step 1: Confirm Unity Editor isn't holding the project open**

Run: `tasklist | grep -i unity` (or PowerShell `Get-Process Unity -ErrorAction SilentlyContinue`). If a Unity process is running against this project, direct edits to `ProjectSettings.asset` risk being overwritten on next Editor save — ask the user to close the Editor first, or confirm it's safe to proceed.

- [x] **Step 2: Edit the resolution and orientation keys**

Change:
```yaml
  defaultScreenOrientation: 4
```
to
```yaml
  defaultScreenOrientation: 0
```
and change:
```yaml
  defaultScreenWidth: 1920
  defaultScreenHeight: 1080
```
to
```yaml
  defaultScreenWidth: 1080
  defaultScreenHeight: 1920
```

- [x] **Step 3: Verify the values were written**

Run:
```bash
grep -n "defaultScreenOrientation\|defaultScreenWidth\|defaultScreenHeight" "ProjectSettings/ProjectSettings.asset"
```
Expected: `defaultScreenOrientation: 0`, `defaultScreenWidth: 1080`, `defaultScreenHeight: 1920`.

---

### Task 2: GameManager (money + reset)

**Files:**
- Create: `Assets/Scripts/GameManager.cs`

**Interfaces:**
- Produces: `GameManager.Instance` (singleton), `double Money { get; }`, `event Action<double> OnMoneyChanged`, `void AddMoney(double amount)`, `void ResetMoney()`.
- Consumes: nothing.

- [x] **Step 1: Write GameManager.cs**

```csharp
using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public double Money { get; private set; }

    public event Action<double> OnMoneyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddMoney(double amount)
    {
        // ponytail: plain double is enough for phase 1 (no upgrades/exponential
        // growth yet). Swap to a BigDouble type when upgrades land, per CLAUDE.md.
        Money += amount;
        OnMoneyChanged?.Invoke(Money);
    }

    public void ResetMoney()
    {
        Money = 0;
        OnMoneyChanged?.Invoke(Money);
    }
}
```

Scene wiring (creating the `GameManager` GameObject) is handled by the automation script in Task 6.

---

### Task 3: FeedScroller snap-index math (TDD)

**Files:**
- Create: `Assets/Scripts/FeedScroller.cs` (static method only in this task; MonoBehaviour drag/snap logic added in Task 5)
- Create: `Assets/Tests/EditMode/Tests.EditMode.asmdef`
- Create: `Assets/Tests/EditMode/FeedScrollerTests.cs`

**Interfaces:**
- Produces: `static int FeedScroller.CalculateNearestIndex(float contentAnchoredY, float cardHeight, int cardCount)` — clamped to `[0, cardCount - 1]`.
- Consumes: nothing.

- [x] **Step 1: Create a runtime assembly definition and the test assembly definition**

Referencing the implicit `Assembly-CSharp` predefined assembly by name from a custom
`.asmdef` does not actually get linked by the compiler (confirmed by inspecting the
batchmode compiler args — no `-r:...Assembly-CSharp.dll` was passed). The reliable fix
is to give the runtime scripts their own named assembly and have the test assembly
reference that by name instead.

`Assets/Scripts/Game.Runtime.asmdef`:
```json
{
    "name": "Game.Runtime",
    "rootNamespace": "",
    "references": [
        "Unity.TextMeshPro"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

`Assets/Tests/EditMode/Tests.EditMode.asmdef`:
```json
{
    "name": "Tests.EditMode",
    "rootNamespace": "",
    "references": [
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner",
        "Game.Runtime"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false
}
```
Save as `Assets/Tests/EditMode/Tests.EditMode.asmdef`.

- [x] **Step 2: Write the failing tests**

```csharp
using NUnit.Framework;

public class FeedScrollerTests
{
    [Test]
    public void CalculateNearestIndex_AtTop_ReturnsZero()
    {
        int index = FeedScroller.CalculateNearestIndex(contentAnchoredY: 0f, cardHeight: 1920f, cardCount: 50);
        Assert.AreEqual(0, index);
    }

    [Test]
    public void CalculateNearestIndex_ExactlyOnSecondCard_ReturnsOne()
    {
        int index = FeedScroller.CalculateNearestIndex(contentAnchoredY: -1920f, cardHeight: 1920f, cardCount: 50);
        Assert.AreEqual(1, index);
    }

    [Test]
    public void CalculateNearestIndex_PartwayPastThirdCard_RoundsToNearest()
    {
        int index = FeedScroller.CalculateNearestIndex(contentAnchoredY: -1920f * 2.6f, cardHeight: 1920f, cardCount: 50);
        Assert.AreEqual(3, index);
    }

    [Test]
    public void CalculateNearestIndex_PastLastCard_ClampsToLastIndex()
    {
        int index = FeedScroller.CalculateNearestIndex(contentAnchoredY: -1920f * 999f, cardHeight: 1920f, cardCount: 50);
        Assert.AreEqual(49, index);
    }

    [Test]
    public void CalculateNearestIndex_BeforeTop_ClampsToZero()
    {
        int index = FeedScroller.CalculateNearestIndex(contentAnchoredY: 500f, cardHeight: 1920f, cardCount: 50);
        Assert.AreEqual(0, index);
    }
}
```
Save as `Assets/Tests/EditMode/FeedScrollerTests.cs`.

- [x] **Step 3: Run tests to verify they fail (compile error)**

```bash
"/c/Program Files/Unity/Hub/Editor/6000.4.5f1/Editor/Unity.exe" -batchmode -projectPath "C:\Users\Kevin Sukias\SoulScroll" -runTests -testPlatform EditMode -testResults "C:\Users\Kevin Sukias\SoulScroll\TestResults\EditMode.xml" -logFile "C:\Users\Kevin Sukias\SoulScroll\TestResults\EditMode.log"
```
Note: no `-quit` here — `-runTests` already exits on its own once the run finishes; combining it with `-quit` was observed to make Unity quit before the test run started.
```
```
Expected: non-zero exit / log shows a compile error referencing `FeedScroller`.

- [x] **Step 4: Write the minimal implementation**

```csharp
using UnityEngine;

public class FeedScroller : MonoBehaviour
{
    public static int CalculateNearestIndex(float contentAnchoredY, float cardHeight, int cardCount)
    {
        if (cardCount <= 0) return 0;
        int raw = Mathf.RoundToInt(-contentAnchoredY / cardHeight);
        return Mathf.Clamp(raw, 0, cardCount - 1);
    }
}
```
Save as `Assets/Scripts/FeedScroller.cs`.

- [x] **Step 5: Run tests to verify they pass**

Same command as Step 3. Expected: exit code `0`; all 5 `FeedScrollerTests` cases `Passed`.

---

### Task 4: FeedSpawner script

**Files:**
- Create: `Assets/Scripts/FeedSpawner.cs`

**Interfaces:**
- Produces: `FeedSpawner` component with private serialized fields `cardPrefab` (RectTransform), `content` (RectTransform), `cardCount` (int) — populated by the automation script in Task 6. On `Start`, instantiates `cardCount` copies of `cardPrefab` under `content`, each given a random solid color via its `Image`.
- Consumes: nothing from earlier tasks.

- [x] **Step 1: Write FeedSpawner.cs**

```csharp
using UnityEngine;
using UnityEngine.UI;

public class FeedSpawner : MonoBehaviour
{
    [SerializeField] private RectTransform cardPrefab;
    [SerializeField] private RectTransform content;
    [SerializeField] private int cardCount = 50;

    private void Start()
    {
        for (int i = 0; i < cardCount; i++)
        {
            RectTransform card = Instantiate(cardPrefab, content);
            card.GetComponent<Image>().color = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.8f, 1f);
        }
    }
}
```
Save as `Assets/Scripts/FeedSpawner.cs`.

---

### Task 5: FeedScroller snap-and-pay behavior

**Files:**
- Modify: `Assets/Scripts/FeedScroller.cs`

**Interfaces:**
- Consumes: `FeedScroller.CalculateNearestIndex` (Task 3), `GameManager.Instance.AddMoney(double)` (Task 2).
- Produces: private serialized fields `cardHeight` (float), `moneyPerSwipe` (double), `snapSpeed` (float) — populated by the automation script in Task 6. Swiping to a new card calls `GameManager.Instance.AddMoney(moneyPerSwipe)` exactly once per settled index change.

- [x] **Step 1: Extend FeedScroller.cs with drag/snap behavior**

```csharp
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class FeedScroller : MonoBehaviour, IEndDragHandler
{
    [SerializeField] private float cardHeight = 1920f;
    [SerializeField] private double moneyPerSwipe = 1.0;
    [SerializeField] private float snapSpeed = 10f;

    private ScrollRect scrollRect;
    private RectTransform content;
    private int settledIndex;
    private int targetIndex;
    private bool isSnapping;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        targetIndex = CalculateNearestIndex(content.anchoredPosition.y, cardHeight, content.childCount);
        isSnapping = true;
    }

    private void Update()
    {
        if (!isSnapping) return;

        float targetY = -targetIndex * cardHeight;
        Vector2 pos = content.anchoredPosition;
        pos.y = Mathf.Lerp(pos.y, targetY, snapSpeed * Time.deltaTime);

        if (Mathf.Abs(pos.y - targetY) < 0.5f)
        {
            pos.y = targetY;
            isSnapping = false;

            if (targetIndex != settledIndex)
            {
                settledIndex = targetIndex;
                GameManager.Instance.AddMoney(moneyPerSwipe);
            }
        }

        content.anchoredPosition = pos;
    }

    public static int CalculateNearestIndex(float contentAnchoredY, float cardHeight, int cardCount)
    {
        if (cardCount <= 0) return 0;
        int raw = Mathf.RoundToInt(-contentAnchoredY / cardHeight);
        return Mathf.Clamp(raw, 0, cardCount - 1);
    }
}
```

- [x] **Step 2: Run the EditMode tests again to confirm no regression**

Same command as Task 3 Step 3. Expected: exit code `0`, all 5 tests still `Passed`.

---

### Task 6: MoneyLabel script + Editor scene automation

**Files:**
- Create: `Assets/Scripts/MoneyLabel.cs`
- Create: `Assets/Editor/FeedSceneSetup.cs`

**Interfaces:**
- `MoneyLabel` consumes `GameManager.Instance.OnMoneyChanged` / `.Money`.
- `FeedSceneSetup.Setup()` (static, `[MenuItem]`) consumes `GameManager`, `FeedSpawner`, `FeedScroller`, `MoneyLabel` (must already compile) and builds the entire runtime hierarchy in `Assets/Scenes/SampleScene.unity`: `EventSystem` (with `InputSystemUIInputModule`), `GameManager` GameObject, `FeedCanvas` → `Feed` (ScrollRect) → `Viewport` → `Content` (with `VerticalLayoutGroup` + `ContentSizeFitter`), the `ColorCard` prefab (`Assets/Prefabs/ColorCard.prefab`) with `FeedSpawner`/`FeedScroller` fields wired via `SerializedObject`, a `MoneyLabel` text object, and a `ResetButton` whose `OnClick` is wired to `GameManager.ResetMoney` via `UnityEventTools.AddPersistentListener`. Saves the scene when done.

- [x] **Step 1: Write MoneyLabel.cs**

```csharp
using TMPro;
using UnityEngine;

public class MoneyLabel : MonoBehaviour
{
    private TMP_Text label;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        GameManager.Instance.OnMoneyChanged += UpdateLabel;
        UpdateLabel(GameManager.Instance.Money);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyChanged -= UpdateLabel;
        }
    }

    private void UpdateLabel(double money)
    {
        label.text = $"${money:0}";
    }
}
```
Save as `Assets/Scripts/MoneyLabel.cs`. (`Start` runs only after every object's `Awake` — including `GameManager`'s — per Unity's execution order guarantee, so `Instance` is never null here.)

- [x] **Step 2: Write the Editor automation script**

```csharp
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;

public static class FeedSceneSetup
{
    private const float CardHeight = 1920f;
    private const int CardCount = 50;

    private const string ScenePath = "Assets/Scenes/SampleScene.unity";

    [MenuItem("Tools/Setup Feed Scene")]
    public static void Setup()
    {
        // -executeMethod doesn't open any scene by default, so SaveScene on the
        // resulting "Untitled" scene silently no-ops. Open the target scene explicitly.
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
        scrollerSO.FindProperty("moneyPerSwipe").doubleValue = 1.0;
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
        layoutGroup.childControlHeight = false;
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
```
Save as `Assets/Editor/FeedSceneSetup.cs`. (Anything under a folder literally named `Editor` compiles into an editor-only assembly automatically — no `.asmdef` needed.)

- [x] **Step 3: Run the automation script headlessly**

```bash
"/c/Program Files/Unity/Hub/Editor/6000.4.5f1/Editor/Unity.exe" -batchmode -projectPath "C:\Users\Kevin Sukias\SoulScroll" -executeMethod FeedSceneSetup.Setup -logFile "C:\Users\Kevin Sukias\SoulScroll\TestResults\SceneSetup.log" -quit
```
Expected: exit code `0`; log contains `Feed scene setup complete.`; `Assets/Prefabs/ColorCard.prefab` now exists; `Assets/Scenes/SampleScene.unity` is modified (check with `grep -c "GameObject:" Assets/Scenes/SampleScene.unity` — count should have grown from the empty template).

- [x] **Step 4: Structural verification (agent-checkable)**

```bash
grep -c "m_Name: FeedCanvas\|m_Name: Feed\|m_Name: Content\|m_Name: MoneyLabel\|m_Name: ResetButton\|m_Name: GameManager" "Assets/Scenes/SampleScene.unity"
```
Expected: 6 matches (one per named object), confirming the hierarchy was actually written into the scene file.

- [x] **Step 5: TMP Essential Resources note**

If, when the user next opens the Editor, the money/reset labels render blank, the project needs the one-time `Window > TextMeshPro > Import TMP Essential Resources` import. This is a manual Editor action; note it to the user rather than attempting to script it.

---

### Task 7: User acceptance check (manual — requires the Editor's Play mode)

This task cannot be performed by the agent (no Game view / Play mode automation available). Hand off to the user:

- [ ] Open the project in Unity Editor, open `Assets/Scenes/SampleScene.unity`, enter Play mode.
- [ ] Confirm the Game view is portrait-shaped (add a 1080×1920 custom aspect preset if needed).
- [ ] Confirm the feed shows one full-screen solid-color card at a time.
- [ ] Confirm swiping up/down snaps fully onto the next/previous card rather than resting at an arbitrary position.
- [ ] Confirm every settled swipe (either direction) increases the money label by `1`.
- [ ] Click Reset and confirm money goes to `$0` while the current card/scroll position is unchanged.
- [ ] Report back anything that looks or feels wrong (e.g., snap is too slow/fast — tune `Snap Speed` on the `Feed` GameObject's `FeedScroller` component; TMP labels blank — see Task 6 Step 5).
