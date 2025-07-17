using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Function : MonoBehaviour
{
    private GameOverFunction gameOverFunc;
    private ClearFunction clearFunc;
    StageManager stageMgr;

    private float fallingLine_y = -10.0f;
    private SpriteRenderer sr;
    //private RigidBody rb;

    // Start is called before the first frame update
    void Start()
    {
        //rb = GetComponent<RigidBody>();
        gameOverFunc = FindObjectOfType<GameOverFunction>();
        clearFunc = FindObjectOfType<ClearFunction>();
        stageMgr = FindObjectOfType<StageManager>();
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //もしダメージをうけて無敵時間なら半透明にする
        Color currentColor = sr.color;
        currentColor.a = stageMgr.isPlayerDamaged ? 0.5f : 1.0f;
        sr.color = currentColor; 
        /*テスト用に4ボタン押すと死んだことになる 本来はhpが0 もしくは　ステージから落ちたら*/
        if(this.gameObject.transform.position.y < fallingLine_y || stageMgr.player_HP <= 0)
        {
            Die();
        }
    }

    /*
    #攻撃処理
    void AttackToEnemy()
    {

    }
    */

    /*
    #ダメージのあるものに衝突したらhpを減らす処理
    
    */

    //死亡処理
    void Die()
    {
        //ゲームオーバーファンクション呼び出し
        gameOverFunc.GameOver();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if(other.gameObject.tag == "Goal")
        {
            clearFunc.GameClear();
        }
    }
}
