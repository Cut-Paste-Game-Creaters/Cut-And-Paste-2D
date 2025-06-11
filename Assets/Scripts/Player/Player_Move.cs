using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Move : MonoBehaviour
{
    /*
     ���ӂ��邱��
    �E�����Ɋւ�����̂�PlayerInput.deltaTime���g�p����B
    �E���͂�PlayerInput���g�p����B
    �E�������Z(rigidbody�Ȃ�)���g���Ƃ��͕K��FixedUpdate
    �E���͂ƕ������Z�̏����͕�����B
    �ETimeScale��0�ɂ���ƁAUpdate�͕��ʂɓ�����FixedUpdate�͓����Ȃ��Ȃ�
     */

    [SerializeField] float moveSpeed = 1.0f;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float groundCheckDistance = 0.1f;
    [SerializeField] float maxHoldTime = 0.2f;          // �ǉ��W�����v�͂�������ő厞�ԁi�b�j
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
        //�v���C���[�̍��E�̓���
        if (PlayerInput.GetKey(KeyCode.A))
        {
            pos.x -= moveSpeed * PlayerInput.GetDeltaTime();
        }
        if (PlayerInput.GetKey(KeyCode.D))
        {
            pos.x += moveSpeed * PlayerInput.GetDeltaTime();
        }

        //�v���C���[�̃W�����v

        // �ڒn���Ă��� W�L�[���������u�ԂɃW�����v�J�n
        if (IsGrounded() && PlayerInput.GetKeyDown(KeyCode.W))
        {
            isJumping = true;
            jumpTimeCounter = maxHoldTime;
        }

        // W�L�[�𗣂�����W�����v�I��
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
                isJumping = false; // ���Ԑ؂�ŏI��
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
