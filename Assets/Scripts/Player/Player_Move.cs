using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Move : MonoBehaviour
{
    /*
     注意すること
    ・動きに関するものはPlayerInput.deltaTimeを使用する。
    ・入力もPlayerInputを使用する。
    ・物理演算(rigidbodyなど)を使うときは必ずFixedUpdate
    ・入力と物理演算の処理は分ける。
    ・TimeScaleを0にすると、Updateは普通に動くがFixedUpdateは動かなくなる
     */

    [SerializeField] float moveSpeed = 1.0f;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float groundCheckDistance = 0.1f;
    [SerializeField] float maxHoldTime = 0.2f;          // 追加ジャンプ力を加える最大時間（秒）
    [SerializeField] LayerMask groundLayer;

    private Rigidbody2D rb; 
    private bool isJumping = false;
    private float jumpTimeCounter;
    private float gravity_init = 0;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gravity_init = rb.gravityScale;
        Time.fixedDeltaTime = 0.01f;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = this.transform.position;
        //プレイヤーの左右の動き
        if (PlayerInput.GetKey(KeyCode.A))
        {
            pos.x -= moveSpeed * PlayerInput.GetDeltaTime();
        }
        if (PlayerInput.GetKey(KeyCode.D))
        {
            pos.x += moveSpeed * PlayerInput.GetDeltaTime();
        }

        //プレイヤーのジャンプ

        // 接地していて Wキーを押した瞬間にジャンプ開始
        if (IsGrounded() && PlayerInput.GetKeyDown(KeyCode.W))
        {
            isJumping = true;
            jumpTimeCounter = maxHoldTime;
        }

        // Wキーを離したらジャンプ終了
        if (PlayerInput.GetKeyUp(KeyCode.W))
        {
            isJumping = false;
        }

        this.transform.position = pos;
    }

    private void FixedUpdate()
    {
        if (isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpTimeCounter -= PlayerInput.GetFixedDeltaTime();
            }
            else
            {
                isJumping = false; // 時間切れで終了
            }
        }

        if (rb.velocity.y < 0)
        {
            rb.gravityScale = gravity_init;
        }
        else
        {
            rb.gravityScale = gravity_init;
        }
    }

    bool IsGrounded()
    {
        Vector3 pos = this.transform.position;
        Vector3 dif = new Vector3(0.5f, 0, 0);
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.down, groundCheckDistance, groundLayer);
        RaycastHit2D hitR = Physics2D.Raycast(pos+dif, Vector2.down, groundCheckDistance, groundLayer);
        RaycastHit2D hitL = Physics2D.Raycast(pos-dif, Vector2.down, groundCheckDistance, groundLayer);
        Debug.DrawRay(pos, Vector2.down * groundCheckDistance, Color.red);
        Debug.DrawRay(pos+dif, Vector2.down * groundCheckDistance, Color.red);
        Debug.DrawRay(pos-dif, Vector2.down * groundCheckDistance, Color.red);
        //Debug.Log("hit" + hit.collider);
        return hit.collider != null || hitR.collider != null || hitL.collider != null;
    }
}
