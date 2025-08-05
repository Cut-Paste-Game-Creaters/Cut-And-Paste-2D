using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TogeController : MonoBehaviour
{
    public int togeDamage;

    public class CopyTogeController
    {
        public int togeDamage;

        public CopyTogeController(TogeController toge)
        {
            togeDamage = toge.togeDamage;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && StageManager.Instance != null && !StageManager.Instance.isPlayerDamaged)
        {
            StageManager.Instance.DamageToPlayer(togeDamage);
        }
    }
}

