using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    public TileData tileData = new TileData(0, 0);
    public List<ObjectData> objectData = new List<ObjectData>();

    /*プレイヤー関連*/
    Player_Function playerFunc;
    RankJudgeAndUpdateFunction rankFunc;
    public int player_HP = 100;

    /*コスト関連*/
    [SerializeField]private float costHeal_timeOut; //costが回復する間隔
	private float timeElapsed;
    public int stageNum = -1;
    private int[] init_ene_array = {100000, 150, 200, 250, 300, 350, 400, 450, 500, 550}; //ステージごとの初期コスト配列
    private int[] healAmount_array = {5, 10, 15, 20, 25, 30, 35, 40, 45, 50}; //ステージごとの回復速度コストの配列
    public int[] initAddCost_EachStage = {0,0,0,0,0,0,0,0,0,0};
    public int have_ene = 10000; //初期コスト
    public int all_sum_cos = 0; //ステージで消費した全てのコスト
    public int erase_cost = 0; //貼り付け箇所の消すコスト
    public int write_cost = 0; //取得箇所の増やすコスト
    public int cut_erase_cost = 0; //カットの時のみの取得箇所の消すコスト
    public bool all_isCut = false; //copyかcutかを判別する変数

    //オブジェクトを別シーンに持ってく関連
    public List<GameObject> EraseObjects = new List<GameObject>();

    void Start()
    {
        playerFunc = FindObjectOfType<Player_Function>();
        rankFunc = FindObjectOfType<RankJudgeAndUpdateFunction>();
    }
    
    void Update()
    {
        if(stageNum != -1)
        {
            HealCost(stageNum);
        }
    }

    /////////////////////////////////////////////////
    public struct TileData
    {
        //現時点でw : 32, h : 18
        public int width;       //幅、高さはマスの個数
        public int height;
        public int direction;   //0右上、1右下、2左下、3左上
        public List<List<TileBase>> tiles;
        public bool hasData;
        public bool isCut;
        public TileData(int w, int h)
        {
            width = w;
            height = h;
            tiles = new List<List<TileBase>>();
            hasData = false;
            direction = -1;
            isCut = false;
        }
    }

    public struct ObjectData
    {
        //カットするオブジェクトの本体
        public GameObject obj;
        //カットするオブジェクトの相対位置
        public Vector3 pos;
    }

    public void DamageToPlayer(int damage) //引数分HPから減らす処理
    {
        player_HP -= damage;
    }

    public void InitAllSumCost()
    {
        all_sum_cos = 0;
        Debug.Log("総消費コストを初期化しました.");
    }

    public void InitHaveCost(int stageNum)
    {
        have_ene = init_ene_array[stageNum];
        if (stageNum == 0)
        {
            foreach(var cost in initAddCost_EachStage)
            {
                have_ene += cost;
            }
        }
            Debug.Log("初期コストを" + stageNum + "ステージの" + have_ene + "に初期化しました.");
    }

    public void StageSelectInitHaveCost()
    {
        //各ステージのランクに応じて設定
    }

    public void HealCost(int stageNum)
    {
          /*一定時間（costHeal_timeOut）ごとに所持コストを回復*/
        timeElapsed += Time.deltaTime;
        if(timeElapsed >= costHeal_timeOut)
        {
            have_ene += healAmount_array[stageNum];
            if(have_ene > init_ene_array[stageNum]) //回復コスト上限を超えて回復しようとする場合は回復コスト上限で書き換える
            {
                have_ene = init_ene_array[stageNum];
            }
            //Debug.Log("所持コスト:" + have_ene);
            timeElapsed = 0.0f;
        }
    }

    public Vector3 GetInfo()
    {
        Vector3 v = Vector3.zero;
        if (tileData.hasData)
        {
            v.x = tileData.width;
            v.y = tileData.height;
            v.z = tileData.direction;
        }

        return v;
    }

    //////////////////////////////////////////////////////////
    void Awake()
    {
        // 既にインスタンスが存在するなら破棄
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }


        // このインスタンスを保持してシーン遷移でも破棄されないようにする
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SaveObjects()
    {

    }
}
