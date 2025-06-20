using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Function : MonoBehaviour
{
    public int hp; //プレイヤーのHP
    private GameOverFunction gameOverFunc;
    private ClearFunction clearFunc;

    private float fallingLine_y = -10.0f;
    //private RigidBody rb;

    // Start is called before the first frame update
    void Start()
    {
        //rb = GetComponent<RigidBody>();
        gameOverFunc = FindObjectOfType<GameOverFunction>();
        clearFunc = FindObjectOfType<ClearFunction>();
    }

    // Update is called once per frame
    void Update()
    {
        /*テスト用に4ボタン押すと死んだことになる 本来はhpが0 もしくは　ステージから落ちたら*/
        if(this.gameObject.transform.position.y < fallingLine_y || hp <= 0)
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "Goal")
        {
            clearFunc.GameClear();
        }
    }
}
