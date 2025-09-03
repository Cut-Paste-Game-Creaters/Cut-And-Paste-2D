using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowObjectController : MonoBehaviour
{
    public float destroyTime = 25.0f;
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
        //移動
        this.transform.position += moveDir * PlayerInput.GetDeltaTime();

        //時間経過で效滅
        nowTime += PlayerInput.GetDeltaTime();
        // SpriteRendererの現在色を取得
        Color c = sr.color;

        // 全体の5分の3を過ぎたらフェード処理開始
        if (nowTime > destroyTime / 5 * 3)
        {
            float t = (nowTime - (destroyTime / 5 * 3)) / (destroyTime - (destroyTime / 5 * 3));

            float fade = Mathf.Lerp(1f, 0.05f, t);

            c.a = fade;


            sr.color = c;
        }
        if (nowTime > destroyTime)
        {
            Destroy(this.gameObject);
        }
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if(other.gameObject.tag =="Player")
        {
            other.gameObject.transform.position += moveDir * PlayerInput.GetDeltaTime();//長澤追加
            Debug.Log("動かしてる");
        }
    }
}
