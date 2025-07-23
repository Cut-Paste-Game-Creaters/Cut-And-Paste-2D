using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BumperForce : MonoBehaviour
{
    public float checkDistance = 2.0f;        // ���̋������Ȃ�u��t�߁v
    public float upwardForce = 5.0f;          // ������ɉ������
    public LayerMask playerLayer;

    private GameObject player;
    private Player_Move pc;
    Rigidbody2D rb;

    public float bounceForce = 100f;

    //�R���X�g���N�^
    public BumperForce(BumperForce bumper)
    {
        this.checkDistance = bumper.checkDistance;
        this.upwardForce = bumper.upwardForce;
        this.bounceForce = bumper.bounceForce;
        this.playerLayer = bumper.playerLayer;
    }

    void Start()
    {
        // �v���C���[����x�����擾�i�^�O�⃌�C���[�Ō�����j
        player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            pc = player.GetComponent<Player_Move>();

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        }
    }



    public bool IsPlayerInLineRange()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, checkDistance, playerLayer);
        return hit.collider != null && hit.collider.CompareTag("Player");
    }


    void FixedUpdate()
    {
        if (player == null || pc == null) return;

        // �v���C���[�������̏�t�߂ɂ��邩

        if (IsPlayerInLineRange() )
        {
            if (rb != null)
            {
                rb.AddForce(Vector2.up * upwardForce, ForceMode2D.Impulse);
                Debug.Log("j��Ղ��傤��");

            }
        }
    }





        private void OnCollisionEnter2D(Collision2D collision)
        {
            Rigidbody2D rb = collision.rigidbody;
            Debug.Log("huretawane");
            if (rb != null)
            {
                // �ڐG�_�̖@���x�N�g�����擾
                Vector2 normal = collision.GetContact(0).normal;

            float speed = rb.velocity.magnitude;
            Debug.Log(speed);


            // ���x�ɂ���ăo�E���h�̗͂𒲐�
            float adjustedBounceForce = 1;
            if (speed < 10f)
            {
                adjustedBounceForce = 3 +speed *0.1f ;//�x���Ă����ȏ�͒��˕Ԃ�
            }else if (speed >= 10f)
            {
                adjustedBounceForce = 4f - speed * 0.1f;//���������璲��
            }



            // �@�������ɗ͂�������
            rb.AddForce(-(normal  ) * bounceForce * adjustedBounceForce, ForceMode2D.Impulse);
                Debug.Log("huretawane");
            }
        }
    




    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * checkDistance);
    }
}
