using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using UnityEngine.Tilemaps;
using System.Linq;

public class Player_Copy : MonoBehaviour
{
    [SerializeField] GameObject frame2;
    [SerializeField] GameObject anounce;
    [SerializeField] TileScriptableObject tileSB; //ScriptableObject
    [SerializeField] ObjectScriptableObject objSB;

    private Tilemap tilemap;
    private StageManager stageMgr;
    private Vector3 startPos = Vector3.zero;
    private Vector3 endPos = Vector3.zero;
    private bool isDrawing = false;
    private SpriteRenderer frameSR;
    private int whichMode = -1;     //0:Copy, 1:Cut
    private bool makeDecision = false;  //�}�E�X��������On
    UndoRedoFunction urFunc;
    private CaptureCopyZone captureCopyZone;
    //public bool all_isCut = false; //�R�s�[�֐��̈�����isCut�Ƌ�ʂ��邽��

    // Start is called before the first frame update
    void Start()
    {
        frame2 = Instantiate(frame2);
        frame2.SetActive(false);
        frame2.layer = LayerMask.NameToLayer("UI");
        //anounce = Instantiate(anounce);
        //anounce.SetActive(false);
        Tilemap[] maps = FindObjectsOfType<Tilemap>();
        foreach (var map in maps)
        {
            if (map.gameObject.tag == "Tilemap")
            {
                tilemap = map;
                break;
            }
        }
        stageMgr = FindObjectOfType<StageManager>();
        urFunc = FindObjectOfType<UndoRedoFunction>();
        captureCopyZone = FindObjectOfType<CaptureCopyZone>();

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
                Time.timeScale = 0f;

                //����܂ŃR�s�[���Ă����̂�������
                InitTileData();
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
                anounce.SetActive(true);
                return;
            }
        }

        //�ǂ��炩��I�ԃt�F�[�Y
        if (makeDecision)
        {
            switch (whichMode)
            {
                case -1:    //�R�s�[���J�b�g���I��
                    if (PlayerInput.GetMouseButtonUp(0))
                    {
                        whichMode = 0;//copy
                        //�R�s�[or�J�b�g����O�ɉ摜���L���v�`������
                        captureCopyZone.CaptureImage(startPos, endPos);
                    }
                    else if (PlayerInput.GetMouseButtonUp(1))
                    {
                        whichMode = 1;//cut
                        //�R�s�[or�J�b�g����O�ɉ摜���L���v�`������
                        captureCopyZone.CaptureImage(startPos, endPos);
                    }
                    else if (PlayerInput.GetKeyDown(KeyCode.Escape)) whichMode = 2;//nothing
                    break;
                case 0:     //�R�s�[����Ȃ�
                    stageMgr.all_isCut = false;
                    CopyContents(startPos, endPos);
                    CopyObject(startPos, endPos);
                    //all_isCut = false;
                    InitWhichMode();
                    break;
                case 1:     //�J�b�g����Ȃ�
                    stageMgr.all_isCut = true;
                    CopyContents(startPos, endPos, true);
                    CopyObject(startPos, endPos, true);
                    //all_isCut = true;
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
        stageMgr.cut_erase_cost = 0;

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
        if (_endPos.x - _startPos.x >= 0)//�E����
        {
            if (_endPos.y - _startPos.y >= 0) //�����
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
            if (_endPos.y - _startPos.y >= 0) //�����
            {
                direction = 3;
            }
            else
            {
                direction = 2;
            }
        }
        stageMgr.tileData.direction = direction;


        //�^�C���̃R�X�g���v�Z����
        CutInCopy(_startPos, _endPos, true);

        //�I�u�W�F�N�g�̃R�X�g�v�Z������
        Collider2D[] cols = Physics2D.OverlapAreaAll(startPos, endPos);
        stageMgr.objectData = new List<StageManager.ObjectData>();
        CutInCopyObject(cols, true);

        //�J�b�g�ł��邩�����łȂ����ŕ�����
        if (stageMgr.all_isCut)
        {
            //�����R�X�g�������R�X�g��菬�����Ȃ�
            if(stageMgr.cut_erase_cost <= stageMgr.have_ene)
            {
                stageMgr.have_ene -= stageMgr.cut_erase_cost; //�R�X�g����
                stageMgr.all_sum_cos += stageMgr.cut_erase_cost; //������R�X�g�ɉ��Z

                Debug.Log("�����R�X�g(�J�b�g��):" + stageMgr.cut_erase_cost + ", " + "�����G�i�W�[:" + stageMgr.have_ene + ", " + "������R�X�g�F" + stageMgr.all_sum_cos);
                CutInCopy(_startPos, _endPos, false);
                CutInCopyObject(cols, false);
                urFunc.InfoPushToStack();
            }
            else
            {
                InitTileData();
                captureCopyZone.disableImage();
                Debug.Log("�J�b�g�ł��܂���I");
            }
        }
        else
        {
            CutInCopyObject(cols, false);
            Debug.Log("�����R�X�g(�J�b�g��):" + stageMgr.cut_erase_cost + ", " + "�����G�i�W�[:" + stageMgr.have_ene);
        }

    }

    //�I��͈͂̃I�u�W�F�N�g���R�s�[����֐�
    public void CopyObject(Vector3 startPos, Vector3 endPos, bool isCut = false)
    {
        
    }

    public void CutInCopyObject(Collider2D[] cols,bool isFirst)
    {
        foreach (var col in cols)
        {
            if (col.gameObject.tag != "Tilemap"
                && col.gameObject.tag != "Player"
                && col.gameObject.tag != "Uncuttable")
            {
                if (isFirst)    //��T��
                {
                    //tileSB.tileDataList.Single(t => t.tile == tilemap.GetTile(p)).p_ene;
                    stageMgr.write_cost += objSB.objectList.Single(t => t.obj.tag == col.gameObject.tag).p_ene;
                    if (stageMgr.all_isCut) stageMgr.cut_erase_cost += objSB.objectList.Single(t => t.obj.tag == col.gameObject.tag).ow_ene;
                }
                else            //��T��
                {
                    //Debug.Log(col.gameObject.name);
                    StageManager.ObjectData c = new StageManager.ObjectData();
                    if (!stageMgr.all_isCut)
                    {
                        c.obj = Instantiate(col.gameObject);
                        c.obj.transform.parent = stageMgr.transform;
                        //����������̂Ȃ�f�[�^�̈����p��
                        ThrowObjectController toc = col.gameObject.GetComponent<ThrowObjectController>();
                        if (toc != null)
                        {
                            Vector3 dir = toc.GetDir();
                            c.obj.GetComponent<ThrowObjectController>().SetDir(dir);
                        }
                    }
                    else
                    {
                        c.obj = col.gameObject;
                        col.gameObject.transform.parent = stageMgr.transform;
                    }
                    c.pos = col.gameObject.transform.position - ChangeVecToInt(startPos);
                    c.obj.SetActive(false);
                    stageMgr.objectData.Add(c);
                }
            }

        }
    }

    public void CutInCopy(Vector3Int _startPos, Vector3 _endPos, bool Count)
    {
        //�R�s�[�͈͂�tile���R�s�[
        for (int y = 0; y < stageMgr.tileData.height; y++)
        {
            //1�񂲂Ƃ�List
            List<TileBase> tBases = new List<TileBase>();
            for (int x = 0; x < stageMgr.tileData.width; x++)
            {
                Vector3Int p = Vector3Int.zero;
                //�����ɂ���Ď擾����
                switch (stageMgr.tileData.direction)
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
                    if(Count)
                    {
                        stageMgr.write_cost += tileSB.tileDataList.Single(t => t.tile == tilemap.GetTile(p)).p_ene; // �擾�����^�C�����^�C���p���b�g�̂ǂ̃^�C�����𔻕ʂ��Ă��̏���R�X�g���{
                        if (stageMgr.all_isCut)
                        {
                            stageMgr.cut_erase_cost += tileSB.tileDataList.Single(t => t.tile == tilemap.GetTile(p)).ow_ene;
                            Debug.Log("������R�X�g" + stageMgr.cut_erase_cost);
                            //tilemap.SetTile(p, null);
                        }
                    }
                    else
                    {
                        if (stageMgr.all_isCut)
                        {
                            tilemap.SetTile(p, null);
                        }
                    }
                }
                tBases.Add(t);
                /*if (t != null) Debug.Log(t.name);
                else Debug.Log("null");*/
            }
            stageMgr.tileData.tiles.Add(tBases);
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

    void InitTileData()
    {
        stageMgr.tileData.tiles = new List<List<TileBase>>();
        stageMgr.tileData.width = 0;
        stageMgr.tileData.height = 0;
        stageMgr.tileData.hasData = false;

        stageMgr.objectData = new List<StageManager.ObjectData>();
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
