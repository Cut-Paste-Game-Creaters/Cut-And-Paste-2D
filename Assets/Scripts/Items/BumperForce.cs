using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BumperForce : MonoBehaviour
{
    public float checkDistance = 2.0f;        // この距離内なら「上付近」
    public float upwardForce = 5.0f;          // 上方向に加える力
    public LayerMask playerLayer;

    private GameObject player;
    private Player_Move pc;
    Rigidbody2D rb;

    public float bounceForce = 100f;

    void Start()
    {
        // プレイヤーを一度だけ取得（タグやレイヤーで見つける）
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

        // プレイヤーが自分の上付近にいるか

        if (IsPlayerInLineRange() )
        {
            if (rb != null)
            {
                rb.AddForce(Vector2.up * upwardForce, ForceMode2D.Impulse);
                Debug.Log("jんぷきょうか");

            }
        }
    }





        private void OnCollisionEnter2D(Collision2D collision)
        {
            Rigidbody2D rb = collision.rigidbody;
            Debug.Log("huretawane");
            if (rb != null)
            {
                // 接触点の法線を取得
                Vector2 normal = collision.GetContact(0).normal;

                // 法線方向に力を加える
                rb.AddForce(-normal * bounceForce, ForceMode2D.Impulse);
                Debug.Log("huretawane");
            }
        }
    




    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * checkDistance);
    }
}
