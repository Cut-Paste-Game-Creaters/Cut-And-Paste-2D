using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateController : MonoBehaviour
{
    public float rotateTime = 3.0f;
    public int dir = -1;

    private SpriteRenderer sr;

    public class CopyRotateController
    {
        public float rotateTime;
        public int dir;

        public CopyRotateController(RotateController rc)
        {
            rotateTime = rc.rotateTime;
            dir = rc.dir;
        }
    }

    IEnumerator Start()
    {
        // SpriteRenderer ‚ğæ‚Éæ“¾
        sr = GetComponent<SpriteRenderer>();

        // StageManager.Instance ‚ª null ‚ÌŠÔ‚Í‘Ò‹@
        while (StageManager.Instance == null)
        {
            yield return null;
        }
    }

    void Update()
    {
        if (StageManager.Instance != null && StageManager.Instance.switch_state)
        {
            float delta = 360.0f * dir * (PlayerInput.GetDeltaTime() / rotateTime);
            transform.Rotate(0.0f, 0.0f, delta);
        }
    }
}
