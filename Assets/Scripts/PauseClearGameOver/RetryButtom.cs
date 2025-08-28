using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetryButtom : MonoBehaviour
{
    private GameOverFunction gameOverFunc;
    private UndoRedoFunction undoRedoFunc;
    private Vector3 originalScale;
    // Start is called before the first frame update
    void Start()
    {
        originalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseEnter()
    {
        // �G�ꂽ�班���傫��
        transform.localScale = originalScale * 1.1f;
    }

    private void OnMouseExit()
    {
        // ���ꂽ�猳�ɖ߂�
        transform.localScale = originalScale;
    }
    private void OnMouseDown()
    {
        if (gameOverFunc == null) gameOverFunc = FindObjectOfType<GameOverFunction>();
        if (undoRedoFunc == null) undoRedoFunc = FindObjectOfType<UndoRedoFunction>();

        if (gameOverFunc != null)
        {
            gameOverFunc.PauseOff();
            undoRedoFunc.Retry();
        }
        else
        {
            Debug.LogWarning("GameOverFunction ��������܂���ł����I");
        }
    }
}
