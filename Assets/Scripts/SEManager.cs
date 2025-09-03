using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SEManager : MonoBehaviour
{
    public static SEManager instance;//OneShotSE�ŕK�v

    public AudioSource audioSource;

    // �Đ����������ʉ��������ɓo�^�iInspector�Őݒ�j
    //�v���C���[
    public AudioClip jumpSE;
    public AudioClip damageSE;
    public AudioClip landingSE;//
    public AudioClip deathbiribiriSE;
    public AudioClip deathyoinSE;
    public AudioClip diveSE;

    //�R�s�y�n
    public AudioClip kachaSE;//�V���b�^�[��
    public AudioClip copySE;//�V���b�^�[��
    public AudioClip cutshortSE;//�͂���
    public AudioClip flip1SE;//
    public AudioClip flip2SE;//�y�[�X�g��
    public AudioClip katiSE;//�͈͑I�����A�ړ����邽�тɂȂ�
    public AudioClip writeSE;

    //UI�n
    public AudioClip closeSE;//
    public AudioClip decideSE;//
    public AudioClip undoSE;//

    public AudioClip iitokiSE;//
    public AudioClip donpafuSE;//
    public AudioClip cheersASE;//
    public AudioClip cheersSSE;//
    public AudioClip levelupSE;
    public AudioClip clearSuikomiSE;//


    //�I�u�W�F�N�g
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

    /// <summary>
    ///�g�p��:SEManager.instance.ClipAtPointSE(SEManager.instance.jumpSE);
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
        audioSource.PlayOneShot(clip); // �������g����
    }




    /// <summary>
    /// �t�F�[�h�C���E�t�F�[�h�A�E�g�t���Ō��ʉ����Đ�
    /// �g�p��@SEManager.instance.ClipAtPointWithFadeInOut(clip, 3f); // 3�b�Đ��i0.2�b�t�F�[�h�C���A0.8�b�t�F�[�h�A�E�g�j
    /// </summary>
    public void ClipAtPointWithFadeInOut(AudioClip clip, float duration, float fadeInTime = 0.2f, float fadeOutTime = 0.8f, float maxVolume = 1f)
    {
        if (clip == null || duration <= 0f) return;

        // �t�F�[�h���Ԃ��Đ����Ԃ𒴂��Ă����璲��
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

        float playTime = Mathf.Max(0, duration); // �Đ�����

        StartCoroutine(FadeInOutRoutine(source, fadeInTime, fadeOutTime, maxVolume, playTime, audioObj));
    }

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

        // �t�F�[�h�C���� �� �Đ��ێ��i�t�F�[�h�A�E�g�O�܂Łj
        float sustainTime = playTime - fadeInTime - fadeOutTime;
        if (sustainTime > 0)
            yield return new WaitForSeconds(sustainTime);

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