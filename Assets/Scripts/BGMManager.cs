using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;

    private AudioSource ambientsound;
    private AudioSource mainBGM;

    //環境音
    public AudioClip bard;
    public AudioClip calm;
    public AudioClip park;
    public AudioClip semi;

    //BGM
    public AudioClip groundStage;
    public AudioClip catsleStage;
    public AudioClip cosmicStage;
    /// <summary>
    /// scene開始時にBGMと環境音を設定
    /// scene遷移する際はStopAllBGM
    /// ゲームオーバー時は環境音のみ
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // AudioSourceを2つ用意
            ambientsound = gameObject.AddComponent<AudioSource>();
            mainBGM = gameObject.AddComponent<AudioSource>();

            ambientsound.loop = true;
            mainBGM.loop = true;
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
            ambientsound.clip = clip1;
            ambientsound.Play();
        }

        if (clip2 != null)
        {
            mainBGM.clip = clip2;
            mainBGM.Play();
        }
    }

    public void StopAllBGM()
    {
        ambientsound.Stop();
        mainBGM.Stop();
    }

    public void SetVolumes(float vol1, float vol2)
    {
        ambientsound.volume = vol1;
        mainBGM.volume = vol2;
    }
}
