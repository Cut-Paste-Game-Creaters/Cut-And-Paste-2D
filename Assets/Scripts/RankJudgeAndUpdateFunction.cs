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
    private int[,] stageRank = {{-50, 150, 600, 2000}, //stage1
                                {-80, 100, 500, 1500}, //stage2
                                {-50, 150, 400, 1200}, //stage3
                                {0, 150, 500, 1500}, //stage4
                                {-50, 150, 700, 1500}, //stage5
                                {-50, 50, 300, 700}, //stage6
                                {-30, 200, 500, 1500}, //stage7
                                {-50, 50, 200, 500}, //stage8
                                {100, 300, 500, 1500}, //stage9
                                {-70, 50, 200, 600}, //stage10
                                {-30, 0, 100, 200}, //stage11
                                {-50, 200, 500, 1500}, //stage12
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
    private int[] clearAddCost = { 100, 60, 40, 30, 10 }; //StageSelect
                                                          //

    private int[] minConsumpCost = { -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000 }; //

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

    public void LoadData()
    {
        //各ステージの最小消費コストを読み込み
        string minConsumpValue = PlayerPrefs.GetString("MinConsunpValue");
        //データがあるなら
        if (minConsumpValue != "")
        {
            minConsumpValue = minConsumpValue.TrimEnd(',');
            string[] values = minConsumpValue.Split(',');

            for (int i = 0; i < values.Length; i++)
            {
                minConsumpCost[i] = int.Parse(values[i]);
            }
        }

        //SelectSceneの使用可能コストを読み込み
        string initAddCostValue = PlayerPrefs.GetString("InitAddCostValue");
        //データがあるなら
        if (initAddCostValue != "")
        {
            initAddCostValue = initAddCostValue.TrimEnd(',');
            string[] values = initAddCostValue.Split(',');

            if (stageMgr == null)
            {
                stageMgr = FindObjectOfType<StageManager>();
            }

            for (int i = 0; i < values.Length; i++)
            {
                stageMgr.initAddCost_EachStage[i] = int.Parse(values[i]);
            }

            stageMgr.InitHaveCost(-1);
        }
    }

    public void SaveData()
    {
        //以下をコメントアウトするとゲーム終了時に全てのデータをリセットして再開できる。
        //ResetData();

        //各ステージの最小消費コストを保存
        string minConsumpValue = "";
        //minConsumpCost[0] = -10000;
        for (int i = 0; i < minConsumpCost.Length; i++)
        {
            minConsumpValue += minConsumpCost[i];
            minConsumpValue += ",";

            //Debug.Log(i + ":" + minConsumpCost[i]);
        }
        PlayerPrefs.SetString("MinConsunpValue", minConsumpValue);

        //SelectSceneの使用可能コストを保存
        string initAddCostValue = "";
        for (int i = 0; i < stageMgr.initAddCost_EachStage.Length; i++)
        {
            initAddCostValue += stageMgr.initAddCost_EachStage[i];
            initAddCostValue += ",";
            Debug.Log(i + ":" + stageMgr.initAddCost_EachStage[i]);
        }
        PlayerPrefs.SetString("InitAddCostValue", initAddCostValue);


        PlayerPrefs.Save();
    }

    public void ResetData()
    {
        for (int i = 0; i < minConsumpCost.Length; i++)
        {
            minConsumpCost[i] = -10000;
        }

        for (int i = 0; i < stageMgr.initAddCost_EachStage.Length; i++)
        {
            stageMgr.initAddCost_EachStage[i] = 0;
        }
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
        if (Regex.IsMatch(input, @"^Stage\d+$")) //なんだかstage+(数字)なら
        {
            int stage_num = stageNumber[SceneManager.GetActiveScene().name];

            //前回の最小コストから前回のrankTextを計算
            string old_text = GetRankText(minConsumpCost[stage_num], stage_num);
            if (minConsumpCost[stage_num] == -10000) old_text = "F";

            //今回のrankTextを計算
            rankText = GetRankText(num, stage_num);

            //minConsumpCost[stage_num] == -1) ｰ
            //もし前回のランクがないか、前回よりも消費コストが少なかったら
            if (isCleared)
            {
                if ((minConsumpCost[stage_num] == -10000 || num < minConsumpCost[stage_num]))
                {
                    minConsumpCost[stage_num] = num; //最小コストを上書き
                                                     //StageSelect
                    AddInitCost(stage_num);
                    Debug.Log("stage_num:" + stageMgr.initAddCost_EachStage[stage_num]);
                    //Debug.Log("stage" + (stage_num + 1) + ":" + num + "");
                    //Debug.Log("stage" + (stage_num + 1) + ":" + minConsumpCost[stage_num] + "");
                }

                //Debug.Log("old:" + old_text + " new:" + rankText);

                //ランクがアップしたかどうかを確認する
                if (IsRankImproved(rankText, old_text))
                {
                    int cost_now = stageMgr.have_ene;
                    clearFunc.DisplayRankUpCost(GetInitCost(rankText) - GetInitCost(old_text));
                }
                else
                {
                    clearFunc.UpDisplayRankUpCost();
                }
            }



        }
        else
        {
            rankText = GetRankText(num, 0);

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

    private int GetInitCost(string text)
    {
        switch (text)
        {
            case "S":
                return clearAddCost[0];
            case "A":
                return clearAddCost[1];
            case "B":
                return clearAddCost[2];
            case "C":
                return clearAddCost[2];
            default:
                return 0;
        }
    }

    private bool IsRankImproved(string rank_after, string rank_before)
    {
        // ランクの優劣を定義（小さいほど良い）
        Dictionary<string, int> rankOrder = new Dictionary<string, int>()
        {
        { "S", 0 },
        { "A", 1 },
        { "B", 2 },
        { "C", 3 },
        { "F", 4 }
        };

        // 入力の正当性チェック
        if (!rankOrder.ContainsKey(rank_after) || !rankOrder.ContainsKey(rank_before))
        {
            Debug.LogWarning("無効なランクが指定されました。");
            return false;
        }

        // 数値が小さいほどランクが良いので、beforeよりafterが小さければ改善している
        return rankOrder[rank_after] < rankOrder[rank_before];
    }

    private string GetRankText(int num, int stage_num)
    {
        //minConsumpCostの初期値は-1000なので、Fを返す
        //と思ったが、前回データがないときに最初がFから始まってしまうのでナシ
        //if (num < 0) return "F";

        if (num < stageRank[stage_num, 0])
        {
            return "S";
        }
        else if (num < stageRank[stage_num, 1])
        {
            return "A";
        }
        else if (num < stageRank[stage_num, 2])
        {
            return "B";
        }
        else if (num < stageRank[stage_num, 3])
        {
            return "C";
        }
        else
        {
            return "F";
        }
    }
}
