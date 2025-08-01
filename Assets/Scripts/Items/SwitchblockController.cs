using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchblockController : MonoBehaviour
{

    public Sprite stateOff;
    public Sprite stateOn;
    private SpriteRenderer sr;
    private StageManager stageManager;

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

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        stageManager = FindObjectOfType<StageManager>();
        prevState = stageManager.switch_state; // ‰Šúó‘Ô‚ğ‹L˜^
        ApplyState(prevState); // ‰Šúó‘Ô‚ğ”½‰f
    }

    // Update is called once per frame
    void Update()
    {
        bool currentState = stageManager.switch_state;

        // ó‘Ô‚ª•Ï‚í‚Á‚½‚Æ‚«‚¾‚¯ˆ—
        if (currentState != prevState)
        {
            ApplyState(currentState);
            prevState = currentState;
        }
    }

    void ApplyState(bool isVisible)
    {
 
        col.enabled = isVisible;

        if (isVisible)
        {
            sr.sprite = stateOn;
        }
        else if(!isVisible)
        {
            sr.sprite = stateOff;
        }
  
 
    }
}
