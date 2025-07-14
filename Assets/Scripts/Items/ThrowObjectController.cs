using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowObjectController : MonoBehaviour
{
    public float destroyTime = 20.0f;

    private Vector3 moveDir = Vector3.zero;
    public float nowTime = 0.0f;

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
    }

    // Update is called once per frame
    void Update()
    {
        //ˆÚ“®
        this.transform.position += moveDir * PlayerInput.GetDeltaTime();

        //ŽžŠÔŒo‰ß‚ÅÁ–Å
        nowTime += PlayerInput.GetDeltaTime();
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
