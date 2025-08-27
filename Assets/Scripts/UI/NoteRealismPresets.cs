using UnityEngine;

/// <summary>
/// 方法A（SetEffect）用：NoteRealismController に値を流し込むプリセット群
/// </summary>
public static class NoteRealismPresets
{
    public enum PresetType
    {
        // ベースライン（効果なし）
        Baseline,

        // “ゲーム感”系
        ArcadeNeon,     // 発色強め・ネオン
        ActionBoost,    // パンチのある演出
        RetroCRT,       // レトロ／CRT風
        HorrorUnease,   // 不安・ホラー
        ComicPop,       // コミック風・パキッと
        SoftFilm,       // 映画的にソフト
        TechNoir,       // 低彩度＋強コントラスト
        MobileLight     // 軽量・汎用
    }

    public static void ApplyPreset(NoteRealismController c, PresetType type)
    {
        if (c == null) return;

        // 共通：フェード時間（好みで調整）
        c.tweenSeconds = 0.18f;

        switch (type)
        {
            case PresetType.Baseline:
                c.targetSaturation = 0f;
                c.targetContrast = 0f;
                c.targetVignette = 0f;
                c.targetGrain = 0f;
                c.targetBloom = 0f;
                c.targetDistortion = 0f;
                c.targetFocusDist = 10f;
                c.targetAperture = 5.6f;
                c.targetFocalLen = 50f;
                break;

            case PresetType.ArcadeNeon:
                c.targetSaturation = +20f;
                c.targetContrast = +15f;
                c.targetVignette = 0.15f;
                c.targetGrain = 0.05f;
                c.targetBloom = 7.0f;
                c.targetDistortion = -8f;   // 樽型で中央強調
                c.targetFocusDist = 10f;   // DoFほぼオフ
                c.targetAperture = 5.6f;
                c.targetFocalLen = 50f;
                break;

            case PresetType.ActionBoost:
                c.targetSaturation = +10f;
                c.targetContrast = +20f;
                c.targetVignette = 0.45f; // 端をグッと締める
                c.targetGrain = 0.10f;
                c.targetBloom = 3.0f;
                c.targetDistortion = -15f;
                c.targetFocusDist = 5f;    // ほんのり浅く
                c.targetAperture = 2.8f;
                c.targetFocalLen = 60f;
                break;

            case PresetType.RetroCRT:
                c.targetSaturation = -5f;
                c.targetContrast = +12f;
                c.targetVignette = 0.35f;
                c.targetGrain = 0.25f; // 粒感
                c.targetBloom = 0.5f;
                c.targetDistortion = +6f;   // クッション型
                c.targetFocusDist = 8f;
                c.targetAperture = 4.0f;
                c.targetFocalLen = 50f;
                break;

            case PresetType.HorrorUnease:
                c.targetSaturation = -35f;
                c.targetContrast = +25f;
                c.targetVignette = 0.60f;
                c.targetGrain = 0.45f;
                c.targetBloom = 0f;
                c.targetDistortion = +18f;
                c.targetFocusDist = 3f;    // ぼけ強め
                c.targetAperture = 1.8f;
                c.targetFocalLen = 40f;
                break;

            case PresetType.ComicPop:
                c.targetSaturation = +25f;  // 色をパキッと
                c.targetContrast = +25f;
                c.targetVignette = 0.10f;
                c.targetGrain = 0.0f;  // クリーン
                c.targetBloom = 2.0f;  // エッジ強調寄り
                c.targetDistortion = -5f;
                c.targetFocusDist = 10f;
                c.targetAperture = 5.6f;
                c.targetFocalLen = 50f;
                break;

            case PresetType.SoftFilm:
                c.targetSaturation = -8f;
                c.targetContrast = +8f;
                c.targetVignette = 0.25f;
                c.targetGrain = 0.20f;
                c.targetBloom = 4.5f;  // 柔らかいハイライト
                c.targetDistortion = 0f;
                c.targetFocusDist = 6f;
                c.targetAperture = 2.2f;
                c.targetFocalLen = 55f;
                break;

            case PresetType.TechNoir:
                c.targetSaturation = -20f;  // 低彩度
                c.targetContrast = +30f;  // 強コントラスト
                c.targetVignette = 0.50f;
                c.targetGrain = 0.30f;
                c.targetBloom = 1.0f;
                c.targetDistortion = +10f;
                c.targetFocusDist = 7f;
                c.targetAperture = 2.8f;
                c.targetFocalLen = 50f;
                break;

            case PresetType.MobileLight:
                c.targetSaturation = +12f;
                c.targetContrast = +8f;
                c.targetVignette = 0.20f;
                c.targetGrain = 0.05f;
                c.targetBloom = 1.5f;
                c.targetDistortion = -5f;
                c.targetFocusDist = 10f;   // DoFオフ寄り（軽量）
                c.targetAperture = 5.6f;
                c.targetFocalLen = 50f;
                break;
        }
    }
}

