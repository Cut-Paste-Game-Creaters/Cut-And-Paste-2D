using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SinVerticalMover : MonoBehaviour
{
    public float amplitude = 1.0f; // �㉺�̐U�ꕝ
    public float frequency = 1.0f; // 1�b�Ԃɉ���㉺���邩
    public bool cos = true;
    int coson = 0;  
    int sinon = 0;  
    public bool sin = true;
    private Vector3 startPos;

    float newY = 0.0f;
    float newX = 0.0f;  

    public class CopySinVerticalMover
    {
        public float amplitude; // �㉺�̐U�ꕝ
        public float frequency; // 1�b�Ԃɉ���㉺���邩
        public bool cos;
        public bool sin;

        public CopySinVerticalMover(SinVerticalMover svm)
        {
            amplitude = svm.amplitude;
            frequency = svm.frequency;
            cos = svm.cos;
            sin = svm.sin;
        }
    }

    void Start()
    {
        if (cos)
        {
            coson = 1;
        }
        else
        {
            coson = 0; 
        }
        if (sin)
        {
            sinon = 1;
        }
        else
        {
            sinon = 0;  
        }

    }

    void Update()
    {
    
            float newY = Mathf.Sin(Time.time * frequency * 2 * Mathf.PI) * amplitude;
            float newX = Mathf.Cos(Time.time * frequency * 2 * Mathf.PI) * amplitude;
            transform.position = new Vector3(startPos.x + coson * newX, startPos.y + sinon * newY, startPos.z);///���PlayerInput.GetDeltaTime();���g��




    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.transform.position += new Vector3(newX,newY,0) * PlayerInput.GetDeltaTime();//nannkaugokann

            Debug.Log("�������Ă�");
        }
    }


}
