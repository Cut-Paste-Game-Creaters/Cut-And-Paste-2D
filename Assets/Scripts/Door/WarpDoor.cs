using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WarpDoor : MonoBehaviour
{
    [SerializeField] GameObject rankText_S;
    [SerializeField] GameObject rankText_A;
    [SerializeField] GameObject rankText_B;
    [SerializeField] GameObject rankText_C;
    [SerializeField] GameObject rankText_F;

    Dictionary<string, GameObject> rankText;
    public string stageName;
    public StageManager stageMgr;
    public bool stopLoad = false;
    private RankJudgeAndUpdateFunction judgeFunc;
    private ClearFunction clearFunc;
    private GameObject currentRank;

    //�R���X�g���N�^
    public WarpDoor(WarpDoor wpDoor)
    {
        stageName = wpDoor.stageName;
        stageMgr = wpDoor.stageMgr;
        stopLoad = wpDoor.stopLoad;
    }

    void Start()
    {
        rankText = new Dictionary<string, GameObject>() // Dictionary�N���X�̐錾�Ə����l�̐ݒ�
        {
            {"S", rankText_S},
            {"A", rankText_A},
            {"B", rankText_B},
            {"C", rankText_C},
            {"F", rankText_F}
        };

        judgeFunc = FindObjectOfType<RankJudgeAndUpdateFunction>();
        clearFunc = FindObjectOfType<ClearFunction>();

        AllTextDisable();
    }

    void Update()
    {
        
        /*if(clearFunc != null) //�N���A���ꂽ��
        {
            if(clearFunc.GetisClear() && !judgeFunc.GetHasJudged())
            {
                UpdateRankText();
            }
        }
        
        if(SceneManager.GetActiveScene().name.Equals("StageSelectScene")) //�X�e�[�W�Z���N�g�V�[���Ȃ�
        {

        }*/
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" && Input.GetKeyDown(KeyCode.W))
        {

            //�V�[���J�ڂ����Ƃ��ɂ���DontDestroyOnLoad�ɉ����c���Ă���폜
            foreach (var obj in stageMgr.EraseObjects)
            {
                Destroy(obj);
            }
            stageMgr.EraseObjects = new List<GameObject>();
            if (stopLoad) return;
            SceneManager.LoadScene(stageName);
        }
    }

    /*void UpdateRankText()
    {
        if(stageName.Equlas(SceneManager.GetActiveScene().name)) //�h�A�̈ړ���̃X�e�[�W���ƌ��݂̃X�e�[�W������v������
        {
            currentRank = rankText[judgeFunc.JudgeAndUpdateRank(stageMgr.all_sum_cos,true)];
        }
    }

    void DisplayText()
    {
        
    }*/

    void AllTextDisable()
    {
        rankText_S.SetActive(false);
        rankText_A.SetActive(false);
        rankText_B.SetActive(false);
        rankText_C.SetActive(false);
        rankText_F.SetActive(false);
    }
}
