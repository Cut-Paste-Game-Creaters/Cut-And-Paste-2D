
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // ホバー検知用

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

    [SerializeField] private int gridColumns = 4;   // 横に並べる列数
    [SerializeField] private int cellVSpace = 10;   // セル内の縦スペース
    [SerializeField] private int cellHSpace = 10;   // セル同士の横/縦スペース

    [Header("Advanced")]
    [Tooltip("起動直後から開いた状態にしたい場合のみON（通常はOFF推奨）。")]
    [SerializeField] private bool openOnStart = false;
    [Tooltip("シーンをまたいでこのUIを残したい場合のみON。通常はOFF。")]
    [SerializeField] private bool persistAcrossScenes = false;

    [System.Serializable]
    private class StagePreview { public string stageName; public Sprite preview; }

    [Header("Preview")]
    [SerializeField] private StagePreview[] stagePreviews = new StagePreview[0];
    [SerializeField] private Vector2 previewSize = new Vector2(240, 240);
    [SerializeField] private Vector2 previewAnchorOffset = new Vector2(-12, -84); // 右上からのオフセット

    // ★ 追加：Window 背景画像設定
    [Header("Window Background")]
    [Tooltip("Window 内に敷く背景画像（windowSizeにピッタリ合わせます）")]
    [SerializeField] private Sprite windowBackgroundSprite;
    [SerializeField] private Color windowBackgroundColor = Color.white;
    [SerializeField] private bool showWindowBackground = true;

    private Image _previewImg; // 生成したプレビュー表示先
    private System.Collections.Generic.Dictionary<string, Sprite> _previewMap;

    // ランタイム生成物
    private GameObject _canvasGO;
    private Canvas _canvas;
    private GameObject _root;         // Blocker + Window 親
    private CanvasGroup _group;       // 可視/入力の一括制御
    private RectTransform _content;

    // 参照
    private RankJudgeAndUpdateFunction _rank;
    private FadeScreen _fade;
    private SEManager _se;
    private static int s_UIPauseLocks = 0;
    private bool _lockAcquired = false;

    private readonly Regex _stageNameRx = new Regex(@"^Stage\d+$");

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
        if (PlayerInput.GetKeyDown(toggleKey))
        {
            if (!IsBuilt) EnsureBuilt();

            bool show = !IsVisible;
            if (show) RefreshList();
            SetVisible(show);
        }
    }

    private void EnsureBuilt()
    {
        if (IsBuilt) return;

        // Canvas
        _canvasGO = new GameObject("StageQuickSelectCanvas");
        _canvasGO.layer = LayerMask.NameToLayer("UI");
        _canvas = _canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 5000; // ほぼ最前面
        var scaler = _canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        _canvasGO.AddComponent<GraphicRaycaster>();
        if (persistAcrossScenes) DontDestroyOnLoad(_canvasGO);

        // Root + Group（初期は完全非表示）
        _root = new GameObject("StageQuickSelectRoot");
        _root.transform.SetParent(_canvasGO.transform, false);
        _group = _root.AddComponent<CanvasGroup>();
        _group.alpha = 0f;
        _group.interactable = false;
        _group.blocksRaycasts = false;
        _canvas.enabled = false;

        // Blocker
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

        // Window（外枠）
        var panel = new GameObject("Window", typeof(Image));
        panel.transform.SetParent(_root.transform, false);
        var pImg = panel.GetComponent<Image>();
        pImg.color = new Color(1, 1, 1, 0.95f);
        var pRT = panel.GetComponent<RectTransform>();
        pRT.sizeDelta = windowSize;
        pRT.anchorMin = pRT.anchorMax = new Vector2(0.5f, 0.5f);
        pRT.anchoredPosition = Vector2.zero;

        // ★ 背景画像（WindowSizeにピッタリ＆最背面）
        if (showWindowBackground && (windowBackgroundSprite != null))
        {
            var bg = new GameObject("WindowBackground", typeof(Image));
            bg.transform.SetParent(panel.transform, false);
            var bgImg = bg.GetComponent<Image>();
            bgImg.sprite = windowBackgroundSprite;
            bgImg.color = windowBackgroundColor;
            bgImg.type = Image.Type.Simple;
            bgImg.preserveAspect = false;     // ちょうどはめ込む
            bgImg.raycastTarget = false;      // ボタン操作を邪魔しない

            var bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0.5f, 0.5f);
            bgRT.anchorMax = new Vector2(0.5f, 0.5f);
            bgRT.pivot = new Vector2(0.5f, 0.5f);
            bgRT.sizeDelta = windowSize;      // ← windowSizeにぴったり
            bgRT.anchoredPosition = Vector2.zero;

            // いちばん下に敷く
            bg.transform.SetSiblingIndex(0);
        }

        // Title
        var title = CreateText(panel.transform, "ステージのきろく", fontSize + 6, TextAnchor.MiddleCenter, FontStyle.Bold);
        var tRT = title.GetComponent<RectTransform>();
        tRT.anchorMin = new Vector2(0, 1);
        tRT.anchorMax = new Vector2(1, 1);
        tRT.pivot = new Vector2(0.5f, 1f);
        tRT.sizeDelta = new Vector2(0, 64);
        tRT.anchoredPosition = new Vector2(0, -8);

        // ScrollView（ボタン群はこの上に乗る）
        var scrollGO = new GameObject("ScrollView", typeof(Image), typeof(ScrollRect), typeof(Mask));
        scrollGO.transform.SetParent(panel.transform, false);
        var svRT = scrollGO.GetComponent<RectTransform>();
        svRT.anchorMin = new Vector2(0, 0);
        svRT.anchorMax = new Vector2(1, 1);
        svRT.offsetMin = new Vector2(16, 16);
        svRT.offsetMax = new Vector2(-16, -80);
        // 背景画像を見せたいので薄く（または完全透明でもOK）
        var svBg = scrollGO.GetComponent<Image>();
        svBg.color = new Color(0, 0, 0, 0.05f);
        scrollGO.GetComponent<Mask>().showMaskGraphic = false;

        var content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup));
        content.transform.SetParent(scrollGO.transform, false);
        _content = content.GetComponent<RectTransform>();

        _content.anchorMin = new Vector2(0, 1);
        _content.anchorMax = new Vector2(1, 1);
        _content.pivot = new Vector2(0.5f, 1f);

        var grid = content.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(
            buttonHeight,
            buttonHeight + rankTextHeight + 16
        );
        grid.spacing = new Vector2(cellHSpace, cellHSpace);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = gridColumns;
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;

        scrollGO.GetComponent<ScrollRect>().content = _content;

        // プレビュー表示（右上）
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
        if (stagePreviews != null)
        {
            foreach (var sp in stagePreviews)
            {
                if (sp != null && !string.IsNullOrEmpty(sp.stageName) && sp.preview != null)
                    _previewMap[sp.stageName] = sp.preview;
            }
        }

        // Close (×)
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
                PlayerInput.isPausing = (s_UIPauseLocks > 0);
            }
        }
        else
        {
            _group.alpha = 0f;
            _group.interactable = false;
            _group.blocksRaycasts = false;
            _canvas.enabled = false;

            if (_lockAcquired)
            {
                s_UIPauseLocks = Mathf.Max(0, s_UIPauseLocks - 1);
                _lockAcquired = false;
                PlayerInput.isPausing = (s_UIPauseLocks > 0);
            }

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

    private void RefreshList()
    {
        if (_content == null) return;

        if (_rank == null) _rank = FindObjectOfType<RankJudgeAndUpdateFunction>();
        if (_rank == null)
        {
            CreateText(_content, "RankJudgeAndUpdateFunction がシーンに存在しません。", fontSize, TextAnchor.MiddleCenter);
            return;
        }

        for (int i = _content.childCount - 1; i >= 0; i--)
            Destroy(_content.GetChild(i).gameObject);

        var stages = _rank.stageNumber.OrderBy(kv => kv.Value);

        int added = 0;
        foreach (var kv in stages)
        {
            string stageName = kv.Key;
            string rank = _rank.GetStageRank(stageName);

            if (string.IsNullOrEmpty(rank) || rank == "NONE") continue;

            CreateStageCell(_content, stageName, rank, () => LoadStage(stageName));
            added++;
        }

        if (added == 0)
            CreateText(_content, "", fontSize, TextAnchor.MiddleCenter);
    }

    private void LoadStage(string stageName)
    {
        if (_fade != null) _fade.StartFadeOut(stageName);
        else UnityEngine.SceneManagement.SceneManager.LoadScene(stageName);

        SetVisible(false);
    }

    // -------- helpers --------
    private Text CreateText(Transform parent, string msg, int size, TextAnchor align, FontStyle style = FontStyle.Normal)
    {
        var go = new GameObject("Text", typeof(Text));
        go.transform.SetParent(parent, false);
        var txt = go.GetComponent<Text>();
        txt.text = msg;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = size;
        txt.alignment = align;
        txt.fontStyle = style;
        txt.color = Color.black;
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 30);
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        return txt;
    }

    private Button CreateButton(Transform parent, string label, System.Action onClick)
    {
        var go = new GameObject("Button", typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        var img = go.GetComponent<Image>();
        img.color = new Color(0.95f, 0.95f, 1f, 1f);

        var le = go.AddComponent<LayoutElement>();
        le.minHeight = buttonHeight;
        le.preferredHeight = buttonHeight;
        le.flexibleHeight = 0;
        le.minWidth = buttonHeight;
        le.preferredWidth = buttonHeight;

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(buttonHeight, buttonHeight);

        var text = CreateText(go.transform, label, fontSize, TextAnchor.MiddleCenter, FontStyle.Bold);
        var tRT = text.GetComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero;
        tRT.anchorMax = Vector2.one;
        tRT.offsetMin = Vector2.zero;
        tRT.offsetMax = Vector2.zero;

        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            if (_se != null && _se.decideSE != null) _se.OneShotSE(_se.decideSE);
            onClick?.Invoke();
        });
        return btn;
    }

    // RankJudgeAndUpdateFunction があればそれを優先し、なければ BuildSettings から Stage名(Stage\d+)を列挙
    private System.Collections.Generic.IEnumerable<string> GetAllStageNames()
    {
        if (_rank == null) _rank = FindObjectOfType<RankJudgeAndUpdateFunction>();
        if (_rank != null && _rank.stageNumber != null && _rank.stageNumber.Count > 0)
        {
            return _rank.stageNumber
                .Where(kv => _stageNameRx.IsMatch(kv.Key))
                .OrderBy(kv => kv.Value)
                .Select(kv => kv.Key)
                .ToList();
        }

        var list = new System.Collections.Generic.List<string>();
        int count = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            var path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            var name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (_stageNameRx.IsMatch(name)) list.Add(name);
        }
        list.Sort((a, b) =>
        {
            int ai = int.Parse(System.Text.RegularExpressions.Regex.Match(a, @"\d+").Value);
            int bi = int.Parse(System.Text.RegularExpressions.Regex.Match(b, @"\d+").Value);
            return ai.CompareTo(bi);
        });
        return list;
    }

    private string GetRankForStage(string stageName)
    {
        if (_rank != null)
        {
            try
            {
                string r = _rank.GetStageRank(stageName);
                if (!string.IsNullOrEmpty(r) && r != "NONE") return r;
            }
            catch { }
        }

        string[] strKeys = {
            $"{stageName}_Rank", $"Rank_{stageName}", $"BestRank_{stageName}", $"StageRank_{stageName}"
        };
        foreach (var k in strKeys)
        {
            if (PlayerPrefs.HasKey(k))
            {
                var r = PlayerPrefs.GetString(k, "");
                if (string.IsNullOrEmpty(r))
                {
                    int v = PlayerPrefs.GetInt(k, 0);
                    r = IntRankToLetter(v);
                }
                r = r?.Trim().ToUpper();
                if (!string.IsNullOrEmpty(r) && r != "NONE") return r;
            }
        }

        string[] clearKeys = { $"Clear_{stageName}", $"{stageName}_Cleared" };
        foreach (var k in clearKeys)
        {
            if (PlayerPrefs.HasKey(k))
            {
                int v = PlayerPrefs.GetInt(k, 0);
                if (v != 0) return "C";
            }
        }
        return "NONE";
    }

    private string IntRankToLetter(int v)
    {
        switch (v)
        {
            case 4: return "S";
            case 3: return "A";
            case 2: return "B";
            case 1: return "C";
            default: return "NONE";
        }
    }

    private void CreateStageCell(Transform parent, string stageName, string rank, System.Action onClick)
    {
        var cell = new GameObject("Cell", typeof(RectTransform), typeof(VerticalLayoutGroup));
        cell.transform.SetParent(parent, false);

        var v = cell.GetComponent<VerticalLayoutGroup>();
        v.childAlignment = TextAnchor.UpperCenter;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;
        v.spacing = 4;
        v.padding = new RectOffset(0, 0, 0, 0);

        int squareSize = buttonHeight;
        var button = CreateSquareButton(cell.transform, stageName, squareSize, onClick);

        var rankGO = CreateText(cell.transform, $"{rank}", rankFontSize, TextAnchor.MiddleCenter);
        var rRT = rankGO.GetComponent<RectTransform>();
        rRT.sizeDelta = new Vector2(0, rankTextHeight);

        rank = (rank ?? "NONE").ToUpper().Trim();
        switch (rank)
        {
            case "S": rankGO.color = Color.yellow; break;
            case "A": rankGO.color = Color.red; break;
            case "B": rankGO.color = Color.blue; break;
            case "C": rankGO.color = Color.black; break;
            default: rankGO.color = Color.black; break;
        }
    }

    private Button CreateSquareButton(Transform parent, string label, int size, System.Action onClick)
    {
        var go = new GameObject("SquareButton", typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        var img = go.GetComponent<Image>();
        img.color = new Color(0.95f, 0.95f, 1f, 1f);

        var le = go.AddComponent<LayoutElement>();
        le.minHeight = size;
        le.preferredHeight = size;
        le.minWidth = size;
        le.preferredWidth = size;

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(size, size);

        var text = CreateText(go.transform, label, fontSize, TextAnchor.MiddleCenter, FontStyle.Bold);
        var tRT = text.GetComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero;
        tRT.anchorMax = Vector2.one;
        tRT.offsetMin = Vector2.zero;
        tRT.offsetMax = Vector2.zero;

        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            if (_se != null && _se.decideSE != null) _se.OneShotSE(_se.decideSE);
            onClick?.Invoke();
        });

        AddEventTrigger(go, EventTriggerType.PointerEnter, () => ShowPreview(label));
        AddEventTrigger(go, EventTriggerType.PointerExit, HidePreview);

        return btn;
    }

    private void AddEventTrigger(GameObject go, EventTriggerType type, System.Action cb)
    {
        var et = go.GetComponent<EventTrigger>();
        if (et == null) et = go.AddComponent<EventTrigger>();

        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(_ => cb?.Invoke());
        et.triggers.Add(entry);
    }

    private void ShowPreview(string stageName)
    {
        if (_previewImg == null) return;

        var m = System.Text.RegularExpressions.Regex.Match(stageName, @"^Stage\d+");
        string key = m.Success ? m.Value : stageName;

        if (_previewMap != null && _previewMap.TryGetValue(key, out var sp) && sp != null)
        {
            _previewImg.sprite = sp;
            _previewImg.enabled = true;
        }
        else
        {
            _previewImg.enabled = false;
        }
    }

    private void HidePreview()
    {
        if (_previewImg != null)
        {
            _previewImg.enabled = false;
            _previewImg.sprite = null;
        }
    }
}

