using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class NoteRealismController : MonoBehaviour
{
    [Header("Target Values (目標値)")]
    public float targetHueShift = 10f;
    public float targetSaturation = 20f;
    public float targetContrast = 10f;
    public float targetVignette = 0.3f;
    public float targetGrain = 0.3f;
    public float targetBloom = 5f;
    public float targetDistortion = 10f;
    public float targetFocusDist = 4f;
    public float targetAperture = 10f;
    public float targetFocalLen = 50f;

    [Header("Tween設定")]
    public float tweenSeconds = 0.2f;

    [Header("References")]
    public PostProcessVolume postProcessVolume;

    // PPSv2 settings
    ColorGrading colorGrading;
    Vignette vignette;
    Grain grain;
    Bloom bloom;
    LensDistortion lensDistortion;
    DepthOfField depthOfField;

    // Original values
    float hueOrig, satOrig, conOrig;
    float vigOrig, graOrig, bloOrig, disOrig;
    float dofFocusOrig, dofApertureOrig, dofFocalLenOrig;

    bool isOn = false;  // 現在の状態（オン/オフ）

    private bool wasPaused = false;///演出制御用

    void Awake()
    {
        if (!postProcessVolume) postProcessVolume = FindObjectOfType<PostProcessVolume>();
        if (!postProcessVolume || postProcessVolume.profile == null)
        {
            Debug.LogWarning("PostProcessVolumeが見つからないか、Profile未設定です。");
            return;
        }

        // 各エフェクト取得
        if (!postProcessVolume.profile.TryGetSettings(out colorGrading))
            colorGrading = postProcessVolume.profile.AddSettings<ColorGrading>();
        if (!postProcessVolume.profile.TryGetSettings(out vignette))
            vignette = postProcessVolume.profile.AddSettings<Vignette>();
        if (!postProcessVolume.profile.TryGetSettings(out grain))
            grain = postProcessVolume.profile.AddSettings<Grain>();
        if (!postProcessVolume.profile.TryGetSettings(out bloom))
            bloom = postProcessVolume.profile.AddSettings<Bloom>();
        if (!postProcessVolume.profile.TryGetSettings(out lensDistortion))
            lensDistortion = postProcessVolume.profile.AddSettings<LensDistortion>();
        if (!postProcessVolume.profile.TryGetSettings(out depthOfField))
            depthOfField = postProcessVolume.profile.AddSettings<DepthOfField>();

        // Override有効化
        colorGrading.hueShift.overrideState = true;
        colorGrading.saturation.overrideState = true;
        colorGrading.contrast.overrideState = true;
        vignette.intensity.overrideState = true;
        grain.intensity.overrideState = true;
        bloom.intensity.overrideState = true;
        lensDistortion.intensity.overrideState = true;
        depthOfField.focusDistance.overrideState = true;
        depthOfField.aperture.overrideState = true;
        depthOfField.focalLength.overrideState = true;

        // 元の値保存
        hueOrig = colorGrading.hueShift.value;
        satOrig = colorGrading.saturation.value;
        conOrig = colorGrading.contrast.value;
        vigOrig = vignette.intensity.value;
        graOrig = grain.intensity.value;
        bloOrig = bloom.intensity.value;
        disOrig = lensDistortion.intensity.value;
        dofFocusOrig = depthOfField.focusDistance.value;
        dofApertureOrig = depthOfField.aperture.value;
        dofFocalLenOrig = depthOfField.focalLength.value;
    }


    public void Update()
    {
        bool isPaused = (Time.timeScale == 0f);

        // 前フレームと違うときだけ処理する
        if (isPaused != wasPaused)
        {
            if (isPaused)
            {
                ApplyRealistic();
            }
            else
            {
                ResetEffects();
            }

            wasPaused = isPaused; // 状態更新
        }
    }
    /// <summary>
    /// オン／オフ切り替え
    /// </summary>
    public void ToggleRealistic()
    {
        if (isOn)
            ResetEffects();
        else
            ApplyRealistic();

        isOn = !isOn;
    }

    /// <summary>
    /// リアリスティック効果をオンにする
    /// </summary>
    public void ApplyRealistic()
    {
        StopAllCoroutines();
        StartCoroutine(LerpEffects(
            targetHueShift, targetSaturation, targetContrast,
            targetVignette, targetGrain, targetBloom, targetDistortion,
            targetFocusDist, targetAperture, targetFocalLen,
            tweenSeconds));
    }

    /// <summary>
    /// 元の状態に戻す
    /// </summary>
    public void ResetEffects()
    {
        StopAllCoroutines();
        StartCoroutine(LerpEffects(
            hueOrig, satOrig, conOrig,
            vigOrig, graOrig, bloOrig, disOrig,
            dofFocusOrig, dofApertureOrig, dofFocalLenOrig,
            tweenSeconds));
    }

    IEnumerator LerpEffects(
        float hue, float sat, float con,
        float vig, float gra, float blo, float dis,
        float dofFocus, float dofAperture, float dofFocal,
        float dur)
    {
        float t = 0f;
        float h0 = colorGrading.hueShift.value;
        float s0 = colorGrading.saturation.value;
        float c0 = colorGrading.contrast.value;
        float v0 = vignette.intensity.value;
        float g0 = grain.intensity.value;
        float b0 = bloom.intensity.value;
        float d0 = lensDistortion.intensity.value;
        float f0 = depthOfField.focusDistance.value;
        float a0 = depthOfField.aperture.value;
        float l0 = depthOfField.focalLength.value;

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dur);

            colorGrading.hueShift.Override(Mathf.Lerp(h0, hue, k));
            colorGrading.saturation.Override(Mathf.Lerp(s0, sat, k));
            colorGrading.contrast.Override(Mathf.Lerp(c0, con, k));
            vignette.intensity.Override(Mathf.Lerp(v0, vig, k));
            grain.intensity.Override(Mathf.Lerp(g0, gra, k));
            bloom.intensity.Override(Mathf.Lerp(b0, blo, k));
            lensDistortion.intensity.Override(Mathf.Lerp(d0, dis, k));
            depthOfField.focusDistance.Override(Mathf.Lerp(f0, dofFocus, k));
            depthOfField.aperture.Override(Mathf.Lerp(a0, dofAperture, k));
            depthOfField.focalLength.Override(Mathf.Lerp(l0, dofFocal, k));

            yield return null;
        }

        // スナップ
        colorGrading.hueShift.Override(hue);
        colorGrading.saturation.Override(sat);
        colorGrading.contrast.Override(con);
        vignette.intensity.Override(vig);
        grain.intensity.Override(gra);
        bloom.intensity.Override(blo);
        lensDistortion.intensity.Override(dis);
        depthOfField.focusDistance.Override(dofFocus);
        depthOfField.aperture.Override(dofAperture);
        depthOfField.focalLength.Override(dofFocal);
    }
}
