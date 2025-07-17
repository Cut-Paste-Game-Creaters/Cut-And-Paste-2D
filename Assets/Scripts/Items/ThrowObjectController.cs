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

    //�R���X�g���N�^
    public ThrowObjectController(ThrowObjectController toC)
    {
        destroyTime = toC.destroyTime;
        nowTime = toC.nowTime;
        disAppearTime = toC.disAppearTime;
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
        //�ړ�
        this.transform.position += moveDir * PlayerInput.GetDeltaTime();

        //���Ԍo�߂ŝ���
        nowTime += PlayerInput.GetDeltaTime();
        //�S�̂�5����2�ɂȂ�����
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
            other.gameObject.transform.position += moveDir * PlayerInput.GetDeltaTime();//���V�ǉ�
            Debug.Log("�������Ă�");
        }
    }
}
