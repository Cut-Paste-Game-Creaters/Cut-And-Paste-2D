using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WarpDoor : MonoBehaviour
{
    [SerializeField] Image rankDisplayImage;
    [SerializeField] Sprite notclear;
    [SerializeField] Sprite clear;
    [SerializeField] Sprite rankText_S;
    [SerializeField] Sprite rankText_A;
    [SerializeField] Sprite rankText_B;
    [SerializeField] Sprite rankText_C;
    [SerializeField] Sprite rankText_F;

    Dictionary<string, GameObject> rankText;
    public string stageName;
    public StageManager stageMgr;
    public bool stopLoad = false;
    private RankJudgeAndUpdateFunction judgeFunc;
    private ClearFunction clearFunc;
    private GameObject currentRank;
    private SpriteRenderer door;

    public string GetStageName()
    {
        return stageName;
    }

    
    
    public class CopyWarpDoor
    {
        public Dictionary<string, GameObject> rankText;
        public string stageName;
        public StageManager stageMgr;
        public bool stopLoad;

        public CopyWarpDoor(WarpDoor wpDoor)
        {
            stageName = wpDoor.stageName;
            stageMgr = wpDoor.stageMgr;
            stopLoad = wpDoor.stopLoad;
        }
    }
 
    void Start()
    {
        //rankText = new Dictionary<string, GameObject>() // Dictionaryクラスの宣言と初期値の設定
        //{
        //    {"S", rankText_S},
        //    {"A", rankText_A},
        //    {"B", rankText_B},
        //    {"C", rankText_C},
        //    {"F", rankText_F}
        //};

        judgeFunc = FindObjectOfType<RankJudgeAndUpdateFunction>();
        clearFunc = FindObjectOfType<ClearFunction>();


        AllTextDisable();

    }



    void Update()
    {
        
        /*if(clearFunc != null) //クリアされたら
        {
            if(clearFunc.GetisClear() && !judgeFunc.GetHasJudged())
            {
                UpdateRankText();
            }
        }
        
        if(SceneManager.GetActiveScene().name.Equals("StageSelectScene")) //ステージセレクトシーンなら
        {

        }*/

    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" && Input.GetKeyDown(KeyCode.W))
        {

            //シーン遷移したときにもしDontDestroyOnLoadに何か残ってたら削除
            foreach (var obj in stageMgr.EraseObjects)
            {
                Destroy(obj);
            }
            stageMgr.EraseObjects = new List<GameObject>();
            if (stopLoad) return;
            SceneManager.LoadScene(stageName);
        }
    }

    //SceneManagerEventでStageSelectをロードしたときにセットされる
    public void SetRankSprite(string stage_rank)
    {
        Sprite doorImage = clear;
        Sprite rankImage = rankText_S;
        if (stage_rank == "NONE")
        {
            rankImage = null;
            doorImage = notclear;
        }
        else if (stage_rank == "S")
        {
            rankImage = rankText_S;
        }
        else if (stage_rank == "A")
        {
            rankImage = rankText_A;
        }
        else if (stage_rank == "B")
        {
            rankImage = rankText_B;
        }
        else if (stage_rank == "C")
        {
            rankImage = rankText_C;
        }
        else if (stage_rank == "F")
        {
            rankImage = rankText_F;
        }

        rankDisplayImage.sprite = rankImage;
        if (door == null)
        {
            door = GetComponent<SpriteRenderer>();
        }
        door.sprite = doorImage;
    }

    /*public WarpDoor GetMyself()
    {
        WarpDoor wpDoor = new WarpDoor();

        wpDoor.stageName = stageName;
        wpDoor.stageMgr = stageMgr;
        wpDoor.stopLoad = stopLoad;

        return wpDoor;
    }*/

    /*void UpdateRankText()
    {
        if(stageName.Equlas(SceneManager.GetActiveScene().name)) //ドアの移動先のステージ名と現在のステージ名が一致したら
        {
            currentRank = rankText[judgeFunc.JudgeAndUpdateRank(stageMgr.all_sum_cos,true)];
        }
    }

    void DisplayText()
    {
        
    }*/

    void AllTextDisable()
    {
        //rankText_S.SetActive(false);
        //rankText_A.SetActive(false);
        //rankText_B.SetActive(false);
        //rankText_C.SetActive(false);
        //rankText_F.SetActive(false);
    }
}
