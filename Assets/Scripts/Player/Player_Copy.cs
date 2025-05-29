using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using UnityEngine.Tilemaps;
using System.Linq;

public class Player_Copy : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] GameObject frame2;
    [SerializeField] GameObject anounce;
    [SerializeField] StageManager stageMgr;
    [SerializeField] TileScriptableObject tileSB; //ScriptableObject

    private Vector3 startPos = Vector3.zero;
    private Vector3 endPos = Vector3.zero;
    private bool isDrawing = false;
    private SpriteRenderer frameSR;
    private int whichMode = -1;     //0:Copy, 1:Cut
    private bool makeDecision = false;  //�}�E�X��������On
    //public bool all_isCut = false; //�R�s�[�֐��̈�����isCut�Ƌ�ʂ��邽��

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
        if (!makeDecision)
        {
            //�}�E�X���E�N���b�N���āA�ړ����ė���
            //�ŏ��ƍŌ�̍��W�����Ƃ�Δ͈͂��w��ł���
            if (PlayerInput.GetMouseButtonDown(0))
            {
                //�͈͑I�������Ԃ��~����(�܂��̓X���[���[)
                Time.timeScale = 0.1f;

                //����܂ŃR�s�[���Ă����̂�������
                InitList(stageMgr.tileData.tiles);
                stageMgr.tileData.tiles = new List<List<TileBase>>();
                stageMgr.tileData.hasData = false;
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


            if (PlayerInput.GetMouseButtonUp(0) && !stageMgr.tileData.hasData)
            {
                makeDecision = true;
                whichMode = -1;
                endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                return;
            }
        }

        //�ǂ��炩��I�ԃt�F�[�Y
        if (makeDecision)
        {
            switch (whichMode)
            {
                case -1:    //�R�s�[���J�b�g���I��
                    anounce.SetActive(true);
                    if (PlayerInput.GetMouseButtonUp(0)) whichMode = 0;//copy
                    else if (PlayerInput.GetMouseButtonUp(1)) whichMode = 1;//cut
                    else if (PlayerInput.GetKeyDown(KeyCode.Escape)) whichMode = 2;//nothing
                    break;
                case 0:     //�R�s�[����Ȃ�
                    CopyContents(startPos, endPos);
                    CopyObject(startPos, endPos);
                    //all_isCut = false;
                    stageMgr.all_isCut = false;
                    InitWhichMode();
                    break;
                case 1:     //�J�b�g����Ȃ�
                    CopyContents(startPos, endPos, true);
                    CopyObject(startPos, endPos, true);
                    //all_isCut = true;
                    stageMgr.all_isCut = true;
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
        //���₷�R�X�g�������� �R�s�[�̎��̓R�s�[����邽�тɏ������@�t�ɂ���ȊO�͍X�V����Ă͂����Ȃ�
        stageMgr.write_cost = 0;

        //�J�b�g�̎��̂ݎg���ϐ�
        int cut_erase_cost = 0;

        //�ʒu��int�ɂ���
        Vector3Int _startPos = ChangeVecToInt(sPos);
        Vector3Int _endPos = ChangeVecToInt(ePos);
        //���������v�Z
        int width = Mathf.Abs((_endPos.x - _startPos.x)) + 1;
        int height = Mathf.Abs((_endPos.y - _startPos.y)) + 1;
        Debug.Log("w:" + width + " h:" + height);

        //tileData�ɃR�s�[�������̂�ۑ�
        stageMgr.tileData.width = width;
        stageMgr.tileData.height = height;


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

        stageMgr.tileData.direction = direction;

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
                if (tilemap.HasTile(p)) //kyosu �������̃Z�����^�C���������Ă���Ȃ�
                {
                    stageMgr.write_cost += tileSB.tileDataList.Single(t => t.tile == tilemap.GetTile(p)).p_ene; // �擾�����^�C�����^�C���p���b�g�̂ǂ̃^�C�����𔻕ʂ��Ă��̏���R�X�g���{
                    if (isCut)
                    {
                        cut_erase_cost += tileSB.tileDataList.Single(t => t.tile == tilemap.GetTile(p)).ow_ene;
                        tilemap.SetTile(p, null);
                    }
                }
                tBases.Add(t);
                
                /*if (t != null) Debug.Log(t.name);
                else Debug.Log("null");*/
            }
            stageMgr.tileData.tiles.Add(tBases);
        }
        if(isCut)
        {
            if(stageMgr.have_ene >= cut_erase_cost) //�����R�X�g���������Ȃ�
            {
                stageMgr.have_ene -= cut_erase_cost; //�R�X�g����
            }
            Debug.Log("�����R�X�g(�J�b�g��):" + cut_erase_cost + ", " + "�����G�i�W�[:" + stageMgr.have_ene);
        }
    }

    //�I��͈͂̃I�u�W�F�N�g���R�s�[����֐�
    public void CopyObject(Vector3 startPos, Vector3 endPos, bool isCut = false)
    {
        Collider2D[] cols = Physics2D.OverlapAreaAll(startPos, endPos);
        stageMgr.objectData = new List<StageManager.ObjectData>();
        foreach (var col in cols)
        {
            if (col.gameObject.tag != "Tilemap"
                && col.gameObject.tag != "Player"
                && col.gameObject.tag != "Uncuttable")
            {
                Debug.Log(col.gameObject.name);
                StageManager.ObjectData c = new StageManager.ObjectData();
                if (!isCut)
                {
                    c.obj = Instantiate(col.gameObject);
                }
                else
                {
                    c.obj = col.gameObject;
                }
                c.pos = col.gameObject.transform.position - ChangeVecToInt(startPos);
                c.obj.SetActive(false);
                stageMgr.objectData.Add(c);
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
        stageMgr.tileData.hasData = true;
        makeDecision = false;
        anounce.SetActive(false);
    }

    //�R�s�[���Ă������̏�����
    void InitList(List<List<TileBase>> list)
    {
        list = new List<List<TileBase>>();
        stageMgr.tileData.hasData = false;
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
