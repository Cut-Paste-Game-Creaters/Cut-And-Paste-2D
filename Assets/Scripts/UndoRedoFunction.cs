using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class UndoRedoFunction : MonoBehaviour
{
    [SerializeField] Tilemap tilemap; //保存したいtilemap
    [SerializeField] TileScriptableObject tileSB; //ScriptableObject
    GameObject playerCam;
    private GameObject player;
    StageManager stageMgr;
    CameraMove camMove;

    Stack<AllStageInfoList> undoStack = new Stack<AllStageInfoList>();
    Stack<AllStageInfoList> redoStack = new Stack<AllStageInfoList>();

    //BoundsInt b; //ステージの全タイルの大きさ情報

    void Awake()
    {
        //InfoPushToStack(); //Awake()でやってるけど本当は「ステージシーンが読み込まれたとき」に1回呼び出す
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        stageMgr = FindObjectOfType<StageManager>();
        playerCam = Camera.main.gameObject;
        camMove = FindObjectOfType<CameraMove>();
        InfoPushToStack();
        //tilemap.CompressBounds(); //タイルを最小まで圧縮
        //b = tilemap.cellBounds; //タイルの存在する範囲を取得 左端下基準の座標
    }

    // Update is called once per frame
    void Update()
    {
        /*if(PlayerInput.GetKeyDown(KeyCode.Alpha1)) //1ボタンが押されたら保存
        {
            InfoPushToStack();
        }*/
        if(PlayerInput.GetKey(KeyCode.Z) && PlayerInput.GetKeyDown(KeyCode.LeftShift) ||
                PlayerInput.GetKeyDown(KeyCode.Z) && PlayerInput.GetKey(KeyCode.LeftShift)) //Shift+Zボタンが押されたらUndo
        {
            Undo();
        }
    }

    public void InfoPushToStack()
    {
        AllStageInfoList allStageInfo = new AllStageInfoList(); //一枚分のステージ, オブジェクトなど全情報

        allStageInfo.stageTileData = RecordStageHistory(); //一枚分の全情報のクラスのタイルデータ部分に保存
        allStageInfo.have_ene = stageMgr.have_ene;
        allStageInfo.all_sum_cos = stageMgr.all_sum_cos;
        allStageInfo.all_isCut = stageMgr.all_isCut;
        allStageInfo.stageObjState = RecordObjectState();
        allStageInfo.playerState = RecordPlayerInfo();
        allStageInfo.copyTileData = RecordCopyTileData();
        allStageInfo.copyObjectData = RecordCopyObjectData();
        undoStack.Push(allStageInfo);
        Debug.Log(undoStack.Count);
    }

    public AllStageTileData RecordStageHistory()
    {
        AllStageTileData allTileData = new AllStageTileData(); //1枚分のステージデータ

        tilemap.CompressBounds(); //タイルを最小まで圧縮
        var b = tilemap.cellBounds; //タイルの存在する範囲を取得 左端下基準の座標
        allTileData.width = b.size.x;
        allTileData.height = b.size.y;
        Debug.Log(b.size.x +  "," + b.size.y);

        for(int i = 0; i < b.size.y; i++)
        {
            List<TileBase> tBases = new List<TileBase>();
            for(int j = 0; j < b.size.x; j++)
            {
                // オフセット付きのローカル座標から絶対タイル座標を計算
                Vector3Int tilePos = new Vector3Int(b.x + j, b.y + i, 0);
                TileBase t = tilemap.GetTile(tilePos);
                tBases.Add(t);
            }
            allTileData.tiles.Add(tBases);
        }
        return allTileData;
    }

    public List<StageOblectState> RecordObjectState()
    {
        List<StageOblectState> stageObjStateList = new List<StageOblectState>();

        tilemap.CompressBounds(); //タイルを最小まで圧縮
        var b = tilemap.cellBounds; //タイルの存在する範囲を取得 左端下基準の座標

        Collider2D[] cols = Physics2D.OverlapAreaAll(new Vector2(-((32 / 2) + 1), -((18 / 2) + 1)), new Vector2(b.size.x, b.size.y));
        foreach(var col in cols)
        {
            if(col.gameObject.tag != "Tilemap"
                && col.gameObject.tag != "Player"
                && col.gameObject.tag != "Uncuttable")
            {
                StageOblectState stageObjState = new StageOblectState();

                /*
                Prefabの名前は同じ種類なら同じ名前にする　switch(1)などはまだ対応できてない
                */
                stageObjState.prefabName = col.name; //prefabの名前を保存 (Clone)が付いていたらそれを削除
                Debug.Log(stageObjState.prefabName);

                stageObjState.objPosition = col.gameObject.transform.position;
                stageObjState.objRotation = col.gameObject.transform.rotation;

                var switchobj = col.gameObject.GetComponent<SwitchController>();
                var canonobj = col.gameObject.GetComponent<CanonController>();
                var throwobj = col.gameObject.GetComponent<ThrowObjectController>();
                var wpdobj = col.gameObject.GetComponent<WarpDoor>();

                if(switchobj != null)
                {
                    stageObjState.mode = switchobj.mode;
                    stageObjState.waitTime = switchobj.waitTime;
                    stageObjState.nowPressState = switchobj.nowPressState;     //今のスイッチの状態
                    stageObjState.hitState = switchobj.hitState;
                }
                else if(canonobj != null)
                {
                    stageObjState.angle = canonobj.angle;            //-90 ～ 0 ～　90
                    stageObjState.firePower = canonobj.firePower;     //打ち出す力
                    stageObjState.fireTime = canonobj.fireTime;
                }
                else if(throwobj != null)
                {
                    stageObjState.moveDir = throwobj.GetDir();
                    stageObjState.nowTime = throwobj.nowTime;
                }
                else if(wpdobj != null)
                {
                    stageObjState.stageName = wpdobj.stageName;
                    stageObjState.stageMgr = wpdobj.stageMgr;
                }

                //全部情報入れたら最後にAdd
                stageObjStateList.Add(stageObjState);
            }
        }
        return stageObjStateList;
    }

    public PlayerState RecordPlayerInfo()
    {
        PlayerState playerState = new PlayerState();

        playerState.objPosition = player.transform.position;
        //(プレイヤーの座標　+or- カメラの幅/2)が左端か右端に入っていたらカメラの座標も変えてあげる
        if((playerState.objPosition.x - camMove.Screen_Left) < tilemap.cellBounds.min.x+1)
        {
            playerState.area = 1; //左端に固定
            Debug.Log("area=" + playerState.area);
        }
        else if((playerState.objPosition.x + (32 / 2)) > tilemap.cellBounds.max.x-1)
        {
            playerState.area = 3; //右端に固定
            Debug.Log("area=" + playerState.area);
        }
        playerState.objRotation = player.transform.rotation;

        return playerState;
    }

    public StageManager.TileData RecordCopyTileData()
    {
        StageManager.TileData copyTileData = new StageManager.TileData();

        copyTileData = stageMgr.tileData;

        return copyTileData;
    }

    public List<StageManager.ObjectData> RecordCopyObjectData()
    {
        List<StageManager.ObjectData> copyObjectData = new List<StageManager.ObjectData>();

        copyObjectData = stageMgr.objectData;

        return copyObjectData;
    }

    public void Undo()
    {
        if(undoStack.Count > 1)
        {
            redoStack.Push(undoStack.Pop());
            UndoTileData();
            UndoCost();
            UndoAllSumCost();
            UndoAllIsCut();
            UndoObjState();
            UndoPlayerState();
            UndoCopyTileData();
            UndoCopyObjectData();
        }
    }

    public void UndoObjState()
    {
        List<StageOblectState> pre_objList = undoStack.Peek().stageObjState;

        tilemap.CompressBounds(); //タイルを最小まで圧縮
        var b = tilemap.cellBounds; //タイルの存在する範囲を取得 左端下基準の座標

        /*画面上オブジェクト削除*/
        Collider2D[] cols = Physics2D.OverlapAreaAll(new Vector2(-((32 / 2) + 1), -((18 / 2) + 1)), new Vector2(b.size.x, b.size.y));
        foreach (var col in cols)
        {
            //ここでどのオブジェクトに流し込むのか判定が必要
            if(col.gameObject.tag != "Tilemap"
                && col.gameObject.tag != "Player"
                && col.gameObject.tag != "Uncuttable")
            {
                Destroy(col.gameObject);
                Debug.Log(col.gameObject.name + "を削除しました.");
            }
        }

        /*画面上オブジェクト生成*/
        foreach (var obj in pre_objList)
        {
            string trimName = Regex.Replace(obj.prefabName, @"\([^)]*\)", "").Trim();
            GameObject prefab = Resources.Load<GameObject>("Prefabs/" + trimName); //Resources/Prefabsフォルダから名前が同じのprefabを探す
            Debug.Log(obj.prefabName);

            if(prefab != null)
            {
                GameObject g_prefab;
                g_prefab = Instantiate(prefab, obj.objPosition, obj.objRotation);
                g_prefab.name = obj.prefabName;

                var switchobj = g_prefab.gameObject.GetComponent<SwitchController>();
                var canonobj = g_prefab.gameObject.GetComponent<CanonController>();
                var throwobj = g_prefab.gameObject.GetComponent<ThrowObjectController>();
                var wpdobj = g_prefab.gameObject.GetComponent<WarpDoor>();

                if(switchobj != null)
                {
                    switchobj.mode = obj.mode;
                    switchobj.waitTime = obj.waitTime;
                    switchobj.nowPressState = obj.nowPressState;     //今のスイッチの状態
                    switchobj.hitState = obj.hitState;
                }
                else if(canonobj != null)
                {
                    canonobj.angle = obj.angle;            //-90 ～ 0 ～　90
                    canonobj.firePower = obj.firePower;     //打ち出す力
                    canonobj.fireTime = obj.fireTime;
                }
                else if(throwobj != null)
                {
                    throwobj.SetDir(obj.moveDir);
                    throwobj.nowTime = obj.nowTime;
                }
                else if(wpdobj != null)
                {
                    wpdobj.stageName = obj.stageName;
                    wpdobj.stageMgr = obj.stageMgr;
                }
                Debug.Log(prefab.name + "を生成しました.");
                //g_prefab.hp = 100;
            }
        }
    }

    public void UndoCost()
    {
        int pre_cost = undoStack.Peek().have_ene;
        stageMgr.have_ene = pre_cost;
        Debug.Log("所持コスト：" + stageMgr.have_ene + "に戻りました.");
    }

    public void UndoAllSumCost()
    {
        int pre_all_sum_cost = undoStack.Peek().all_sum_cos;
        stageMgr.all_sum_cos = pre_all_sum_cost;
        Debug.Log("所持コスト：" + stageMgr.all_sum_cos + "に戻りました.");
    }

    public void UndoAllIsCut()
    {
        bool pre_all_isCut = undoStack.Peek().all_isCut;
        stageMgr.all_isCut = pre_all_isCut;
        Debug.Log("所持コスト：" + stageMgr.all_isCut + "に戻りました.");
    }

    public void UndoTileData()
    {
        
            //redoStack.Push(undoStack.Pop());
            tilemap.ClearAllTiles();

            AllStageTileData stageTileData = undoStack.Peek().stageTileData;

            for(int i = 0; i < stageTileData.height; i++)
            {
                for(int j = 0; j < stageTileData.width; j++)
                {
                    tilemap.SetTile(new Vector3Int(j - ((32 / 2) + 1), i - ((18 / 2) + 1)), stageTileData.tiles[i][j]); //カメラの高さor幅 / 2　+ 1
                }
            }
            Debug.Log("1つ前に戻りました");
    }

    void UndoPlayerState()
    {
        PlayerState pre_playerState = undoStack.Peek().playerState;

        //情報流し込み
        //座標指定
        player.transform.position = pre_playerState.objPosition;
        //area = 左端なら左端にカメラ移動, 右端なら右端にカメラ移動
        float camPosX = 0;
        //座標指定
        if(pre_playerState.area == 1)
        {
            camPosX = (tilemap.cellBounds.min.x+1 + camMove.Screen_Left);
        }
        else if(pre_playerState.area == 3)
        {
            camPosX = (tilemap.cellBounds.max.x-1 - (32 / 2));
        }
        playerCam.transform.position = new Vector3(camPosX, 0, -10); //カメラ移動
        //回転指定
        player.transform.rotation = pre_playerState.objRotation;
    }

    void UndoCopyTileData()
    {
        StageManager.TileData pre_copyTileData = undoStack.Peek().copyTileData;

        stageMgr.tileData = pre_copyTileData;
    }

    void UndoCopyObjectData()
    {
        List<StageManager.ObjectData> pre_copyObjectData = undoStack.Peek().copyObjectData;

        stageMgr.objectData = pre_copyObjectData;
    }

    //リトライ
    public void Retry()
    {
        while(undoStack.Count > 2)
        {
            undoStack.Pop();
        }

        Undo();
        stageMgr.player_HP = 10;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //ステージ全体のタイルを格納するクラス
    public class AllStageTileData
    {
        //現時点でw : 32, h : 18
        public int width;
        public int height;
        public List<List<TileBase>> tiles = new List<List<TileBase>>();
    }
    public class StageOblectState
    {
        //共通情報
        public string prefabName; //保存した情報をどれに入れるのか一致させるために必要
        public Vector3 objPosition;
        public Quaternion objRotation;

        //switch情報
        public SwitchController.SwitchMode mode;
        public float waitTime;
        public bool nowPressState;     //今のスイッチの状態
        public int hitState;

        //canon情報
        public float angle;            //-90 ～ 0 ～　90
        public float firePower;     //打ち出す力
        public float fireTime;

        //ThrowObject関連
        public Vector3 moveDir;
        public float nowTime;

        //WArpDoor関連
        public string stageName;
        public StageManager stageMgr;

        //これ以降必要な情報追加する
        /*public SwitchController swCon;
        public CanonController canonCon;
        public ThrowObjectController toCon;
        public WarpDoor wpdCon;*/
    }
    public class PlayerState
    {
        public Vector3 objPosition;
        public Quaternion objRotation;
        public int area; //1 = 左端, 2 = カメラはステージの端っこにいない, 3 = 右端
    }
    public class AllStageInfoList
    {
        public AllStageTileData stageTileData = new AllStageTileData();
        public int have_ene = 0;
        public int all_sum_cos = 0;
        public bool all_isCut;
        public List<StageOblectState> stageObjState = new List<StageOblectState>();
        public PlayerState playerState = new PlayerState();
        public StageManager.TileData copyTileData = new StageManager.TileData();
        public List<StageManager.ObjectData> copyObjectData = new List<StageManager.ObjectData>();
    }
}
