using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealItemScript : MonoBehaviour
{
    private StageManager stageManager;
    private int Healamount = 20;

    void Start()
    {
        stageManager = FindObjectOfType<StageManager>();

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (stageManager.player_HP + Healamount <= stageManager.player_MAXHP)
        {
            stageManager.player_HP += Healamount;
            Debug.Log(Healamount + "‰ñ•œ");
            SEManager.instance.ClipAtPointSE(SEManager.instance.healSE);
        }
        else
        {
            stageManager.player_HP = stageManager.player_MAXHP;
        }


        Destroy(this.gameObject);
    }
}
