using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class GameUIController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text_HP;
    [SerializeField] TextMeshProUGUI text_nowCost;
    [SerializeField] TextMeshProUGUI text_nowRank;
    [SerializeField] TextMeshProUGUI text_nextRank;

    private StageManager stageManager;
    private RankJudgeAndUpdateFunction judgeFunc;
    //private Tilemap tilemap;
    private Vector3 initPos;

    void Start()
    {
        stageManager = FindObjectOfType<StageManager>();
        judgeFunc = FindObjectOfType<RankJudgeAndUpdateFunction>();
    }

    // Update is called once per frame
    void Update()
    {
        if(judgeFunc == null)
        {
            judgeFunc = FindObjectOfType<RankJudgeAndUpdateFunction>();
        }
        text_HP.text = "PlayerHP\n:" + stageManager.player_HP;
        text_nowCost.text = "nowCost\n:" + stageManager.have_ene;
        //�W���b�W����֐��������Ă��Ă�B2�ڂ̕ϐ��͐�΂�false�B
        //2�ڂ̕ϐ���true�ɂ���ƍŏ�����R�X�g���X�V����A���ɃW���b�W���I������Ɣ��肳���
        text_nowRank.text = judgeFunc.JudgeAndUpdateRank(stageManager.all_sum_cos - stageManager.player_HP, false);
        text_nextRank.text = "next Rank ... left " + judgeFunc.culcCostToNextRank();
    }
}

/* void Start()
 * Tilemap[] maps = FindObjectsOfType<Tilemap>();
        foreach (var map in maps)
        {
            if (map.gameObject.tag == "Copy_Tilemap")
            {
                tilemap = map;
                break;
            }
        }
        initPos = tilemap.transform.localPosition;*/
/*
 * void Update()
         ���݃R�s�[���Ă���f�[�^��UI�ɕ\���������B�ǂ����H
        �܂��A�R�s�[���Ă���^�C���f�[�^�����ƂɐV����tilemap���쐬����B
        �쐬����tilemap�̃T�C�Y���A������v�Z����UI�̂Ƃ���Ɏ��܂�悤�ɂ���B
         
        if (stageManager.tileData.hasData)
        {
            tilemap.ClearAllTiles();
            tilemap.gameObject.transform.position = initPos;
            int w = stageManager.tileData.width;
            int h = stageManager.tileData.height;
            float scaleRateX = 6.0f / (float)w;
            float scaleRateY = 6.0f / (float)h;
            float scaleRate = scaleRateX < scaleRateY ? scaleRateX : scaleRateY;
            tilemap.gameObject.transform.localScale = new Vector3(scaleRate,scaleRate,1);
            float newX = 1;
            float newY = 1;
            switch (stageManager.tileData.direction)
            {
                case 0:
                    newX = -w * 0.5f * scaleRate;
                    newY = -h * 0.5f * scaleRate;
                    break;
                case 1:
                    newX = -w * 0.5f * scaleRate;
                    newY = -h * 0.5f * -scaleRate - scaleRate;
                    break;
                case 2:
                    newX = -w * 0.5f * -scaleRate - scaleRate;
                    newY = -h * 0.5f * -scaleRate - scaleRate;
                    break;
                case 3:
                    newX = -w * 0.5f * -scaleRate - scaleRate;
                    newY = -h * 0.5f * scaleRate;
                    break;
                    default:break;
            }
            tilemap.gameObject.transform.localPosition = new Vector3(initPos.x + newX,initPos.y + newY,initPos.z);
            for(int i = 0; i < h; i++)
            {
                for(int j = 0; j < w; j++)
                {
                    //�R�s�[�����Ƃ��̕����ɂ���Č��_���قȂ�
                    Vector3Int _p = Vector3Int.zero;
                    switch (stageManager.tileData.direction)
                    {
                        case 0:
                            _p = new Vector3Int( j,i,0);
                            break;
                        case 1:
                            _p = new Vector3Int( j,-i, 0);
                            break;
                        case 2:
                            _p = new Vector3Int( -j,-i, 0);
                            break;
                        case 3:
                            _p = new Vector3Int( -j,i,0);
                            break;
                        default: break;
                    }

                    tilemap.SetTile(_p, stageManager.tileData.tiles[i][j]);
                }
            }
        }
        else
        {
            tilemap.ClearAllTiles();
        }
        */