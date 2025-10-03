using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Advanced")]
    [Tooltip("起動直後から開いた状態にしたい場合のみON（通常はOFF推奨）。")]
    [SerializeField] private bool openOnStart = false;
    [Tooltip("シーンをまたいでこのUIを残したい場合のみON。通常はOFF。")]
    [SerializeField] private bool persistAcrossScenes = false;

    // ランタイム生成物（初期は null。必要時に生成）
    private GameObject _canvasGO;
    private Canvas _canvas;
    private GameObject _root;         // Blocker + Window 親
    private CanvasGroup _group;       // 可視/入力の一括制御
    private RectTransform _content;

    // 参照
    private RankJudgeAndUpdateFunction _rank;
    private FadeScreen _fade;
    private SEManager _se;
    private static int s_UIPauseLocks = 0; // UIによる一時停止のロック数（全体で共有）
    private bool _lockAcquired = false;    // このUIがロックを持っているか

    private readonly Regex _stageNameRx = new Regex(@"^Stage\\d+$");

    private bool IsBuilt => _canvasGO != null;
    private bool IsVisible => IsBuilt && _canvas.enabled && _group != null && _group.interactable;

    // -------- lifecycle --------
    private void Awake()
    {
        // 既存の取り残しを一掃（同名Canvasを全部消す）
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
        // openOnStart=false の場合はここで何も作らない＝画面に何も出ない
    }

    private void OnDestroy()
    {
        if (!persistAcrossScenes && _canvasGO != null)
            Destroy(_canvasGO);

        // 保険：可視/不可視に関係なく、このUIがロックを持っていたら解放
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

    // -------- build / destroy --------
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

        // Window
        var panel = new GameObject("Window", typeof(Image));
        panel.transform.SetParent(_root.transform, false);
        var pImg = panel.GetComponent<Image>();
        pImg.color = new Color(1, 1, 1, 0.95f);
        var pRT = panel.GetComponent<RectTransform>();
        pRT.sizeDelta = windowSize;
        pRT.anchorMin = pRT.anchorMax = new Vector2(0.5f, 0.5f);
        pRT.anchoredPosition = Vector2.zero;

        // Title
        var title = CreateText(panel.transform, "クリア済みステージにジャンプ", fontSize + 6, TextAnchor.MiddleCenter, FontStyle.Bold);
        var tRT = title.GetComponent<RectTransform>();
        tRT.anchorMin = new Vector2(0, 1);
        tRT.anchorMax = new Vector2(1, 1);
        tRT.pivot = new Vector2(0.5f, 1f);
        tRT.sizeDelta = new Vector2(0, 64);
        tRT.anchoredPosition = new Vector2(0, -8);

        // ScrollView
        var scrollGO = new GameObject("ScrollView", typeof(Image), typeof(ScrollRect), typeof(Mask));
        scrollGO.transform.SetParent(panel.transform, false);
        var svRT = scrollGO.GetComponent<RectTransform>();
        svRT.anchorMin = new Vector2(0, 0);
        svRT.anchorMax = new Vector2(1, 1);
        svRT.offsetMin = new Vector2(16, 16);
        svRT.offsetMax = new Vector2(-16, -80);
        scrollGO.GetComponent<Image>().color = new Color(0, 0, 0, 0.05f);
        scrollGO.GetComponent<Mask>().showMaskGraphic = false;

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(scrollGO.transform, false);
        _content = content.GetComponent<RectTransform>();
        _content.anchorMin = new Vector2(0, 1);
        _content.anchorMax = new Vector2(1, 1);
        _content.pivot = new Vector2(0.5f, 1f);
        var vlg = content.GetComponent<VerticalLayoutGroup>();
        vlg.childForceExpandHeight = false;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.spacing = 8;
        vlg.padding = new RectOffset(10, 10, 10, 10);
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollGO.GetComponent<ScrollRect>().content = _content;

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

            // ---- ここから：コピペ禁止（ロック方式） ----
            if (!_lockAcquired)
            {
                s_UIPauseLocks++;
                _lockAcquired = true;
                PlayerInput.isPausing = (s_UIPauseLocks > 0);
            }
            // ---- ここまで ----
        }
        else
        {
            _group.alpha = 0f;
            _group.interactable = false;
            _group.blocksRaycasts = false;
            _canvas.enabled = false;

            // ---- ここから：コピペ許可（ロック解放） ----
            if (_lockAcquired)
            {
                s_UIPauseLocks = Mathf.Max(0, s_UIPauseLocks - 1);
                _lockAcquired = false;
                PlayerInput.isPausing = (s_UIPauseLocks > 0);
            }
            // ---- ここまで ----

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



    // -------- list --------
    // ===== リスト生成/更新（差し替え） =====
    private void RefreshList()
    {
        if (_content == null) return;

        // できる限り毎回探す
        if (_rank == null) _rank = FindObjectOfType<RankJudgeAndUpdateFunction>();
        if (_rank == null)
        {
            CreateText(_content, "RankJudgeAndUpdateFunction がシーンに存在しません。", fontSize, TextAnchor.MiddleCenter);
            return;
        }

        // 表示クリア
        for (int i = _content.childCount - 1; i >= 0; i--)
            Destroy(_content.GetChild(i).gameObject);

        var stages = _rank.stageNumber.OrderBy(kv => kv.Value);

        int added = 0;
        foreach (var kv in stages)
        {
            string stageName = kv.Key;
            string rank = _rank.GetStageRank(stageName);

            if (string.IsNullOrEmpty(rank) || rank == "NONE") continue;

            var btn = CreateButton(_content, $"{stageName} に行く", () => LoadStage(stageName));
            var rankTextGO = CreateText(_content, $"ランク: {rank}", rankFontSize, TextAnchor.MiddleCenter);
            var rt = rankTextGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, rankTextHeight);

            switch (rank)
            {
                case "S": rankTextGO.color = Color.yellow; break;
                case "A": rankTextGO.color = Color.red; break;
                case "B": rankTextGO.color = Color.blue; break;
                case "C": rankTextGO.color = Color.black; break;
                default: rankTextGO.color = Color.black; break;
            }

            added++;
        }

        if (added == 0)
            CreateText(_content, "まだクリア済みのステージはありません。", fontSize, TextAnchor.MiddleCenter);
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
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");///フォント後で変えるか
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

        // ★ これが肝：レイアウトに高さを伝える
        var le = go.AddComponent<LayoutElement>();
        le.minHeight = buttonHeight;          // Inspectorの Button Height を反映
        le.preferredHeight = buttonHeight;
        le.flexibleHeight = 0;

        // 横幅は VerticalLayoutGroup が親幅いっぱいに広げるので sizeDelta.X は 0 のままでOK
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, buttonHeight);

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


    // ===== ここからヘルパーを追加 =====

    // RankJudgeAndUpdateFunction があればそれを優先し、なければ BuildSettings から Stage名(Stage\\d+)を列挙
    private System.Collections.Generic.IEnumerable<string> GetAllStageNames()
    {
        // まず RankJudge… を探す
        if (_rank == null) _rank = FindObjectOfType<RankJudgeAndUpdateFunction>();
        if (_rank != null && _rank.stageNumber != null && _rank.stageNumber.Count > 0)
        {
            // value（番号）順
            return _rank.stageNumber
                .Where(kv => _stageNameRx.IsMatch(kv.Key))
                .OrderBy(kv => kv.Value)
                .Select(kv => kv.Key)
                .ToList();
        }

        // 無い/空 → Build Settings から拾う
        var list = new System.Collections.Generic.List<string>();
        int count = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            var path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            var name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (_stageNameRx.IsMatch(name)) list.Add(name);
        }
        // 名前の末尾数字で昇順ソート（Stage1, Stage2, …）
        list.Sort((a, b) =>
        {
            int ai = int.Parse(System.Text.RegularExpressions.Regex.Match(a, @"\d+").Value);
            int bi = int.Parse(System.Text.RegularExpressions.Regex.Match(b, @"\d+").Value);
            return ai.CompareTo(bi);
        });
        return list;
    }

    // Rank を取得：RankJudge… → 代表的な PlayerPrefs キー群 → クリアフラグ
    private string GetRankForStage(string stageName)
    {
        // RankJudge… があればそれを使う
        if (_rank != null)
        {
            try
            {
                string r = _rank.GetStageRank(stageName);
                if (!string.IsNullOrEmpty(r) && r != "NONE") return r;
            }
            catch { /* 念のため例外無視でフォールバック */ }
        }

        // よくあるキーの総当り（文字列/数値）
        // 例）"Stage1_Rank", "Rank_Stage1", "BestRank_Stage1", "StageRank_Stage1"
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
                    // 数値で保存してるケース（0=NONE,1=C…等）を想定して読む
                    int v = PlayerPrefs.GetInt(k, 0);
                    r = IntRankToLetter(v); // 下のヘルパー
                }
                r = r?.Trim().ToUpper();
                if (!string.IsNullOrEmpty(r) && r != "NONE") return r;
            }
        }

        // クリアフラグだけ保存しているケース
        // 例）"Clear_Stage1"=1 や $"{stageName}_Cleared"=true
        string[] clearKeys = { $"Clear_{stageName}", $"{stageName}_Cleared" };
        foreach (var k in clearKeys)
        {
            if (PlayerPrefs.HasKey(k))
            {
                int v = PlayerPrefs.GetInt(k, 0);
                if (v != 0) return "C"; // ランク不明なら暫定でCにして表示（必要ならここ変更可）
            }
        }

        return "NONE";
    }

    private string IntRankToLetter(int v)
    {
        // よくある割当：0=NONE,1=C,2=B,3=A,4=S
        switch (v)
        {
            case 4: return "S";
            case 3: return "A";
            case 2: return "B";
            case 1: return "C";
            default: return "NONE";
        }
    }
}
