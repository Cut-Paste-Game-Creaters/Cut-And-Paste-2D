using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Blackhole : MonoBehaviour
{
    public float gravityForce = 10f; // ���͂̋���
    float force = 1f;
    float distance = 1f;
    public float radius = 5f;        // ���͂��͂��͈�

    public float trueDuration = 5.0f;   // true�̎���
    public float falseDuration = 2.0f;  // false�̎���
    private bool currentValue = false;
    private float timer = 0f;

    public float rotationSpeed = 0.5f;

    void FixedUpdate()
    {

        timer += Time.deltaTime;

        if (currentValue && timer >= trueDuration)
        {
            currentValue = false;
            timer = 0f;
            Debug.Log("Switched to FALSE");
        }
        else if (!currentValue && timer >= falseDuration)
        {
            currentValue = true;
            timer = 0f;
            Debug.Log("Switched to TRUE");
        }

        if (currentValue)
        {
            transform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);


            // �w��͈͓��̃R���C�_�[�����ׂĎ擾
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);

            foreach (Collider2D col in colliders)
            {
                Rigidbody2D rb = col.attachedRigidbody;

                // �������g�͖����ARigidbody���Ȃ����̂�����
                if (rb != null && rb.gameObject != this.gameObject)
                {
                    distance = Vector2.Distance(transform.position, rb.transform.position);
                    Vector2 direction = (transform.position - rb.transform.position).normalized;
                    force = (radius - distance) / radius;///Mathf.Max(distance, 0.5f); //�ꎟ�֐��I
                    //force = 1 / Mathf.Max(distance, 0.1f);// �߂Â��قǋ��� �񎟊֐��I�@distance�̍ŏ��l��0.5f�ɐݒ�
                    rb.AddForce(direction * (force) * gravityForce * Time.fixedDeltaTime, ForceMode2D.Force);
                }
            }



        }
    }

    // Gizmos�Ŕ͈͉���
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
