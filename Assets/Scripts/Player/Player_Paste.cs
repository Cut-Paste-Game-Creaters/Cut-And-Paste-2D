using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class Player_Paste : MonoBehaviour
{
    [SerializeField] GameObject frame1;
    [SerializeField] TileScriptableObject tileSB; //ScriptableObject
    [SerializeField] ObjectScriptableObject objSB; //ScriptableObject
    //[SerializeField] GameObject rectPrefab;

    private Tilemap tilemap;
    private StageManager stageManager;
    private SpriteRenderer sr;
    private Vector3 frameData = Vector3.zero;
    UndoRedoFunction urFunc;
    //private GameObject rect;
    // Start is called before the first frame update
    void Start()
    {
        frame1 = Instantiate(frame1);
        sr = frame1.GetComponent<SpriteRenderer>();
        sr.enabled = false;
        //rect = Instantiate(rectPrefab);
        Tilemap[] maps = FindObjectsOfType<Tilemap>();
        foreach (var map in maps)
        {
            if (map.gameObject.tag == "Tilemap")
            {
                tilemap = map;
                break;
            }
        }
        stageManager = FindObjectOfType<StageManager>();
        urFunc = FindObjectOfType<UndoRedoFunction>();
    }

    // Update is called once per frame
    void Update()
    {
        if (stageManager.tileData.hasData)
        {
            PasteTiles(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    void PasteTiles(Vector3 mousePos)
    {
        stageManager.erase_cost = 0; //最初に貼り付け箇所のコストを初期化

        //現在のマウス位置をもらい、原点とする
        Vector3Int mPos = ChangeVecToInt(mousePos);

        //右クリックを押してる間枠表示
        if (PlayerInput.GetMouseButton(1))
        {
            sr.enabled = true;
            frameData = stageManager.GetInfo();
            sr.size = frameData;
            Vector3 framePos = mPos;

            switch (frameData.z)
            {
                //最初の位置は真ん中の右上なので個別に調節する必要がある
                case 0:     //右上
                    framePos.x += frameData.x / 2;
                    framePos.y += frameData.y / 2;
                    break;
                case 1:     //右下
                    framePos.x += frameData.x / 2;
                    framePos.y -= frameData.y / 2;
                    framePos.y++;
                    break;
                case 2:     //左下
                    framePos.x -= frameData.x / 2;
                    framePos.y -= frameData.y / 2;
                    framePos.x++;
                    framePos.y++;
                    break;
                case 3:     //左上
                    framePos.x -= frameData.x / 2;
                    framePos.y += frameData.y / 2;
                    framePos.x++;
                    break;
                default: break;
            }

            //貼り付け時の消すコストを計算する
            CheckCost(false, mPos); //タイルセットしないで計算
            CheckObjectCost(false);      //オブジェクトの消すコストを計算する
            //Debug.Log("erase:"+stageManager.erase_cost);

            frame1.transform.position = framePos;
        }
        //右クリックで貼り付け
        if (PlayerInput.GetMouseButtonUp(1))
        {
            sr.enabled = false;     //枠を非表示にする

            //貼り付け時の消すコストを計算する
            CheckCost(false, mPos); //タイルセットしないで計算
            CheckObjectCost(false);      //オブジェクトの消すコストを計算する
            Debug.Log("erase:" + stageManager.erase_cost);

            int divide = 1;
            if(stageManager.all_isCut)
            {
                divide = 2;
            }

            if(stageManager.have_ene >= (stageManager.erase_cost + stageManager.write_cost)) //所持コストから引けるなら
            {
                stageManager.have_ene -= (stageManager.erase_cost + stageManager.write_cost / divide); //コスト引く
                stageManager.all_sum_cos += (stageManager.erase_cost + stageManager.write_cost / divide); //総消費コストに加算
                Debug.Log("消えるコスト：" + stageManager.erase_cost + "," + "増やすコスト：" + stageManager.write_cost / divide + ", " + "所持コスト：" + stageManager.have_ene);
                Debug.Log("総消費コスト：" + stageManager.all_sum_cos);
                CheckCost(true, mPos);
                CheckObjectCost(true);
                //オブジェクトをペースト
                PasteObject();

                if (stageManager.all_isCut) //1回のみペーストにする処理
                {
                    InitTileData();
                }

                urFunc.InfoPushToStack();
            }
            //Debug.Log("isCut == " + stageManager.all_isCut);

        }
    }

    public void PasteObject()
    {
        if (stageManager.objectData.Count > 0)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            foreach (var c in stageManager.objectData)
            {
                if (stageManager.all_isCut)
                {
                    c.obj.SetActive(true);
                    c.obj.transform.position = c.pos + ChangeVecToInt(mousePos);
                    c.obj.transform.parent = null;
                }
                else
                {
                    if(c.obj.transform.parent != null)
                    {
                        c.obj.transform.parent = null;
                    }
                    //コピーしたものを複製する
                    GameObject b = Instantiate(c.obj);
                    //もし投げられるオブジェクトならデータの引き継ぎ
                    ThrowObjectController toc = c.obj.GetComponent<ThrowObjectController>();
                    if (toc != null)
                    {
                        Vector3 dir = toc.GetDir();
                        b.GetComponent<ThrowObjectController>().SetDir(dir);
                    }
                    b.SetActive(true);
                    b.transform.position = c.pos + ChangeVecToInt(mousePos);
                }
                stageManager.EraseObjects.Add(c.obj);
            }
        }

    }

    public void CheckCost(bool isSetTile, Vector3Int mPos)
    {
        //if(!isSetTile)stageManager.erase_cost = 0;
        //タイルをペースト
        for (int y = 0; y < stageManager.tileData.height; y++)
        {
            for (int x = 0; x < stageManager.tileData.width; x++)
            {
                //コピーしたときの方向によって原点が異なる
                Vector3Int _p = Vector3Int.zero;
                switch (stageManager.tileData.direction)
                {
                    case 0:
                        _p = new Vector3Int(mPos.x + x, mPos.y + y, 0);
                        break;
                    case 1:
                        _p = new Vector3Int(mPos.x + x, mPos.y - y, 0);
                        break;
                    case 2:
                        _p = new Vector3Int(mPos.x - x, mPos.y - y, 0);
                        break;
                    case 3:
                        _p = new Vector3Int(mPos.x - x, mPos.y + y, 0);
                        break;
                    default: break;
                }

                if (isSetTile)
                {
                    tilemap.SetTile(_p, stageManager.tileData.tiles[y][x]);
                }
                else
                {
                    if (tilemap.HasTile(_p)) //kyosu もしそのセルがタイルを持っているなら
                    {
                        stageManager.erase_cost += tileSB.tileDataList.Single(t => t.tile == tilemap.GetTile(_p)).ow_ene; // 取得したタイルがタイルパレットのどのタイルかを判別してその消費コストを＋
                        
                    }
                }
            }
        }
    }

    public void CheckObjectCost(bool isErase)
    {
        Vector3Int _p = Vector3Int.zero;
        Vector3Int mPos = ChangeVecToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition)
            ,stageManager.tileData.direction);
        int w = stageManager.tileData.width;
        int h = stageManager.tileData.height;

        
        //範囲を計算する
        switch (stageManager.tileData.direction)
        {
            case 0:
                _p = new Vector3Int(mPos.x + w, mPos.y + h, 0);
                break;
            case 1:
                _p = new Vector3Int(mPos.x + w, mPos.y - h, 0);
                break;
            case 2:
                _p = new Vector3Int(mPos.x - w, mPos.y - h, 0);
                break;
            case 3:
                _p = new Vector3Int(mPos.x - w, mPos.y + h, 0);
                break;
            default: break;
        }

        //デバッグ用　多分選択範囲を示している
        //rect.transform.position = (mPos + _p) / 2;
        //rect.transform.localScale = new Vector3(Mathf.Abs(_p.x - mPos.x), Mathf.Abs(_p.y - mPos.y), 1);


        Collider2D[] cols = Physics2D.OverlapAreaAll(
            new Vector2(mPos.x,mPos.y), new Vector2(_p.x,_p.y)
            );
        //上書き範囲内のコライダーの消すコストを計算する
        foreach(var col in cols)
        {
            if (col.gameObject.tag != "Tilemap"
                && col.gameObject.tag != "Player"
                && col.gameObject.tag != "Uncuttable")
            {
                Debug.Log("erase:" + col.gameObject.name);
                if (!isErase) stageManager.erase_cost += objSB.objectList.Single(t => t.obj.tag == col.gameObject.tag).ow_ene;
                else Destroy(col.gameObject);       //上書きするオブジェクトを消す
            }
                
        }
    }

    public Vector3Int ChangeVecToInt(Vector3 v)
    {
        Vector3Int pos = Vector3Int.zero;
        pos.x = (int)Mathf.Floor(v.x);
        pos.y = (int)Mathf.Floor(v.y);
        //pos.z = (int)Mathf.Floor(v.z);

        return pos;
    }

    public Vector3Int ChangeVecToInt(Vector3 v,int dir)
    {
        Vector3Int pos = Vector3Int.zero;
        switch (dir)
        {
            case 0:     //右上にコピー　つまり左下にキャスト
                pos = new Vector3Int((int)Mathf.Floor(v.x),(int)Mathf.Floor(v.y),0);
                break;
            case 1:     //右下にコピー　つまり左上にキャスト
                pos = new Vector3Int((int)Mathf.Floor(v.x), (int)Mathf.Ceil(v.y),0);
                break;
            case 2:     //左下にコピー　つまり右上にキャスト
                pos = new Vector3Int((int)Mathf.Ceil(v.x), (int)Mathf.Ceil(v.y), 0);
                break;
            case 3:     //左上にコピー　つまり右下にキャスト
                pos = new Vector3Int((int)Mathf.Ceil(v.x), (int)Mathf.Floor(v.y), 0);
                break;
            default:break;
        }

        return pos;
    }

    void InitTileData()
    {
        stageManager.tileData.tiles = new List<List<TileBase>>();
        stageManager.tileData.width = 0;
        stageManager.tileData.height = 0;
        stageManager.tileData.hasData = false;

        stageManager.objectData = new List<StageManager.ObjectData>();
    }
}

