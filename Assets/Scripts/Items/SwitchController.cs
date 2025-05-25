using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchController : MonoBehaviour
{
    public enum SwitchMode
    {
        Fixed,
        Push,
        Toggle,
        Timer,
    }
    
    public Sprite stateOff;
    public Sprite stateOn;
    public SwitchMode mode;
    public float waitTime = 1.0f;

    private SpriteRenderer sr;
    private bool nowPressState = false;     //今のスイッチの状態
    private int hitState = -1;        //collider enter=0, stay=1, exit=2,押されてないとき-1


    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        //enumは定数の列挙型なのでintキャストで数字にできる
        switch ((int)mode)
        {
            case 0:
                FixedSwitch();
                break;
            case 1:
                PushSwitch();
                break;
            case 2:
                ToggleSwitch();
                break;
            case 3:
                TimerSwitch();
                break;
            default: break;
        }

        if (!nowPressState)
        {
            sr.sprite = stateOff;
        }
        else
        {
            sr.sprite = stateOn;
        }

        if (hitState == 2) hitState = -1;
    }

    //一度おしたらずっとONになる
    private void FixedSwitch()
    {
        if(hitState == 0&& !nowPressState)nowPressState = true;
    }

    //押している間だけONになる
    private void PushSwitch()
    {
        if(hitState == 1)nowPressState=true;
        else nowPressState=false;
    }

    //押すたびにOnとOffが切り替わる
    private void ToggleSwitch()
    {
        if (hitState == 0)
        {
            nowPressState = !nowPressState;
        }
    }

    //押したら時間差でOffになる
    private void TimerSwitch()
    {
        if(hitState == 0)
        {
            nowPressState = true;
            Invoke(nameof(TurnOffSwitch), waitTime);
        }
    }

    void TurnOffSwitch()
    {
        nowPressState = false;
    }


    public bool GetState()
    {
        return nowPressState;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        hitState = 0;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        hitState = 1;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        hitState = 2;
    }
}
