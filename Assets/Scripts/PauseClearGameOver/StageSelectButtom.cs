using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageSelectButtom : MonoBehaviour
{
    private GameOverFunction gameOverFunc;
    private Vector3 originalScale;
    // Start is called before the first frame update
    void Start()
    {
        gameOverFunc = FindObjectOfType<GameOverFunction>();
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
        if (gameOverFunc != null)
        {
            gameOverFunc.LoadStageSelect();
        }
        else
        {
            Debug.LogWarning("GameOverFunction ��������܂���ł����I");
        }
    }
}

