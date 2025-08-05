using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;

    private AudioSource bgmSource1;
    private AudioSource bgmSource2;

    public AudioClip ambientsound;//環境音
    public AudioClip mainBGM;
    /// <summary>
    /// 初めにBGMと環境音を設定
    /// 現実っぽくしたいときはBGM:環境音＝１：９
    /// ゲームオーバー時は環境音のみ
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // AudioSourceを2つ用意
            bgmSource1 = gameObject.AddComponent<AudioSource>();
            bgmSource2 = gameObject.AddComponent<AudioSource>();

            bgmSource1.loop = true;
            bgmSource2.loop = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayDualBGM(AudioClip clip1, AudioClip clip2)
    {
        if (clip1 != null)
        {
            bgmSource1.clip = clip1;
            bgmSource1.Play();
        }

        if (clip2 != null)
        {
            bgmSource2.clip = clip2;
            bgmSource2.Play();
        }
    }

    public void StopAllBGM()
    {
        bgmSource1.Stop();
        bgmSource2.Stop();
    }

    public void SetVolumes(float vol1, float vol2)
    {
        bgmSource1.volume = vol1;
        bgmSource2.volume = vol2;
    }
}
