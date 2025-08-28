using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Video;

public class ClearFunction : MonoBehaviour
{
    public VideoClip ClearMovie;
    public VideoClip ResultMovie;
    [SerializeField] private UnityEngine.UI.RawImage movieImage;
    [SerializeField] private GameObject ResultUI;
    private Canvas canvas;
    private bool isClear = false;
    private FadeScreen fadeCurtain;
    // Start is called before the first frame update
    void Start()
    {

        canvas = GetComponent<Canvas>();
        canvas.enabled = false;
        isClear = false;
        fadeCurtain = FindObjectOfType<FadeScreen>();
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position =
            new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z + 1);
    }

    public bool GetisClear()
    {
        return isClear;
    }

    public IEnumerator GameClear()
    {
        PlayerInput.isPausing = true;
        isClear = true;
        Time.timeScale = 0f;

        ResultUI.SetActive(false);//演出が終わるまで非表示
        canvas.enabled = true;
        StartCoroutine(ShowClearUI());

        yield return new WaitForSecondsRealtime(1f);//演出が終わるまで待つ
        ResultUI.SetActive(true);



    }

    public void PauseOff()
    {
        canvas.enabled = false;
        PlayerInput.isPausing = false;
        Time.timeScale = 1.0f;
    }

    public void LoadStageSelect()
    {
        PauseOff();
        if (fadeCurtain == null) fadeCurtain = FindObjectOfType<FadeScreen>();
        fadeCurtain.StartFadeOut("StageSelectScene");
        //SceneManager.LoadScene("StageSelectScene");
    }

    private IEnumerator ShowClearUI()
    {

        movieImage.enabled = true;
        yield return VideoManager.Instance.PlayAndWait(ClearMovie);
        movieImage.enabled = false; // ← 黒画面を隠す


    }





}
