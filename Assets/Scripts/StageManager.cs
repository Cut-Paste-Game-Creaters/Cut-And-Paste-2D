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
