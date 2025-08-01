using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowObjectController : MonoBehaviour
{
    public float destroyTime = 20.0f;
    public float nowTime = 0.0f;
    public float disAppearTime = 0.5f;

    private Vector3 moveDir = Vector3.zero;
    private SpriteRenderer sr;
    private bool nowDisplayState = true;

    public class CopyThrowObjectController
    {
        public float destroyTime;
        public float nowTime;
        public float disAppearTime;
        public Vector3 moveDir;

        public CopyThrowObjectController(ThrowObjectController toC)
        {
            destroyTime = toC.destroyTime;
            nowTime = toC.nowTime;
            disAppearTime = toC.disAppearTime;
            moveDir = toC.GetDir();
        }
    }

    public void SetDir(Vector3 dir)
    {
        moveDir = dir;
    }
    public Vector3 GetDir()
    {
        return moveDir;
    }
    // Start is called before the first frame update
    void Start()
    {
        //Destroy(this.gameObject, destroyTime);
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //ˆÚ“®
        this.transform.position += moveDir * PlayerInput.GetDeltaTime();

        //ŽžŠÔŒo‰ß‚ÅÁ–Å
        nowTime += PlayerInput.GetDeltaTime();
        //‘S‘Ì‚Ì5•ª‚Ì2‚É‚È‚Á‚½‚ç
        if(nowTime > destroyTime / 5 * 3)
        {
            sr.enabled = Mathf.Floor(nowTime / disAppearTime) % 2 == 0;
        }
        if(nowTime > destroyTime)
        {
            Destroy(this.gameObject);
        }
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if(other.gameObject.tag =="Player")
        {
            other.gameObject.transform.position += moveDir * PlayerInput.GetDeltaTime();//’·àV’Ç‰Á
            Debug.Log("“®‚©‚µ‚Ä‚é");
        }
    }
}
