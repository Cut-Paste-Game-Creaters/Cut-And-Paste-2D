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
    private StageManager stageManager;
    public bool nowPressState = false;     //���̃X�C�b�`�̏��
    public int hitState = -1;        //collider enter=0, stay=1, exit=2,������ĂȂ��Ƃ�-1
    private AnimationManager animManager = null;

    public class CopySwitchController
    {
        public Sprite stateOff;
        public Sprite stateOn;
        public SwitchMode mode;
        public bool nowPressState;     //���̃X�C�b�`�̏��
        public int hitState;

        public CopySwitchController(SwitchController swc)
        {
            stateOff = swc.stateOff;
            stateOn = swc.stateOn;
            mode = swc.mode;
            nowPressState = swc.nowPressState;
            hitState = swc.hitState;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        stageManager = FindObjectOfType<StageManager>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (stageManager == null) stageManager = FindObjectOfType<StageManager>();
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

        //if (!stageManager.switch_state)
        //{
        //    sr.sprite = stateOff;
        //}
        //else
        //{
        //    sr.sprite = stateOn;
        //}
        if(nowPressState != stageManager.switch_state)
        {
            if (animManager == null) animManager = FindObjectOfType<AnimationManager>();
            if (stageManager.switch_state)
            {
                animManager.Play("switch_on", sr);
            }
            else
            {
                animManager.Play("switch_off", sr);
            }
            nowPressState = stageManager.switch_state;
        }

        if (hitState == 2) hitState = -1;
    }

    //��x�������炸����ON�ɂȂ�
    private void FixedSwitch()
    {
        if(hitState == 0&& !stageManager.switch_state) stageManager.switch_state = true;
    }

    //�����Ă���Ԃ���ON�ɂȂ�
    private void PushSwitch()
    {
        if(hitState == 1) stageManager.switch_state = true;
        else stageManager.switch_state = false;
    }

    //�������т�On��Off���؂�ւ��
    private void ToggleSwitch()
    {
        if (hitState == 0)
        {
            stageManager.switch_state = !stageManager.switch_state;
        }
    }

    //�������玞�ԍ���Off�ɂȂ�
    private void TimerSwitch()
    {
        if(hitState == 0)
        {
            stageManager.switch_state = true;
            Invoke(nameof(TurnOffSwitch), waitTime);
        }
    }

    void TurnOffSwitch()
    {
        stageManager.switch_state = false;
    }


    public bool GetState()
    {
        return stageManager.switch_state;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        hitState = 0;
        SEManager.instance.ClipAtPointSE(SEManager.instance.SwitchSE);
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
