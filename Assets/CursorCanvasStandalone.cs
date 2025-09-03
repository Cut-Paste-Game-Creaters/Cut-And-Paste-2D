using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static UnityEngine.CullingGroup;

/// <summary>
/// 単体で動くカーソル用Canvasコントローラ
/// - 自分で StageManager を取得（シーン切替にも追従）
/// - isSelectZone / isPasting を監視して Copy / Paste / Default を自動切替
/// - 表示中のカーソルのみマウス追従（LateUpdate）
/// - Canvasは最前面（sortingOrder）& Raycast 無効でクリックをブロックしない
/// </summary>
[RequireComponent(typeof(Canvas))]
public class CursorCanvasStandalone : MonoBehaviour
{
    [Header("UIカーソル（Canvas配下の RectTransform）")]
    [SerializeField] private RectTransform copyCursor;
    [SerializeField] private RectTransform pasteCursor;

    [Header("Canvas 設定")]
    [SerializeField] private int sortOrder = 10000;          // 最前面にしたい大きい値
    [SerializeField] private bool forceOverlay = true;        // 可能なら Overlay へ
    [SerializeField] private bool disableGraphicRaycaster = true; // クリックをブロックしない

    [Header("StageManager 自動取得")]
    [SerializeField] private bool autoFindStageManager = true;
    [SerializeField] private float retryFindInterval = 1.0f;  // 見つからない時の再探索間隔(秒)

    [Header("デバッグ")]
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

        // 常に最前面へ
        _canvas.overrideSorting = true;
        _canvas.sortingOrder = sortOrder;

        // Raycast を無効化（クリックをブロックしない）
        if (disableGraphicRaycaster)
        {
            var gr = GetComponent<GraphicRaycaster>();
            if (gr) gr.enabled = false;
        }
        DisableRaycastOnChildren(copyCursor);
        DisableRaycastOnChildren(pasteCursor);

        // 初期状態は OS カーソル
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
        // 念のため OSカーソルへ戻す
        SetState(CursorState.Default, force: true);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _stageManager = null;           // いったん無効化
        _nextFindTime = 0f;             // すぐ再探索
        if (autoFindStageManager) TryFindStageManager(now: true);
    }

    private void Update()
    {
        // StageManager の自動取得（見つからない場合は一定間隔で再探索）
        if (autoFindStageManager && _stageManager == null && Time.unscaledTime >= _nextFindTime)
        {
            TryFindStageManager(now: true);
        }

        // 監視して状態更新（変化時のみ切替）
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
            // StageManager が居ない間は常にデフォルト
            if (_state != CursorState.Default) SetState(CursorState.Default, force: true);
        }
    }

    private void LateUpdate()
    {
        // 表示中のカーソルだけマウス追従
        if (_state == CursorState.Default) return;

        RectTransform active = (_state == CursorState.Copy) ? copyCursor : pasteCursor;
        if (!active) return;

        // Overlay なら null、Camera/World なら canvas.worldCamera
        Camera cam = (_canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : _canvas.worldCamera;

        Vector2 localPos;
        RectTransform parent = active.parent as RectTransform;
        if (parent && RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, Input.mousePosition, cam, out localPos))
        {
            active.anchoredPosition = localPos; // pivot がクリック基準（ホットスポット）
        }
    }

    // 既存: SetState をこの順番に少しだけ整える
    private void SetState(CursorState newState, bool force, bool silent = false)
    {
        if (!force && newState == _state) return;

        var prev = _state;

        if (!silent) OnExitState(prev);   // ← 前の状態を抜ける瞬間に一度だけ

        // いったん全部消す
        if (copyCursor) copyCursor.gameObject.SetActive(false);
        if (pasteCursor) pasteCursor.gameObject.SetActive(false);

        // 見た目/OSカーソル切替
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

        if (!silent) OnEnterState(newState); // ← 新しい状態に入った瞬間に一度だけ
    }



    private void TryFindStageManager(bool now)
    {
        _stageManager = FindObjectOfType<StageManager>(includeInactive: true);
        if (debugLog) Debug.Log($"[CursorCanvasStandalone] StageManager found: {_stageManager}");

        if (_stageManager == null)
        {
            // 次回再探索時刻
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
        // 例) 状態から抜ける瞬間に一度だけ
        // if (prev == CursorState.Copy)  Debug.Log("Copyを抜けた瞬間だけ実行");
        if (prev == CursorState.Copy)
        {

        }
    }
    private void OnEnterState(CursorState next)
    {
        // 例) 新しい状態に入った瞬間に一度だけ
        // switch (next)
        // {
        //     case CursorState.Copy:  audioSource.PlayOneShot(copySfx); break;
        //     case CursorState.Paste: audioSource.PlayOneShot(pasteSfx); break;
        //     case CursorState.Default: /* 何か一度だけ */ break;
        // }
        switch (next)
        {
            case CursorState.Copy: break;
            case CursorState.Paste: break;
            case CursorState.Default: /* 何か一度だけ */ break;
        }
    }
}


