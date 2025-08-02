using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SEManager : MonoBehaviour
{
    public static SEManager instance;//OneShotSEで必要

    public AudioSource audioSource;

    // 再生したい効果音をここに登録（Inspectorで設定）
    public AudioClip jumpSE;
    public AudioClip blackhole1SE;
    public AudioClip blackhole2SE;
    public AudioClip bumper2SE;
    public AudioClip cheersSE;
    public AudioClip cheers2SE;
    public AudioClip clearSuikomiSE;
    public AudioClip closeSE;
    public AudioClip copySE;
    public AudioClip cutshortSE;
    public AudioClip damageSE;
    public AudioClip death1SE;
    public AudioClip death2SE;
    public AudioClip decideSE;
    public AudioClip flip1SE;
    public AudioClip healSE;
    public AudioClip iitokiSE;
    public AudioClip keyOpenSE;
    public AudioClip landingSE;
    public AudioClip mekuruSE;


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
    /// </summary>
    public void ClipAtPointWithFadeInOut(AudioClip clip, float fadeInTime = 0.2f, float fadeOutTime = 0.8f, float maxVolume = 1f)
    {
        if (clip == null) return;

        GameObject audioObj = new GameObject("SE_" + clip.name);
        audioObj.transform.position = Camera.main.transform.position;

        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = 0f;
        source.Play();

        float clipLength = clip.length;
        float playTime = Mathf.Max(0, clipLength - fadeOutTime); // フェードアウト前まで再生

        StartCoroutine(FadeInOutRoutine(source, fadeInTime, fadeOutTime, maxVolume, playTime, audioObj));
    }

    /// <summary>
    /// フェードインして再生し、終わり際にフェードアウト
    /// </summary>
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

        // フェードイン後 → 再生を維持（フェードアウト開始まで待つ）
        yield return new WaitForSeconds(playTime - fadeInTime);

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