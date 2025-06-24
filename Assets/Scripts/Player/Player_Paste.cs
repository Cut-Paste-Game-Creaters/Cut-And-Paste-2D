using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class Player_Paste : MonoBehaviour
{
    [SerializeField] GameObject frame1;
    [SerializeField] TileScriptableObject tileSB; //ScriptableObject
    [SerializeField] ObjectScriptableObject objSB; //ScriptableObject
    //[SerializeField] GameObject rectPrefab;

    private Tilemap tilemap;
    private StageManager stageManager;
    private SpriteRenderer sr;
    private Vector3 frameData = Vector3.zero;
    UndoRedoFunction urFunc;
    //private GameObject rect;
    // Start is called before the first frame update
    void Start()
    {
        frame1 = Instantiate(frame1);
        sr = frame1.GetComponent<SpriteRenderer>();
        sr.enabled = false;
        //rect = Instantiate(rectPrefab);
        Tilemap[] maps = FindObjectsOfType<Tilemap>();
        foreach (var map in maps)
        {
            if (map.gameObject.tag == "Tilemap")
            {
                tilemap = map;
                break;
            }
        }
        stageManager = FindObjectOfType<StageManager>();
        urFunc = FindObjectOfType<UndoRedoFunction>();
    }

    // Update is called once per frame
    void Update()
    {
        if (stageManager.tileData.hasData)
        {
            PasteTiles(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    void PasteTiles(Vector3 mousePos)
    {
        stageManager.erase_cost = 0; //�ŏ��ɓ\��t���ӏ��̃R�X�g��������

        //���݂̃}�E�X�ʒu�����炢�A���_�Ƃ���
        Vector3Int mPos = ChangeVecToInt(mousePos);

        //�E�N���b�N�������Ă�Ԙg�\��
        if (PlayerInput.GetMouseButton(1))
        {
            sr.enabled = true;
            frameData = stageManager.GetInfo();
            sr.size = frameData;
            Vector3 framePos = mPos;

            switch (frameData.z)
            {
                //�ŏ��̈ʒu�͐^�񒆂̉E��Ȃ̂Ōʂɒ��߂���K�v������
                case 0:     //�E��
                    framePos.x += frameData.x / 2;
                    framePos.y += frameData.y / 2;
                    break;
                case 1:     //�E��
                    framePos.x += frameData.x / 2;
                    framePos.y -= frameData.y / 2;
                    framePos.y++;
                    break;
                case 2:     //����
                    framePos.x -= frameData.x / 2;
                    framePos.y -= frameData.y / 2;
                    framePos.x++;
                    framePos.y++;
                    break;
                case 3:     //����
                    framePos.x -= frameData.x / 2;
                    framePos.y += frameData.y / 2;
                    framePos.x++;
                    break;
                default: break;
            }

            //�\��t�����̏����R�X�g���v�Z����
            CheckCost(false, mPos); //�^�C���Z�b�g���Ȃ��Ōv�Z
            CheckObjectCost(false);      //�I�u�W�F�N�g�̏����R�X�g���v�Z����
            //Debug.Log("erase:"+stageManager.erase_cost);

            frame1.transform.position = framePos;
        }
        //�E�N���b�N�œ\��t��
        if (PlayerInput.GetMouseButtonUp(1))
        {
            sr.enabled = false;     //�g���\���ɂ���

            //�\��t�����̏����R�X�g���v�Z����
            CheckCost(false, mPos); //�^�C���Z�b�g���Ȃ��Ōv�Z
            CheckObjectCost(false);      //�I�u�W�F�N�g�̏����R�X�g���v�Z����
            Debug.Log("erase:" + stageManager.erase_cost);

            int divide = 1;
            if(stageManager.all_isCut)
            {
                divide = 2;
            }

            if(stageManager.have_ene >= (stageManager.erase_cost + stageManager.write_cost)) //�����R�X�g���������Ȃ�
            {
                stageManager.have_ene -= (stageManager.erase_cost + stageManager.write_cost / divide); //�R�X�g����
                stageManager.all_sum_cos += (stageManager.erase_cost + stageManager.write_cost / divide); //������R�X�g�ɉ��Z
                Debug.Log("������R�X�g�F" + stageManager.erase_cost + "," + "���₷�R�X�g�F" + stageManager.write_cost / divide + ", " + "�����R�X�g�F" + stageManager.have_ene);
                Debug.Log("������R�X�g�F" + stageManager.all_sum_cos);
                CheckCost(true, mPos);
                CheckObjectCost(true);
                //�I�u�W�F�N�g���y�[�X�g
                PasteObject();

                if (stageManager.all_isCut) //1��̂݃y�[�X�g�ɂ��鏈��
                {
                    InitTileData();
                }

                urFunc.InfoPushToStack();
            }
            //Debug.Log("isCut == " + stageManager.all_isCut);

        }
    }

    public void PasteObject()
    {
        if (stageManager.objectData.Count > 0)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            foreach (var c in stageManager.objectData)
            {
                if (stageManager.all_isCut)
                {
                    c.obj.SetActive(true);
                    c.obj.transform.position = c.pos + ChangeVecToInt(mousePos);
                    c.obj.transform.parent = null;
                }
                else
                {
                    if(c.obj.transform.parent != null)
                    {
                        c.obj.transform.parent = null;
                    }
                    //�R�s�[�������̂𕡐�����
                    GameObject b = Instantiate(c.obj);
                    //������������I�u�W�F�N�g�Ȃ�f�[�^�̈����p��
                    ThrowObjectController toc = c.obj.GetComponent<ThrowObjectController>();
                    if (toc != null)
                    {
                        Vector3 dir = toc.GetDir();
                        b.GetComponent<ThrowObjectController>().SetDir(dir);
                    }
                    b.SetActive(true);
                    b.transform.position = c.pos + ChangeVecToInt(mousePos);
                }
                stageManager.EraseObjects.Add(c.obj);
            }
        }

    }

    public void CheckCost(bool isSetTile, Vector3Int mPos)
    {
        //if(!isSetTile)stageManager.erase_cost = 0;
        //�^�C�����y�[�X�g
        for (int y = 0; y < stageManager.tileData.height; y++)
        {
            for (int x = 0; x < stageManager.tileData.width; x++)
            {
                //�R�s�[�����Ƃ��̕����ɂ���Č��_���قȂ�
                Vector3Int _p = Vector3Int.zero;
                switch (stageManager.tileData.direction)
                {
                    case 0:
                        _p = new Vector3Int(mPos.x + x, mPos.y + y, 0);
                        break;
                    case 1:
                        _p = new Vector3Int(mPos.x + x, mPos.y - y, 0);
                        break;
                    case 2:
                        _p = new Vector3Int(mPos.x - x, mPos.y - y, 0);
                        break;
                    case 3:
                        _p = new Vector3Int(mPos.x - x, mPos.y + y, 0);
                        break;
                    default: break;
                }

                if (isSetTile)
                {
                    tilemap.SetTile(_p, stageManager.tileData.tiles[y][x]);
                }
                else
                {
                    if (tilemap.HasTile(_p)) //kyosu �������̃Z�����^�C���������Ă���Ȃ�
                    {
                        stageManager.erase_cost += tileSB.tileDataList.Single(t => t.tile == tilemap.GetTile(_p)).ow_ene; // �擾�����^�C�����^�C���p���b�g�̂ǂ̃^�C�����𔻕ʂ��Ă��̏���R�X�g���{
                        
                    }
                }
            }
        }
    }

    public void CheckObjectCost(bool isErase)
    {
        Vector3Int _p = Vector3Int.zero;
        Vector3Int mPos = ChangeVecToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition)
            ,stageManager.tileData.direction);
        int w = stageManager.tileData.width;
        int h = stageManager.tileData.height;

        
        //�͈͂��v�Z����
        switch (stageManager.tileData.direction)
        {
            case 0:
                _p = new Vector3Int(mPos.x + w, mPos.y + h, 0);
                break;
            case 1:
                _p = new Vector3Int(mPos.x + w, mPos.y - h, 0);
                break;
            case 2:
                _p = new Vector3Int(mPos.x - w, mPos.y - h, 0);
                break;
            case 3:
                _p = new Vector3Int(mPos.x - w, mPos.y + h, 0);
                break;
            default: break;
        }

        //�f�o�b�O�p�@�����I��͈͂������Ă���
        //rect.transform.position = (mPos + _p) / 2;
        //rect.transform.localScale = new Vector3(Mathf.Abs(_p.x - mPos.x), Mathf.Abs(_p.y - mPos.y), 1);


        Collider2D[] cols = Physics2D.OverlapAreaAll(
            new Vector2(mPos.x,mPos.y), new Vector2(_p.x,_p.y)
            );
        //�㏑���͈͓��̃R���C�_�[�̏����R�X�g���v�Z����
        foreach(var col in cols)
        {
            if (col.gameObject.tag != "Tilemap"
                && col.gameObject.tag != "Player"
                && col.gameObject.tag != "Uncuttable")
            {
                Debug.Log("erase:" + col.gameObject.name);
                if (!isErase) stageManager.erase_cost += objSB.objectList.Single(t => t.obj.tag == col.gameObject.tag).ow_ene;
                else Destroy(col.gameObject);       //�㏑������I�u�W�F�N�g������
            }
                
        }
    }

    public Vector3Int ChangeVecToInt(Vector3 v)
    {
        Vector3Int pos = Vector3Int.zero;
        pos.x = (int)Mathf.Floor(v.x);
        pos.y = (int)Mathf.Floor(v.y);
        //pos.z = (int)Mathf.Floor(v.z);

        return pos;
    }

    public Vector3Int ChangeVecToInt(Vector3 v,int dir)
    {
        Vector3Int pos = Vector3Int.zero;
        switch (dir)
        {
            case 0:     //�E��ɃR�s�[�@�܂荶���ɃL���X�g
                pos = new Vector3Int((int)Mathf.Floor(v.x),(int)Mathf.Floor(v.y),0);
                break;
            case 1:     //�E���ɃR�s�[�@�܂荶��ɃL���X�g
                pos = new Vector3Int((int)Mathf.Floor(v.x), (int)Mathf.Ceil(v.y),0);
                break;
            case 2:     //�����ɃR�s�[�@�܂�E��ɃL���X�g
                pos = new Vector3Int((int)Mathf.Ceil(v.x), (int)Mathf.Ceil(v.y), 0);
                break;
            case 3:     //����ɃR�s�[�@�܂�E���ɃL���X�g
                pos = new Vector3Int((int)Mathf.Ceil(v.x), (int)Mathf.Floor(v.y), 0);
                break;
            default:break;
        }

        return pos;
    }

    void InitTileData()
    {
        stageManager.tileData.tiles = new List<List<TileBase>>();
        stageManager.tileData.width = 0;
        stageManager.tileData.height = 0;
        stageManager.tileData.hasData = false;

        stageManager.objectData = new List<StageManager.ObjectData>();
    }
}

