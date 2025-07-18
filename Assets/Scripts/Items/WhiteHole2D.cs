using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteHole2D : MonoBehaviour
{
    public float repelForce = 10f; // �����͂̊�{���x
    public float radius = 5f;      // �����͈́i���S���炱�̋������ɂ�����̂ɍ�p�j

    //�R���X�g���N�^
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
                if (distance == 0) continue; // ���S��v���͕�������܂�Ȃ��̂ŃX�L�b�v

                Vector2 direction = (rb.transform.position - transform.position).normalized;

                // 0�ɋ߂Â��قǋ����Ȃ�i�����j
                float force = repelForce / Mathf.Max(distance, 0.1f);

                rb.AddForce(direction * force * Time.fixedDeltaTime, ForceMode2D.Force);
            }
        }
    }

    // �͈͂�Gizmos�ŉ���
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
