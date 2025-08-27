using UnityEngine;

/// <summary>
/// ���@A�iSetEffect�j�p�FNoteRealismController �ɒl�𗬂����ރv���Z�b�g�Q
/// </summary>
public static class NoteRealismPresets
{
    public enum PresetType
    {
        // �x�[�X���C���i���ʂȂ��j
        Baseline,

        // �g�Q�[�����h�n
        ArcadeNeon,     // ���F���߁E�l�I��
        ActionBoost,    // �p���`�̂��鉉�o
        RetroCRT,       // ���g���^CRT��
        HorrorUnease,   // �s���E�z���[
        ComicPop,       // �R�~�b�N���E�p�L�b��
        SoftFilm,       // �f��I�Ƀ\�t�g
        TechNoir,       // ��ʓx�{���R���g���X�g
        MobileLight     // �y�ʁE�ėp
    }

    public static void ApplyPreset(NoteRealismController c, PresetType type)
    {
        if (c == null) return;

        // ���ʁF�t�F�[�h���ԁi�D�݂Œ����j
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
                c.targetDistortion = -8f;   // �M�^�Œ�������
                c.targetFocusDist = 10f;   // DoF�قڃI�t
                c.targetAperture = 5.6f;
                c.targetFocalLen = 50f;
                break;

            case PresetType.ActionBoost:
                c.targetSaturation = +10f;
                c.targetContrast = +20f;
                c.targetVignette = 0.45f; // �[���O�b�ƒ��߂�
                c.targetGrain = 0.10f;
                c.targetBloom = 3.0f;
                c.targetDistortion = -15f;
                c.targetFocusDist = 5f;    // �ق�̂��
                c.targetAperture = 2.8f;
                c.targetFocalLen = 60f;
                break;

            case PresetType.RetroCRT:
                c.targetSaturation = -5f;
                c.targetContrast = +12f;
                c.targetVignette = 0.35f;
                c.targetGrain = 0.25f; // ����
                c.targetBloom = 0.5f;
                c.targetDistortion = +6f;   // �N�b�V�����^
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
                c.targetFocusDist = 3f;    // �ڂ�����
                c.targetAperture = 1.8f;
                c.targetFocalLen = 40f;
                break;

            case PresetType.ComicPop:
                c.targetSaturation = +25f;  // �F���p�L�b��
                c.targetContrast = +25f;
                c.targetVignette = 0.10f;
                c.targetGrain = 0.0f;  // �N���[��
                c.targetBloom = 2.0f;  // �G�b�W�������
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
                c.targetBloom = 4.5f;  // �_�炩���n�C���C�g
                c.targetDistortion = 0f;
                c.targetFocusDist = 6f;
                c.targetAperture = 2.2f;
                c.targetFocalLen = 55f;
                break;

            case PresetType.TechNoir:
                c.targetSaturation = -20f;  // ��ʓx
                c.targetContrast = +30f;  // ���R���g���X�g
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
                c.targetFocusDist = 10f;   // DoF�I�t���i�y�ʁj
                c.targetAperture = 5.6f;
                c.targetFocalLen = 50f;
                break;
        }
    }
}

