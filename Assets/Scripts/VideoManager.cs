using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using System.Collections;

public class VideoManager : MonoBehaviour///��d�ɉf�����Đ��ł��Ȃ��d�l�ɂȂ��Ă��܂��B
{
    public static VideoManager Instance;

    private VideoPlayer videoPlayer;

    // ���ݓo�^���̏I���R�[���o�b�N�i���d�o�^�h�~�p�j
    private UnityAction currentOnFinished;

    // ���[�v�Đ����A�e���[�v���B���ƂɌĂԃR�[���o�b�N�i�C�Ӂj
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
        videoPlayer.waitForFirstFrame = true; // �^�����u�ԑ΍�

        // �f�o�b�O�p
        videoPlayer.errorReceived += (vp, msg) => Debug.LogError($"[VideoManager] Video error: {msg}");
        videoPlayer.prepareCompleted += (vp) => Debug.Log("[VideoManager] prepareCompleted");
    }

    /// <summary>
    /// ������Đ��i�I�����R�[���o�b�N�C�Ӂj�BPrepare ���o�R���Ċm���ɍĐ��B
    /// </summary>
    public void PlayVideo(VideoClip clip, UnityAction onFinished = null)
    {
        StartCoroutine(PreparePlayInternal(clip, onFinished, loop: false, onEachLoop: null));
    }

    // �⏕�FPlayAndWait ����ĂԂ��߂̓��`���\�b�h
    public void PlayClip(VideoClip clip, UnityAction onFinished = null)
    {
        PlayVideo(clip, onFinished);
    }

    /// <summary>
    /// �R���[�`���Łu�Đ����I���܂ő҂v
    /// </summary>
    public IEnumerator PlayAndWait(VideoClip clip)
    {
        bool finished = false;
        Debug.Log("[VideoManager] �Đ�kaisi");

        yield return PreparePlayInternal(clip, () => finished = true, loop: false, onEachLoop: null);

        // �I������܂Ŗ��t���[���ҋ@
        while (!finished)
            yield return null;

        Debug.Log("[VideoManager] �Đ�owari");
    }

    private void OnLoopPointReached(VideoPlayer vp)
    {
        var cb = currentOnFinished;
        currentOnFinished = null;
        videoPlayer.loopPointReached -= OnLoopPointReached;
        cb?.Invoke();
    }

    /// <summary>
    /// ���[�v�Đ��B�e���[�v���B���Ƃ� onEachLoop ���Ăԁi�C�Ӂj
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
    /// �Đ���~�i���[�v/�ʏ�ǂ���ł��j�B���[�v�ݒ�ƃn���h�������Z�b�g�B
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

    // ---- �������|�C���g�F�m���� Prepare ��҂��Ă��� Play ���� ----
    private IEnumerator PreparePlayInternal(VideoClip clip, UnityAction onFinished, bool loop, UnityAction onEachLoop)
    {
        if (clip == null)
        {
            Debug.LogWarning("[VideoManager] VideoClip ���ݒ肳��Ă��܂���B");
            yield break;
        }

        // �����̃n���h������
        videoPlayer.loopPointReached -= OnLoopPointReached;
        videoPlayer.loopPointReached -= OnLoopPointReachedLoop;

        currentOnFinished = onFinished;
        currentOnLoop = onEachLoop;
        videoPlayer.isLooping = loop;

        if (loop && currentOnLoop != null)
            videoPlayer.loopPointReached += OnLoopPointReachedLoop;
        if (!loop && currentOnFinished != null)
            videoPlayer.loopPointReached += OnLoopPointReached;

        // �N���b�v�����ւ� �� Prepare
        videoPlayer.clip = clip;
        videoPlayer.Prepare();

        // ���������܂őҋ@�iTime.timeScale=0 �ł����j
        while (!videoPlayer.isPrepared)
            yield return null;

        // �Đ�
        videoPlayer.Play();
        Debug.Log($"[VideoManager] Play: {clip.name}, loop={loop}");
    }
}

