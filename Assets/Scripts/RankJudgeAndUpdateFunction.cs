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

    //private int allCost = 24; //チE��ト用変数�E�総消費コスチE
    private int[,] stageRank = {{100, 500, 1000, 2000}, //stage1のランク基準数値
                                {0, 100, 300, 1000}, //stage2のランク基準数値
                                {0, 150, 400, 1200}, //stage3のランク基準数値
                                {50, 200, 500, 1200}, //stage4のランク基準数値
                                {-50, 100, 300, 1000}, //stage5のランク基準数値
                                {-80, 50, 250, 700}, //stage6のランク基準数値
                                {40, 200, 500, 1000}, //stage7のランク基準数値
                                {0, 100, 300, 700}, //stage8のランク基準数値
                                {25, 50, 75, 100}, //stage9のランク基準数値
                                {25, 50, 75, 100}, //stage10のランク基準数値
                                {25, 50, 75, 100}, //stage10のランク基準数値

                                }; //S~Fの判定基溁E

    //private int allCost = 24; //チE��ト用変数�E�総消費コスチE
    private int[] clearAddCost = { 100, 50, 30, 20, 5 }; //S�E�FでStageSelectの初期コストに追加するコスチE
                                                         //吁E��チE�Eジで増えるコスト�E同じ

    private int[] minConsumpCost = { -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000 }; //スチE�Eジ最低消費コスト�E刁E

    public Dictionary<string, int> stageNumber = new Dictionary<string, int>() // Dictionaryクラスの宣言と初期値の設宁E
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
        {"StageTemplate",10 }
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
        //ゴールしためE
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

    /*最低消費コストを更新*/
    /*public void UpdateMinConCost(int num)
    {
        int stage_num = stageNumber[SceneManager.GetActiveScene().name];
        if(num < minConsumpCost[stage_num]) //今までの最低消費コストより小さければ
        {
            minConsumpCost[stage_num] = num; //最低消費コストを書き換ぁE
        }
    }*/

    /*現在の総消費コストから次のランクに下がるまでのコストを計算する関数*/
    public int culcCostToNextRank()
    {
        int nowCost = 0;
        if (stageMgr != null)
        {
            nowCost = stageMgr.all_sum_cos - stageMgr.player_HP; //総消費コスチE
        }
        int stage_num = 0;          //スチE�Eジナンバ�Eは0で初期化。もしデバッグ用スチE�Eジだったらstage1のコストを流用
        if (Regex.IsMatch(SceneManager.GetActiveScene().name, @"^Stage\d+$")) //シーン名がStageなんとかなめE
        {
            stage_num = stageNumber[SceneManager.GetActiveScene().name];
        }

        //もしF以上消費してたら0を返す、そぁE��なければ計箁E
        if (nowCost < stageRank[stage_num, 3])
        {
            for (int i = 0; i < stageRank.GetLength(0); i++)
            {
                if (nowCost < stageRank[stage_num, i]) return stageRank[stage_num, i] - nowCost;
            }
        }

        return 0;
    }

    /*ほか�Eスクリプトから吁E��チE�Eジのランクを取得する関数*/
    public string GetStageRank(string stageName)
    {
        //スチE�Eジ名から数字を取征E
        if (stageNumber.TryGetValue(stageName, out int stageNum))
        {
            int minCost = minConsumpCost[stageNum];
            //最小スコアがなぁE��らNONE
            if (minCost == -10000)
            {
                return "NONE";
            }
            //吁E��コアからランクを返す
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
        else     //スチE�Eジ名から取得できなかったら
        {
            return "NONE";
        }

        return "NONE";
    }

    /*総消費コスト�Eランク判定と最低消費コスト�E書き換えをおこなぁE��数*/
    public string JudgeAndUpdateRank(int num, bool isCleared) //num == 総消費コスチE
    {
        string input = SceneManager.GetActiveScene().name;
        if (Regex.IsMatch(input, @"^Stage\d+$")) //シーン名がStageなんとかなめE
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

            //まだ書き換えされてぁE��ぁEminConsumpCost[stage_num] == -1) また�E 今までの最低消費コストより小さければ
            if (isCleared && (minConsumpCost[stage_num] == -10000 || num < minConsumpCost[stage_num]))
            {
                minConsumpCost[stage_num] = num; //最低消費コストを書き換ぁE
                //StageSelectの初期コストを更新する
                AddInitCost(stage_num);
                Debug.Log("スチE�Eジ" + (stage_num + 1) + "の最低消費コストが" + num + "に更新されました");
            }
        }
        else
        {
            //チE��チE��用スチE�Eジの場合、最小消費コスト�E更新されなぁE
            //Debug.Log("数字�EスチE�Eジではありません。デバッグ用スチE�Eジです、E);
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
