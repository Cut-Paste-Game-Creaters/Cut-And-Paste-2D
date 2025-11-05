// Assets/Scripts/UI/FadeScreen.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// スプライト原寸（Simple描画）で紙をスライドさせて
/// 覆う→シーン切替→（指定秒待つ）→離れる を実現するフェード演出。
/// ・StartFadeOut / SceneStartListener と互換
/// ・シーンに置かなくても初回呼び出しで自動生成（DontDestroyOnLoad）
/// ・新シーンの最初のフレームから“紙で覆われた状態”を保証
/// </summary>
public class FadeScreen : MonoBehaviour
{
    // ---------------------- Singleton ----------------------
    private static FadeScreen _instance;
    public static FadeScreen Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("FadeScreen(PaperTransition)");
                _instance = go.AddComponent<FadeScreen>();
                DontDestroyOnLoad(go);
            }
            if (!_instance._built || _instance._paperRT == null) _instance.BuildCanvasAndPaper();
            return _instance;
        }
    }
    public static FadeScreen Get() => Instance;

    // ---------------------- Inspector ----------------------
    [Header("Paper Sprite (Simple)")]
    [Tooltip("紙として使うスプライト（原寸を基準に描画）")]
    [SerializeField] private Sprite paperSprite = null;

    [Tooltip("紙の色（乗算）。白=無影響")]
    [SerializeField] private Color paperColor = Color.white;

    [Tooltip("原寸を基準にした拡大率（1=原寸）。画面を覆いたい場合は大きめに")]
    [SerializeField, Min(0.01f)] private float nativeScale = 1.0f;

    [Tooltip("スプライト未設定時のフォールバックサイズ（px）")]
    [SerializeField] private Vector2 fallbackSize = new Vector2(1920, 1080);

    [Header("Timing")]
    [Tooltip("紙が画面を覆うまでの時間（秒）")]
    [SerializeField, Min(0f)] private float coverDuration = 0.50f;
    [Tooltip("紙が画面外に退くまでの時間（秒）")]
    [SerializeField, Min(0f)] private float uncoverDuration = 0.50f;
    [Tooltip("新シーン読み込み後、紙が離れるまで“待つ時間（秒）”。0で即時離脱")]
    [SerializeField, Min(0f)] private float waitAfterSceneLoaded = 0.0f;

    [Header("Direction")]
    [Tooltip("紙が入ってくる方向（例：右→左なら (-1,0)、下→上なら (0,1)）")]
    [SerializeField] private Vector2 slideDirection = new Vector2(0, -1);

    [Header("Advanced")]
    [Tooltip("フェード用CanvasのSortingOrder（最前面にしたいので大きめ推奨）")]
    [SerializeField] private int sortingOrder = 50000;
    [SerializeField] private AnimationCurve easeIn = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve easeOut = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("遷移中にAudioListener.pauseでBGM/SEを止める")]
    [SerializeField] private bool pauseAudioDuringTransition = true;

    [Tooltip("遷移中にプレイヤー入力を停止（PlayerInput.isPausing を使用）")]
    [SerializeField] private bool pauseInputDuringTransition = true;

    // ---------------------- Runtime objects ----------------------
    private Canvas _canvas;
    private Image _paper;
    private RectTransform _paperRT;

    private bool _built = false;
    private bool _isBusy = false;

    private Vector2 _offPos; // 画面外位置（方向＆紙サイズから算出）
    private Vector2 _onPos;  // 画面中央（= 0,0）

    private string _pendingNextScene = null;

    // 新シーン開始時に「覆い状態」から始めるフラグ
    private bool _shouldStartCovered = false;

    // ============================================================
    //  Unity lifecycle
    // ============================================================
    private void Awake()
    {
        // シーン上に既存があれば統一
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (!_built || _paperRT == null) BuildCanvasAndPaper();
    }

    private void OnEnable()
    {
        // シーンイベント登録（順序：activeSceneChanged -> sceneLoaded）
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        SceneManager.sceneLoaded -= SceneStartListener;
        SceneManager.sceneLoaded += SceneStartListener;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        SceneManager.sceneLoaded -= SceneStartListener;
    }

    // ============================================================
    //  Public API（既存互換）
    // ============================================================

    /// <summary>
    /// 紙が覆ってからシーンをLoadします。
    /// </summary>
    public void StartFadeOut(string nextSceneName)
    {
        if (_isBusy) return;

        var inst = Instance; // 初期化二重保証
        _pendingNextScene = nextSceneName;
        StartCoroutine(Co_CoverThenLoad());
    }

    /// <summary>
    /// 新シーン読み込み完了後に呼ばれ、紙を退かします。
    /// SceneManagerEvent からの登録に加え、本クラスでも OnEnable で自動登録しています（どちらでもOK）。
    /// </summary>
    public void SceneStartListener(Scene scene, LoadSceneMode mode)
    {

        if (_shouldStartCovered)
        {
            StartCoroutine(Co_UncoverWithDelay(waitAfterSceneLoaded));
            _shouldStartCovered = false;
        }
        else
        {
            // 念のため：覆いから始まっていなければ、即覆ってから指定時間待って離れる
            StartCoroutine(Co_ForceCoverThenUncoverWithDelay(waitAfterSceneLoaded));
        }
    }

    // ============================================================
    //  Scene hooks（“最初のフレームから覆い状態”を保証）
    // ============================================================
    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        if (_shouldStartCovered)
        {
            if (!_built || _paperRT == null) BuildCanvasAndPaper();
            NormalizeDirection();
            ApplySpriteSize();
            RecomputePositions();
            HardSetCovered();
        }
    }

    // ============================================================
    //  Builders
    // ============================================================
    private void BuildCanvasAndPaper()
    {
        if (_built) return;

        // Canvas
        var canvasGO = new GameObject("PaperCanvas");
        canvasGO.transform.SetParent(transform, false);

        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = sortingOrder;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Paper Image (Simple)
        var paperGO = new GameObject("Paper", typeof(Image));
        paperGO.transform.SetParent(canvasGO.transform, false);
        _paper = paperGO.GetComponent<Image>();
        _paper.raycastTarget = false;
        _paper.color = paperColor;
        _paper.type = Image.Type.Simple; // ★原寸ベース

        if (paperSprite != null)
        {
            _paper.sprite = paperSprite;
        }

        _paperRT = paperGO.GetComponent<RectTransform>();

        // ★中央アンカー固定（伸縮しない）
        _paperRT.anchorMin = new Vector2(0.5f, 0.5f);
        _paperRT.anchorMax = new Vector2(0.5f, 0.5f);
        _paperRT.pivot = new Vector2(0.5f, 0.5f);
        _paperRT.anchoredPosition = Vector2.zero;

        ApplySpriteSize();

        // 初期位置は画面外
        NormalizeDirection();
        RecomputePositions();
        _paperRT.anchoredPosition = _offPos;

        _built = true;
        _isBusy = false;
    }

    /// <summary>
    /// スプライト原寸＋倍率（nativeScale）でRectサイズを確定。
    /// スプライト未設定時は fallbackSize を使用。
    /// </summary>
    private void ApplySpriteSize()
    {
        if (_paperRT == null) return;

        if (_paper != null && _paper.sprite != null)
        {
            _paper.SetNativeSize(); // スプライトのピクセルサイズをRectへ
            // 倍率
            var s = _paperRT.sizeDelta;
            _paperRT.sizeDelta = s * Mathf.Max(0.01f, nativeScale);
        }
        else
        {
            // スプライトが無い場合のフォールバックサイズ
            _paperRT.sizeDelta = fallbackSize * Mathf.Max(0.01f, nativeScale);
        }

        // レイアウトを即反映（rect.size を正しく取得したい）
        Canvas.ForceUpdateCanvases();
    }

    // ============================================================
    //  Coroutines
    // ============================================================
    private IEnumerator Co_CoverThenLoad()
    {
        _isBusy = true;

        // 入力停止
        bool prevPauseFlag = false;
        if (pauseInputDuringTransition)
        {
            prevPauseFlag = PlayerInput.isPausing;
            PlayerInput.isPausing = true;
        }

        // BGMのみ停止
        if (pauseAudioDuringTransition)
        {
            var bgm = FindObjectOfType<BGMManager>();
            if (bgm != null)
                bgm.PauseMainBGM();
        }


        // 紙で覆う（スプライトサイズに応じた距離を使用）
        if (!_built || _paperRT == null) BuildCanvasAndPaper();
        NormalizeDirection();
        ApplySpriteSize();
        RecomputePositions();

        SEManager.instance.ClipAtPointSE(SEManager.instance.flip1SE);

        yield return Slide(_paperRT, _offPos, _onPos, coverDuration, easeIn);

        // 新シーンは“覆われた状態で開始”したいのでフラグ＋ハードセット
        _shouldStartCovered = true;
        HardSetCovered();

        // シーン切替（覆ってから）
        if (!string.IsNullOrEmpty(_pendingNextScene))
            SceneManager.LoadScene(_pendingNextScene);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // 紙の退去は SceneStartListener 側で継続（指定秒待ってから）
        // 入力・BGMは退去完了まで維持
    }

    private IEnumerator Co_UncoverWithDelay(float delaySec)
    {
        if (!_built || _paperRT == null) BuildCanvasAndPaper();
        NormalizeDirection();
        ApplySpriteSize();
        RecomputePositions();
        HardSetCovered();

        // 指定時間待機（ポーズ中でも進むようにunscaledRealtime）
        if (delaySec > 0f)
            yield return new WaitForSecondsRealtime(delaySec);

        SEManager.instance.ClipAtPointSE(SEManager.instance.flip1SE);

        yield return Slide(_paperRT, _onPos, _offPos, uncoverDuration, easeOut);
        Debug.Log("入力できるようになるハズ");
        // BGM再開
        if (pauseAudioDuringTransition)
        {
            var bgm = FindObjectOfType<BGMManager>();
            if (bgm != null) bgm.ResumeMainBGM();
        }
        // 入力復帰

        if (pauseInputDuringTransition)
            PlayerInput.isPausing = false;
        Time.timeScale = 1.0f;
        _pendingNextScene = null;
        _isBusy = false;
    }

    private IEnumerator Co_ForceCoverThenUncoverWithDelay(float delaySec)
    {
        if (!_built || _paperRT == null) BuildCanvasAndPaper();
        NormalizeDirection();
        ApplySpriteSize();
        RecomputePositions();

        // 覆う（瞬間セット）
        HardSetCovered();

        if (delaySec > 0f)

        yield return new WaitForSecondsRealtime(delaySec);

        yield return Slide(_paperRT, _onPos, _offPos, uncoverDuration, easeOut);

        if (pauseAudioDuringTransition)
            AudioListener.pause = false;
        if (pauseInputDuringTransition)
            PlayerInput.isPausing = false;

        _pendingNextScene = null;
        _isBusy = false;
    }

    // 従来の即時版（内部で使用する場合があれば）
    private IEnumerator Co_Uncover()
    {
        yield return Co_UncoverWithDelay(0f);
    }

    private IEnumerator Co_ForceCoverThenUncover()
    {
        yield return Co_ForceCoverThenUncoverWithDelay(0f);
    }

    // ============================================================
    //  Helpers
    // ============================================================
    private void HardSetCovered()
    {
        if (_paperRT == null) return;
        _paperRT.anchoredPosition = _onPos; // 画面中央（覆い）
        if (_paper != null) _paper.enabled = true;
        if (_canvas != null) _canvas.enabled = true;
    }

    private IEnumerator Slide(RectTransform rt, Vector2 from, Vector2 to, float duration, AnimationCurve ease)
    {
        if (rt == null)
        {
            if (!_built || _paperRT == null) BuildCanvasAndPaper();
            rt = _paperRT;
            if (rt == null) yield break;
        }

        NormalizeDirection();
        ApplySpriteSize();     // ← サイズが確定していることが重要
        RecomputePositions();  // ← サイズに基づいてオフ位置を算出

        float t = 0f;
        rt.anchoredPosition = from;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // ポーズ中でも進行
            float p = (duration <= 0f) ? 1f : Mathf.Clamp01(t / duration);
            float k = ease != null ? ease.Evaluate(p) : p;
            rt.anchoredPosition = Vector2.Lerp(from, to, k);
            yield return null;
        }
        rt.anchoredPosition = to;
    }

    private void NormalizeDirection()
    {
        if (slideDirection == Vector2.zero)
            slideDirection = new Vector2(1, 0);
        // 方向の符号だけ利用するため正規化は不要だが、念のため
        slideDirection = slideDirection.normalized;
    }

    /// <summary>
    /// 画面外へ出し切れる距離を、紙のサイズと画面サイズから算出。
    /// 紙が小さくても完全に視界外→中央にスライドできるようにする。
    /// </summary>
    private void RecomputePositions()
    {
        Canvas.ForceUpdateCanvases();

        var paperSize = (_paperRT != null) ? _paperRT.rect.size : fallbackSize * Mathf.Max(0.01f, nativeScale);
        float margin = 32f;

        Vector2 halfScreen = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 halfPaper = new Vector2(paperSize.x * 0.5f, paperSize.y * 0.5f);

        // 軸ごとに必要十分なオフセットを算出（方向の符号のみ使用）
        float sx = Mathf.Sign(Mathf.Abs(slideDirection.x) < 1e-3f ? 0f : slideDirection.x);
        float sy = Mathf.Sign(Mathf.Abs(slideDirection.y) < 1e-3f ? 0f : slideDirection.y);

        Vector2 travel = new Vector2(
            (halfScreen.x + halfPaper.x + margin) * sx,
            (halfScreen.y + halfPaper.y + margin) * sy
        );

        // 方向が片側ゼロなら片軸のみ利用（例：横だけ、縦だけ）
        if (Mathf.Abs(slideDirection.x) < 1e-3f) travel.x = 0f;
        if (Mathf.Abs(slideDirection.y) < 1e-3f) travel.y = 0f;

        _onPos = Vector2.zero;
        _offPos = -travel; // 反対側から入る
    }

    // ============================================================
    //  Editor support (optional)
    // ============================================================
