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
        // êGÇÍÇΩÇÁè≠ÇµëÂÇ´Ç≠
        transform.localScale = originalScale * 1.1f;
    }

    private void OnMouseExit()
    {
        // ó£ÇÍÇΩÇÁå≥Ç…ñﬂÇ∑
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
            Debug.LogWarning("GameOverFunction Ç™å©Ç¬Ç©ÇËÇ‹ÇπÇÒÇ≈ÇµÇΩÅI");
        }
    }
}

