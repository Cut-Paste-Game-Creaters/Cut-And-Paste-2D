using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using UnityEngine.Tilemaps;
using System.Linq;

public class Player_Copy : MonoBehaviour
{
    [SerializeField] GameObject frame2;
    [SerializeField] GameObject anounce;
    [SerializeField] TileScriptableObject tileSB; //ScriptableObject
    [SerializeField] ObjectScriptableObject objSB;

    private Tilemap tilemap;
    private StageManager stageMgr;
    private Vector3 startPos = Vector3.zero;
    private Vector3 endPos = Vector3.zero;
    private bool isDrawing = false;
    private SpriteRenderer frameSR;
    private int whichMode = -1;     //0:Copy, 1:Cut
    private bool makeDecision = false;  //マウス離したらOn
    UndoRedoFunction urFunc;
    private CaptureCopyZone captureCopyZone;
    //public bool all_isCut = false; //コピー関数の引数のisCutと区別するため

    // Start is called before the first frame update
    void Start()
    {
        frame2 = Instantiate(frame2);
        frame2.SetActive(false);
        frame2.layer = LayerMask.NameToLayer("UI");
        //anounce = Instantiate(anounce);
        //anounce.SetActive(false);
        Tilemap[] maps = FindObjectsOfType<Tilemap>();
        foreach (var map in maps)
        {
            if (map.gameObject.tag == "Tilemap")
            {
                tilemap = map;
                break;
            }
        }
        stageMgr = FindObjectOfType<StageManager>();
        urFunc = FindObjectOfType<UndoRedoFunction>();
        captureCopyZone = FindObjectOfType<CaptureCopyZone>();

        anounce.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        CopyTiles();
    }

    //コピーする範囲を決定する
    void CopyTiles()
    {
        if (!makeDecision)
        {
            //マウスを右クリックして、移動して離す
            //最初と最後の座標だけとれば範囲が指定できる
            if (PlayerInput.GetMouseButtonDown(0))
            {
                //範囲選択時時間を停止する(またはスローモー)
                Time.timeScale = 0f;

                //これまでコピーしてたものを初期化
                InitTileData();
                //最初の位置取得(小数点)
                startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);


                //四角を描く
                frame2.SetActive(true);
                if (frameSR == null)
                {
                    frameSR = frame2.GetComponent<SpriteRenderer>();
                }
                frame2.transform.localPosition = startPos;
                frame2.transform.localScale = Vector3.one;
                isDrawing = true;
            }

            if (isDrawing && PlayerInput.GetMouseButton(0))
            {
                //四角のサイズを変える
                Vector3 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 size = currentPos - startPos;
                frameSR.size = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
                Vector3 nowpos = (currentPos + startPos) / 2;
                nowpos.z = 0;
                frame2.transform.localPosition = nowpos;
            }


            if (PlayerInput.GetMouseButtonUp(0) && !stageMgr.tileData.hasData)
            {
                makeDecision = true;
                whichMode = -1;
                endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                anounce.SetActive(true);
                return;
            }
        }

