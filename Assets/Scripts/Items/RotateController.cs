using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateController : MonoBehaviour
{
    public float rotateTime = 3.0f;
    public int dir = -1;

    private SpriteRenderer sr;
    private StageManager stageManager;

    void Start()
    {
        stageManager = FindObjectOfType<StageManager>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (stageManager.switch_state)
        {
            float delta = 360.0f * dir * (PlayerInput.GetDeltaTime() / rotateTime);
            transform.Rotate(0.0f, 0.0f, delta);
        }
    }
}

