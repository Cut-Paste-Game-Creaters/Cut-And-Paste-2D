using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SEManager : MonoBehaviour
{
    public static SEManager instance;//OneShotSE�ŕK�v

    public AudioSource audioSource;

    // �Đ����������ʉ��������ɓo�^�iInspector�Őݒ�j
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
        // �V���O���g���p�^�[����1�ɐ���
        if (instance == null)
        {
            instance = this; //�ǂ�����ł����ڃA�N�Z�X�ł���悤�ɂ���
            DontDestroyOnLoad(gameObject); // �V�[�����܂����Ŏc��
        }
        else
        {
            Destroy(gameObject); // �d���r��
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
        audioSource.PlayOneShot(clip); // �������g����
    }




    /// <summary>
    /// �t�F�[�h�C���E�t�F�[�h�A�E�g�t���Ō��ʉ����Đ�
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
        float playTime = Mathf.Max(0, clipLength - fadeOutTime); // �t�F�[�h�A�E�g�O�܂ōĐ�

        StartCoroutine(FadeInOutRoutine(source, fadeInTime, fadeOutTime, maxVolume, playTime, audioObj));
    }

    /// <summary>
    /// �t�F�[�h�C�����čĐ����A�I���ۂɃt�F�[�h�A�E�g
    /// </summary>
    private IEnumerator FadeInOutRoutine(AudioSource source, float fadeInTime, float fadeOutTime, float maxVolume, float playTime, GameObject objToDestroy)
    {
        // �t�F�[�h�C��
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, maxVolume, t / fadeInTime);
            yield return null;
        }

        source.volume = maxVolume;

        // �t�F�[�h�C���� �� �Đ����ێ��i�t�F�[�h�A�E�g�J�n�܂ő҂j
        yield return new WaitForSeconds(playTime - fadeInTime);

        // �t�F�[�h�A�E�g
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