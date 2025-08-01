using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    public TileData tileData = new TileData(0, 0);
    public List<ObjectData> objectData = new List<ObjectData>();

    /*�v���C���[�֘A*/
    Player_Function playerFunc;
    RankJudgeAndUpdateFunction rankFunc;
    public int player_HP = 100;
    public int player_MAXHP = 100;
    public bool isPlayerDamaged = false;
    private float noDamageTime = 1.0f;
    private float nowNoDanageTime = 0.0f;

    /*�X�C�b�`�֘A*/
    public bool switch_state = false;

    /*���֘A*/
    public bool key_lock_state = false;

    /*�X�C�b�`�ƌ��̏������*/
    //[HideInInspector]
    public Pair<bool, bool>[] switch_key_states;

    /*�R�X�g�֘A*/
    [SerializeField]private float costHeal_timeOut; //cost���񕜂���Ԋu
	private float timeElapsed;
    public int stageNum = -1;
    private int[] init_ene_array = {100000, 150, 200, 250, 300, 350, 400, 450, 500, 550}; //�X�e�[�W���Ƃ̏����R�X�g�z��
    private int[] healAmount_array = {5, 10, 15, 20, 25, 30, 35, 40, 45, 50}; //�X�e�[�W���Ƃ̉񕜑��x�R�X�g�̔z��
    public int[] initAddCost_EachStage = {0,0,0,0,0,0,0,0,0,0};
    public int have_ene = 10000; //�����R�X�g
    public int all_sum_cos = 0; //�X�e�[�W�ŏ�����S�ẴR�X�g
    public int erase_cost = 0; //�\��t���ӏ��̏����R�X�g
    public int write_cost = 0; //�擾�ӏ��̑��₷�R�X�g
    public int cut_erase_cost = 0; //�J�b�g�̎��݂̂̎擾�ӏ��̏����R�X�g
    public bool all_isCut = false; //copy��cut���𔻕ʂ���ϐ�

    //�I�u�W�F�N�g��ʃV�[���Ɏ����Ă��֘A
    [HideInInspector]
    public List<GameObject> EraseObjects = new List<GameObject>();

    void Start()
    {
        playerFunc = FindObjectOfType<Player_Function>();
        rankFunc = FindObjectOfType<RankJudgeAndUpdateFunction>();

        //�e�X�e�[�W�ł̃X�C�b�`�ƌ��̏������
        //Pair.bool1 = switchState, Pair.bool2 = keyState
        switch_key_states = new Pair<bool, bool>[]
        {
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(true, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),
        };
    }
    
    void Update()
    {
        if(stageNum != -1)
        {
            HealCost(stageNum);
        }

        if(isPlayerDamaged)
        {
            nowNoDanageTime += PlayerInput.GetDeltaTime();
            if(nowNoDanageTime > noDamageTime)
            {
                isPlayerDamaged = false;
            }
        }
    }

    [System.Serializable] // Unity�G�f�B�^�ŃV���A���C�Y�\�ɂ���
    public class Pair<T1, T2>
    {
        public T1 first;
        public T2 second;

        public Pair(T1 first, T2 second)
        {
            this.first = first;
            this.second = second;
        }
    }

    /////////////////////////////////////////////////
    public struct TileData
    {
        //�����_��w : 32, h : 18
        public int width;       //���A�����̓}�X�̌�
        public int height;
        public int direction;   //0�E��A1�E���A2�����A3����
        public List<List<TileBase>> tiles;
        public bool hasData;
        public bool isCut;
        public TileData(int w, int h)
        {
            width = w;
            height = h;
            tiles = new List<List<TileBase>>();
            hasData = false;
            direction = -1;
            isCut = false;
        }
    }

    public struct ObjectData
    {
        //�J�b�g����I�u�W�F�N�g�̖{��
        public GameObject obj;
        //�J�b�g����I�u�W�F�N�g�̑��Έʒu
        public Vector3 pos;
    }

    public Dictionary<string, int> stageNumber = new Dictionary<string, int>() // Dictionary�N���X�̐錾�Ə����l�̐ݒ�
    {
        {"Stage1", 0},
        {"Stage2", 1},
        {"Stage3", 2},
        {"Stage4", 3},
        {"Stage5", 4},
        {"Stage6", 5},
        {"Stage7", 6},
        {"Stage8", 7},
        {"Stage9", 8},
        {"Stage10", 9},
        {"StageTemplate",10 }
    };

    public void DamageToPlayer(int damage) //������HP���猸�炷����
    {
        if (!isPlayerDamaged)
        {
            player_HP -= damage;
            isPlayerDamaged = true;
            nowNoDanageTime = 0.0f;
        }

    }

    public void InitAllSumCost()
    {
        all_sum_cos = 0;
        Debug.Log("������R�X�g�����������܂���.");
    }

    public void InitHaveCost(int stageNum)
    {
        have_ene = init_ene_array[stageNum];
        if (stageNum == 0)
        {
            foreach(var cost in initAddCost_EachStage)
            {
                have_ene += cost;
            }
        }
            Debug.Log("�����R�X�g��" + stageNum + "�X�e�[�W��" + have_ene + "�ɏ��������܂���.");
    }

    public void StageSelectInitHaveCost()
    {
        //�e�X�e�[�W�̃����N�ɉ����Đݒ�
    }

    public void HealCost(int stageNum)
    {
          /*��莞�ԁicostHeal_timeOut�j���Ƃɏ����R�X�g����*/
        timeElapsed += Time.deltaTime;
        if(timeElapsed >= costHeal_timeOut)
        {
            have_ene += healAmount_array[stageNum];
            if(have_ene > init_ene_array[stageNum]) //�񕜃R�X�g����𒴂��ĉ񕜂��悤�Ƃ���ꍇ�͉񕜃R�X�g����ŏ���������
            {
                have_ene = init_ene_array[stageNum];
            }
            //Debug.Log("�����R�X�g:" + have_ene);
            timeElapsed = 0.0f;
        }
    }

    public Vector3 GetInfo()
    {
        Vector3 v = Vector3.zero;
        if (tileData.hasData)
        {
            v.x = tileData.width;
            v.y = tileData.height;
            v.z = tileData.direction;
        }

        return v;
    }

    public void ResetObjectState()
    {
        string input = SceneManager.GetActiveScene().name;
        if (Regex.IsMatch(input, @"^Stage\d+$")) //�V�[������Stage�Ȃ�Ƃ��Ȃ�
        {
            int stage_num = stageNumber[SceneManager.GetActiveScene().name];
            switch_state = switch_key_states[stage_num].first;
            key_lock_state = switch_key_states[stage_num].second;
        }
        else
        {
            switch_state = false;
            key_lock_state = false;
        }
    }

    //////////////////////////////////////////////////////////
    void Awake()
    {
        // ���ɃC���X�^���X�����݂���Ȃ�j��
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }


        // ���̃C���X�^���X��ێ����ăV�[���J�ڂł��j������Ȃ��悤�ɂ���
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SaveObjects()
    {

    }
}
