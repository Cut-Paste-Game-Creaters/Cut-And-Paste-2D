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

    public class CopyClearDoor
    {
        public Sprite unOpenSprite;
        public Sprite openSprite;
        public bool isLocked;

        public CopyClearDoor(ClearDoorScript cDoor)
        {
            unOpenSprite = cDoor.GetUnOpenSprite();
            openSprite = cDoor.GetOpenSprite();
            isLocked = cDoor.GetIsLocked();
        }
    }

    public bool GetIsLocked()
    {
        return isLocked;
    }

    public void SetIsLocked(bool isLocked_)
    {
        isLocked = isLocked_;
    }

    public Sprite GetUnOpenSprite()
    {
        return unOpenSprite;
    }
    public void SetUnOpenSprite(Sprite uos)
    {
        unOpenSprite = uos;
    }

    public Sprite GetOpenSprite()
    {
        return openSprite;
    }

    public void SetOpenSprite(Sprite os)
    {
        openSprite = os;
    }

    // Start is called before the first frame update
    void Start()
    {
        stageManager = FindObjectOfType<StageManager>();
        sr = GetComponent<SpriteRenderer>();
        //鍵があるなら、ロックしてtag="Untagged"にしておく。
        if (isLocked)
        {
            sr.sprite = unOpenSprite;
            this.gameObject.tag = "Untagged";
        }   //鍵がないなら、解放してtag="Goal"にしておく
        else
        {
            sr.sprite = openSprite;
            this.gameObject.tag = "Goal";
        }
        //Player_functionでtag=="Goal"のものと接触したらゴール判定にする
    }

    // Update is called once per frame
    void Update()
    {
        //鍵がかかってるドアで、鍵が取得されたら開放する。
        if(isLocked && stageManager.key_lock_state)
        {
            sr.sprite = openSprite;
            this.gameObject.tag = "Goal";
        }
    }
}
