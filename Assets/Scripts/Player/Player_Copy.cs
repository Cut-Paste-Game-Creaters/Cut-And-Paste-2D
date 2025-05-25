using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using UnityEngine.Tilemaps;

public class Player_Copy : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] GameObject frame2;
    [SerializeField] GameObject anounce;

    private Vector3 startPos = Vector3.zero;
    private Vector3 endPos = Vector3.zero;
    private bool isDrawing = false;
    private SpriteRenderer frameSR;
    private int whichMode = -1;     //0:Copy, 1:Cut
    private bool makeDecision = false;  //�}�E�X��������On

    //�R�s�[����^�C���̃f�[�^
    private struct TileData
    {
        //�����_��w : 32, h : 18
        public int width;
        public int height;
        public int direction;   //0�E��A1�E���A2�����A3����
        public List<List<TileBase>> tiles;
        public bool hasData;
        public TileData(int w,int h)
        {
            width = w;
            height = h;
            tiles = new List<List<TileBase>>();
            hasData = false;
            direction = -1;
        }
    }
    private struct ObjectData
    {
        //�J�b�g����I�u�W�F�N�g�̖{��
        public GameObject obj;
        //�J�b�g����I�u�W�F�N�g�̑��Έʒu
        public Vector3 pos;
    }

    private TileData tileData = new TileData(0,0);
    private List<ObjectData> objects = new List<ObjectData>();


    // Start is called before the first frame update
    void Start()
    {
        frame2 = Instantiate(frame2);
        frame2.SetActive(false);
        anounce = Instantiate(anounce);
        anounce.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        CopyTiles();
    }

    //�R�s�[����͈͂����肷��
    void CopyTiles()
    {
        //�}�E�X���E�N���b�N���āA�ړ����ė���
        //�ŏ��ƍŌ�̍��W�����Ƃ�Δ͈͂��w��ł���
        if (PlayerInput.GetMouseButtonDown(0) && !makeDecision)
        {
            //�͈͑I�������Ԃ��~����(�܂��̓X���[���[)
            Time.timeScale = 0.1f;

            //����܂ŃR�s�[���Ă����̂�������
            InitList(tileData.tiles);
            tileData.tiles = new List<List<TileBase>>();
            tileData.hasData = false;
            //�ŏ��̈ʒu�擾(�����_)
            startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);


            //�l�p��`��
            frame2.SetActive(true);
            if (frameSR == null)
            {
                frameSR = frame2.GetComponent<SpriteRenderer>();
            }
            frame2.transform.localPosition = startPos;
            frame2.transform.localScale = Vector3.one;
            isDrawing = true;
        }

        if (isDrawing && PlayerInput.GetMouseButton(0))
        {
            //�l�p�̃T�C�Y��ς���
            Vector3 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 size = currentPos - startPos;
            frameSR.size = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
            Vector3 nowpos = (currentPos + startPos) / 2;
            nowpos.z = 0;
            frame2.transform.localPosition = nowpos;
        }


        if (PlayerInput.GetMouseButtonUp(0) && !tileData.hasData)
        {
            makeDecision = true;
            whichMode = -1;
            endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        //�ǂ��炩��I�ԃt�F�[�Y
        if (makeDecision)
        {
            switch (whichMode)
            {
                case -1:    //�R�s�[���J�b�g���I��
                    anounce.SetActive(true);
                    if (PlayerInput.GetMouseButtonDown(0)) whichMode = 0;//copy
                    else if (PlayerInput.GetMouseButtonDown(1)) whichMode = 1;//cut
                    else if (PlayerInput.GetKeyDown(KeyCode.Escape)) whichMode = 2;//nothing
                    break;
                case 0:     //�R�s�[����Ȃ�
                    CopyContents(startPos, endPos);
                    CopyObject(startPos, endPos);
                    InitWhichMode();
                    break;
                case 1:     //�J�b�g����Ȃ�
                    CopyContents(startPos, endPos, true);
                    CopyObject(startPos, endPos, true);
                    InitWhichMode();
                    break;
                default:
                    InitWhichMode();
                    break;
            }
        }
    }

    //���ۂɃ^�C�����R�s�[������֐�
    void CopyContents(Vector3 sPos, Vector3 ePos, bool isCut = false)
    {
        //�ʒu��int�ɂ���
        Vector3Int _startPos = ChangeVecToInt(sPos);
        Vector3Int _endPos = ChangeVecToInt(ePos);
        //���������v�Z
        int width = Mathf.Abs((_endPos.x - _startPos.x)) + 1;
        int height = Mathf.Abs((_endPos.y - _startPos.y)) + 1;
        Debug.Log("w:" + width + " h:" + height);

        //tileData�ɃR�s�[�������̂�ۑ�
        tileData.width = width;
        tileData.height = height;


        //�R�s�[�̌������擾����
        int direction = 0;
        if (endPos.x - startPos.x >= 0)//�E����
        {
            if (endPos.y - startPos.y >= 0) //�����
            {
                direction = 0;
            }
            else
            {
                direction = 1;
            }
        }
        else  //������
        {
            if (endPos.y - startPos.y >= 0) //�����
            {
                direction = 3;
            }
            else
            {
                direction = 2;
            }
        }

        tileData.direction = direction;

        //�R�s�[�͈͂�tile���R�s�[
        for (int y = 0; y < height; y++)
        {
            //1�񂲂Ƃ�List
            List<TileBase> tBases = new List<TileBase>();
            for (int x = 0; x < width; x++)
            {
                Vector3Int p = Vector3Int.zero;
                //�����ɂ���Ď擾����
                switch (direction)
                {
                    case 0:
                        p = new Vector3Int(
                        _startPos.x + x,
                        _startPos.y + y, 0);
                        break;
                    case 1:
                        p = new Vector3Int(
                        _startPos.x + x,
                        _startPos.y - y, 0);
                        break;
                    case 2:
                        p = new Vector3Int(
                        _startPos.x - x,
                        _startPos.y - y, 0);
                        break;
                    case 3:
                        p = new Vector3Int(
                        _startPos.x - x,
                        _startPos.y + y, 0);
                        break;
                    default: break;
                }

                TileBase t = tilemap.GetTile(p);
                tBases.Add(t);
                if (isCut)
                {
                    tilemap.SetTile(p, null);
                }
                /*if (t != null) Debug.Log(t.name);
                else Debug.Log("null");*/
            }
            tileData.tiles.Add(tBases);
        }

    }

    //�I��͈͂̃I�u�W�F�N�g���R�s�[����֐�
    public void CopyObject(Vector3 startPos, Vector3 endPos, bool isCut = false)
    {
        Collider2D[] cols = Physics2D.OverlapAreaAll(startPos, endPos);
        objects = new List<ObjectData>();
        foreach (var col in cols)
        {
            if (col.gameObject.tag != "Tilemap"
                && col.gameObject.tag != "Player"
                && col.gameObject.tag != "Uncuttable")
            {
                Debug.Log(col.gameObject.name);
                ObjectData c = new ObjectData();
                if (!isCut)
                {
                    c.obj = Instantiate(col.gameObject);
                }
                else
                {
                    c.obj = col.gameObject;
                    Debug.Log("Cut!!!!");
                }
                c.pos = col.gameObject.transform.position - ChangeVecToInt(startPos);
                c.obj.SetActive(false);
                objects.Add(c);
            }

        }
    }

    //�R�s�[���J�b�g���̑I�����I�������̋��ʂ̏���
    void InitWhichMode()
    {
        //��~����
        Time.timeScale = 1f;
        //�l�p������
        isDrawing = false;
        frame2.SetActive(false);

        //�f�[�^�ێ��t���Oon!
        tileData.hasData = true;
        makeDecision = false;
        anounce.SetActive(false);
    }

    //�R�s�[���Ă������̏�����
    void InitList(List<List<TileBase>> list)
    {
        list = new List<List<TileBase>>();
        tileData.hasData = false;
    }

    //�}�E�X�̍��W���^�C���̍��W�ɕϊ�����֐�
    public Vector3Int ChangeVecToInt(Vector3 v)
    {
        Vector3Int pos = Vector3Int.zero;
        pos.x = (int)Mathf.Floor(v.x);
        pos.y = (int)Mathf.Floor(v.y);
        //pos.z = (int)Mathf.Floor(v.z);

        return pos;
    }
}
