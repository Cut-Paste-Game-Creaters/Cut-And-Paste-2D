using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraMove : MonoBehaviour
{
    private GameObject player;
    private Tilemap tilemap;
    public float Screen_Left = 9.3f;
    // Start is called before the first frame update
    void Start()
    {
        Tilemap[] maps = FindObjectsOfType<Tilemap>();
        foreach(var map in maps)
        {
            if (map.gameObject.tag == "Tilemap")
            {
                tilemap = map;
                break;
            }
        }
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        float posx = transform.position.x;
        float posy = transform.position.y;
        //もし画面端でないなら
        //プレイヤーのX - (画面幅/2) = 左端のXだったら止める
        if (player.transform.position.x - Screen_Left > tilemap.cellBounds.min.x+1//←側のブロック分引く
            &&
            player.transform.position.x + 16 < tilemap.cellBounds.max.x-1
            )
        {
            posx = player.transform.position.x;
        }

        if(
            player.transform.position.y - 9 >= tilemap.cellBounds.min.y      //プレイヤーの下端
            &&
            player.transform.position.y + 9 < tilemap.cellBounds.max.y      //プレイヤーの上端
            )
        {
            posy = player.transform.position.y;
        }
        this.transform.position = new Vector3(posx,posy, -10);
        //Debug.Log("t:"+tilemap.cellBounds.min.x);
        //Debug.Log("p:"+player.transform.position.x);
    }
}//-6.66 -16 16-6.7
