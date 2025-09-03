using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SEManager : MonoBehaviour
{
    public static SEManager instance;//OneShotSEで必要

    public AudioSource audioSource;

    // 再生したい効果音をここに登録（Inspectorで設定）
    //プレイヤー
    public AudioClip jumpSE;
    public AudioClip damageSE;
    public AudioClip landingSE;//
    public AudioClip deathbiribiriSE;
    public AudioClip deathyoinSE;
    public AudioClip diveSE;

    //コピペ系
    public AudioClip kachaSE;//シャッター音
    public AudioClip copySE;//シャッター音
    public AudioClip cutshortSE;//はさみ
    public AudioClip flip1SE;//
    public AudioClip flip2SE;//ペースト時
    public AudioClip katiSE;//範囲選択時、移動するたびになる
    public AudioClip writeSE;

    //UI系
    public AudioClip closeSE;//
    public AudioClip decideSE;//
    public AudioClip undoSE;//

    public AudioClip iitokiSE;//
    public AudioClip donpafuSE;//
    public AudioClip cheersASE;//
    public AudioClip cheersSSE;//
    public AudioClip levelupSE;
    public AudioClip clearSuikomiSE;//


    //オブジェクト
    public AudioClip blackholeSE;
    public AudioClip blackholeyoinSE;
    public AudioClip bumperSE;
    public AudioClip healSE;
    public AudioClip keyOpenSE;
    public AudioClip CannonfireSE;
    public AudioClip SwitchSE;











    void Update()
    {

    }


private void Awake()
    {
        // シングルトンパターンで1つに制御
        if (instance == null)
        {
            instance = this; //どこからでも直接アクセスできるようにする
            DontDestroyOnLoad(gameObject); // シーンをまたいで残す
        }
        else
        {
            Destroy(gameObject); // 重複排除
        }
    }

    /// <summary>
    ///使用例:SEManager.instance.ClipAtPointSE(SEManager.instance.jumpSE);
    /// </summary>
    /// <param name="clip"></param>
    public void ClipAtPointSE(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        }
    }

    public void OneShotSE(AudioClip clip)
    {
        audioSource.PlayOneShot(clip); // 正しい使い方
    }




    /// <summary>
    /// フェードイン・フェードアウト付きで効果音を再生
    /// 使用例　SEManager.instance.ClipAtPointWithFadeInOut(clip, 3f); // 3秒再生（0.2秒フェードイン、0.8秒フェードアウト）
    /// </summary>
    public void ClipAtPointWithFadeInOut(AudioClip clip, float duration, float fadeInTime = 0.2f, float fadeOutTime = 0.8f, float maxVolume = 1f)
    {
        if (clip == null || duration <= 0f) return;

        // フェード時間が再生時間を超えていたら調整
        float totalFade = fadeInTime + fadeOutTime;
        if (totalFade > duration)
        {
            float scale = duration / totalFade;
            fadeInTime *= scale;
            fadeOutTime *= scale;
        }

        GameObject audioObj = new GameObject("SE_" + clip.name);
        audioObj.transform.position = Camera.main.transform.position;

        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = 0f;
        source.Play();

        float playTime = Mathf.Max(0, duration); // 再生時間

        StartCoroutine(FadeInOutRoutine(source, fadeInTime, fadeOutTime, maxVolume, playTime, audioObj));
    }

    private IEnumerator FadeInOutRoutine(AudioSource source, float fadeInTime, float fadeOutTime, float maxVolume, float playTime, GameObject objToDestroy)
    {
        // フェードイン
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, maxVolume, t / fadeInTime);
            yield return null;
        }

        source.volume = maxVolume;

        // フェードイン後 → 再生維持（フェードアウト前まで）
        float sustainTime = playTime - fadeInTime - fadeOutTime;
        if (sustainTime > 0)
            yield return new WaitForSeconds(sustainTime);

        // フェードアウト
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(maxVolume, 0f, t / fadeOutTime);
            yield return null;
        }

        source.Stop();
        Destroy(objToDestroy);
    }




}