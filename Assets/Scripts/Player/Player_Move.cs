using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Idle,
    MoveRight,
    MoveLeft,
    Jump,
    Damage,
}

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
    [SerializeField] float objectCheckDistance = 0.1f;
    [SerializeField] float maxHoldTime = 0.2f;          // 追加ジャンプ力を加える最大時間（秒）
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask longobjectLayer;

    private StageManager stageManager;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private PlayerState currentState=PlayerState.Idle;
    private bool isJumping = false;
    private float jumpTimeCounter;
    private float gravity_init = 0;
    private AnimationManager animManager;
    // Start is called before the first frame update


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        gravity_init = rb.gravityScale;
        Time.fixedDeltaTime = 0.01f;
        animManager = FindObjectOfType<AnimationManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(animManager==null)animManager = FindObjectOfType<AnimationManager>();
        HandleInput();
        UpdateState();

        /*
        Vector3 pos = this.transform.position;
        float dx = 0.0f;
        //プレイヤーの左右の動き
        if (PlayerInput.GetKey(KeyCode.A))
        {
            dx   = -moveSpeed * PlayerInput.GetDeltaTime();
            sr.flipX = true;
            if(!isJumping)animManager.Play("player_walk", sr,true);
            isWaiting = false;
        }
        if (PlayerInput.GetKey(KeyCode.D))
        {
            dx = moveSpeed * PlayerInput.GetDeltaTime();
            sr.flipX = false;
            if (!isJumping) animManager.Play("player_walk", sr,true);
            isWaiting = false;
        }
        //移動してないならwait
        if(Mathf.Abs(dx)< 0.001f)
        {
            if (!isWaiting)
            {
                animManager.Play("player_wait", sr);
                isWaiting = true;
            }
        }

            pos.x += dx;

        //プレイヤーのジャンプ

        // 接地していて Wキーを押した瞬間にジャンプ開始
        if (IsGrounded() && PlayerInput.GetKeyDown(KeyCode.W))
        {
            SEManager.instance.ClipAtPointSE(SEManager.instance.jumpSE);//効果音
            isJumping = true;
            jumpTimeCounter = maxHoldTime;
            animManager.Play("player_jump", sr);
           
        }

        // Wキーを離したらジャンプ終了
        if (PlayerInput.GetKeyUp(KeyCode.W))
        {
            isJumping = false;
        }

        this.transform.position = pos;
        */

    }

    private void HandleInput()
    {
        float move = PlayerInput.GetAxisRaw("Horizontal");

        Vector3 pos = this.transform.position;

        if (move > 0)
        {
            pos.x += moveSpeed * PlayerInput.GetDeltaTime();
            currentState = PlayerState.MoveRight;
            sr.flipX = false;
        }
        else if (move < 0)
        {
            pos.x -= moveSpeed * PlayerInput.GetDeltaTime();
            currentState = PlayerState.MoveLeft;
            sr.flipX = true;
        }
        else
        {
            currentState = PlayerState.Idle;
        }

        if (IsGrounded() && PlayerInput.GetKeyDown(KeyCode.W))
        {
            SEManager.instance.ClipAtPointSE(SEManager.instance.jumpSE);//効果音
            isJumping = true;
            jumpTimeCounter = maxHoldTime;
 
        }
        if (!IsGrounded())
        {
            currentState = PlayerState.Jump;
        }
        // Wキーを離したらジャンプ終了
        if (PlayerInput.GetKeyUp(KeyCode.W))
        {
            isJumping = false;
        }

        this.transform.position = pos;

        if (stageManager == null) stageManager = FindObjectOfType<StageManager>();
        if(stageManager.isPlayerDamaged)currentState = PlayerState.Damage;
    }

    private void UpdateState()
    {

        // 必要に応じて状態に応じた処理を書く（アニメーションなど）
        switch (currentState)
        {
            case PlayerState.Idle:
                animManager.Play("player_wait", sr,true);
                break;
            case PlayerState.MoveRight:
                animManager.Play("player_walk", sr, true);
                break;
            case PlayerState.MoveLeft:
                animManager.Play("player_walk", sr, true);
                break;
            case PlayerState.Jump:
                animManager.Play("player_jump", sr, true);
                break;
            case PlayerState.Damage:
                animManager.Play("player_damage",sr,true);
                break;
            default:break;
        }
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
        Vector3 dif = new Vector3(0.35f, 0, 0);
        Vector3 dif_Lobj = new Vector3(0.05f, 0, 0);
        RaycastHit2D hit_T = Physics2D.Raycast(pos, Vector2.down, groundCheckDistance, groundLayer);
        RaycastHit2D hitR_T = Physics2D.Raycast(pos+ dif, Vector2.down, groundCheckDistance, groundLayer);
        RaycastHit2D hitL_T = Physics2D.Raycast(pos- dif, Vector2.down, groundCheckDistance, groundLayer);
        Debug.DrawRay(pos, Vector2.down * groundCheckDistance, Color.red);
        Debug.DrawRay(pos+ dif, Vector2.down * groundCheckDistance, Color.red);
        Debug.DrawRay(pos- dif, Vector2.down * groundCheckDistance, Color.red);

        RaycastHit2D hit_O = Physics2D.Raycast(pos, Vector2.down, objectCheckDistance, longobjectLayer);
        RaycastHit2D hitR_O = Physics2D.Raycast(pos + dif_Lobj, Vector2.down, objectCheckDistance, longobjectLayer);
        RaycastHit2D hitL_O = Physics2D.Raycast(pos - dif_Lobj, Vector2.down, objectCheckDistance, longobjectLayer);
        Debug.DrawRay(pos, Vector2.down * objectCheckDistance, Color.blue);
        Debug.DrawRay(pos + dif_Lobj, Vector2.down * objectCheckDistance, Color.blue);
        Debug.DrawRay(pos - dif_Lobj, Vector2.down * objectCheckDistance, Color.blue);
        //Debug.Log("hit" + hit.collider);
        return rb.velocity.y <= 0.5f && (hit_T.collider != null || hitR_T.collider != null || hitL_T.collider != null || hit_O.collider != null || hitR_O.collider != null || hitL_O.collider != null);//y方向の速度が下向き（または停止）を追加
    }
}