#if UNITY_EDITOR
    [ContextMenu("Test / Cover (stay)")]
    private void _TestCoverOnly()
    {
        var inst = Instance;
        StopAllCoroutines();
        NormalizeDirection();
        ApplySpriteSize();
        RecomputePositions();
        StartCoroutine(Slide(_paperRT, _offPos, _onPos, coverDuration, easeIn));
    }

    [ContextMenu("Test / Uncover (with delay)")]
    private void _TestUncoverOnly()
    {
        var inst = Instance;
        StopAllCoroutines();
        NormalizeDirection();
        ApplySpriteSize();
        RecomputePositions();
        StartCoroutine(Co_UncoverWithDelay(waitAfterSceneLoaded));
    }
#endif
}





/*
public class FadeScreen : MonoBehaviour
{
    public static FadeScreen Instance;
    [SerializeField] Image SceneBlackScreen;

    [Header("フェードアウト・インのかかる時間")]
    public float fadeTime;      //3.0f→3秒


    private bool isFadeStart = false;
    private float elapseTime = 0.0f;    //フェードの経過時間
    private bool isSceneStarted = false;
    private bool isSceneEnding = false;
    private string sceneName;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        if(SceneManager.GetActiveScene().name != "TitleScene")
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        Debug.Log("fadeScreen wake");

        SceneBlackScreen.gameObject.SetActive(false);
    }

    public void SceneStartListener(Scene nextScene, LoadSceneMode mode)
    {
        isSceneStarted = true;
    }

    void Update()
    {
        if(isSceneStarted && FadeInScreen())
        {
            PlayerInput.isPausing = false;
            Time.timeScale = 1.0f;
            isSceneStarted=false;
        }

        if (isSceneEnding)
        {
            LoadSceneWithFadeOut();
        }
    }

    public void StartFadeOut(string _sceneName)
    {
        isSceneEnding = true;
        sceneName=_sceneName;
    }


    public void StartFadeIn()
    {
        isSceneStarted = true;
    }

    private void LoadSceneWithFadeOut()
    {
        PlayerInput.isPausing = true;
        Time.timeScale = 0.0f;
        if (FadeOutScreen())
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    public bool FadeOutScreen()
    {
        if (!isFadeStart)   //フェードスタート時に行う処理
        {
            SceneBlackScreen.gameObject.SetActive(true);
            elapseTime = 0.0f;
            isFadeStart = true;
            return false;
        }
        else
        {
            if (elapseTime < fadeTime)
            {
                elapseTime += Time.unscaledDeltaTime;
                Color nowColor = SceneBlackScreen.color;
                nowColor.a = Mathf.Lerp(0.0f, 1.0f, elapseTime / fadeTime);
                SceneBlackScreen.color = nowColor;
                return false;
            }
            else
            {
                Color nowColor = SceneBlackScreen.color;
                nowColor.a = 1.0f;  //完全に暗転
                SceneBlackScreen.color = nowColor;
                isFadeStart = false;
                isSceneEnding = false;
                return true;
            }
        }
    }

 
}
*/