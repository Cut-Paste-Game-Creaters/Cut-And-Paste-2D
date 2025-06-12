using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Function : MonoBehaviour
{
    [SerializeField] private int hp; //プレイヤーのHP
    [SerializeField] GameOverFunction gameOverFunc;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        /*テスト用に4ボタン押すと死んだことになる 本来はhpが0 もしくは　ステージから落ちたら*/
        if(PlayerInput.GetKeyDown(KeyCode.Alpha4) || hp <= 0)
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
}
