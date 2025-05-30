using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraMove : MonoBehaviour
{
    [SerializeField]
    GameObject player;
    [SerializeField]
    Tilemap tilemap;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //もし画面端でないなら
        //プレイヤーのX - (画面幅/2) = 左端のXだったら止める
        if(player.transform.position.x - 16 > tilemap.cellBounds.min.x+1//←側のブロック分引く
            &&
            player.transform.position.x + 16 < tilemap.cellBounds.max.x-1)
        {
            this.transform.position = new Vector3(player.transform.position.x, 0, -10);
        }
        Debug.Log("t:"+tilemap.cellBounds.min.x);
        Debug.Log("p:"+player.transform.position.x);
    }
}
