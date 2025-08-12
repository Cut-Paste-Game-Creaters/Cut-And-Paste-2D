using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealItemScript : MonoBehaviour
{
    private int Healamount = 30;

    void Start()
    {
        // もう StageManager の参照取得は不要なので空でOK
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤーにぶつかったときのみ処理
        if (StageManager.Instance == null) return;

        if (other.CompareTag("Player")) // ← ここを修正
        {
            SEManager.instance.ClipAtPointSE(SEManager.instance.healSE);

            if (StageManager.Instance.player_HP + Healamount <= StageManager.Instance.player_MAXHP)
            {
                StageManager.Instance.player_HP += Healamount;
                Debug.Log(Healamount + " 回復");
            }
            else
            {
                StageManager.Instance.player_HP = StageManager.Instance.player_MAXHP;
            }

            Destroy(this.gameObject);
        }
    }
}
