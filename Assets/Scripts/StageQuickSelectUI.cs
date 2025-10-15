using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; //�z�o�[���m�p
using System.Collections;

public class StageQuickSelectUI : MonoBehaviour
{
    [Header("Toggle")]
    [SerializeField] private KeyCode toggleKey = KeyCode.O;

    [Header("UI Settings")]
    [SerializeField] private Vector2 windowSize = new Vector2(520, 800);
    [SerializeField] private int buttonHeight = 60;
    [SerializeField] private int rankTextHeight = 28;
    [SerializeField] private int fontSize = 22;
    [SerializeField] private int rankFontSize = 18;

    [SerializeField] private int gridColumns = 4;   // ���ɕ��ׂ��
    [SerializeField] private int cellVSpace = 10;   // �Z�����̏c�X�y�[�X
    [SerializeField] private int cellHSpace = 10;   // �Z�����m�̉�/�c�X�y�[�X

    [Header("Advanced")]
    [Tooltip("�N�����ォ��J������Ԃɂ������ꍇ�̂�ON�i�ʏ��OFF�����j�B")]
    [SerializeField] private bool openOnStart = false;
    [Tooltip("�V�[�����܂����ł���UI���c�������ꍇ�̂�ON�B�ʏ��OFF�B")]
    [SerializeField] private bool persistAcrossScenes = false;

    [System.Serializable]
    private class StagePreview { public string stageName; public Sprite preview; }

    [Header("Preview")]
    [SerializeField] private StagePreview[] stagePreviews = new StagePreview[0];
    [SerializeField] private Vector2 previewSize = new Vector2(240, 240);

    private Image _previewImg;                          // ���������v���r���[�\����
    private System.Collections.Generic.Dictionary<string, Sprite> _previewMap;

    // �����^�C���������i������ null�B�K�v���ɐ����j
    private GameObject _canvasGO;
    private Canvas _canvas;
    private GameObject _root;         // Blocker + Window �e
    private CanvasGroup _group;       // ��/���͂̈ꊇ����
    private RectTransform _content;

    // �Q��
    private RankJudgeAndUpdateFunction _rank;
    private FadeScreen _fade;
    private SEManager _se;
    private static int s_UIPauseLocks = 0; // UI�ɂ��ꎞ��~�̃��b�N���i�S�̂ŋ��L�j
    private bool _lockAcquired = false;    // ����UI�����b�N�������Ă��邩

    private readonly Regex _stageNameRx = new Regex(@"^Stage\\d+$");

    private bool IsBuilt => _canvasGO != null;
    private bool IsVisible => IsBuilt && _canvas.enabled && _group != null && _group.interactable;

    // -------- lifecycle --------
    private void Awake()
    {
        // �����̎��c������|�i����Canvas��S�������j
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
        // openOnStart=false �̏ꍇ�͂����ŉ������Ȃ�����ʂɉ����o�Ȃ�
    }

