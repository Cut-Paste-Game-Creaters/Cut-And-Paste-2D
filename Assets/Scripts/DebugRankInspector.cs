using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class DebugRankInspector : MonoBehaviour
{
    void Start()
    {
        var rank = FindObjectOfType<RankJudgeAndUpdateFunction>();
        Debug.Log(rank ? "[Rank] 見つかりました。" : "[Rank] 見つかりません。StageSelectに配置されていません。");

        if (rank)
        {
            Debug.Log($"[Rank] stageNumber.Count={rank.stageNumber?.Count ?? 0}");
            foreach (var kv in rank.stageNumber.OrderBy(k => k.Value))
                Debug.Log($"[Rank] #{kv.Value} : key='{kv.Key}'");
        }
        if (rank && rank.stageNumber != null)
        {
            foreach (var kv in rank.stageNumber.OrderBy(k => k.Value))
            {
                string name = kv.Key;
                string gsr = rank.GetStageRank(name); // 既存API
                string p1 = PlayerPrefs.GetString($"StageRank_{name}", "NONE");
                string p2 = PlayerPrefs.GetString($"Rank_{name}", "NONE");

                Debug.Log($"[RankCheck] {name}  GetStageRank='{gsr}'  PlayerPrefs(StageRank_)='{p1}'  PlayerPrefs(Rank_)='{p2}'");
            }
        }
    }
}