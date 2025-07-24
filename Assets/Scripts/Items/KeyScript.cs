using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyLockScript : MonoBehaviour
{
    private StageManager stageManager;

    void Start()
    {
        stageManager = FindObjectOfType<StageManager>();
        //stageManager.key_lock_state = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.gameObject.CompareTag("Player")) // "Player"�^�O�̃I�u�W�F�N�g�ƏՓ˂�����
        {
            stageManager.key_lock_state = true;
            Destroy(this.gameObject);
        }

    }
}
