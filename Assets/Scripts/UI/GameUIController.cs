using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class GameUIController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text_HP;
    [SerializeField] TextMeshProUGUI text_nowCost;
    [SerializeField] TextMeshProUGUI text_allCost;

    private StageManager stageManager;
    private Tilemap tilemap;

    void Start()
    {
        Tilemap[] maps = FindObjectsOfType<Tilemap>();
        foreach (var map in maps)
        {
            if (map.gameObject.tag == "Copy_Tilemap")
            {
                tilemap = map;
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        stageManager = FindObjectOfType<StageManager>();
        text_HP.text = "PlayerHP\n:" + stageManager.player_HP;
        text_nowCost.text = "nowCost\n:" + stageManager.have_ene;
        text_allCost.text = "allCost\n:" + stageManager.all_sum_cos;

        /*
         ���݃R�s�[���Ă���f�[�^��UI�ɕ\���������B�ǂ����H
        �܂��A�R�s�[���Ă���^�C���f�[�^�����ƂɐV����tilemap���쐬����B
        �쐬����tilemap�̃T�C�Y���A������v�Z����UI�̂Ƃ���Ɏ��܂�悤�ɂ���B
         */
        if (stageManager.tileData.hasData)
        {
            tilemap.ClearAllTiles();
            int w = stageManager.tileData.width;
            int h = stageManager.tileData.height;
            float scaleRateX = 6.0f / (float)w;
            float scaleRateY = 6.0f / (float)h;
            float scaleRate = scaleRateX < scaleRateY ? scaleRateX : scaleRateY;
            tilemap.gameObject.transform.localScale = new Vector3(scaleRate,scaleRate,1);
            for(int i = 0; i < h; i++)
            {
                for(int j = 0; j < w; j++)
                {
                    //�R�s�[�����Ƃ��̕����ɂ���Č��_���قȂ�
                    Vector3Int _p = Vector3Int.zero;
                    switch (stageManager.tileData.direction)
                    {
                        case 0:
                            _p = new Vector3Int(  j - w/2    ,  i - h/2    , 0);
                            break;
                        case 1:
                            _p = new Vector3Int(  j - w/2    , -i + h/2    , 0);
                            break;
                        case 2:
                            _p = new Vector3Int( -j + w/2    , -i + h/2    , 0);
                            break;
                        case 3:
                            _p = new Vector3Int( -j + w/2    ,  i - h/2    , 0);
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
    }
}
