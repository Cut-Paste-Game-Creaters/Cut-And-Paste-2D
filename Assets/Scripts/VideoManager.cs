using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using System.Collections;

public class VideoManager : MonoBehaviour///二重に映像が再生できない仕様になっています。
{
    public static VideoManager Instance;

    private VideoPlayer videoPlayer;

    // 現在登録中の終了コールバック（多重登録防止用）
    private UnityAction currentOnFinished;

    // ループ再生時、各ループ到達ごとに呼ぶコールバック（任意）
    private UnityAction currentOnLoop;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null) videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.isLooping = false;
        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true; // 真っ黒瞬間対策

        // デバッグ用
        videoPlayer.errorReceived += (vp, msg) => Debug.LogError($"[VideoManager] Video error: {msg}");
        videoPlayer.prepareCompleted += (vp) => Debug.Log("[VideoManager] prepareCompleted");
    }

    /// <summary>
    /// 動画を再生（終了時コールバック任意）。Prepare を経由して確実に再生。
    /// </summary>
    public void PlayVideo(VideoClip clip, UnityAction onFinished = null)
    {
        StartCoroutine(PreparePlayInternal(clip, onFinished, loop: false, onEachLoop: null));
    }

    // 補助：PlayAndWait から呼ぶための同義メソッド
    public void PlayClip(VideoClip clip, UnityAction onFinished = null)
    {
        PlayVideo(clip, onFinished);
    }

    /// <summary>
    /// コルーチンで「再生が終わるまで待つ」
    /// </summary>
    public IEnumerator PlayAndWait(VideoClip clip)
    {
        bool finished = false;
        Debug.Log("[VideoManager] 再生kaisi");

        yield return PreparePlayInternal(clip, () => finished = true, loop: false, onEachLoop: null);

        // 終了するまで毎フレーム待機
        while (!finished)
            yield return null;

        Debug.Log("[VideoManager] 再生owari");
    }

    private void OnLoopPointReached(VideoPlayer vp)
    {
        var cb = currentOnFinished;
        currentOnFinished = null;
        videoPlayer.loopPointReached -= OnLoopPointReached;
        cb?.Invoke();
    }

    /// <summary>
    /// ループ再生。各ループ到達ごとに onEachLoop を呼ぶ（任意）
    /// </summary>
    public void PlayLoop(VideoClip clip, UnityAction onEachLoop = null)
    {
        StartCoroutine(PreparePlayInternal(clip, onFinished: null, loop: true, onEachLoop: onEachLoop));
    }

    private void OnLoopPointReachedLoop(VideoPlayer vp)
    {
        currentOnLoop?.Invoke();
    }

    /// <summary>
    /// 再生停止（ループ/通常どちらでも）。ループ設定とハンドラをリセット。
    /// </summary>
    public void StopVideo()
    {
        videoPlayer.loopPointReached -= OnLoopPointReached;
        videoPlayer.loopPointReached -= OnLoopPointReachedLoop;
        currentOnFinished = null;
        currentOnLoop = null;

        videoPlayer.isLooping = false;
        if (videoPlayer.isPlaying) videoPlayer.Stop();
    }

    // ---- ここがポイント：確実に Prepare を待ってから Play する ----
    private IEnumerator PreparePlayInternal(VideoClip clip, UnityAction onFinished, bool loop, UnityAction onEachLoop)
    {
        if (clip == null)
        {
            Debug.LogWarning("[VideoManager] VideoClip が設定されていません。");
            yield break;
        }

        // 既存のハンドラ解除
        videoPlayer.loopPointReached -= OnLoopPointReached;
        videoPlayer.loopPointReached -= OnLoopPointReachedLoop;

        currentOnFinished = onFinished;
        currentOnLoop = onEachLoop;
        videoPlayer.isLooping = loop;

        if (loop && currentOnLoop != null)
            videoPlayer.loopPointReached += OnLoopPointReachedLoop;
        if (!loop && currentOnFinished != null)
            videoPlayer.loopPointReached += OnLoopPointReached;

        // クリップ差し替え → Prepare
        videoPlayer.clip = clip;
        videoPlayer.Prepare();

        // 準備完了まで待機（Time.timeScale=0 でも回る）
        while (!videoPlayer.isPrepared)
            yield return null;

        // 再生
        videoPlayer.Play();
        Debug.Log($"[VideoManager] Play: {clip.name}, loop={loop}");
    }
}

