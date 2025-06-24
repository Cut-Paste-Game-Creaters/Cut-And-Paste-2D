using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBarController : MonoBehaviour
{
    public float rotateTime = 3.0f;
    float elapsed = 0.0f;
    public int dir = -1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        elapsed += PlayerInput.GetDeltaTime();
        this.transform.localEulerAngles = new Vector3(0.0f, 0.0f, dir * 360.0f * (elapsed / rotateTime));
    }
}
