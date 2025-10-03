using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class RankJudgeAndUpdateFunction : MonoBehaviour
{
    private StageManager stageMgr;
    ClearFunction clearFunc;
    private RankDisplay rankDisplay;
    public string rankText = "F";
    bool hasJudged = false;

    //private int allCost = 24; //
    private int[,] stageRank = {{-50, 300, 1000, 2000}, //stage1
                                {-80, 200, 500, 1500}, //stage2
                                {-50, 150, 400, 1200}, //stage3
                                {0, 200, 500, 1500}, //stage4
                                {-50, 300, 700, 1500}, //stage5
                                {-50, 50, 300, 700}, //stage6
                                {-30, 300, 700, 1500}, //stage7
                                {-50, 50, 200, 500}, //stage8
                                {100, 300, 500, 1500}, //stage9
                                {-70, 50, 200, 600}, //stage10
                                {-30, 0, 100, 200}, //stage11
                                {0, 200, 500, 1500}, //stage12
                                {-90, 0, 200, 500}, //stage13
                                {30, 150, 400, 1000}, //stage14
                                {-80, 50, 200, 700}, //stage15
                                {-50, 100, 300, 700}, //stage16
                                {-70, 50, 200, 500}, //stage17
                                {0, 100, 300, 700}, //stage18
                                {25, 50, 75, 100}, //stage19
                                {25, 50, 75, 100}, //stage20
                                }; //S~F

    //private int allCost = 24; //
    private int[] clearAddCost = { 100, 60, 40, 30, 10}; //StageSelect
                                                           //

    private int[] minConsumpCost = { -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000}; //

    public Dictionary<string, int> stageNumber = new Dictionary<string, int>() // Dictionary
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
        {"Stage10", 9},
        {"Stage11", 10},
        {"Stage12", 11},
        {"Stage13", 12},
        {"Stage14", 13},
        {"Stage15", 14},
        {"Stage16", 15},
        {"Stage17", 16},
        {"Stage18", 17},
        {"Stage19", 18},
        {"Stage20", 19},
        {"StageTemplate",20 }
    };


    // Start is called before the first frame update
    void Start()
    {
        InitStatus();
    }

    public void InitStatus()
    {
        stageMgr = null;
        clearFunc = null;
        rankDisplay = null;
        hasJudged = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (stageMgr == null)
        {
            stageMgr = FindObjectOfType<StageManager>();
        }
        if (rankDisplay == null)
        {
            rankDisplay = FindObjectOfType<RankDisplay>();
        }
        //
        if (clearFunc != null)
        {
            if ((clearFunc.GetisClear() && !hasJudged))
            {
                JudgeAndUpdateRank(stageMgr.all_sum_cos - stageMgr.player_HP, true);
                //stageMgr.all_sum_cos = 0;
                rankDisplay.SetText(rankText);

                rankDisplay.InitTextSize();
            }
            if (clearFunc.GetisClear() && hasJudged)
            {
                rankDisplay.AnimateText();
            }
        }
        else
        {
            clearFunc = FindObjectOfType<ClearFunction>();
        }
    }

    /*ｰ*/
    /*public void UpdateMinConCost(int num)
    {
        int stage_num = stageNumber[SceneManager.GetActiveScene().name];
        if(num < minConsumpCost[stage_num]) //
        {
            minConsumpCost[stage_num] = num; //
        }
    }*/

    /**/
    public int culcCostToNextRank()
    {
        int nowCost = 0;
        if (stageMgr != null)
        {
            nowCost = stageMgr.all_sum_cos - stageMgr.player_HP; //
        }
        int stage_num = 0;          //
        if (Regex.IsMatch(SceneManager.GetActiveScene().name, @"^Stage\d+$")) //
        {
            stage_num = stageNumber[SceneManager.GetActiveScene().name];
        }

        //
        if (nowCost < stageRank[stage_num, 3])
        {
            for (int i = 0; i < stageRank.GetLength(0); i++)
            {
                if (nowCost < stageRank[stage_num, i]) return stageRank[stage_num, i] - nowCost;
            }
        }

        return 0;
    }

    /**/
    public string GetStageRank(string stageName)
    {
        //
        if (stageNumber.TryGetValue(stageName, out int stageNum))
        {
            int minCost = minConsumpCost[stageNum];
            //
            if (minCost == -10000)
            {
                return "NONE";
            }
            //
            if (minCost < stageRank[stageNum, 0])
            {
                return "S";
            }
            else if (minCost < stageRank[stageNum, 1])
            {
                return "A";
            }
            else if (minCost < stageRank[stageNum, 2])
            {
                return "B";
            }
            else if (minCost < stageRank[stageNum, 3])
            {
                return "C";
            }
            else if (minCost >= stageRank[stageNum, 3])
            {
                return "F";
            }
        }
        else     //
        {
            return "NONE";
        }

        return "NONE";
    }

    /**/
    public string JudgeAndUpdateRank(int num, bool isCleared) //num == 
    {
        string input = SceneManager.GetActiveScene().name;
        if (Regex.IsMatch(input, @"^Stage\d+$")) //
        {
            int stage_num = stageNumber[SceneManager.GetActiveScene().name];
            if (num < stageRank[stage_num, 0])
            {
                rankText = "S";
            }
            else if (num < stageRank[stage_num, 1])
            {
                rankText = "A";
            }
            else if (num < stageRank[stage_num, 2])
            {
                rankText = "B";
            }
            else if (num < stageRank[stage_num, 3])
            {
                rankText = "C";
            }
            else if (num >= stageRank[stage_num, 3])
            {
                rankText = "F";
            }

            //minConsumpCost[stage_num] == -1) ｰ
            if (isCleared && (minConsumpCost[stage_num] == -10000 || num < minConsumpCost[stage_num]))
            {
                minConsumpCost[stage_num] = num; //
                //StageSelect
                AddInitCost(stage_num);
                Debug.Log("" + (stage_num + 1) + "" + num + "");
            }
        }
        else
        {
            //
            //Debug.Log(");
            int stage_num = 0;
            if (num < stageRank[stage_num, 0])
            {
                rankText = "S";
            }
            else if (num < stageRank[stage_num, 1])
            {
                rankText = "A";
            }
            else if (num < stageRank[stage_num, 2])
            {
                rankText = "B";
            }
            else if (num < stageRank[stage_num, 3])
            {
                rankText = "C";
            }
            else if (num >= stageRank[stage_num, 3])
            {
                rankText = "F";
            }

        }

        if (isCleared) hasJudged = true;
        return rankText;
    }

    public void AddInitCost(int stage_num)
    {
        switch (rankText)
        {
            case "S":
                stageMgr.initAddCost_EachStage[stage_num] = clearAddCost[0];
                break;
            case "A":
                stageMgr.initAddCost_EachStage[stage_num] = clearAddCost[1];
                break;
            case "B":
                stageMgr.initAddCost_EachStage[stage_num] = clearAddCost[2];
                break;
            case "C":
                stageMgr.initAddCost_EachStage[stage_num] = clearAddCost[3];
                break;
            default:
                break;
        }
    }
}
