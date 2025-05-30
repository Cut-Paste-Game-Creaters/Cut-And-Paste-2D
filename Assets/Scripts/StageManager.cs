using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    public TileData tileData = new TileData(0, 0);
    public List<ObjectData> objectData = new List<ObjectData>();

    /*�R�X�g�֘A*/
    [SerializeField]private float costHeal_timeOut; //cost���񕜂���Ԋu
	private float timeElapsed;
    private int overwrite_sum_cos = 0; //kyosu
    private int[] init_ene_array = {100, 150, 200, 250, 300, 350, 400, 450, 500, 550}; //�X�e�[�W���Ƃ̏����R�X�g�z��
    private int[] healAmount_array = {5, 10, 15, 20, 25, 30, 35, 40, 45, 50}; //�X�e�[�W���Ƃ̉񕜑��x�R�X�g�̔z��
    private int stage = 0; //0=�X�e�[�W1
    public int have_ene = 10000; //�����R�X�g
    private int all_sum_cos = 0; //�X�e�[�W�ŏ�����S�ẴR�X�g
    public int erase_cost = 0; //�\��t���ӏ��̏����R�X�g
    public int write_cost = 0; //�擾�ӏ��̑��₷�R�X�g
    public int cut_erase_cost = 0; //�J�b�g�̎��݂̂̎擾�ӏ��̏����R�X�g
    public bool all_isCut = false; //copy��cut���𔻕ʂ���ϐ�

    /////////////////////////////////////////////////
    public struct TileData
    {
        //�����_��w : 32, h : 18
        public int width;
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


}
