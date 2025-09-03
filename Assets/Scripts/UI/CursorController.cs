using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    [Header("UIカーソル（Canvas配下）")]
    public RectTransform CopycursorImage;
    public RectTransform PastecursorImage;

    private Canvas cursorCanvas;

    private enum CursorState { Default, Copy, Paste }
    private CursorState currentState = CursorState.Default;

    void Start()
    {
        cursorCanvas = CopycursorImage ? CopycursorImage.GetComponentInParent<Canvas>() : null;

        // 初期化（全部消してOSカーソル表示）
        SetState(CursorState.Default, force: true);
    }

    void OnDisable()
    {
        // 破棄・無効化時の保険（OSカーソルを戻す）
        SetState(CursorState.Default, force: true);
    }

    void LateUpdate()
    {
        // 表示中のカーソルのみマウス追従（Default時は何もしない）
        if (currentState == CursorState.Default) return;

        RectTransform active = (currentState == CursorState.Copy) ? CopycursorImage : PastecursorImage;
        if (!active) return;

        // Overlayならnull、Camera/Worldならカメラを使う
        Camera cam = (cursorCanvas && cursorCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            ? null : cursorCanvas ? cursorCanvas.worldCamera : null;

        Vector2 localPos;
        RectTransform parent = active.parent as RectTransform;
        if (parent && RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, Input.mousePosition, cam, out localPos))
        {
            active.anchoredPosition = localPos;
        }
    }

    /// <summary>
    /// 監視側（GameUIController 等）から毎フレーム呼び出してOK。
    /// 内部で状態が変わったときだけ切替処理を行います。
    /// 優先順位：SelectZone(コピー) > Pasting(ペースト) > 通常
    /// </summary>
    public void UpdateCursor(bool isSelectZone, bool isPasting)
    {
        CursorState next =
            isSelectZone ? CursorState.Copy :
            isPasting ? CursorState.Paste :
                           CursorState.Default;

        if (next != currentState)
        {
            SetState(next, force: false);
        }
    }

    private void SetState(CursorState newState, bool force)
    {
        if (!force && newState == currentState) return;

        // いったん全非表示
        if (CopycursorImage) CopycursorImage.gameObject.SetActive(false);
        if (PastecursorImage) PastecursorImage.gameObject.SetActive(false);

        switch (newState)
        {
            case CursorState.Default:
                Cursor.visible = true;   // OSカーソル見せる
                break;
            case CursorState.Copy:
                Cursor.visible = false;  // OSカーソル隠す → UIカーソル表示
                if (CopycursorImage) CopycursorImage.gameObject.SetActive(true);
                break;
            case CursorState.Paste:
                Cursor.visible = false;
                if (PastecursorImage) PastecursorImage.gameObject.SetActive(true);
                break;
        }

        currentState = newState;
    }
}
