using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeScreen : MonoBehaviour
{
    public static FadeScreen Instance;
    [SerializeField] Image SceneBlackScreen;

    [Header("フェードアウト・インのかかる時間")]
    public float fadeTime;      //3.0f→3秒


    private bool isFadeStart = false;
    private float elapseTime = 0.0f;    //フェードの経過時間
    private bool isSceneStarted = false;
    private bool isSceneEnding = false;
    private string sceneName;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        if(SceneManager.GetActiveScene().name != "TitleScene")
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        Debug.Log("fadeScreen wake");

        SceneBlackScreen.gameObject.SetActive(false);
    }

    public void SceneStartListener(Scene nextScene, LoadSceneMode mode)
    {
        isSceneStarted = true;
    }

    void Update()
    {
        if(isSceneStarted && FadeInScreen())
        {
            PlayerInput.isPausing = false;
            Time.timeScale = 1.0f;
            isSceneStarted=false;
        }

        if (isSceneEnding)
        {
            LoadSceneWithFadeOut();
        }
    }

    public void StartFadeOut(string _sceneName)
    {
        isSceneEnding = true;
        sceneName=_sceneName;
    }


    public void StartFadeIn()
    {
        isSceneStarted = true;
    }

    private void LoadSceneWithFadeOut()
    {
        PlayerInput.isPausing = true;
        Time.timeScale = 0.0f;
        if (FadeOutScreen())
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    public bool FadeOutScreen()
    {
        if (!isFadeStart)   //フェードスタート時に行う処理
        {
            SceneBlackScreen.gameObject.SetActive(true);
            elapseTime = 0.0f;
            isFadeStart = true;
            return false;
        }
        else
        {
            if (elapseTime < fadeTime)
            {
                elapseTime += Time.unscaledDeltaTime;
                Color nowColor = SceneBlackScreen.color;
                nowColor.a = Mathf.Lerp(0.0f, 1.0f, elapseTime / fadeTime);
                SceneBlackScreen.color = nowColor;
                return false;
            }
            else
            {
                Color nowColor = SceneBlackScreen.color;
                nowColor.a = 1.0f;  //完全に暗転
                SceneBlackScreen.color = nowColor;
                isFadeStart = false;
                isSceneEnding = false;
                return true;
            }
        }
    }

    public bool FadeInScreen()
    {
        if (!isFadeStart)   //フェードスタート時に行う処理
        {
            elapseTime = 0.0f;
            isFadeStart = true;
            return false;
        }
        else
        {
            if (elapseTime < fadeTime)
            {
                elapseTime += Time.unscaledDeltaTime;
                Color nowColor = SceneBlackScreen.color;
                nowColor.a = Mathf.Lerp(1.0f, 0.0f, elapseTime / fadeTime);
                SceneBlackScreen.color = nowColor;
                return false;
            }
            else
            {
                Color nowColor = SceneBlackScreen.color;
                nowColor.a = 0.0f;  //完全に明転
                SceneBlackScreen.color = nowColor;
                isFadeStart = false;
                SceneBlackScreen.gameObject.SetActive(false);
                return true;
            }
        }
    }
}
