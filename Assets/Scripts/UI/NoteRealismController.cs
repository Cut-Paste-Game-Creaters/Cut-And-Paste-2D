using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class NoteRealismController : MonoBehaviour
{
    [Header("Target Values (色調)")]
    public float targetHueShift = 10f;
    public float targetSaturation = 20f;          // VHSっぽくするなら負値推奨（-15 ～ -20）
    public float targetContrast = 10f;
    public float targetPostExposure = 0f;         // 露出微調整
    public Color targetColorFilter = Color.white; // 全体色味フィルタ

    [Header("Target Values (レンズ/歪み/粒状)")]
    public float targetVignette = 0.30f;
    public float targetVignetteSmoothness = 0.90f;

    public float targetGrain = 0.70f;
    public float targetGrainSize = 1.00f;
    public bool targetGrainColored = true;

    public float targetBloom = 3.20f;
    public float targetBloomThreshold = 1.05f;
    public Texture targetBloomDirtTex;
    public float targetBloomDirtIntensity = 0.20f;

    public float targetDistortion = -14f;         // VHS/CRT感は負値（樽型）推奨
    public Vector2 targetDistortionCenter = new Vector2(0.5f, 0.5f);
    public float targetDistortionScale = 1.0f;

    public float targetChromAb = 0.22f;           // 色ズレ

    [Header("Target Values (被写界深度)")]
    public float targetFocusDist = 4f;
    public float targetAperture = 10f;
    public float targetFocalLen = 50f;

    [Header("Target Values (任意: 自動露出)")]
    public float targetExposureKeyValue = 1.0f;   // 弱く動かすとフリッカー風

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
    ChromaticAberration chromAb;
    AutoExposure autoExposure;

    // Original values
    float hueOrig, satOrig, conOrig, postExpOrig;
    Color colorFilterOrig;

    float vigOrig, vigSmoothOrig;
    float graOrig, graSizeOrig; bool graColoredOrig;

    float bloOrig, bloThOrig, bloDirtIntOrig; Texture bloDirtTexOrig;

    float disOrig; Vector2 disCenterOrig; float disScaleOrig;

    float dofFocusOrig, dofApertureOrig, dofFocalLenOrig;

    float caOrig;
    float exposureKeyOrig;

    bool isOn = false; // トグル状態
    bool wasPaused = false; // 演出制御用

    void Awake()
    {
        if (!postProcessVolume) postProcessVolume = FindObjectOfType<PostProcessVolume>();
        if (!postProcessVolume || postProcessVolume.profile == null)
        {
            Debug.LogWarning("PostProcessVolumeが見つからないか、Profile未設定です。");
            enabled = false;
            return;
        }

        var p = postProcessVolume.profile;

        // 取得 or 追加
        if (!p.TryGetSettings(out colorGrading)) colorGrading = p.AddSettings<ColorGrading>();
        if (!p.TryGetSettings(out vignette)) vignette = p.AddSettings<Vignette>();
        if (!p.TryGetSettings(out grain)) grain = p.AddSettings<Grain>();
        if (!p.TryGetSettings(out bloom)) bloom = p.AddSettings<Bloom>();
        if (!p.TryGetSettings(out lensDistortion)) lensDistortion = p.AddSettings<LensDistortion>();
        if (!p.TryGetSettings(out depthOfField)) depthOfField = p.AddSettings<DepthOfField>();
        // 追加要素
        if (!p.TryGetSettings(out chromAb)) chromAb = p.AddSettings<ChromaticAberration>();
        p.TryGetSettings(out autoExposure); // 任意。無ければnullのまま

        // Override有効化（必要分だけ）
        colorGrading.hueShift.overrideState = true;
        colorGrading.saturation.overrideState = true;
        colorGrading.contrast.overrideState = true;
        colorGrading.postExposure.overrideState = true;
        colorGrading.colorFilter.overrideState = true;

        vignette.intensity.overrideState = true;
        vignette.smoothness.overrideState = true;

        grain.intensity.overrideState = true;
        grain.size.overrideState = true;
        grain.colored.overrideState = true;

        bloom.intensity.overrideState = true;
        bloom.threshold.overrideState = true;
        bloom.dirtIntensity.overrideState = true;
        // dirtTexture は override フラグ無し（参照差し替えでOK）

        lensDistortion.intensity.overrideState = true;
        lensDistortion.centerX.overrideState = true;
        lensDistortion.centerY.overrideState = true;
        lensDistortion.scale.overrideState = true;

        depthOfField.focusDistance.overrideState = true;
        depthOfField.aperture.overrideState = true;
        depthOfField.focalLength.overrideState = true;

        if (chromAb) chromAb.intensity.overrideState = true;
        if (autoExposure) autoExposure.keyValue.overrideState = true;

        // 元の値保存
        hueOrig = colorGrading.hueShift.value;
        satOrig = colorGrading.saturation.value;
        conOrig = colorGrading.contrast.value;
        postExpOrig = colorGrading.postExposure.value;
        colorFilterOrig = colorGrading.colorFilter.value;

        vigOrig = vignette.intensity.value;
        vigSmoothOrig = vignette.smoothness.value;

        graOrig = grain.intensity.value;
        graSizeOrig = grain.size.value;
        graColoredOrig = grain.colored.value;

        bloOrig = bloom.intensity.value;
        bloThOrig = bloom.threshold.value;
        bloDirtIntOrig = bloom.dirtIntensity.value;
        bloDirtTexOrig = bloom.dirtTexture.value;

        disOrig = lensDistortion.intensity.value;
        disCenterOrig = new Vector2(lensDistortion.centerX.value, lensDistortion.centerY.value);
        disScaleOrig = lensDistortion.scale.value;

        dofFocusOrig = depthOfField.focusDistance.value;
        dofApertureOrig = depthOfField.aperture.value;
        dofFocalLenOrig = depthOfField.focalLength.value;

        caOrig = chromAb ? chromAb.intensity.value : 0f;
        exposureKeyOrig = autoExposure ? autoExposure.keyValue.value : 1.0f;
    }

    void Update()
    {
        // ポーズに合わせて自動ON/OFF（必要なければ削ってOK）
        bool isPaused = (Time.timeScale == 0f);
        if (isPaused != wasPaused)
        {
            if (isPaused) ApplyRealistic();
            else ResetEffects();
            wasPaused = isPaused;
        }
    }

    /// <summary>手動トグル</summary>
    public void ToggleRealistic()
    {
        if (isOn) ResetEffects();
        else ApplyRealistic();
        isOn = !isOn;
    }

    /// <summary>ターゲットへ適用</summary>
    public void ApplyRealistic()
    {
        StopAllCoroutines();
        StartCoroutine(LerpEffects(
            // ColorGrading
            targetHueShift, targetSaturation, targetContrast, targetPostExposure, targetColorFilter,
            // Vignette
            targetVignette, targetVignetteSmoothness,
            // Grain
            targetGrain, targetGrainSize, targetGrainColored,
            // Bloom
            targetBloom, targetBloomThreshold, targetBloomDirtTex, targetBloomDirtIntensity,
            // Distortion
            targetDistortion, targetDistortionCenter, targetDistortionScale,
            // DOF
            targetFocusDist, targetAperture, targetFocalLen,
            // CA
            targetChromAb,
            // AutoExposure
            targetExposureKeyValue,
            tweenSeconds
        ));
    }

    /// <summary>元へ戻す</summary>
    public void ResetEffects()
    {
        StopAllCoroutines();
        StartCoroutine(LerpEffects(
            // ColorGrading
            hueOrig, satOrig, conOrig, postExpOrig, colorFilterOrig,
            // Vignette
            vigOrig, vigSmoothOrig,
            // Grain
            graOrig, graSizeOrig, graColoredOrig,
            // Bloom
            bloOrig, bloThOrig, bloDirtTexOrig, bloDirtIntOrig,
            // Distortion
            disOrig, disCenterOrig, disScaleOrig,
            // DOF
            dofFocusOrig, dofApertureOrig, dofFocalLenOrig,
            // CA
            caOrig,
            // AutoExposure
            exposureKeyOrig,
            tweenSeconds
        ));
    }

    IEnumerator LerpEffects(
        // ColorGrading
        float hue, float sat, float con, float postExp, Color colorFilter,
        // Vignette
        float vig, float vigSmooth,
        // Grain
        float gra, float graSize, bool graColored,
        // Bloom
        float blo, float bloTh, Texture bloDirtTex, float bloDirtInt,
        // Distortion
        float dis, Vector2 disCenter, float disScale,
        // DOF
        float dofFocus, float dofAperture, float dofFocal,
        // Chromatic Aberration
        float ca,
        // AutoExposure
        float exposureKey,
        float dur)
    {
        float t = 0f;

        // 現在値を取得
        float h0 = colorGrading.hueShift.value;
        float s0 = colorGrading.saturation.value;
        float c0 = colorGrading.contrast.value;
        float pe0 = colorGrading.postExposure.value;
        Color cf0 = colorGrading.colorFilter.value;

        float v0 = vignette.intensity.value;
        float vs0 = vignette.smoothness.value;

        float g0 = grain.intensity.value;
        float gs0 = grain.size.value;
        bool gc0 = grain.colored.value;

        float b0 = bloom.intensity.value;
        float bt0 = bloom.threshold.value;
        float bdi0 = bloom.dirtIntensity.value;
        Texture bdt0 = bloom.dirtTexture.value;

        float d0 = lensDistortion.intensity.value;
        Vector2 dc0 = new Vector2(lensDistortion.centerX.value, lensDistortion.centerY.value);
        float dsc0 = lensDistortion.scale.value;

        float f0 = depthOfField.focusDistance.value;
        float a0 = depthOfField.aperture.value;
        float l0 = depthOfField.focalLength.value;

        float ca0 = chromAb ? chromAb.intensity.value : 0f;
        float ex0 = autoExposure ? autoExposure.keyValue.value : 1.0f;

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = dur > 0f ? Mathf.Clamp01(t / dur) : 1f;

            // ColorGrading
            colorGrading.hueShift.Override(Mathf.Lerp(h0, hue, k));
            colorGrading.saturation.Override(Mathf.Lerp(s0, sat, k));
            colorGrading.contrast.Override(Mathf.Lerp(c0, con, k));
            colorGrading.postExposure.Override(Mathf.Lerp(pe0, postExp, k));
            colorGrading.colorFilter.Override(Color.Lerp(cf0, colorFilter, k));

            // Vignette
            vignette.intensity.Override(Mathf.Lerp(v0, vig, k));
            vignette.smoothness.Override(Mathf.Lerp(vs0, vigSmooth, k));

            // Grain
            grain.intensity.Override(Mathf.Lerp(g0, gra, k));
            grain.size.Override(Mathf.Lerp(gs0, graSize, k));
            // colored は補間できないので閾値で切替（終端で確定でもOK）
            grain.colored.Override(k < 0.5f ? gc0 : graColored);

            // Bloom
            bloom.intensity.Override(Mathf.Lerp(b0, blo, k));
            bloom.threshold.Override(Mathf.Lerp(bt0, bloTh, k));
            bloom.dirtIntensity.Override(Mathf.Lerp(bdi0, bloDirtInt, k));
            // dirtTexture は段階で切替（終端で確定でもOK）
            bloom.dirtTexture.value = (k < 0.5f ? bdt0 : bloDirtTex);

            // Distortion
            lensDistortion.intensity.Override(Mathf.Lerp(d0, dis, k));
            lensDistortion.centerX.Override(Mathf.Lerp(dc0.x, disCenter.x, k));
            lensDistortion.centerY.Override(Mathf.Lerp(dc0.y, disCenter.y, k));
            lensDistortion.scale.Override(Mathf.Lerp(dsc0, disScale, k));

            // DOF
            depthOfField.focusDistance.Override(Mathf.Lerp(f0, dofFocus, k));
            depthOfField.aperture.Override(Mathf.Lerp(a0, dofAperture, k));
            depthOfField.focalLength.Override(Mathf.Lerp(l0, dofFocal, k));

            // Chromatic Aberration
            if (chromAb) chromAb.intensity.Override(Mathf.Lerp(ca0, ca, k));

            // AutoExposure
            if (autoExposure) autoExposure.keyValue.Override(Mathf.Lerp(ex0, exposureKey, k));

            yield return null;
        }

        // スナップ（最終値を確定）
        colorGrading.hueShift.Override(hue);
        colorGrading.saturation.Override(sat);
        colorGrading.contrast.Override(con);
        colorGrading.postExposure.Override(postExp);
        colorGrading.colorFilter.Override(colorFilter);

        vignette.intensity.Override(vig);
        vignette.smoothness.Override(vigSmooth);

        grain.intensity.Override(gra);
        grain.size.Override(graSize);
        grain.colored.Override(graColored);

        bloom.intensity.Override(blo);
        bloom.threshold.Override(bloTh);
        bloom.dirtIntensity.Override(bloDirtInt);
        bloom.dirtTexture.value = bloDirtTex;

        lensDistortion.intensity.Override(dis);
        lensDistortion.centerX.Override(disCenter.x);
        lensDistortion.centerY.Override(disCenter.y);
        lensDistortion.scale.Override(disScale);

        depthOfField.focusDistance.Override(dofFocus);
        depthOfField.aperture.Override(dofAperture);
        depthOfField.focalLength.Override(dofFocal);

        if (chromAb) chromAb.intensity.Override(ca);
        if (autoExposure) autoExposure.keyValue.Override(exposureKey);
    }
}
