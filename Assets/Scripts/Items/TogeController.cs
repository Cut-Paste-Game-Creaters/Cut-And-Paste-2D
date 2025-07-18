using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TogeController : MonoBehaviour
{
    StageManager stageMgr;
    public int togeDamage;

    //コンストラクタ
    public TogeController(TogeController toge)
    {
        togeDamage = toge.togeDamage;
    }

    // Start is called before the first frame update
    void Start()
    {
        stageMgr = FindObjectOfType<StageManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player") //衝突したのがプレイヤーなら
        {
            stageMgr.DamageToPlayer(togeDamage);
            Debug.Log("トゲからプレイヤーへ" + togeDamage + "のダメージ");
        }
    }
}
