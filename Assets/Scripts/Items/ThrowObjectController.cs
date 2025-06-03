using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowObjectController : MonoBehaviour
{
    public float destroyTime = 5.0f;

    private int i = 0;
    private Vector3 moveDir = Vector3.zero;
    private float nowTime = 0.0f;

    public void SetDir(Vector3 dir)
    {
        moveDir = dir;
    }
    // Start is called before the first frame update
    void Start()
    {
        //Destroy(this.gameObject, destroyTime);
    }

    // Update is called once per frame
    void Update()
    {
        //�ړ�
        this.transform.position += moveDir * PlayerInput.GetDeltaTime();

        //���Ԍo�߂ŏ���
        nowTime += PlayerInput.GetDeltaTime();
        if(nowTime > destroyTime)
        {
            Destroy(this.gameObject);
        }
    }
}
