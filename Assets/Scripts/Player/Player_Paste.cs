using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player_Paste : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] StageManager stageManager;
    [SerializeField] GameObject frame1;

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
        //現在のマウス位置をもらい、原点とする
        Vector3Int mPos = ChangeVecToInt(mousePos);
        //右クリックで貼り付け
        if (PlayerInput.GetMouseButtonUp(1))
        {
            //オブジェクトをペースト
            PasteObject();


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

                    tilemap.SetTile(_p, stageManager.tileData.tiles[y][x]);
                }
            }

            if (stageManager.tileData.isCut)
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
}
