using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteHole2D : MonoBehaviour
{
    public float repelForce = 10f; // 反発力の基本強度
    public float radius = 5f;      // 反発範囲（中心からこの距離内にあるものに作用）

    //コンストラクタ
    public WhiteHole2D(WhiteHole2D w_hole)
    {
        repelForce = w_hole.repelForce;
        radius = w_hole.radius;
    }

    void FixedUpdate()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D col in colliders)
        {
            Rigidbody2D rb = col.attachedRigidbody;

            if (rb != null && rb.gameObject != this.gameObject)
            {
                float distance = Vector2.Distance(transform.position, rb.transform.position);
                if (distance == 0) continue; // 完全一致時は方向が定まらないのでスキップ

                Vector2 direction = (rb.transform.position - transform.position).normalized;

                // 0に近づくほど強くなる（反比例）
                float force = repelForce / Mathf.Max(distance, 0.1f);

                rb.AddForce(direction * force * Time.fixedDeltaTime, ForceMode2D.Force);
            }
        }
    }

    // 範囲をGizmosで可視化
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
