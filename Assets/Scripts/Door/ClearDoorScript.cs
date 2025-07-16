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
        //��������Ȃ�A���b�N����tag="Untagged"�ɂ��Ă����B
        if (isLocked)
        {
            sr.sprite = unOpenSprite;
            this.gameObject.tag = "Untagged";
        }   //�����Ȃ��Ȃ�A�������tag="Goal"�ɂ��Ă���
        else
        {
            sr.sprite = openSprite;
            this.gameObject.tag = "Goal";
        }
        //Player_function��tag=="Goal"�̂��̂ƐڐG������S�[������ɂ���
    }

    // Update is called once per frame
    void Update()
    {
        //�����������Ă�h�A�ŁA�����擾���ꂽ��J������B
        if(isLocked && stageManager.key_lock_state)
        {
            sr.sprite = openSprite;
            this.gameObject.tag = "Goal";
        }
    }
}
