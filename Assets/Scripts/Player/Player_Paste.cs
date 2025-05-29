using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class Player_Paste : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] StageManager stageManager;
    [SerializeField] GameObject frame1;
    [SerializeField] TileScriptableObject tileSB; //ScriptableObject

    private SpriteRenderer sr;
    // Start is called before the first frame update
    void Start()
    {
        frame1 = Instantiate(frame1);
        sr = frame1.GetComponent<SpriteRenderer>();
        sr.enabled = false;
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
        //右クリックで貼り付け
        if (PlayerInput.GetMouseButtonUp(1))
        {
            //オブジェクトをペースト
            PasteObject();


            //タイルをペースト
            /*for (int y = 0; y < stageManager.tileData.height; y++)
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

                    if (tilemap.HasTile(_p)) //kyosu もしそのセルがタイルを持っているなら
                    {
                        stageManager.erase_cost += tileSB.tileDataList.Single(t => t.tile == tilemap.GetTile(_p)).ow_ene; // 取得したタイルがタイルパレットのどのタイルかを判別してその消費コストを＋
                    }
                    tilemap.SetTile(_p, stageManager.tileData.tiles[y][x]);
                }
            }*/
            CheckCost(false, mPos); //タイルセットしないで計算

            int divide = 1;
            if(stageManager.all_isCut)
            {
                divide = 2;
            }

            if(stageManager.have_ene >= (stageManager.erase_cost + stageManager.write_cost)) //所持コストから引けるなら
            {
                stageManager.have_ene -= (stageManager.erase_cost + stageManager.write_cost / divide); //コスト引く
                CheckCost(true, mPos);
                Debug.Log("消えるコスト：" + stageManager.erase_cost + "," + "増やすコスト：" + stageManager.write_cost + ", " + "所持コスト：" + stageManager.have_ene);
            }
            Debug.Log("isCut == " + stageManager.all_isCut);

            if (stageManager.tileData.isCut) //1回のみペーストにする処理
            {
                InitTileData();
            }
        }
    }

    public void PasteObject()
    {
        if (stageManager.objectData.Count > 0)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            foreach (StageManager.ObjectData c in stageManager.objectData)
            {
                if (stageManager.tileData.isCut)
                {
                    c.obj.SetActive(true);
                    c.obj.transform.position = c.pos + ChangeVecToInt(mousePos);
                }
                else
                {

                }
                    
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

    void InitTileData()
    {
        stageManager.tileData.tiles = new List<List<TileBase>>();
        stageManager.tileData.width = 0;
        stageManager.tileData.height = 0;
        stageManager.tileData.hasData = false;

        stageManager.objectData = new List<StageManager.ObjectData>();
    }

    public void CheckCost(bool isSetTile, Vector3Int mPos)
    {
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

                    if(isSetTile)
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
}