        //どちらかを選ぶフェーズ
        if (makeDecision)
        {
            switch (whichMode)
            {
                case -1:    //コピーかカットか選ぶ
                    if (PlayerInput.GetMouseButtonUp(0))
                    {
                        whichMode = 0;//copy
                        //コピーorカットする前に画像をキャプチャする
                        captureCopyZone.CaptureImage(startPos, endPos);
                    }
                    else if (PlayerInput.GetMouseButtonUp(1))
                    {
                        whichMode = 1;//cut
                        //コピーorカットする前に画像をキャプチャする
                        captureCopyZone.CaptureImage(startPos, endPos);
                    }
                    else if (PlayerInput.GetKeyDown(KeyCode.Escape)) whichMode = 2;//nothing
                    break;
                case 0:     //コピーするなら
                    stageMgr.all_isCut = false;
                    CopyContents(startPos, endPos);
                    CopyObject(startPos, endPos);
                    //all_isCut = false;
                    InitWhichMode();
                    break;
                case 1:     //カットするなら
                    stageMgr.all_isCut = true;
                    CopyContents(startPos, endPos, true);
                    CopyObject(startPos, endPos, true);
                    //all_isCut = true;
                    InitWhichMode();
                    break;
                default:
                    InitWhichMode();
                    break;
            }
        }
    }

    //実際にタイルをコピーをする関数
    void CopyContents(Vector3 sPos, Vector3 ePos, bool isCut = false)
    {
        //増やすコストを初期化 コピーの時はコピーされるたびに初期化　逆にそれ以外は更新されてはいけない
        stageMgr.write_cost = 0;

        //カットの時のみ使う変数
        stageMgr.cut_erase_cost = 0;

        //位置をintにする
        Vector3Int _startPos = ChangeVecToInt(sPos);
        Vector3Int _endPos = ChangeVecToInt(ePos);
        //幅高さを計算
        int width = Mathf.Abs((_endPos.x - _startPos.x)) + 1;
        int height = Mathf.Abs((_endPos.y - _startPos.y)) + 1;
        Debug.Log("w:" + width + " h:" + height);

        //tileDataにコピーしたものを保存
        stageMgr.tileData.width = width;
        stageMgr.tileData.height = height;


        //コピーの向きを取得する
        int direction = 0;
        if (_endPos.x - _startPos.x >= 0)//右向き
        {
            if (_endPos.y - _startPos.y >= 0) //上向き
            {
                direction = 0;
            }
            else
            {
                direction = 1;
            }
        }
        else  //左向き
        {
            if (_endPos.y - _startPos.y >= 0) //上向き
            {
                direction = 3;
            }
            else
            {
                direction = 2;
            }
        }
        stageMgr.tileData.direction = direction;


        //タイルのコストを計算する
        CutInCopy(_startPos, _endPos, true);

        //オブジェクトのコスト計算をする
        Collider2D[] cols = Physics2D.OverlapAreaAll(startPos, endPos);
        stageMgr.objectData = new List<StageManager.ObjectData>();
        CutInCopyObject(cols, true);

        //カットであるかそうでないかで分ける
        if (stageMgr.all_isCut)
        {
            //消すコストが所持コストより小さいなら
            if(stageMgr.cut_erase_cost <= stageMgr.have_ene)
            {
                stageMgr.have_ene -= stageMgr.cut_erase_cost; //コスト引く
                stageMgr.all_sum_cos += stageMgr.cut_erase_cost; //総消費コストに加算

                Debug.Log("消すコスト(カット時):" + stageMgr.cut_erase_cost + ", " + "所持エナジー:" + stageMgr.have_ene + ", " + "総消費コスト：" + stageMgr.all_sum_cos);
                CutInCopy(_startPos, _endPos, false);
                CutInCopyObject(cols, false);
                urFunc.InfoPushToStack();
            }
            else
            {
                InitTileData();
                captureCopyZone.disableImage();
                Debug.Log("カットできません！");
            }
        }
        else
        {
            CutInCopyObject(cols, false);
            Debug.Log("消すコスト(カット時):" + stageMgr.cut_erase_cost + ", " + "所持エナジー:" + stageMgr.have_ene);
        }

    }

    //選択範囲のオブジェクトをコピーする関数
    public void CopyObject(Vector3 startPos, Vector3 endPos, bool isCut = false)
    {
        
    }

    public void CutInCopyObject(Collider2D[] cols,bool isFirst)
    {
        foreach (var col in cols)
        {
            if (col.gameObject.tag != "Tilemap"
                && col.gameObject.tag != "Player"
                && col.gameObject.tag != "Uncuttable")
            {
                if (isFirst)    //一週目
                {
                    //tileSB.tileDataList.Single(t => t.tile == tilemap.GetTile(p)).p_ene;
                    stageMgr.write_cost += objSB.objectList.Single(t => t.obj.tag == col.gameObject.tag).p_ene;
                    if (stageMgr.all_isCut) stageMgr.cut_erase_cost += objSB.objectList.Single(t => t.obj.tag == col.gameObject.tag).ow_ene;
                }
                else            //二週目
                {
                    //Debug.Log(col.gameObject.name);
                    StageManager.ObjectData c = new StageManager.ObjectData();
                    if (!stageMgr.all_isCut)
                    {
                        c.obj = Instantiate(col.gameObject);
                        c.obj.transform.parent = stageMgr.transform;
                        //投げられるものならデータの引き継ぎ
                        ThrowObjectController toc = col.gameObject.GetComponent<ThrowObjectController>();
                        if (toc != null)
                        {
                            Vector3 dir = toc.GetDir();
                            c.obj.GetComponent<ThrowObjectController>().SetDir(dir);
                        }
                    }
                    else
                    {
                        c.obj = col.gameObject;
                        col.gameObject.transform.parent = stageMgr.transform;
                    }
                    c.pos = col.gameObject.transform.position - ChangeVecToInt(startPos);
                    c.obj.SetActive(false);
                    stageMgr.objectData.Add(c);
                }
            }

        }
    }

    public void CutInCopy(Vector3Int _startPos, Vector3 _endPos, bool Count)
    {
        //コピー範囲のtileをコピー
        for (int y = 0; y < stageMgr.tileData.height; y++)
        {
            //1列ごとのList
            List<TileBase> tBases = new List<TileBase>();
            for (int x = 0; x < stageMgr.tileData.width; x++)
            {
                Vector3Int p = Vector3Int.zero;
                //向きによって取得する
                switch (stageMgr.tileData.direction)
                {
                    case 0:
                        p = new Vector3Int(
                        _startPos.x + x,
                        _startPos.y + y, 0);
                        break;
                    case 1:
                        p = new Vector3Int(
                        _startPos.x + x,
                        _startPos.y - y, 0);
                        break;
                    case 2:
                        p = new Vector3Int(
                        _startPos.x - x,
                        _startPos.y - y, 0);
                        break;
                    case 3:
                        p = new Vector3Int(
                        _startPos.x - x,
                        _startPos.y + y, 0);
                        break;
                    default: break;
                }

                TileBase t = tilemap.GetTile(p);
                if (tilemap.HasTile(p)) //kyosu もしそのセルがタイルを持っているなら
                {
                    if(Count)
                    {
                        stageMgr.write_cost += tileSB.tileDataList.Single(t => t.tile == tilemap.GetTile(p)).p_ene; // 取得したタイルがタイルパレットのどのタイルかを判別してその消費コストを＋
                        if (stageMgr.all_isCut)
                        {
                            stageMgr.cut_erase_cost += tileSB.tileDataList.Single(t => t.tile == tilemap.GetTile(p)).ow_ene;
                            Debug.Log("消えるコスト" + stageMgr.cut_erase_cost);
                            //tilemap.SetTile(p, null);
                        }
                    }
                    else
                    {
                        if (stageMgr.all_isCut)
                        {
                            tilemap.SetTile(p, null);
                        }
                    }
                }
                tBases.Add(t);
                /*if (t != null) Debug.Log(t.name);
                else Debug.Log("null");*/
            }
            stageMgr.tileData.tiles.Add(tBases);
        }
    }

    //コピーかカットかの選択が終わった後の共通の処理
    void InitWhichMode()
    {
        //停止解除
        Time.timeScale = 1f;
        //四角を消す
        isDrawing = false;
        frame2.SetActive(false);

        //データ保持フラグon!
        stageMgr.tileData.hasData = true;
        makeDecision = false;
        anounce.SetActive(false);
    }

    //コピーしていた情報の初期化
    void InitList(List<List<TileBase>> list)
    {
        list = new List<List<TileBase>>();
        stageMgr.tileData.hasData = false;
    }

    void InitTileData()
    {
        stageMgr.tileData.tiles = new List<List<TileBase>>();
        stageMgr.tileData.width = 0;
        stageMgr.tileData.height = 0;
        stageMgr.tileData.hasData = false;

        stageMgr.objectData = new List<StageManager.ObjectData>();
    }

    //マウスの座標をタイルの座標に変換する関数
    public Vector3Int ChangeVecToInt(Vector3 v)
    {
        Vector3Int pos = Vector3Int.zero;
        pos.x = (int)Mathf.Floor(v.x);
        pos.y = (int)Mathf.Floor(v.y);
        //pos.z = (int)Mathf.Floor(v.z);

        return pos;
    }
}
