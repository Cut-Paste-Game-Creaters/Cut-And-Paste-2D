using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using UnityEngine.UI;

public class UndoRedoFunction : MonoBehaviour
{
    [SerializeField] TileScriptableObject tileSB; //ScriptableObject
    GameObject playerCam;
    private GameObject player;
    private Player_Copy p_copy;
    private CaptureCopyZone ccz;
    StageManager stageMgr;
    CameraMove camMove;
    Vector3Int stageStartPos;
    private Tilemap tilemap; //ステージtilemap

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
        p_copy = player.GetComponent<Player_Copy>();
        stageMgr = FindObjectOfType<StageManager>();
        ccz = FindObjectOfType<CaptureCopyZone>();
        playerCam = Camera.main.gameObject;
        camMove = FindObjectOfType<CameraMove>();
        Tilemap[] maps = FindObjectsOfType<Tilemap>();
        foreach (var map in maps)
        {
            if (map.gameObject.tag == "Tilemap")
            {
                tilemap = map;
            }
        }
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
        allStageInfo.cStageMgr = RecordStageManagerInfo();
        allStageInfo.stageObjState = RecordObjectState();
        allStageInfo.playerState = RecordPlayerInfo();
        allStageInfo.copyTileData = RecordCopyTileData();
        //allStageInfo.copyObjectData = RecordCopyObjectData();
        //allStageInfo.copySelectedObjects = RecordCopySelectedObjects();
        allStageInfo.copyImage = RecordCopyImage();
        undoStack.Push(allStageInfo);
        Debug.Log("stackに保存されました" + undoStack.Count);
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
                if(i == 0 && j == 0)
                {
                    stageStartPos = tilePos;
                }
                TileBase t = tilemap.GetTile(tilePos);
                tBases.Add(t);
            }
            allTileData.tiles.Add(tBases);
        }
        return allTileData;
    }

    public List<StageObjectState> RecordObjectState()
    {
        List<StageObjectState> stageObjStateList = new List<StageObjectState>();

        tilemap.CompressBounds(); //タイルを最小まで圧縮
        var b = tilemap.cellBounds; //タイルの存在する範囲を取得 左端下基準の座標

        Collider2D[] cols = Physics2D.OverlapAreaAll(new Vector2(b.x/*((32 / 2) + 1)*/, b.y/*((18 / 2) + 1)*/), new Vector2(b.size.x, b.size.y));
        foreach(var col in cols)
        {
            if(col.gameObject.tag != "Tilemap"
                && col.gameObject.tag != "Player"
                && col.gameObject.tag != "Uncuttable")
            {
                StageObjectState stageObjState = new StageObjectState();

                /*
                Prefabの名前は同じ種類なら同じ名前にする　switch(1)などはまだ対応できてない
                */
                stageObjState.prefabName = col.name; //prefabの名前を保存 (Clone)が付いていたらそれを削除
                stageObjState.objtag = col.gameObject.tag; //オブジェクトのtagを保存
                Debug.Log(stageObjState.prefabName);

                stageObjState.objPosition = col.gameObject.transform.position;
                stageObjState.objRotation = col.gameObject.transform.rotation;

                var switchobj = col.gameObject.GetComponent<SwitchController>();
                var s_blockobj = col.gameObject.GetComponent<SwitchblockController>();
                var canonobj = col.gameObject.GetComponent<CanonController>();
                var throwobj = col.gameObject.GetComponent<ThrowObjectController>();
                var wpdobj = col.gameObject.GetComponent<WarpDoor>();
                var bumobj = col.gameObject.GetComponent<BumperForce>();
                var togeobj = col.gameObject.GetComponent<TogeController>();
                var b_h_obj = col.gameObject.GetComponent<Blackhole>();
                var w_h_obj = col.gameObject.GetComponent<WhiteHole2D>();
                var svmobj = col.gameObject.GetComponent<SinVerticalMover>();
                var rcobj = col.gameObject.GetComponent<RotateController>();
                var vfobj = col.gameObject.GetComponent<VanishFloor>();

                if(switchobj != null)
                {
                    stageObjState.swc = new SwitchController.CopySwitchController(switchobj);
                }
                if(s_blockobj != null)
                {
                    stageObjState.sbc = new SwitchblockController.CopySwitchblockController(s_blockobj);
                }
                else if(canonobj != null)
                {
                    stageObjState.cc = new CanonController.CopyCanonController(canonobj);
                }
                else if(throwobj != null)
                {
                    stageObjState.toC = new ThrowObjectController.CopyThrowObjectController(throwobj);
                }
                else if(wpdobj != null)
                {
                    stageObjState.cwpDoor = new WarpDoor.CopyWarpDoor(wpdobj);
                    //WarpDoor wpdoor = wpdobj.GetMyself();
                    //stageObjState.wpDoor = new WarpDoor(wpdobj);
                }
                else if(bumobj != null)
                {
                    stageObjState.bumper = new BumperForce.CopyBumperForce(bumobj);
                }
                else if(togeobj != null)
                {
                    stageObjState.toge = new TogeController.CopyTogeController(togeobj);
                }
                else if(b_h_obj != null)
                {
                    stageObjState.b_hole = new Blackhole.CopyBlackhole(b_h_obj);
                }
                else if(w_h_obj != null)
                {
                    stageObjState.w_hole = new WhiteHole2D.CopyWhiteHole2D(w_h_obj);
                }
                else if(svmobj != null)
                {
                    stageObjState.svm = new SinVerticalMover.CopySinVerticalMover(svmobj);
                }
                else if(rcobj != null)
                {
                    stageObjState.rc = new RotateController.CopyRotateController(rcobj);
                }
                else if(vfobj != null)
                {
                    stageObjState.vf = new VanishFloor.CopyVanishFloor(vfobj);
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

    public StageManager.CopyStageManager RecordStageManagerInfo()
    {
        StageManager.CopyStageManager cStageMgr = new StageManager.CopyStageManager(stageMgr);
        return cStageMgr;
    }

    public StageManager.TileData RecordCopyTileData()
    {
        StageManager.TileData copyTileData = new StageManager.TileData(stageMgr.tileData.width, stageMgr.tileData.height);

        copyTileData.direction = stageMgr.tileData.direction;
        for(int y = 0; y < copyTileData.height; y++)
        {
            List<TileBase> tbs = new List<TileBase>();
            for(int x = 0; x < copyTileData.width; x++)
            {
                tbs.Add(stageMgr.tileData.tiles[y][x]);
                //Debug.Log(stageMgr.tileData.tiles[y][x]);
                
            }
            copyTileData.tiles.Add(tbs);
        }
        copyTileData.hasData= stageMgr.tileData.hasData;
        copyTileData.isCut= stageMgr.tileData.isCut;

        return copyTileData;
    }

    public List<StageManager.ObjectData> RecordCopyObjectData()
    {
        List<StageManager.ObjectData> copyObjectData = new List<StageManager.ObjectData>();

        //copyObjectData.obj = stageMgr.objectData.obj;
        //copyObjectData.pos = stageMgr.objectData.pos;
        foreach(StageManager.ObjectData s_obj in stageMgr.objectData)
        {
            StageManager.ObjectData obj_d = new StageManager.ObjectData();

            obj_d.obj = s_obj.obj;
            obj_d.pos = s_obj.pos;

            copyObjectData.Add(obj_d);
            //Debug.Log(obj_d.obj.name);
        }

        return copyObjectData;
    }

    public Stack<GameObject> RecordCopySelectedObjects()
    {
        Stack<GameObject> cs_objs = new Stack<GameObject>();

        /*while(p_copy.GetSelectedObjects().Count > 0)
        {
            GameObject obj = p_copy.GetSelectedObjects().Peek();
            cs_objs.Push(obj);
        }*/

        return cs_objs;
    }

    public Image RecordCopyImage()
    {
        Image copyImage;

        copyImage = ccz.GetImage();

        return copyImage;
    }

    public void Undo()
    {
        if(undoStack.Count > 1)
        {
            redoStack.Push(undoStack.Pop());
            UndoTileData();
            //UndoCost();
            //UndoAllSumCost();
            //UndoAllIsCut();
            UndoStageManagerInfo();
            UndoObjState();
            UndoPlayerState();
            UndoCopyTileData();
            //UndoCopyObjectData();
            //UndoCopySelectedObjects();
            UndoCopyImage();
        }
    }

    public void UndoObjState()
    {
        List<StageObjectState> pre_objList = undoStack.Peek().stageObjState;

        tilemap.CompressBounds(); //タイルを最小まで圧縮
        var b = tilemap.cellBounds; //タイルの存在する範囲を取得 左端下基準の座標

        /*画面上オブジェクト削除*/
        Collider2D[] cols = Physics2D.OverlapAreaAll(new Vector2(b.x/*-((32 / 2) + 1)*/, b.y/*-((18 / 2) + 1))*/), new Vector2(b.size.x, b.size.y));
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
            //GameObject prefab = Resources.Load<GameObject>("Prefabs/Object/" + trimName); //Resources/Prefabsフォルダから名前が同じのprefabを探す
            GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/Object"); //objectfile内の全オブジェクトを取得
            GameObject prefab = null;
            foreach(GameObject pre in prefabs)
            {
                if(pre.tag.Equals(obj.objtag))
                {
                    prefab = pre;
                }
            }
            Debug.Log(obj.prefabName);

            if(prefab != null)
            {
                GameObject g_prefab;
                g_prefab = Instantiate(prefab, obj.objPosition, obj.objRotation);
                g_prefab.name = obj.prefabName;

                var switchobj = g_prefab.gameObject.GetComponent<SwitchController>();
                var s_blockobj = g_prefab.gameObject.GetComponent<SwitchblockController>();
                var canonobj = g_prefab.gameObject.GetComponent<CanonController>();
                var throwobj = g_prefab.gameObject.GetComponent<ThrowObjectController>();
                var wpdobj = g_prefab.gameObject.GetComponent<WarpDoor>();
                var bumobj = g_prefab.gameObject.GetComponent<BumperForce>();
                var togeobj = g_prefab.gameObject.GetComponent<TogeController>();
                var b_h_obj = g_prefab.gameObject.GetComponent<Blackhole>();
                var w_h_obj = g_prefab.gameObject.GetComponent<WhiteHole2D>();
                var svmobj = g_prefab.gameObject.GetComponent<SinVerticalMover>();
                var rcobj = g_prefab.gameObject.GetComponent<RotateController>();
                var vfobj = g_prefab.gameObject.GetComponent<VanishFloor>();

                if(switchobj != null)
                {
                    obj.CopyData(switchobj);
                }
                else if(s_blockobj != null)
                {
                    obj.CopyData(s_blockobj);
                }
                else if(canonobj != null)
                {
                    obj.CopyData(canonobj);
                }
                else if(throwobj != null)
                {
                    obj.CopyData(throwobj);
                }
                else if(wpdobj != null)
                {
                    obj.CopyData(wpdobj);
                }
                else if(bumobj != null)
                {
                    obj.CopyData(bumobj);
                    //Debug.Log(prefab.name + "を生成しました." + bumobj.checkDistance);
                }
                else if(togeobj != null)
                {
                    obj.CopyData(togeobj);
                }
                else if(b_h_obj != null)
                {
                    obj.CopyData(b_h_obj);
                }
                else if(w_h_obj != null)
                {
                    obj.CopyData(w_h_obj);
                }
                else if(svmobj != null)
                {
                    obj.CopyData(svmobj);
                }
                else if(rcobj != null)
                {
                    obj.CopyData(rcobj);
                }
                else if(vfobj != null)
                {
                    obj.CopyData(vfobj);
                }
                //Debug.Log(prefab.name + "を生成しました.");
                //g_prefab.hp = 100;
            }
        }
    }

    /*public void UndoCost()
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
    }*/

    public void UndoStageManagerInfo()
    {
        StageManager.CopyStageManager pre_csm = undoStack.Peek().cStageMgr;

        stageMgr.have_ene = pre_csm.have_ene;
        stageMgr.all_sum_cos = pre_csm.all_sum_cos;
        stageMgr.write_cost = pre_csm.write_cost;
        stageMgr.all_isCut = pre_csm.all_isCut;
        stageMgr.switch_state = pre_csm.switch_state;
        stageMgr.key_lock_state = pre_csm.key_lock_state;
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
                    tilemap.SetTile(new Vector3Int(j + stageStartPos.x/* - ((32 / 2) + 1)*/, i + stageStartPos.y/* - ((18 / 2) + 2)*/), stageTileData.tiles[i][j]); //カメラの高さor幅 / 2　+ 1
                }
            }
            //Debug.Log("1つ前に戻りました");
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

        //stageMgr.tileData = pre_copyTileData;

        stageMgr.tileData = new StageManager.TileData(pre_copyTileData.width, pre_copyTileData.height);

        //stageMgr.tileData.width = pre_copyTileData.width;
        //stageMgr.tileData.height = pre_copyTileData.height;
        stageMgr.tileData.direction = pre_copyTileData.direction;
        for(int y = 0; y < pre_copyTileData.height; y++)
        {
            List<TileBase> tbs = new List<TileBase>();
            for(int x = 0; x < pre_copyTileData.width; x++)
            {
                tbs.Add(pre_copyTileData.tiles[y][x]);
            }
            stageMgr.tileData.tiles.Add(tbs);
        }
        int j = 0;
        foreach(List<TileBase> b in stageMgr.tileData.tiles)
        {
            j++;
            for(int i = 0; i < b.Count; i++)
            {
                Debug.Log(b[i]);
            }
        }
        Debug.Log(j + "回回りました");
        stageMgr.tileData.hasData = pre_copyTileData.hasData;
        stageMgr.tileData.isCut = pre_copyTileData.isCut;
        //stageMgr.tileData.tiles = pre_copyTileData.tiles;
        
    }

    void UndoCopyObjectData()
    {
        stageMgr.objectData.Clear(); //全要素削除

        List<StageManager.ObjectData> pre_copyObjectData = undoStack.Peek().copyObjectData;

        //stageMgr.objectData = pre_copyObjectData;

        foreach(StageManager.ObjectData p_obj in pre_copyObjectData)
        {
            StageManager.ObjectData obj_d = new StageManager.ObjectData();

            obj_d.obj = p_obj.obj;
            obj_d.pos = p_obj.pos;

            stageMgr.objectData.Add(obj_d);
        }
    }

    void UndoCopySelectedObjects()
    {
        Stack<GameObject> pre_copySelectedObjects = undoStack.Peek().copySelectedObjects;

        p_copy.SetSelectedObjects(pre_copySelectedObjects);
    }

    void UndoCopyImage()
    {
        Image pre_copyImage = undoStack.Peek().copyImage;

        ccz.SetImage(pre_copyImage);
        if(stageMgr.tileData.hasData)
        {
            ccz.ableImage();
        }
        else
        {
            ccz.disableImage();
        }
    }

    //リトライ
    public void Retry()
    {
        /*while(undoStack.Count > 2)
        {
            undoStack.Pop();
        }

        Undo();*/
        stageMgr.player_HP = 100;
        //オブジェクトの状態を初期化(switch&key)
        stageMgr.ResetObjectState();

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
    public class StageObjectState
    {
        //共通情報
        public string prefabName; //保存した情報をどれに入れるのか一致させるために必要
        public string objtag;
        public Vector3 objPosition;
        public Quaternion objRotation;

        //switch情報
        public SwitchController.CopySwitchController swc;

        //switchblock
        public SwitchblockController.CopySwitchblockController sbc;

        //canon情報
        public CanonController.CopyCanonController cc;

        //ThrowObject関連
        public ThrowObjectController.CopyThrowObjectController toC;

        //WArpDoor関連
        public WarpDoor.CopyWarpDoor cwpDoor;

        //Bumper関連
        public BumperForce.CopyBumperForce bumper;

        //Toge関連
        public TogeController.CopyTogeController toge;

        //BlackHole関連
        public Blackhole.CopyBlackhole b_hole;

        //WhiteHole
        public WhiteHole2D.CopyWhiteHole2D w_hole;

        //SinVeerticalMover
        public SinVerticalMover.CopySinVerticalMover svm;

        //RotateController
        public RotateController.CopyRotateController rc;

        //VanishFloor
        public VanishFloor.CopyVanishFloor vf;

        //データコピー関数
        //SwitchControllerのコピー処理
        public void CopyData(SwitchController copySwc)
        {
            copySwc.stateOff = swc.stateOff;
            copySwc.stateOn = swc.stateOn;
            copySwc.mode = swc.mode;
            copySwc.nowPressState = swc.nowPressState;
            copySwc.hitState = swc.hitState;
        }
        //SwitchBlockControllerのコピー処理
        public void CopyData(SwitchblockController copySbc)
        {
            copySbc.stateOff = sbc.stateOff;
            copySbc.stateOn = sbc.stateOn;
        }
        //CanonControllerのコピー処理
        public void CopyData(CanonController copyCc)
        {
            copyCc.angle = cc.angle;
            copyCc.firePower = cc.firePower;
            copyCc.firespeed = cc.firespeed;
            copyCc.fireTime = cc.fireTime;
        }
        //ThrowObjectControllerのコピー処理
        public void CopyData(ThrowObjectController copyToCon)
        {
            copyToCon.destroyTime = toC.destroyTime;
            copyToCon.nowTime = toC.nowTime;
            copyToCon.disAppearTime = toC.disAppearTime;
            copyToCon.SetDir(toC.moveDir);
        }
        //WarpDoorのコピー処理
        public void CopyData(WarpDoor copyWpd)
        {
            copyWpd.stageName = cwpDoor.stageName;
            copyWpd.stageMgr = cwpDoor.stageMgr;
            copyWpd.stopLoad = cwpDoor.stopLoad;
        }
        //BumperForceのコピー処理
        public void CopyData(BumperForce copyBumper)
        {
            copyBumper.checkDistance = bumper.checkDistance;
            copyBumper.upwardForce = bumper.upwardForce;
            copyBumper.bounceForce = bumper.bounceForce;
            copyBumper.playerLayer = bumper.playerLayer;
        }
        //TogeControllerのコピー処理
        public void CopyData(TogeController copyToge)
        {
            copyToge.togeDamage = toge.togeDamage;
        }
        //BlackHoleのコピー処理
        public void CopyData(Blackhole copyB_hole)
        {
            copyB_hole.gravityForce = b_hole.gravityForce;
            copyB_hole.radius = b_hole.radius;
            copyB_hole.trueDuration = b_hole.trueDuration;
            copyB_hole.falseDuration = b_hole.falseDuration;
            copyB_hole.rotationSpeed = b_hole.rotationSpeed;
        }
        //WhiteHoleのコピー処理
        public void CopyData(WhiteHole2D copyW_hole)
        {
            copyW_hole.repelForce = w_hole.repelForce;
            copyW_hole.radius = w_hole.radius;
        }
        //SinVerticalMoverのコピー処理
        public void CopyData(SinVerticalMover copy_svm)
        {
            copy_svm.amplitude = svm.amplitude;
            copy_svm.frequency = svm.frequency;
            copy_svm.cos = svm.cos;
            copy_svm.sin = svm.sin;
        }
        //RotateControllerのコピー処理
        public void CopyData(RotateController copy_rc)
        {
            copy_rc.rotateTime = rc.rotateTime;
            copy_rc.dir = rc.dir;
        }
        //VanishFloorのコピー処理
        public void CopyData(VanishFloor copy_vf)
        {
            copy_vf.stayTime = vf.stayTime;
            copy_vf.SetElapsed(vf.elapsed);
            copy_vf.SetIsCollision(vf.isCollision);
        }

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
        public StageManager.CopyStageManager cStageMgr;
        public List<StageObjectState> stageObjState = new List<StageObjectState>();
        public PlayerState playerState = new PlayerState();
        public StageManager.TileData copyTileData;
        public List<StageManager.ObjectData> copyObjectData = new List<StageManager.ObjectData>();
        public Stack<GameObject> copySelectedObjects = new Stack<GameObject>();
        public Image copyImage;
    }
}
