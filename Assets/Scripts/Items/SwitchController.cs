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
    private bool nowPressState = false;     //���̃X�C�b�`�̏��
    private int hitState = -1;        //collider enter=0, stay=1, exit=2,������ĂȂ��Ƃ�-1


    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        //enum�͒萔�̗񋓌^�Ȃ̂�int�L���X�g�Ő����ɂł���
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

    //��x�������炸����ON�ɂȂ�
    private void FixedSwitch()
    {
        if(hitState == 0&& !nowPressState)nowPressState = true;
    }

    //�����Ă���Ԃ���ON�ɂȂ�
    private void PushSwitch()
    {
        if(hitState == 1)nowPressState=true;
        else nowPressState=false;
    }

    //�������т�On��Off���؂�ւ��
    private void ToggleSwitch()
    {
        if (hitState == 0)
        {
            nowPressState = !nowPressState;
        }
    }

    //�������玞�ԍ���Off�ɂȂ�
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
