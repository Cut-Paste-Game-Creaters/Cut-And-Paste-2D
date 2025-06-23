using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class RankJudgeAndUpdateFunction : MonoBehaviour
{
    private StageManager stageMgr;
    ClearFunction clearFunc;
    public string rankText = "F";
    bool hasJudged = false;

    //private int allCost = 24; //テスト用変数（総消費コスト
    private int[,] stageRank = {{25, 50, 75, 100}, //stage1のランク基準数値
                                {30, 60, 90, 120}, //stage2のランク基準数値
                                {25, 50, 75, 100}, //stage3のランク基準数値
                                {25, 50, 75, 100}, //stage4のランク基準数値
                                {25, 50, 75, 100}, //stage5のランク基準数値
                                {25, 50, 75, 100}, //stage6のランク基準数値
                                {25, 50, 75, 100}, //stage7のランク基準数値
                                {25, 50, 75, 100}, //stage8のランク基準数値
                                {25, 50, 75, 100}, //stage9のランク基準数値
                                {25, 50, 75, 100}, //stage10のランク基準数値
                                }; //S~Fの判定基準

    private int[] minConsumpCost = {-1, -1, -1, -1, -1,-1, -1, -1, -1, -1}; //ステージ最低消費コスト配列

    public Dictionary<string, int> stageNumber = new Dictionary<string, int>() // Dictionaryクラスの宣言と初期値の設定
    {
        {"Stage1", 0},
        {"Stage2", 1},
        {"Stage3", 2},
        {"Stage4", 3},
        {"Stage5", 4},
        {"Stage6", 5},
        {"Stage7", 6},
        {"Stage8", 7},
        {"Stage9", 8},
        {"Stage10", 9}
    };


    // Start is called before the first frame update
    void Start()
    {
        stageMgr = FindObjectOfType<StageManager>();
        clearFunc = FindObjectOfType<ClearFunction>();
    }

    // Update is called once per frame
    void Update()
    {
        //ゴールしたら
        if(clearFunc != null)
        {
            if(clearFunc.GetisClear()  && !hasJudged || PlayerInput.GetKeyDown(KeyCode.Alpha2) && !hasJudged)
            {
                JudgeAndUpdateRank(stageMgr.all_sum_cos);
                //stageMgr.all_sum_cos = 0;
            }
        }
    }

    /*最低消費コストを更新*/
    /*public void UpdateMinConCost(int num)
    {
        int stage_num = stageNumber[SceneManager.GetActiveScene().name];
        if(num < minConsumpCost[stage_num]) //今までの最低消費コストより小さければ
        {
            minConsumpCost[stage_num] = num; //最低消費コストを書き換え
        }
    }*/

    /*総消費コストのランク判定と最低消費コストの書き換えをおこなう関数*/
    public void JudgeAndUpdateRank(int num) //num == 総消費コスト
    {
        string input = SceneManager.GetActiveScene().name;
        if(Regex.IsMatch(input, @"^Stage\d+$")) //シーン名がStageなんとかなら
        {
            int stage_num = stageNumber[SceneManager.GetActiveScene().name];
            if(num <= stageRank[stage_num, 0])
            {
                rankText = "S";
            }
            else if(num <= stageRank[stage_num, 1])
            {
                rankText = "A";
            }
            else if(num <= stageRank[stage_num, 2])
            {
                rankText = "B";
            }
            else if(num <= stageRank[stage_num, 3])
            {
                rankText = "C";
            }
            else if(num > stageRank[stage_num, 3])
            {
                rankText = "F";
            }

            //まだ書き換えされていない(minConsumpCost[stage_num] == -1) または 今までの最低消費コストより小さければ
            if(minConsumpCost[stage_num] == -1|| num < minConsumpCost[stage_num])
            {
                minConsumpCost[stage_num] = num; //最低消費コストを書き換え
                Debug.Log("ステージ" + (stage_num + 1) + "の最低消費コストが" + num + "に更新されました");
            }
        }
        else
        {
            Debug.Log("ステージでないため判定できません.");
        }

        hasJudged = true;
    }
}
