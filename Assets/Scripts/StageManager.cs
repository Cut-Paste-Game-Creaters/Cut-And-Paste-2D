using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Linq;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    public TileData tileData = new TileData(0, 0);
    public List<ObjectData> objectData = new List<ObjectData>();
    public Sprite copySprite;

    /*�v���C���[�֘A*/
    Player_Function playerFunc;
    RankJudgeAndUpdateFunction rankFunc;
    public int player_HP = 100;
    public int player_MAXHP = 300;
    public bool isPlayerDamaged = false;
    private float noDamageTime = 1.0f;
    private float nowNoDanageTime = 0.0f;
    public bool isSelectZone = false;           //�v���C���[�����͈͂�I�����Ă��邩
    public bool isPasting = false;           //�v���C���[�����y�[�X�g���悤�Ƃ��Ă��邩
    public bool isPlayerDead = false;


    /*�X�C�b�`�֘A*/
    public bool switch_state = false;

    /*���֘A*/
    public bool key_lock_state = false;

    /*�X�C�b�`�ƌ��̏������*/
    //[HideInInspector]
    public Pair<bool, bool>[] switch_key_states;

    /*�R�X�g�֘A*/
    [SerializeField] private ObjectScriptableObject objSB;
    [SerializeField] private TileScriptableObject tileSB;
    [SerializeField] private float costHeal_timeOut; //cost���񕜂���Ԋu
    private float timeElapsed;
    public int stageNum = -1;
    private int[] init_ene_array = { 100000, 100000, 100000, 100000, 100000, 100000, 100000, 100000, 100000, 100000 }; //�X�e�[�W���Ƃ̏����R�X�g�z��
    private int[] costHeal_timeOut_array = { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 }; //�X�e�[�W���Ƃ̃R�X�g�񕜊Ԋu
    private int[] healAmount_array = { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50 }; //�X�e�[�W���Ƃ̉񕜑��x�R�X�g�̔z��
    public int[] initAddCost_EachStage = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public int have_ene = 10000; //�����R�X�g
    public int all_sum_cos = 0; //�X�e�[�W�ŏ�����S�ẴR�X�g
    public int erase_cost = 0; //�\��t���ӏ��̏����R�X�g
    public int write_cost = 0; //�擾�ӏ��̑��₷�R�X�g
    public int cut_erase_cost = 0; //�J�b�g�̎��݂̂̎擾�ӏ��̏����R�X�g
    public bool all_isCut = false; //copy��cut���𔻕ʂ���ϐ�

    //�I�u�W�F�N�g��ʃV�[���Ɏ����Ă��֘A
    [HideInInspector]
    public List<GameObject> EraseObjects = new List<GameObject>();

    //���̑�
    private GameUIController gameUI;
    private Tilemap tilemap = null;

    void Start()
    {
        playerFunc = FindObjectOfType<Player_Function>();
        rankFunc = FindObjectOfType<RankJudgeAndUpdateFunction>();
        gameUI = FindObjectOfType<GameUIController>();

        //�e�X�e�[�W�ł̃X�C�b�`�ƌ��̏������
        //Pair.bool1 = switchState, Pair.bool2 = keyState
        switch_key_states = new Pair<bool, bool>[]
        {
            new Pair<bool, bool>(false, false),///Stage1
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),///Stage5
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(true, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),///Stage10
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(true, false),
            new Pair<bool, bool>(false, false),///Stage15
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(true, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),
            new Pair<bool, bool>(false, false),///Stage20
        };
    }

    void Update()
    {
        if (stageNum != -1)
        {
            HealCost(stageNum);
        }

        if (gameUI == null)
        {
            gameUI = FindObjectOfType<GameUIController>();
        }

        if (isPlayerDamaged)
        {
            nowNoDanageTime += PlayerInput.GetDeltaTime();
            if (nowNoDanageTime > noDamageTime)
            {
                isPlayerDamaged = false;
            }
        }

        //�����^�C�����I�u�W�F�N�g���\�����Ȃ��Ȃ�false, �ǂ��炩��\������Ȃ�true
        bool isDisplayInformation = false;

        /*�v���C���[���͈͑I�𒆂���Ȃ��Ƃ��A���I�u�W�F�N�g�Ƀ}�E�X�J�[�\���𓖂Ă��Ƃ�
         ���ASpace���������Ƃ�*/
        if (!isSelectZone && Input.GetKey(KeyCode.Space))
        {

            // �}�E�X�̃X�N���[�����W�����[���h���W�ɕϊ�
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            /*�^�C����*/
            Vector3Int pos = Vector3Int.zero;       //�}�E�X�̂�����W�̃^�C���̍��W
            pos.x = (int)Mathf.Floor(mousePos.x);
            pos.y = (int)Mathf.Floor(mousePos.y);
            pos.z = 0;
            if (tilemap == null)
            {
                tilemap = GameObject.FindWithTag("Tilemap").GetComponent<Tilemap>();
            }
            if (tilemap != null && tilemap.HasTile(pos))
            {
                int tile_writeCost = tileSB.tileDataList.Single(t => t.tile == tilemap.GetTile(pos)).p_ene;
                int tile_overwriteCost = tileSB.tileDataList.Single(t => t.tile == tilemap.GetTile(pos)).ow_ene;
                gameUI.DisplayObjectCost(tile_writeCost, tile_overwriteCost);
                isDisplayInformation = true;
            }


            /*�I�u�W�F�N�g��*/

            /// �}�E�X�ʒu�ɂ���2D�R���C�_�[���擾
            Collider2D hitCollider = Physics2D.OverlapPoint(mousePos);

            if (hitCollider != null
                && hitCollider.gameObject.tag != "Player"
                && hitCollider.gameObject.tag != "Tilemap"
                && hitCollider.gameObject.tag != "Uncuttable"
                )
            {
                //���̃I�u�W�F�N�g�̏����R�X�g�A���₷�R�X�g��\������
                int object_writeCost = objSB.objectList.Single(t => t.obj.tag == hitCollider.gameObject.tag).p_ene;
                int object_overwriteCost = objSB.objectList.Single(t => t.obj.tag == hitCollider.gameObject.tag).ow_ene;
                gameUI.DisplayObjectCost(object_writeCost, object_overwriteCost);
                isDisplayInformation = true;
            }


        }
        //�����ǂ�����\�����Ȃ��Ȃ����
        if (!isDisplayInformation)
        {
            gameUI.UnDisplayObjectCost();
        }

        gameUI.AppearAllCostDisplay(isSelectZone); //�R�X�g�ꗗ�\�\��

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

    public struct CopyStageManager
    {
        public int player_HP;
        public int have_ene;
        public int all_sum_cos;
        public int write_cost;
        public bool all_isCut;
        public bool switch_state;
        public bool key_lock_state;

        public CopyStageManager(StageManager smgr)
        {
            player_HP = smgr.player_HP;
            have_ene = smgr.have_ene;
            all_sum_cos = smgr.all_sum_cos;
            write_cost = smgr.write_cost;
            all_isCut = smgr.all_isCut;
            switch_state = smgr.switch_state;
            key_lock_state = smgr.key_lock_state;
        }
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
        {"Stage11", 10},
        {"Stage12", 11},
        {"Stage13", 12},
        {"Stage14", 13},
        {"Stage15", 14},
        {"Stage16", 15},
        {"Stage17", 16},
        {"Stage18", 17},
        {"Stage19", 18},
        {"Stage20", 19},
        {"StageTemplate",20 }
    };

    public void DamageToPlayer(int damage) //������HP���猸�炷����
    {
        if (!isPlayerDamaged)
        {
            player_HP -= damage;
            isPlayerDamaged = true;
            nowNoDanageTime = 0.0f;
            SEManager.instance.ClipAtPointSE(SEManager.instance.damageSE);
            FindObjectOfType<CameraMove>().Shake();                  // �f�t�H���g
        }
    }

    public void InitPlayerHP()
    {
        player_HP = 100;
    }

    public void InitAllSumCost()
    {
        all_sum_cos = 0;
        Debug.Log("������R�X�g�����������܂���.");
    }

    public void InitHaveCost(int stageNum)
    {
        if (stageNum == -1) //Stage������Ȃ��Ƃ�, (StageSelect�̂Ƃ�)
        {
            have_ene = 0; //�����R�X�g0�ɂ��Ċe�X�e�[�W�̃����N�ɉ������R�X�g�𑫂�
            foreach (var cost in initAddCost_EachStage)
            {
                have_ene += cost;
                Debug.Log("�����R�X�g��" + cost + "���ǉ�����܂���");
                have_ene += 10000;
            }
        }
        else //stage���̎���, ���ꂼ��̏����R�X�g�ɐݒ�
        {
            have_ene = init_ene_array[stageNum];
        }
        Debug.Log("�����R�X�g��" + stageNum + "�X�e�[�W��" + have_ene + "�ɏ��������܂���.");
    }

    public void StageSelectInitHaveCost()
    {
        //�e�X�e�[�W�̃����N�ɉ����Đݒ�
    }

    public void InitHealTimeOut(int num)
    {
        costHeal_timeOut = costHeal_timeOut_array[num];
    }

    public void HealCost(int stageNum)
    {
        /*��莞�ԁicostHeal_timeOut�j���Ƃɏ����R�X�g����*/
        timeElapsed += PlayerInput.GetDeltaTime();
        if (timeElapsed >= costHeal_timeOut)
        {
            have_ene += healAmount_array[stageNum];
            if (have_ene > init_ene_array[stageNum]) //�񕜃R�X�g����𒴂��ĉ񕜂��悤�Ƃ���ꍇ�͉񕜃R�X�g����ŏ���������
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
            switch_state = true;
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
