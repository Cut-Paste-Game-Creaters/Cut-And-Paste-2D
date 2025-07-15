using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearDoorScript : MonoBehaviour
{
    [SerializeField] Sprite unOpenSprite;
    [SerializeField] Sprite openSprite;
    [SerializeField] bool isLocked = true;

    private SpriteRenderer sr;
    private StageManager stageManager;
    // Start is called before the first frame update
    void Start()
    {
        stageManager = FindObjectOfType<StageManager>();
        sr = GetComponent<SpriteRenderer>();
        if (isLocked)
        {
            sr.sprite = unOpenSprite;
        }
        else
        {
            sr.sprite = openSprite;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isLocked && stageManager.key_lock_state)
        {
            sr.sprite = openSprite;
        }
    }
}
