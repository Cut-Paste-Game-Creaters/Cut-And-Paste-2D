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

    /// <summary>
    /// 画面揺れ用
    public float shakeDuration = 0.25f;   // デフォルトの揺れ時間
    public float shakeMagnitude = 0.25f;  // デフォルトの揺れ強さ

    private float _shakeTimeLeft = 0f;
    private float _shakeDurationActive = 0f;
    private float _shakeMagnitudeActive = 0f;
    /// </summary>
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
        if (!PlayerInput.isPausing)
        {
            float posx = transform.position.x;
            float posy = transform.position.y;
            //もし画面端でないなら
            //プレイヤーのX - (画面幅/2) = 左端のXだったら止める
            if (player.transform.position.x - Screen_Left > tilemap.cellBounds.min.x + 1//←側のブロック分引く
                &&
                player.transform.position.x + 16 < tilemap.cellBounds.max.x - 1
                )
            {
                posx = player.transform.position.x;
            }

            if (
                player.transform.position.y - 9 >= tilemap.cellBounds.min.y      //プレイヤーの下端
                &&
                player.transform.position.y + 9 < tilemap.cellBounds.max.y      //プレイヤーの上端
                )
            {
                posy = player.transform.position.y;
            }
            this.transform.position = new Vector3(posx, posy, -10);

            ///画面揺れ用
            if (_shakeTimeLeft > 0f)
            {
                float damper = _shakeTimeLeft / _shakeDurationActive; // 終盤ほど弱く
                float offsetX = Random.Range(-1f, 1f) * _shakeMagnitudeActive * damper;
                float offsetY = Random.Range(-1f, 1f) * _shakeMagnitudeActive * damper;

                transform.position += new Vector3(offsetX, offsetY, 0f);

                _shakeTimeLeft -= Time.deltaTime;
            }
        }
        //Debug.Log("t:"+tilemap.cellBounds.min.x);
        //Debug.Log("p:"+player.transform.position.x);
        //-6.66 -16 16-6.7



 
 
    }


    public void Shake()
    {
        Shake(shakeDuration, shakeMagnitude);
    }

    public void Shake(float duration, float magnitude)
    {

            _shakeDurationActive = Mathf.Max(0.0001f, duration);
            _shakeMagnitudeActive = Mathf.Max(0f, magnitude);
            _shakeTimeLeft = _shakeDurationActive;

    }



}