    private void OnDestroy()
    {
        if (!persistAcrossScenes && _canvasGO != null)
            Destroy(_canvasGO);

        // �ی��F��/�s���Ɋ֌W�Ȃ��A����UI�����b�N�������Ă�������
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
        _canvas.sortingOrder = 5000; // �قڍőO��
        var scaler = _canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        _canvasGO.AddComponent<GraphicRaycaster>();
        if (persistAcrossScenes) DontDestroyOnLoad(_canvasGO);

        // Root + Group�i�����͊��S��\���j
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
        var title = CreateText(panel.transform, "�N���A�ς݃X�e�[�W�ɃW�����v", fontSize + 6, TextAnchor.MiddleCenter, FontStyle.Bold);
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

        var content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup));
        content.transform.SetParent(scrollGO.transform, false);
        _content = content.GetComponent<RectTransform>();

        // �i�A���J�[�͂��̂܂܂�OK�j
        _content.anchorMin = new Vector2(0, 1);
        _content.anchorMax = new Vector2(1, 1);
        _content.pivot = new Vector2(0.5f, 1f);

        // �� �Z���T�C�Y���u�����`�{�^���v�{�u�����N�s�v
        var grid = content.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(
            buttonHeight,                          // ���������`�{�^���̈��
            buttonHeight + rankTextHeight + 16     // �������{�^���{�����N�{�]��
        );
        grid.spacing = new Vector2(cellHSpace, cellHSpace);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = gridColumns;
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;

        scrollGO.GetComponent<ScrollRect>().content = _content;

        // ���ǉ��F�v���r���[�p Image
        _previewImg = new GameObject("Preview", typeof(Image)).GetComponent<Image>();
        _previewImg.transform.SetParent(panel.transform, false);
        var prt = _previewImg.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.5f, 0f);   // �������
        prt.anchorMax = new Vector2(0.5f, 0f);
        prt.pivot = new Vector2(0.5f, 0f);       // ���������s�{�b�g��
        prt.sizeDelta = previewSize;
        prt.anchoredPosition = new Vector2(0f, 30f); // ������60px�قǏ�ɕ\���i�����j        // �E�ォ��̑��Έʒu
        _previewImg.color = Color.white;
        _previewImg.preserveAspect = true;
        _previewImg.enabled = false;                        // �����͔�\��

        // ���ǉ��F�v���r���[�}�b�v���\�z
        _previewMap = new System.Collections.Generic.Dictionary<string, Sprite>();
        if (stagePreviews != null)
        {
            foreach (var sp in stagePreviews)
            {
                if (sp != null && !string.IsNullOrEmpty(sp.stageName) && sp.preview != null)
                {
                    _previewMap[sp.stageName] = sp.preview;
                }
            }
        }


        // Close (�~)
        var close = CreateButton(panel.transform, "�~", () => SetVisible(false));
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

            // ---- ��������F�R�s�y�֎~�i���b�N�����j ----
            if (!_lockAcquired)
            {
                s_UIPauseLocks++;
                _lockAcquired = true;
                PlayerInput.isPausing = (s_UIPauseLocks > 0);
            }
            // ---- �����܂� ----
        }
        else
        {
            _group.alpha = 0f;
            _group.interactable = false;
            _group.blocksRaycasts = false;
            _canvas.enabled = false;

            // ---- ��������F�R�s�y���i���b�N����j ----
            if (_lockAcquired)
            {
                StartCoroutine(DelayedUnlock());
            }
            // ---- �����܂� ----

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

    private IEnumerator DelayedUnlock()
    {
        // �����҂i��F0.1�b��1?2�t���[�����x�j
        yield return new WaitForSeconds(0.3f);

        s_UIPauseLocks = Mathf.Max(0, s_UIPauseLocks - 1);
        _lockAcquired = false;
        PlayerInput.isPausing = (s_UIPauseLocks > 0);
    }

    // -------- list --------
    // ===== ���X�g����/�X�V�i�����ւ��j =====
    private void RefreshList()
    {
        if (_content == null) return;

        // �ł�����薈��T��
        if (_rank == null) _rank = FindObjectOfType<RankJudgeAndUpdateFunction>();
        if (_rank == null)
        {
            CreateText(_content, "RankJudgeAndUpdateFunction ���V�[���ɑ��݂��܂���B", fontSize, TextAnchor.MiddleCenter);
            return;
        }

        // �\���N���A
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
            CreateText(_content, "�܂��N���A�ς݂̃X�e�[�W�͂���܂���B", fontSize, TextAnchor.MiddleCenter);
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
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");///�t�H���g��ŕς��邩
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

        // �� ���ꂪ�́F���C�A�E�g�ɍ�����`����
        var le = go.AddComponent<LayoutElement>();
        le.minHeight = buttonHeight;          // Inspector�� Button Height �𔽉f
        le.preferredHeight = buttonHeight;
        le.flexibleHeight = 0;

        // �� �ǉ��F�����`�ɂ��邽�ߕ����Œ�
        le.minWidth = buttonHeight;
        le.preferredWidth = buttonHeight;

        // �i�C�ӁjRectTransform �� sizeDelta �������`��
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


    // ===== ��������w���p�[��ǉ� =====

    // RankJudgeAndUpdateFunction ������΂����D�悵�A�Ȃ���� BuildSettings ���� Stage��(Stage\\d+)���
    private System.Collections.Generic.IEnumerable<string> GetAllStageNames()
    {
        // �܂� RankJudge�c ��T��
        if (_rank == null) _rank = FindObjectOfType<RankJudgeAndUpdateFunction>();
        if (_rank != null && _rank.stageNumber != null && _rank.stageNumber.Count > 0)
        {
            // value�i�ԍ��j��
            return _rank.stageNumber
                .Where(kv => _stageNameRx.IsMatch(kv.Key))
                .OrderBy(kv => kv.Value)
                .Select(kv => kv.Key)
                .ToList();
        }

        // ����/�� �� Build Settings ����E��
        var list = new System.Collections.Generic.List<string>();
        int count = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            var path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            var name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (_stageNameRx.IsMatch(name)) list.Add(name);
        }
        // ���O�̖��������ŏ����\�[�g�iStage1, Stage2, �c�j
        list.Sort((a, b) =>
        {
            int ai = int.Parse(System.Text.RegularExpressions.Regex.Match(a, @"\d+").Value);
            int bi = int.Parse(System.Text.RegularExpressions.Regex.Match(b, @"\d+").Value);
            return ai.CompareTo(bi);
        });
        return list;
    }

    // Rank ���擾�FRankJudge�c �� ��\�I�� PlayerPrefs �L�[�Q �� �N���A�t���O
    private string GetRankForStage(string stageName)
    {
        // RankJudge�c ������΂�����g��
        if (_rank != null)
        {
            try
            {
                string r = _rank.GetStageRank(stageName);
                if (!string.IsNullOrEmpty(r) && r != "NONE") return r;
            }
            catch { /* �O�̂��ߗ�O�����Ńt�H�[���o�b�N */ }
        }

        // �悭����L�[�̑�����i������/���l�j
        // ��j"Stage1_Rank", "Rank_Stage1", "BestRank_Stage1", "StageRank_Stage1"
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
                    // ���l�ŕۑ����Ă�P�[�X�i0=NONE,1=C�c���j��z�肵�ēǂ�
                    int v = PlayerPrefs.GetInt(k, 0);
                    r = IntRankToLetter(v); // ���̃w���p�[
                }
                r = r?.Trim().ToUpper();
                if (!string.IsNullOrEmpty(r) && r != "NONE") return r;
            }
        }

        // �N���A�t���O�����ۑ����Ă���P�[�X
        // ��j"Clear_Stage1"=1 �� $"{stageName}_Cleared"=true
        string[] clearKeys = { $"Clear_{stageName}", $"{stageName}_Cleared" };
        foreach (var k in clearKeys)
        {
            if (PlayerPrefs.HasKey(k))
            {
                int v = PlayerPrefs.GetInt(k, 0);
                if (v != 0) return "C"; // �����N�s���Ȃ�b���C�ɂ��ĕ\���i�K�v�Ȃ炱���ύX�j
            }
        }

        return "NONE";
    }

    private string IntRankToLetter(int v)
    {
        // �悭���銄���F0=NONE,1=C,2=B,3=A,4=S
        switch (v)
        {
            case 4: return "S";
            case 3: return "A";
            case 2: return "B";
            case 1: return "C";
            default: return "NONE";
        }
    }

    // 1�Z�����u�����`�{�^���v�{�u�����N�v
    private void CreateStageCell(Transform parent, string stageName, string rank, System.Action onClick)
    {
        // �Z���̃��[�g
        var cell = new GameObject("Cell", typeof(RectTransform), typeof(VerticalLayoutGroup));
        cell.transform.SetParent(parent, false);

        // �Z�������C�A�E�g�i�c�j
        var v = cell.GetComponent<VerticalLayoutGroup>();
        v.childAlignment = TextAnchor.UpperCenter;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = true;   // ���̓Z�������ς�
        v.childForceExpandHeight = false; // �����͎q�̐����ɏ]��
        v.spacing = 4;
        v.padding = new RectOffset(0, 0, 0, 0);

        // �����`�{�^���i�Z�����ɍ��킹�� �� ���T�C�Y�� squareSize�j
        int squareSize = buttonHeight;
        var button = CreateSquareButton(cell.transform, stageName, squareSize, onClick);

        // �����N�\���i�{�^�����j
        var rankGO = CreateText(cell.transform, $"{rank}", rankFontSize, TextAnchor.MiddleCenter);
        var rRT = rankGO.GetComponent<RectTransform>();
        rRT.sizeDelta = new Vector2(0, rankTextHeight);

        // �����N�F
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


    // �����`�{�^���i�Z���p�j
    private Button CreateSquareButton(Transform parent, string label, int size, System.Action onClick)
    {
        var go = new GameObject("SquareButton", typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        // �w�i
        var img = go.GetComponent<Image>();
        img.color = new Color(0.95f, 0.95f, 1f, 1f);

        // �T�C�Y�w��i�����`�j
        var le = go.AddComponent<LayoutElement>();
        le.minHeight = size;
        le.preferredHeight = size;
        le.minWidth = size;
        le.preferredWidth = size;

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(size, size);

        // �{�^�����e�L�X�g�i�����j
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
        // �z�o�[�Ńv���r���[�\��
        AddEventTrigger(go, EventTriggerType.PointerEnter, () => ShowPreview(label)); // label �� stageName ��n���Ă���
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

        // �󂯎���������� "Stage1 �ɍs��" �̂悤�ȏꍇ�ɔ����A�擪�̒P�� Stage\d+ ���E��
        var m = System.Text.RegularExpressions.Regex.Match(stageName, @"^Stage\d+");
        string key = m.Success ? m.Value : stageName;

        if (_previewMap != null && _previewMap.TryGetValue(key, out var sp) && sp != null)
        {
            _previewImg.sprite = sp;
            _previewImg.enabled = true;
        }
        else
        {
            _previewImg.enabled = false; // �o�^���Ȃ���Ώo���Ȃ�
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
