using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseInStage : MonoBehaviour
{
    [SerializeField] Canvas canvas;

    private StageManager stageMgr;
    private FadeScreen fadeCurtain;
    private BGMManager m_BGMManager;
    // Start is called before the first frame update
    void Start()
    {
        canvas.enabled = false;
        fadeCurtain = FindObjectOfType<FadeScreen>();
    }

    // Update is called once per frame
    void Update()
    {
        if (stageMgr == null) stageMgr = FindObjectOfType<StageManager>();
        this.transform.position =
            new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z + 1);
        if (PlayerInput.GetKeyDown(KeyCode.Escape) && (!stageMgr.isSelectZone && !stageMgr.isPasting))
        {
            if (m_BGMManager == null) m_BGMManager = FindObjectOfType<BGMManager>();
            m_BGMManager.DecreaseBGMVolume();
            canvas.enabled = true;
            PlayerInput.isPausing = true;
            Time.timeScale = 0f;
        }
    }

    public void PauseOff()
    {
        canvas.enabled = false;
        Time.timeScale = 1.0f;
        m_BGMManager.ResetBGMVolume();
        StartCoroutine(DoAfterFrame());
    }

    public void LoadStageSelect()
    {
        PauseOff();
        if (fadeCurtain == null) fadeCurtain = FindObjectOfType<FadeScreen>();
        fadeCurtain.StartFadeOut("StageSelectScene");
        //SceneManager.LoadScene("StageSelectScene");
    }

    IEnumerator DoAfterFrame()
    {
        // そのフレームの Update, LateUpdate, 物理処理 などが終わるのを待つ
        yield return new WaitForEndOfFrame();

        // ここに処理を書くと、レンダリング直前に実行される

        PlayerInput.isPausing = false;
    }
}
