using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Blackhole : MonoBehaviour
{
    public float gravityForce = 10f; // 引力の強さ
    float force = 1f;
    float distance = 1f;
    public float radius = 5f;        // 引力が届く範囲

    public float trueDuration = 5.0f;   // trueの時間
    public float falseDuration = 2.0f;  // falseの時間
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


            // 指定範囲内のコライダーをすべて取得
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);

            foreach (Collider2D col in colliders)
            {
                Rigidbody2D rb = col.attachedRigidbody;

                // 自分自身は無視、Rigidbodyがないものも無視
                if (rb != null && rb.gameObject != this.gameObject)
                {
                    distance = Vector2.Distance(transform.position, rb.transform.position);
                    Vector2 direction = (transform.position - rb.transform.position).normalized;
                    force = (radius - distance) / radius;///Mathf.Max(distance, 0.5f); //一次関数的
                    //force = 1 / Mathf.Max(distance, 0.1f);// 近づくほど強い 二次関数的　distanceの最小値を0.5fに設定
                    rb.AddForce(direction * (force) * gravityForce * Time.fixedDeltaTime, ForceMode2D.Force);
                }
            }



        }
    }

    // Gizmosで範囲可視化
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
