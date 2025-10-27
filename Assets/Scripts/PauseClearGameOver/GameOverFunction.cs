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
    [SerializeField] private Transform cameraFocusTarget; // �񂹂����Ώہi��Fplayer.transform�j
    [SerializeField] private float cameraMoveDistance = 3f; // �ǂꂾ���񂹂邩
    [SerializeField] private float cameraMoveDuration = 0.6f; // ���b�����Ċ񂹂邩�i�����ԁj
    [SerializeField] private bool cameraLookAtTarget = false; // �K�v�Ȃ璍��

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

        // �� �ǉ��F���̃J�����p����ێ�
        if (Camera.main != null)
        {
            _camOriginalPos = Camera.main.transform.position;
            _camOriginalRot = Camera.main.transform.rotation;
        }

        // �f�t�H���g�Ńv���C���[�Ɋ񂹂����ꍇ
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

        yield return new WaitForSecondsRealtime(0.8f);//���o���I���܂ő҂�

        SEManager.instance.ClipAtPointSE(SEManager.instance.deathbiribiriSE);
        SEManager.instance.ClipAtPointSE(SEManager.instance.deathyoinSE);
        yield return new WaitForSecondsRealtime(1.5f);//���o���I���܂ő҂�

        ///�J�����ړ�
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

        // �J�n�ʒu�i�J�����̌��݈ʒu�j
        Vector3 from = cam.position;
        // �ڕW�ʒu�iXY ���^�[�Q�b�g�Ɉ�v�AZ �̓J�����̂܂܁j
        Vector3 to = new Vector3(cameraFocusTarget.position.x,
                                 cameraFocusTarget.position.y,
                                 cam.position.z);

        float t = 0f;
        while (t < cameraMoveDuration)
        {
            t += Time.unscaledDeltaTime;               // Time.timeScale=0 �ł��i��
            float u = Mathf.Clamp01(t / cameraMoveDuration);
            u = Mathf.SmoothStep(0f, 1f, u);          // �����}�C���h��

            cam.position = Vector3.Lerp(from, to, u);

            if (cameraLookAtTarget)
            {
                Vector3 lookPos = cameraFocusTarget.position;
                lookPos.z = cam.position.z;           // 2D�z��FZ �͍��킹��
                cam.LookAt(lookPos);
            }

            yield return null;
        }

        // �덷�Ȃ��s�^�b�ƍ��킹��i�X�i�b�v�j
        cam.position = to;
    }

    IEnumerator DoAfterFrame()
    {
        // ���̃t���[���� Update, LateUpdate, �������� �Ȃǂ��I���̂�҂�
        yield return new WaitForEndOfFrame();

        // �����ɏ����������ƁA�����_�����O���O�Ɏ��s�����

        PlayerInput.isPausing = false;
    }
}
