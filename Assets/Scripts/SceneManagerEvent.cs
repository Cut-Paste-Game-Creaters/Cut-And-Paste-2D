using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class SceneManagerEvent : MonoBehaviour
{
    StageManager stageMgr;
    RankJudgeAndUpdateFunction rankFunc;
    CaptureCopyZone captureFunc;

    // Start is called before the first frame update
    void Start()
    {
        stageMgr = FindObjectOfType<StageManager>();
        rankFunc = FindObjectOfType<RankJudgeAndUpdateFunction>();
        captureFunc = this.gameObject.GetComponent<CaptureCopyZone>();
        // イベントにイベントハンドラーを追加
        SceneManager.sceneLoaded += SceneLoaded;
        SceneManager.sceneLoaded += captureFunc.LoadSceneObject;


        //WarpDoorのscriptを集める
        WarpDoor[] doorList = GameObject.FindObjectsOfType<WarpDoor>();
        foreach (WarpDoor door in doorList)
        {
            //ステージ名を取得、そのステージのランクを取得、spriteを表示
            string stageName = door.GetStageName();
            string stage_rank = rankFunc.GetStageRank(stageName);
            door.SetRankSprite(stage_rank);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(captureFunc == null)
        {
            this.gameObject.GetComponent<CaptureCopyZone>();
        }
    }

    // イベントハンドラー（イベント発生時に動かしたい処理）
    void SceneLoaded (Scene nextScene, LoadSceneMode mode) {
        Debug.Log(nextScene.name);
        Debug.Log(mode);
        string input = SceneManager.GetActiveScene().name;
        if(stageMgr != null && rankFunc != null)
        {
            stageMgr.InitAllSumCost(); //総消費コスト初期化
            stageMgr.ResetObjectState();    //オブジェクトの状態を初期化(switch&key)
            rankFunc.InitStatus();
            if(Regex.IsMatch(input, @"^Stage\d+$")) //シーン名がStageなんとかなら
            {
                stageMgr.InitHaveCost(rankFunc.stageNumber[SceneManager.GetActiveScene().name]); //所持コストを初期コストに初期化
                stageMgr.stageNum = rankFunc.stageNumber[SceneManager.GetActiveScene().name];
                stageMgr.InitHealTimeOut(rankFunc.stageNumber[SceneManager.GetActiveScene().name]);
            }
            //ステージセレクトならドアにランクを表示する
            else if(input == "StageSelectScene")
            {
                //WarpDoorのscriptを集める
                WarpDoor[] doorList = GameObject.FindObjectsOfType<WarpDoor>();
                foreach(WarpDoor door in doorList)
                {
                    //ステージ名を取得、そのステージのランクを取得、spriteを表示
                    string stageName = door.GetStageName();
                    string stage_rank = rankFunc.GetStageRank(stageName);
                    door.SetRankSprite(stage_rank);
                }
            }
            else
            {
                stageMgr.InitHaveCost(0); //そうじゃないならStage1の初期コストに常に設定
                stageMgr.stageNum = -1;
                //セレクト画面に遷移したときは各ステージのランクに応じて初期コストを設定
            }
        }

        /*if(captureFunc != null) //シーン遷移時画像表示
        {
            if(stageMgr.tileData.hasData || stageMgr.objectData.Count > 0)
            {
                captureFunc.ableImage();
            }
            else
            {
                captureFunc.disableImage();
            }
        }*/
    }
}
