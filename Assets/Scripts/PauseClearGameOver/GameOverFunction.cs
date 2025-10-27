using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverFunction : MonoBehaviour
{

    private GameObject player;
    private Vector3 initPlayerPos;
    private FadeScreen fadeCurtain;
    private BGMManager m_BGMManager;
    private RandomRingWarp2D warper;
    [SerializeField] private GameObject GameoverUI;
    [SerializeField] private Transform cameraFocusTarget; // 寄せたい対象（例：player.transform）
    [SerializeField] private float cameraMoveDistance = 3f; // どれだけ寄せるか
    [SerializeField] private float cameraMoveDuration = 0.6f; // 何秒かけて寄せるか（実時間）
    [SerializeField] private bool cameraLookAtTarget = false; // 必要なら注視

    private Vector3 _camOriginalPos;
    private Quaternion _camOriginalRot;


    // Start is called before the first frame update
    void Start()
    {
        GameoverUI.SetActive(false);

        player = GameObject.FindWithTag("Player");
        initPlayerPos = player.transform.position;

        fadeCurtain = FindObjectOfType<FadeScreen>();
        warper = GetComponent<RandomRingWarp2D>();

        // ★ 追加：元のカメラ姿勢を保持
        if (Camera.main != null)
        {
            _camOriginalPos = Camera.main.transform.position;
            _camOriginalRot = Camera.main.transform.rotation;
        }

        // デフォルトでプレイヤーに寄せたい場合
        if (cameraFocusTarget == null && player != null)
        {
            cameraFocusTarget = player.transform;
        }
    }


    // Update is called once per frame
    void Update()
    {
        // this.transform.position =
        //     new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z + 1);
    }

    public IEnumerator GameOver()
    {

        PlayerInput.isPausing = true;
        Time.timeScale = 0f;
        if (m_BGMManager == null) m_BGMManager = FindObjectOfType<BGMManager>();
        m_BGMManager.StopBackgroundMusic();
        GameoverUI.SetActive(true);
        if (warper != null)
        {
            warper.WarpRandomAroundCamera2D();
        }

        yield return new WaitForSecondsRealtime(0.8f);//演出が終わるまで待つ

        SEManager.instance.ClipAtPointSE(SEManager.instance.deathbiribiriSE);
        SEManager.instance.ClipAtPointSE(SEManager.instance.deathyoinSE);
        yield return new WaitForSecondsRealtime(1.5f);//演出が終わるまで待つ

        ///カメラ移動
        yield return StartCoroutine(MoveCameraTowardsTarget());

    }

    public void PauseOff()
    {
        player.transform.position = initPlayerPos;

        if (Camera.main != null)
        {
            Camera.main.transform.position = _camOriginalPos;
            Camera.main.transform.rotation = _camOriginalRot;
        }
        Time.timeScale = 1.0f;
        GameoverUI.SetActive(false);
        StartCoroutine(DoAfterFrame());
    }

    public void LoadStageSelect()
    {
        PauseOff();
        fadeCurtain.StartFadeOut("StageSelectScene");
        //SceneManager.LoadScene("StageSelectScene");
    }


    private IEnumerator MoveCameraTowardsTarget()
    {
        if (Camera.main == null || cameraFocusTarget == null) yield break;

        Transform cam = Camera.main.transform;

        // 開始位置（カメラの現在位置）
        Vector3 from = cam.position;
        // 目標位置（XY をターゲットに一致、Z はカメラのまま）
        Vector3 to = new Vector3(cameraFocusTarget.position.x,
                                 cameraFocusTarget.position.y,
                                 cam.position.z);

        float t = 0f;
        while (t < cameraMoveDuration)
        {
            t += Time.unscaledDeltaTime;               // Time.timeScale=0 でも進む
            float u = Mathf.Clamp01(t / cameraMoveDuration);
            u = Mathf.SmoothStep(0f, 1f, u);          // 少しマイルドに

            cam.position = Vector3.Lerp(from, to, u);

            if (cameraLookAtTarget)
            {
                Vector3 lookPos = cameraFocusTarget.position;
                lookPos.z = cam.position.z;           // 2D想定：Z は合わせる
                cam.LookAt(lookPos);
            }

            yield return null;
        }

        // 誤差なくピタッと合わせる（スナップ）
        cam.position = to;
    }

    IEnumerator DoAfterFrame()
    {
        // そのフレームの Update, LateUpdate, 物理処理 などが終わるのを待つ
        yield return new WaitForEndOfFrame();

        // ここに処理を書くと、レンダリング直前に実行される

        PlayerInput.isPausing = false;
    }
}
