using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    [Header("UI�J�[�\���iCanvas�z���j")]
    public RectTransform CopycursorImage;
    public RectTransform PastecursorImage;

    private Canvas cursorCanvas;

    private enum CursorState { Default, Copy, Paste }
    private CursorState currentState = CursorState.Default;

    void Start()
    {
        cursorCanvas = CopycursorImage ? CopycursorImage.GetComponentInParent<Canvas>() : null;

        // �������i�S��������OS�J�[�\���\���j
        SetState(CursorState.Default, force: true);
    }

    void OnDisable()
    {
        // �j���E���������̕ی��iOS�J�[�\����߂��j
        SetState(CursorState.Default, force: true);
    }

    void LateUpdate()
    {
        // �\�����̃J�[�\���̂݃}�E�X�Ǐ]�iDefault���͉������Ȃ��j
        if (currentState == CursorState.Default) return;

        RectTransform active = (currentState == CursorState.Copy) ? CopycursorImage : PastecursorImage;
        if (!active) return;

        // Overlay�Ȃ�null�ACamera/World�Ȃ�J�������g��
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
    /// �Ď����iGameUIController ���j���疈�t���[���Ăяo����OK�B
    /// �����ŏ�Ԃ��ς�����Ƃ������ؑ֏������s���܂��B
    /// �D�揇�ʁFSelectZone(�R�s�[) > Pasting(�y�[�X�g) > �ʏ�
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

        // ��������S��\��
        if (CopycursorImage) CopycursorImage.gameObject.SetActive(false);
        if (PastecursorImage) PastecursorImage.gameObject.SetActive(false);

        switch (newState)
        {
            case CursorState.Default:
                Cursor.visible = true;   // OS�J�[�\��������
                break;
            case CursorState.Copy:
                Cursor.visible = false;  // OS�J�[�\���B�� �� UI�J�[�\���\��
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
