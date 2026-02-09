
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StageQuickSelectUI : MonoBehaviour
{
    [Header("Toggle")]
    [SerializeField] private KeyCode toggleKey = KeyCode.O;

    [Header("UI Settings")]
    [SerializeField] private Vector2 windowSize = new Vector2(520, 600);
    [SerializeField] private int buttonHeight = 60;
    [SerializeField] private int rankTextHeight = 28;
    [SerializeField] private int fontSize = 22;
    [SerializeField] private int rankFontSize = 18;

    [Header("Grid Settings")]
    [SerializeField] private int gridColumns = 4;
    [SerializeField] private int cellVSpace = 10;
    [SerializeField] private int cellHSpace = 10;

    [Header("Font Settings")]
    [Tooltip("ここでフォントを指定できます（未設定なら標準フォントを使用）")]
    [SerializeField] private Font customFont;

    [Header("Advanced")]
    [SerializeField] private bool openOnStart = false;
    [SerializeField] private bool persistAcrossScenes = false;

    [System.Serializable]
    private class StagePreview { public string stageName; public Sprite preview; }

    [Header("Preview")]
    [SerializeField] private StagePreview[] stagePreviews = new StagePreview[0];
    [SerializeField] private Vector2 previewSize = new Vector2(240, 240);
    [SerializeField] private Vector2 previewAnchorOffset = new Vector2(-12, -84);

    [Header("Window Background")]
    [SerializeField] private Sprite windowBackgroundSprite;
    [SerializeField] private Color windowBackgroundColor = Color.white;
    [SerializeField] private bool showWindowBackground = true;

    private Image _previewImg;
    private System.Collections.Generic.Dictionary<string, Sprite> _previewMap;

    private GameObject _canvasGO;
    private Canvas _canvas;
    private GameObject _root;
    private CanvasGroup _group;
    private RectTransform _content;

    private RankJudgeAndUpdateFunction _rank;
    private FadeScreen _fade;
    private SEManager _se;
    private static int s_UIPauseLocks = 0;
    private bool _lockAcquired = false;

    private readonly Regex _stageNameRx = new Regex(@"^Stage\\d+$");

    private bool IsBuilt => _canvasGO != null;
    private bool IsVisible => IsBuilt && _canvas.enabled && _group != null && _group.interactable;

    private void Awake()
    {
        foreach (var c in GameObject.FindObjectsOfType<Canvas>(true))
            if (c.gameObject.name == "StageQuickSelectCanvas") Destroy(c.gameObject);

        _rank = FindObjectOfType<RankJudgeAndUpdateFunction>();
        _fade = FindObjectOfType<FadeScreen>();
        _se = FindObjectOfType<SEManager>();

        if (openOnStart)
        {
            EnsureBuilt();
            RefreshList();
            SetVisible(true);
        }
    }

    private void OnDestroy()
    {
        if (!persistAcrossScenes && _canvasGO != null)
            Destroy(_canvasGO);

        if (_lockAcquired)
        {
            s_UIPauseLocks = Mathf.Max(0, s_UIPauseLocks - 1);
            _lockAcquired = false;
            PlayerInput.isPausing = (s_UIPauseLocks > 0);
        }
    }

    private void Update()
    {
        // toggleKey が押されたら
        if (PlayerInput.GetKeyDown(toggleKey))
        {
            if (!IsBuilt) EnsureBuilt();

            // 表示中なら閉じる、非表示なら開く
            bool show = !IsVisible;

            // 表示状態を切り替える
            if (show)
            {
                RefreshList();
                SetVisible(true);
            }
            else
            {
                // UI表示中でも toggleKey で閉じられるように
                SetVisible(false);
            }
        }
    }


    private void EnsureBuilt()
    {
        if (IsBuilt) return;

        _canvasGO = new GameObject("StageQuickSelectCanvas");
        _canvasGO.layer = LayerMask.NameToLayer("UI");
        _canvas = _canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 5000;
        var scaler = _canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        _canvasGO.AddComponent<GraphicRaycaster>();
        if (persistAcrossScenes) DontDestroyOnLoad(_canvasGO);

        _root = new GameObject("StageQuickSelectRoot");
        _root.transform.SetParent(_canvasGO.transform, false);
        _group = _root.AddComponent<CanvasGroup>();
        _group.alpha = 0f;
        _group.interactable = false;
        _group.blocksRaycasts = false;
        _canvas.enabled = false;

        var blocker = new GameObject("Blocker", typeof(Image), typeof(Button));
        blocker.transform.SetParent(_root.transform, false);
        var blkImg = blocker.GetComponent<Image>();
        blkImg.color = new Color(0, 0, 0, 0.35f);
        var blkRT = blocker.GetComponent<RectTransform>();
        blkRT.anchorMin = Vector2.zero;
        blkRT.anchorMax = Vector2.one;
        blkRT.offsetMin = Vector2.zero;
        blkRT.offsetMax = Vector2.zero;
        blocker.GetComponent<Button>().onClick.AddListener(() => SetVisible(false));

        var panel = new GameObject("Window", typeof(Image));
        panel.transform.SetParent(_root.transform, false);
        var pImg = panel.GetComponent<Image>();
        pImg.color = new Color(1, 1, 1, 0.95f);
        var pRT = panel.GetComponent<RectTransform>();
        pRT.sizeDelta = windowSize;
        pRT.anchorMin = pRT.anchorMax = new Vector2(0.5f, 0.5f);
        pRT.anchoredPosition = Vector2.zero;

        if (showWindowBackground && (windowBackgroundSprite != null))
        {
            var bg = new GameObject("WindowBackground", typeof(Image));
            bg.transform.SetParent(panel.transform, false);
            var bgImg = bg.GetComponent<Image>();
            bgImg.sprite = windowBackgroundSprite;
            bgImg.color = windowBackgroundColor;
            bgImg.type = Image.Type.Simple;
            bgImg.preserveAspect = false;
            bgImg.raycastTarget = false;
            var bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0.5f, 0.5f);
            bgRT.anchorMax = new Vector2(0.5f, 0.5f);
            bgRT.pivot = new Vector2(0.5f, 0.5f);
            bgRT.sizeDelta = windowSize;
            bgRT.anchoredPosition = Vector2.zero;
            bg.transform.SetSiblingIndex(0);
        }

        var title = CreateText(panel.transform, "ステージのきろく（全１６ステージ）", fontSize + 6, TextAnchor.MiddleCenter, FontStyle.Bold);
        var tRT = title.GetComponent<RectTransform>();
        tRT.anchorMin = new Vector2(0, 1);
        tRT.anchorMax = new Vector2(1, 1);
        tRT.pivot = new Vector2(0.5f, 1f);
        tRT.sizeDelta = new Vector2(0, 64);
        tRT.anchoredPosition = new Vector2(0, -8);

        var scrollGO = new GameObject("ScrollView", typeof(Image), typeof(ScrollRect), typeof(Mask));
        scrollGO.transform.SetParent(panel.transform, false);
        var svRT = scrollGO.GetComponent<RectTransform>();
        svRT.anchorMin = new Vector2(0, 0);
        svRT.anchorMax = new Vector2(1, 1);
        svRT.offsetMin = new Vector2(16, 16);
        svRT.offsetMax = new Vector2(-16, -80);
        var svBg = scrollGO.GetComponent<Image>();
        svBg.color = new Color(0, 0, 0, 0.05f);
        scrollGO.GetComponent<Mask>().showMaskGraphic = false;

        var content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup));
        content.transform.SetParent(scrollGO.transform, false);
        _content = content.GetComponent<RectTransform>();

        var grid = content.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(buttonHeight, buttonHeight + rankTextHeight + 16);
        grid.spacing = new Vector2(cellHSpace, cellVSpace);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = gridColumns;

        // ▼ 中央寄せ設定 ▼
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;  // ← Unity標準
        grid.childAlignment = TextAnchor.UpperCenter;         // ← これで中央寄せされる

        // ▼ ScrollRectのcontent位置を中央基準に修正 ▼
        var contentRT = _content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0.5f, 1f);
        contentRT.anchorMax = new Vector2(0.5f, 1f);
        contentRT.pivot = new Vector2(0.5f, 0.6f);
        contentRT.anchoredPosition = Vector2.zero;

        scrollGO.GetComponent<ScrollRect>().content = _content;



        _previewImg = new GameObject("Preview", typeof(Image)).GetComponent<Image>();
        _previewImg.transform.SetParent(panel.transform, false);
        var prt = _previewImg.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(1, 1);
        prt.anchorMax = new Vector2(1, 1);
        prt.pivot = new Vector2(1, 0);
        prt.sizeDelta = previewSize;
        prt.anchoredPosition = previewAnchorOffset;
        _previewImg.color = Color.white;
        _previewImg.preserveAspect = true;
        _previewImg.enabled = false;

        _previewMap = new System.Collections.Generic.Dictionary<string, Sprite>();
        foreach (var sp in stagePreviews)
        {
            if (sp != null && !string.IsNullOrEmpty(sp.stageName) && sp.preview != null)
                _previewMap[sp.stageName] = sp.preview;
        }

        var close = CreateButton(panel.transform, "×", () => SetVisible(false));
        var cRT = close.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(1, 1);
        cRT.anchorMax = new Vector2(1, 1);
        cRT.pivot = new Vector2(1, 1);
        cRT.sizeDelta = new Vector2(44, 44);
        cRT.anchoredPosition = new Vector2(-12, -12);
    }

    private void SetVisible(bool visible)
    {
        if (!IsBuilt) return;

        if (visible)
        {
            _canvas.enabled = true;
            _group.alpha = 1f;
            _group.interactable = true;
            _group.blocksRaycasts = true;

            if (!_lockAcquired)
            {
                s_UIPauseLocks++;
                _lockAcquired = true;
            }
            PlayerInput.isPausing = true;
        }
        else
        {
            _group.alpha = 0f;
            _group.interactable = false;
            _group.blocksRaycasts = false;
            _canvas.enabled = false;

            // 入力解除を0.5秒遅らせる
            StartCoroutine(DelayedUnlock(0.5f));

            if (!persistAcrossScenes)
            {
                Destroy(_canvasGO);
                _canvasGO = null;
                _canvas = null;
                _root = null;
                _group = null;
                _content = null;
            }
        }
    }

    private IEnumerator DelayedUnlock(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (_lockAcquired)
        {
            s_UIPauseLocks = Mathf.Max(0, s_UIPauseLocks - 1);
            _lockAcquired = false;
        }

        PlayerInput.isPausing = (s_UIPauseLocks > 0);
    }

    private Text CreateText(Transform parent, string msg, int size, TextAnchor align, FontStyle style = FontStyle.Normal)
    {
        var go = new GameObject("Text", typeof(Text));
        go.transform.SetParent(parent, false);
        var txt = go.GetComponent<Text>();
        txt.text = msg;
        txt.font = customFont != null ? customFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = size;
        txt.alignment = align;
        txt.fontStyle = style;
        txt.color = Color.black;
        return txt;
    }

    private Button CreateButton(Transform parent, string label, System.Action onClick)
    {
        var go = new GameObject("Button", typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = new Color(0.95f, 0.95f, 1f, 1f);
        var text = CreateText(go.transform, label, fontSize, TextAnchor.MiddleCenter, FontStyle.Bold);
        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            if (_se != null && _se.decideSE != null) _se.OneShotSE(_se.decideSE);
            onClick?.Invoke();
        });
        return btn;
    }

    private void RefreshList()
    {
        if (_content == null) return;
        if (_rank == null) _rank = FindObjectOfType<RankJudgeAndUpdateFunction>();
        foreach (Transform child in _content) Destroy(child.gameObject);
        foreach (var kv in _rank.stageNumber.OrderBy(kv => kv.Value))
        {
            string stageName = kv.Key;
            string rank = _rank.GetStageRank(stageName);
            //if (string.IsNullOrEmpty(rank) || rank == "NONE") continue;
            CreateStageCell(_content, stageName, rank, () => LoadStage(stageName));
        }
    }

    private void LoadStage(string stageName)
    {
        if (_fade != null) _fade.StartFadeOut(stageName);
        else UnityEngine.SceneManagement.SceneManager.LoadScene(stageName);
        SetVisible(false);
    }

    private void CreateStageCell(Transform parent, string stageName, string rank, System.Action onClick)
    {
        var cell = new GameObject("Cell", typeof(RectTransform), typeof(VerticalLayoutGroup));
        cell.transform.SetParent(parent, false);

        var button = CreateButton(cell.transform, stageName, onClick);

        // === プレビュー表示機能を追加 ===
        var eventTrigger = button.gameObject.AddComponent<EventTrigger>();

        // カーソルが乗った時
        var entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener((_) =>
        {
            if (_previewMap != null && _previewMap.ContainsKey(stageName))
            {
                _previewImg.sprite = _previewMap[stageName];
                _previewImg.enabled = true;
            }
        });
        eventTrigger.triggers.Add(entryEnter);

        // カーソルが外れた時
        var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback.AddListener((_) =>
        {
            _previewImg.enabled = false;
        });
        eventTrigger.triggers.Add(entryExit);
        // ============================

        CreateText(cell.transform, $"{rank}", rankFontSize, TextAnchor.MiddleCenter);
    }

}

