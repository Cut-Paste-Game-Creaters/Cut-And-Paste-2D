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

    /*コスト関連*/
    [SerializeField]private float costHeal_timeOut; //costが回復する間隔
	private float timeElapsed;
    private int overwrite_sum_cos = 0; //kyosu
    private int[] init_ene_array = {100, 150, 200, 250, 300, 350, 400, 450, 500, 550}; //ステージごとの初期コスト配列
    private int[] healAmount_array = {5, 10, 15, 20, 25, 30, 35, 40, 45, 50}; //ステージごとの回復速度コストの配列
    private int stage = 0; //0=ステージ1
    public int have_ene = 10000; //初期コスト
    private int all_sum_cos = 0; //ステージで消費した全てのコスト
    public int erase_cost = 0; //貼り付け箇所の消すコスト
    public int write_cost = 0; //取得箇所の増やすコスト
    public int cut_erase_cost = 0; //カットの時のみの取得箇所の消すコスト
    public bool all_isCut = false; //copyかcutかを判別する変数

    /////////////////////////////////////////////////
    public struct TileData
    {
        //現時点でw : 32, h : 18
        public int width;
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


}
