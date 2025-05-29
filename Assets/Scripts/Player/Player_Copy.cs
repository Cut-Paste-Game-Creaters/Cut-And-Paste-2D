using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using UnityEngine.Tilemaps;
using System.Linq;

public class Player_Copy : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] GameObject frame2;
    [SerializeField] GameObject anounce;
    [SerializeField] StageManager stageMgr;
    [SerializeField] TileScriptableObject tileSB; //ScriptableObject

    private Vector3 startPos = Vector3.zero;
    private Vector3 endPos = Vector3.zero;
    private bool isDrawing = false;
    private SpriteRenderer frameSR;
    private int whichMode = -1;     //0:Copy, 1:Cut
    private bool makeDecision = false;  //マウス離したらOn
    //public bool all_isCut = false; //コピー関数の引数のisCutと区別するため

    // Start is called before the first frame update
    void Start()
    {
        frame2 = Instantiate(frame2);
        frame2.SetActive(false);
        anounce = Instantiate(anounce);
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
                Time.timeScale = 0.1f;

                //これまでコピーしてたものを初期化
                InitList(stageMgr.tileData.tiles);
                stageMgr.tileData.tiles = new List<List<TileBase>>();
                stageMgr.tileData.hasData = false;
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
                return;
            }
        }

        //どちらかを選ぶフェーズ
        if (makeDecision)
        {
            switch (whichMode)
            {
                case -1:    //コピーかカットか選ぶ
                    anounce.SetActive(true);
                    if (PlayerInput.GetMouseButtonUp(0)) whichMode = 0;//copy
                    else if (PlayerInput.GetMouseButtonUp(1)) whichMode = 1;//cut
                    else if (PlayerInput.GetKeyDown(KeyCode.Escape)) whichMode = 2;//nothing
                    break;
                case 0:     //コピーするなら
                    CopyContents(startPos, endPos);
                    CopyObject(startPos, endPos);
                    //all_isCut = false;
                    stageMgr.all_isCut = false;
                    InitWhichMode();
                    break;
                case 1:     //カットするなら
                    CopyContents(startPos, endPos, true);
                    CopyObject(startPos, endPos, true);
                    //all_isCut = true;
                    stageMgr.all_isCut = true;
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
        int cut_erase_cost = 0;

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
        if (endPos.x - startPos.x >= 0)//右向き
        {
            if (endPos.y - startPos.y >= 0) //上向き
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
            if (endPos.y - startPos.y >= 0) //上向き
            {
                direction = 3;
            }
            else
            {
                direction = 2;
            }
        }

        stageMgr.tileData.direction = direction;

        //コピー範囲のtileをコピー
        for (int y = 0; y < height; y++)
        {
            //1列ごとのList
            List<TileBase> tBases = new List<TileBase>();
            for (int x = 0; x < width; x++)
            {
                Vector3Int p = Vector3Int.zero;
                //向きによって取得する
                switch (direction)
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
                    stageMgr.write_cost += tileSB.tileDataList.Single(t => t.tile == tilemap.GetTile(p)).p_ene; // 取得したタイルがタイルパレットのどのタイルかを判別してその消費コストを＋
                    if (isCut)
                    {
                        cut_erase_cost += tileSB.tileDataList.Single(t => t.tile == tilemap.GetTile(p)).ow_ene;
                        tilemap.SetTile(p, null);
                    }
                }
                tBases.Add(t);
                
                /*if (t != null) Debug.Log(t.name);
                else Debug.Log("null");*/
            }
            stageMgr.tileData.tiles.Add(tBases);
        }
        if(isCut)
        {
            if(stageMgr.have_ene >= cut_erase_cost) //所持コストから引けるなら
            {
                stageMgr.have_ene -= cut_erase_cost; //コスト引く
            }
            Debug.Log("消すコスト(カット時):" + cut_erase_cost + ", " + "所持エナジー:" + stageMgr.have_ene);
        }
    }

    //選択範囲のオブジェクトをコピーする関数
    public void CopyObject(Vector3 startPos, Vector3 endPos, bool isCut = false)
    {
        Collider2D[] cols = Physics2D.OverlapAreaAll(startPos, endPos);
        stageMgr.objectData = new List<StageManager.ObjectData>();
        foreach (var col in cols)
        {
            if (col.gameObject.tag != "Tilemap"
                && col.gameObject.tag != "Player"
                && col.gameObject.tag != "Uncuttable")
            {
                Debug.Log(col.gameObject.name);
                StageManager.ObjectData c = new StageManager.ObjectData();
                if (!isCut)
                {
                    c.obj = Instantiate(col.gameObject);
                }
                else
                {
                    c.obj = col.gameObject;
                }
                c.pos = col.gameObject.transform.position - ChangeVecToInt(startPos);
                c.obj.SetActive(false);
                stageMgr.objectData.Add(c);
            }

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
