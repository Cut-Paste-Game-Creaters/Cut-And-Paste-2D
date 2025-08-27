using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class BGMManager : MonoBehaviour
{
    [System.Serializable] // Unity�G�f�B�^�ŃV���A���C�Y�\�ɂ���
    public class Pair<T1, T2>
    {
        public T1 first;
        public T2 second;

        public Pair(T1 first, T2 second)
        {
            this.first = first;
            this.second = second;
        }
    }

    public static BGMManager instance;

    [SerializeField] private float lowVolume = 0.0f;
    [SerializeField] private Pair<AudioClip, AudioClip> stageSelectSounds;
    [SerializeField] private Pair<AudioClip, AudioClip>[] sceneSounds;

    private AudioSource ambientsound;
    private AudioSource mainBGM;

    public Dictionary<string, int> stageNumber = new Dictionary<string, int>() // Dictionary�N���X�̐錾�Ə����l�̐ݒ�
    {
        {"Stage1", 0},
        {"Stage2", 1},
        {"Stage3", 2},
        {"Stage4", 3},
        {"Stage5", 4},
        {"Stage6", 5},
        {"Stage7", 6},
        {"Stage8", 7},
        {"Stage9", 8},
        {"Stage10", 9},
        {"StageTemplate",10 }
    };

    /// <summary>
    /// scene�J�n����BGM�Ɗ�����ݒ�
    /// scene�J�ڂ���ۂ�StopAllBGM
    /// �Q�[���I�[�o�[���͊����̂�
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // AudioSource��2�p��
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

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "StageSelectScene")
        {
            PlayDualBGM(stageSelectSounds.first, stageSelectSounds.second);
        }
    }

    public void OnSceneLoaded(Scene nextScene, LoadSceneMode mode)
    {
        StopAllBGM();
        if(SceneManager.GetActiveScene().name == "StageSelectScene")
        {
            PlayDualBGM(stageSelectSounds.first,stageSelectSounds.second);
        }
        else if (Regex.IsMatch(SceneManager.GetActiveScene().name, @"^Stage\d+$")) //�V�[������Stage�Ȃ�Ƃ��Ȃ�
        {
            int stage_num = stageNumber[SceneManager.GetActiveScene().name];
            PlayDualBGM(sceneSounds[stage_num].first, sceneSounds[stage_num].second);
        }
    }

    public void PlayDualBGM(AudioClip clip1, AudioClip clip2)
    {

        if (clip1 != null)
        {
            mainBGM.clip = clip1;
            mainBGM.Play();
        }
        if (clip2 != null)
        {
            ambientsound.clip = clip2;
            ambientsound.Play();
            ambientsound.volume = 0.1f;
        }
    }

    public void StopBackgroundMusic()
    {
        mainBGM.Stop();
    }

    public void DecreaseBGMVolume()
    {
        mainBGM.volume = 0f;
        ambientsound.volume = 0.3f;
    }

    public void ResetBGMVolume()
    {
        mainBGM.volume = 1.0f;
        ambientsound.volume = 0.1f;
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
