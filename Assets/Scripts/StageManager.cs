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
