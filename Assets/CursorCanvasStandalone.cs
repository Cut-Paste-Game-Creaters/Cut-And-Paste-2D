using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static UnityEngine.CullingGroup;

/// <summary>
/// �P�̂œ����J�[�\���pCanvas�R���g���[��
/// - ������ StageManager ���擾�i�V�[���ؑւɂ��Ǐ]�j
/// - isSelectZone / isPasting ���Ď����� Copy / Paste / Default �������ؑ�
/// - �\�����̃J�[�\���̂݃}�E�X�Ǐ]�iLateUpdate�j
/// - Canvas�͍őO�ʁisortingOrder�j& Raycast �����ŃN���b�N���u���b�N���Ȃ�
/// </summary>
[RequireComponent(typeof(Canvas))]
public class CursorCanvasStandalone : MonoBehaviour
{
    [Header("UI�J�[�\���iCanvas�z���� RectTransform�j")]
    [SerializeField] private RectTransform copyCursor;
    [SerializeField] private RectTransform pasteCursor;

    [Header("Canvas �ݒ�")]
    [SerializeField] private int sortOrder = 10000;          // �őO�ʂɂ������傫���l
    [SerializeField] private bool forceOverlay = true;        // �\�Ȃ� Overlay ��
    [SerializeField] private bool disableGraphicRaycaster = true; // �N���b�N���u���b�N���Ȃ�

    [Header("StageManager �����擾")]
    [SerializeField] private bool autoFindStageManager = true;
    [SerializeField] private float retryFindInterval = 1.0f;  // ������Ȃ����̍ĒT���Ԋu(�b)

    [Header("�f�o�b�O")]
    [SerializeField] private bool debugLog = false;

    private Canvas _canvas;
    private StageManager _stageManager;
    private float _nextFindTime;

    private enum CursorState { Default, Copy, Paste }
    private CursorState _state = CursorState.Default;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();

        if (forceOverlay) _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // ��ɍőO�ʂ�
        _canvas.overrideSorting = true;
        _canvas.sortingOrder = sortOrder;

        // Raycast �𖳌����i�N���b�N���u���b�N���Ȃ��j
        if (disableGraphicRaycaster)
        {
            var gr = GetComponent<GraphicRaycaster>();
            if (gr) gr.enabled = false;
        }
        DisableRaycastOnChildren(copyCursor);
        DisableRaycastOnChildren(pasteCursor);

        // ������Ԃ� OS �J�[�\��
        SetState(CursorState.Default, force: true);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        if (autoFindStageManager) TryFindStageManager(now: true);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        // �O�̂��� OS�J�[�\���֖߂�
        SetState(CursorState.Default, force: true);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _stageManager = null;           // �������񖳌���
        _nextFindTime = 0f;             // �����ĒT��
        if (autoFindStageManager) TryFindStageManager(now: true);
    }

    private void Update()
    {
        // StageManager �̎����擾�i������Ȃ��ꍇ�͈��Ԋu�ōĒT���j
        if (autoFindStageManager && _stageManager == null && Time.unscaledTime >= _nextFindTime)
        {
            TryFindStageManager(now: true);
        }

        // �Ď����ď�ԍX�V�i�ω����̂ݐؑցj
        if (_stageManager != null)
        {
            bool isSelect = _stageManager.isSelectZone;
            bool isPaste = _stageManager.isPasting;

            CursorState next =
                isSelect ? CursorState.Copy :
                isPaste ? CursorState.Paste :
                           CursorState.Default;

            if (next != _state) SetState(next, force: false);
        }
        else
        {
            // StageManager �����Ȃ��Ԃ͏�Ƀf�t�H���g
            if (_state != CursorState.Default) SetState(CursorState.Default, force: true);
        }
    }

    private void LateUpdate()
    {
        // �\�����̃J�[�\�������}�E�X�Ǐ]
        if (_state == CursorState.Default) return;

        RectTransform active = (_state == CursorState.Copy) ? copyCursor : pasteCursor;
        if (!active) return;

        // Overlay �Ȃ� null�ACamera/World �Ȃ� canvas.worldCamera
        Camera cam = (_canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : _canvas.worldCamera;

        Vector2 localPos;
        RectTransform parent = active.parent as RectTransform;
        if (parent && RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, Input.mousePosition, cam, out localPos))
        {
            active.anchoredPosition = localPos; // pivot ���N���b�N��i�z�b�g�X�|�b�g�j
        }
    }

    // ����: SetState �����̏��Ԃɏ�������������
    private void SetState(CursorState newState, bool force, bool silent = false)
    {
        if (!force && newState == _state) return;

        var prev = _state;

        if (!silent) OnExitState(prev);   // �� �O�̏�Ԃ𔲂���u�ԂɈ�x����

        // ��������S������
        if (copyCursor) copyCursor.gameObject.SetActive(false);
        if (pasteCursor) pasteCursor.gameObject.SetActive(false);

        // ������/OS�J�[�\���ؑ�
        switch (newState)
        {
            case CursorState.Default:
                Cursor.visible = true;
                break;
            case CursorState.Copy:
                Cursor.visible = false;
                SEManager.instance.ClipAtPointSE(SEManager.instance.kachaSE);
                if (copyCursor) copyCursor.gameObject.SetActive(true);
                break;
            case CursorState.Paste:
                Cursor.visible = false;
                if (pasteCursor) pasteCursor.gameObject.SetActive(true);
                break;
        }

        _state = newState;

        if (!silent) OnEnterState(newState); // �� �V������Ԃɓ������u�ԂɈ�x����
    }



    private void TryFindStageManager(bool now)
    {
        _stageManager = FindObjectOfType<StageManager>(includeInactive: true);
        if (debugLog) Debug.Log($"[CursorCanvasStandalone] StageManager found: {_stageManager}");

        if (_stageManager == null)
        {
            // ����ĒT������
            _nextFindTime = Time.unscaledTime + Mathf.Max(0.2f, retryFindInterval);
        }
    }

    private void DisableRaycastOnChildren(RectTransform root)
    {
        if (!root) return;
        var graphics = root.GetComponentsInChildren<Graphic>(true);
        foreach (var g in graphics) g.raycastTarget = false;
    }

    private void OnExitState(CursorState prev)
    {
        // ��) ��Ԃ��甲����u�ԂɈ�x����
        // if (prev == CursorState.Copy)  Debug.Log("Copy�𔲂����u�Ԃ������s");
        if (prev == CursorState.Copy)
        {

        }
    }
    private void OnEnterState(CursorState next)
    {
        // ��) �V������Ԃɓ������u�ԂɈ�x����
        // switch (next)
        // {
        //     case CursorState.Copy:  audioSource.PlayOneShot(copySfx); break;
        //     case CursorState.Paste: audioSource.PlayOneShot(pasteSfx); break;
        //     case CursorState.Default: /* ������x���� */ break;
        // }
        switch (next)
        {
            case CursorState.Copy: break;
            case CursorState.Paste: break;
            case CursorState.Default: /* ������x���� */ break;
        }
    }
}


