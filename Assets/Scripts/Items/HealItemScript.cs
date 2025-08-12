using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealItemScript : MonoBehaviour
{
    private int Healamount = 30;

    void Start()
    {
        // ���� StageManager �̎Q�Ǝ擾�͕s�v�Ȃ̂ŋ��OK
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // �v���C���[�ɂԂ������Ƃ��̂ݏ���
        if (StageManager.Instance == null) return;

        if (other.CompareTag("Player")) // �� �������C��
        {
            SEManager.instance.ClipAtPointSE(SEManager.instance.healSE);

            if (StageManager.Instance.player_HP + Healamount <= StageManager.Instance.player_MAXHP)
            {
                StageManager.Instance.player_HP += Healamount;
                Debug.Log(Healamount + " ��");
            }
            else
            {
                StageManager.Instance.player_HP = StageManager.Instance.player_MAXHP;
            }

            Destroy(this.gameObject);
        }
    }
}
