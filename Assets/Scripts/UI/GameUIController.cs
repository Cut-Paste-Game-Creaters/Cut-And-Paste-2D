using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameUIController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text_HP;
    [SerializeField] TextMeshProUGUI text_nowCost;
    [SerializeField] TextMeshProUGUI text_allCost;

    private StageManager stageManager;

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
    }
}
