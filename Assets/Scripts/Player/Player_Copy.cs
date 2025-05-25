using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using UnityEngine.Tilemaps;

public class Player_Copy : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] GameObject frame2;
    [SerializeField] GameObject anounce;

    private Vector3 startPos = Vector3.zero;
    private Vector3 endPos = Vector3.zero;
    private bool isDrawing = false;
    private SpriteRenderer frameSR;
    private int whichMode = -1;     //0:Copy, 1:Cut
    private bool makeDecision = false;  //マウス離したらOn

    //コピーするタイルのデータ
    private struct TileData
    {
        //現時点でw : 32, h : 18
        public int width;
        public int height;
        public int direction;   //0右上、1右下、2左下、3左上
        public List<List<TileBase>> tiles;
        public bool hasData;
        public TileData(int w,int h)
        {
            width = w;
            height = h;
            tiles = new List<List<TileBase>>();
            hasData = false;
            direction = -1;
        }
    }
    private struct ObjectData
    {
        //カットするオブジェクトの本体
        public GameObject obj;
        //カットするオブジェクトの相対位置
        public Vector3 pos;
    }

    private TileData tileData = new TileData(0,0);
    private List<ObjectData> objects = new List<ObjectData>();


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
        //マウスを右クリックして、移動して離す
        //最初と最後の座標だけとれば範囲が指定できる
        if (PlayerInput.GetMouseButtonDown(0) && !makeDecision)
        {
            //範囲選択時時間を停止する(またはスローモー)
            Time.timeScale = 0.1f;

            //これまでコピーしてたものを初期化
            InitList(tileData.tiles);
            tileData.tiles = new List<List<TileBase>>();
            tileData.hasData = false;
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


        if (PlayerInput.GetMouseButtonUp(0) && !tileData.hasData)
        {
            makeDecision = true;
            whichMode = -1;
            endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        //どちらかを選ぶフェーズ
        if (makeDecision)
        {
            switch (whichMode)
            {
                case -1:    //コピーかカットか選ぶ
                    anounce.SetActive(true);
                    if (PlayerInput.GetMouseButtonDown(0)) whichMode = 0;//copy
                    else if (PlayerInput.GetMouseButtonDown(1)) whichMode = 1;//cut
                    else if (PlayerInput.GetKeyDown(KeyCode.Escape)) whichMode = 2;//nothing
                    break;
                case 0:     //コピーするなら
                    CopyContents(startPos, endPos);
                    CopyObject(startPos, endPos);
                    InitWhichMode();
                    break;
                case 1:     //カットするなら
                    CopyContents(startPos, endPos, true);
                    CopyObject(startPos, endPos, true);
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
        //位置をintにする
        Vector3Int _startPos = ChangeVecToInt(sPos);
        Vector3Int _endPos = ChangeVecToInt(ePos);
        //幅高さを計算
        int width = Mathf.Abs((_endPos.x - _startPos.x)) + 1;
        int height = Mathf.Abs((_endPos.y - _startPos.y)) + 1;
        Debug.Log("w:" + width + " h:" + height);

        //tileDataにコピーしたものを保存
        tileData.width = width;
        tileData.height = height;


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

        tileData.direction = direction;

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
                tBases.Add(t);
                if (isCut)
                {
                    tilemap.SetTile(p, null);
                }
                /*if (t != null) Debug.Log(t.name);
                else Debug.Log("null");*/
            }
            tileData.tiles.Add(tBases);
        }

    }

    //選択範囲のオブジェクトをコピーする関数
    public void CopyObject(Vector3 startPos, Vector3 endPos, bool isCut = false)
    {
        Collider2D[] cols = Physics2D.OverlapAreaAll(startPos, endPos);
        objects = new List<ObjectData>();
        foreach (var col in cols)
        {
            if (col.gameObject.tag != "Tilemap"
                && col.gameObject.tag != "Player"
                && col.gameObject.tag != "Uncuttable")
            {
                Debug.Log(col.gameObject.name);
                ObjectData c = new ObjectData();
                if (!isCut)
                {
                    c.obj = Instantiate(col.gameObject);
                }
                else
                {
                    c.obj = col.gameObject;
                    Debug.Log("Cut!!!!");
                }
                c.pos = col.gameObject.transform.position - ChangeVecToInt(startPos);
                c.obj.SetActive(false);
                objects.Add(c);
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
        tileData.hasData = true;
        makeDecision = false;
        anounce.SetActive(false);
    }

    //コピーしていた情報の初期化
    void InitList(List<List<TileBase>> list)
    {
        list = new List<List<TileBase>>();
        tileData.hasData = false;
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
