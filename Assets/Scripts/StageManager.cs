using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    public TileData tileData = new TileData(0, 0);
    public List<ObjectData> objectData = new List<ObjectData>();

    /*プレイヤー関連*/
    Player_Function playerFunc;
    RankJudgeAndUpdateFunction rankFunc;
    public int player_HP = 100;
    public int player_MAXHP = 100;
    public bool isPlayerDamaged = false;
    private float noDamageTime = 1.0f;
    private float nowNoDanageTime = 0.0f;

    /*スイッチ関連*/
    public bool switch_state = false;

    /*鍵関連*/
    public bool key_lock_state = false;

    /*スイッチと鍵の初期状態*/
    //[HideInInspector]
    public Pair<bool, bool>[] switch_key_states;

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
    [HideInInspector]
    public List<GameObject> EraseObjects = new List<GameObject>();

    void Start()
    {
        playerFunc = FindObjectOfType<Player_Function>();
        rankFunc = FindObjectOfType<RankJudgeAndUpdateFunction>();

        //各ステージでのスイッチと鍵の初期状態
        //Pair.bool1 = switchState, Pair.bool2 = keyState
        switch_key_states = new Pair<bool, bool>[]
        {
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(true, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),
        };
    }
    
    void Update()
    {
        if(stageNum != -1)
        {
            HealCost(stageNum);
        }

        if(isPlayerDamaged)
        {
            nowNoDanageTime += PlayerInput.GetDeltaTime();
            if(nowNoDanageTime > noDamageTime)
            {
                isPlayerDamaged = false;
            }
        }
    }

    [System.Serializable] // Unityエディタでシリアライズ可能にする
    public class Pair<T1, T2>
    {
        public T1 first;
        public T2 second;

        public Pair(T1 first, T2 second)
        {
            this.first = first;
            this.second = second;
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

    public Dictionary<string, int> stageNumber = new Dictionary<string, int>() // Dictionaryクラスの宣言と初期値の設定
    {
        {"Stage1", 0},
        {"Stage2", 1},
        {"Stage3", 2},
        {"Stage4", 3},
        {"Stage5", 4},
        {"Stage6", 5},
        {"Stage7", 6},
        {"Stage8", 7},
        {"Stage9", 8},
        {"Stage10", 9},
        {"StageTemplate",10 }
    };

    public void DamageToPlayer(int damage) //引数分HPから減らす処理
    {
        if (!isPlayerDamaged)
        {
            player_HP -= damage;
            isPlayerDamaged = true;
            nowNoDanageTime = 0.0f;
        }

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

    public void ResetObjectState()
    {
        string input = SceneManager.GetActiveScene().name;
        if (Regex.IsMatch(input, @"^Stage\d+$")) //シーン名がStageなんとかなら
        {
            int stage_num = stageNumber[SceneManager.GetActiveScene().name];
            switch_state = switch_key_states[stage_num].first;
            key_lock_state = switch_key_states[stage_num].second;
        }
        else
        {
            switch_state = false;
            key_lock_state = false;
        }
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
