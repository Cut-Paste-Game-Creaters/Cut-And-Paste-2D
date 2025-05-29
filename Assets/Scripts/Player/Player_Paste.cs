using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class Player_Paste : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] StageManager stageManager;
    [SerializeField] GameObject frame1;
    [SerializeField] TileScriptableObject tileSB; //ScriptableObject

    private SpriteRenderer sr;
    // Start is called before the first frame update
    void Start()
    {
        frame1 = Instantiate(frame1);
        sr = frame1.GetComponent<SpriteRenderer>();
        sr.enabled = false;
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
        //�E�N���b�N�œ\��t��
        if (PlayerInput.GetMouseButtonUp(1))
        {
            //�I�u�W�F�N�g���y�[�X�g
            PasteObject();


            //�^�C�����y�[�X�g
            /*for (int y = 0; y < stageManager.tileData.height; y++)
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

                    if (tilemap.HasTile(_p)) //kyosu �������̃Z�����^�C���������Ă���Ȃ�
                    {
                        stageManager.erase_cost += tileSB.tileDataList.Single(t => t.tile == tilemap.GetTile(_p)).ow_ene; // �擾�����^�C�����^�C���p���b�g�̂ǂ̃^�C�����𔻕ʂ��Ă��̏���R�X�g���{
                    }
                    tilemap.SetTile(_p, stageManager.tileData.tiles[y][x]);
                }
            }*/
            CheckCost(false, mPos); //�^�C���Z�b�g���Ȃ��Ōv�Z

            int divide = 1;
            if(stageManager.all_isCut)
            {
                divide = 2;
            }

            if(stageManager.have_ene >= (stageManager.erase_cost + stageManager.write_cost)) //�����R�X�g���������Ȃ�
            {
                stageManager.have_ene -= (stageManager.erase_cost + stageManager.write_cost / divide); //�R�X�g����
                CheckCost(true, mPos);
                Debug.Log("������R�X�g�F" + stageManager.erase_cost + "," + "���₷�R�X�g�F" + stageManager.write_cost + ", " + "�����R�X�g�F" + stageManager.have_ene);
            }
            Debug.Log("isCut == " + stageManager.all_isCut);

            if (stageManager.tileData.isCut) //1��̂݃y�[�X�g�ɂ��鏈��
            {
                InitTileData();
            }
        }
    }

    public void PasteObject()
    {
        if (stageManager.objectData.Count > 0)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            foreach (StageManager.ObjectData c in stageManager.objectData)
            {
                if (stageManager.tileData.isCut)
                {
                    c.obj.SetActive(true);
                    c.obj.transform.position = c.pos + ChangeVecToInt(mousePos);
                }
                else
                {

                }
                    
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

    void InitTileData()
    {
        stageManager.tileData.tiles = new List<List<TileBase>>();
        stageManager.tileData.width = 0;
        stageManager.tileData.height = 0;
        stageManager.tileData.hasData = false;

        stageManager.objectData = new List<StageManager.ObjectData>();
    }

    public void CheckCost(bool isSetTile, Vector3Int mPos)
    {
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

                    if(isSetTile)
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
}

