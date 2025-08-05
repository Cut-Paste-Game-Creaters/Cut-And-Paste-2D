using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchblockController : MonoBehaviour
{
    public Sprite stateOff;
    public Sprite stateOn;

    private SpriteRenderer sr;
    private Collider2D col;
    private bool prevState;

    public class CopySwitchblockController
    {
        public Sprite stateOff;
        public Sprite stateOn;

        public CopySwitchblockController(SwitchblockController sbc)
        {
            stateOff = sbc.stateOff;
            stateOn = sbc.stateOn;
        }
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        if (StageManager.Instance != null)
        {
            prevState = StageManager.Instance.switch_state;
            ApplyState(prevState);
        }
        else
        {
            Debug.LogWarning("StageManager.Instance is null in SwitchblockController.");
        }
    }

    void Update()
    {
        if (StageManager.Instance == null) return;

        bool currentState = StageManager.Instance.switch_state;

        if (currentState != prevState)
        {
            ApplyState(currentState);
            prevState = currentState;
        }
    }

    void ApplyState(bool isVisible)
    {
        col.enabled = isVisible;

        sr.sprite = isVisible ? stateOn : stateOff;
    }
}
