using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] GameObject noteBook;
    [SerializeField] float rotSpeed = 1.0f;
    [SerializeField] float camTime = 3.0f;
    [SerializeField] Sprite frontImage;
    [SerializeField] Sprite backImage;

    private RectTransform noteTransform;
    private bool isAnimStart = false;
    private int animState = 0;
    private Image noteImage;
    private float camSize = 2.0f;
    private float timeElapsed = 0f;
    private FadeScreen fadeScreen;

    void Start()
    {
        noteTransform = noteBook.GetComponent<RectTransform>();
        noteImage = noteBook.transform.GetChild(0).GetComponent<Image>();
        noteImage.sprite=frontImage;
        fadeScreen = GetComponent<FadeScreen>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            noteTransform.localEulerAngles = new Vector3(0, 0, 0);
            animState = 1;
            timeElapsed = 0.0f;
        }

        if(animState==1)
        {
            noteTransform.localEulerAngles = new Vector3(0, noteTransform.localEulerAngles.y + rotSpeed, 0);
            if(noteTransform.localEulerAngles.y > 90.0f)
            {
                noteImage.sprite = backImage;
            }
            else
            {
                noteImage.sprite = frontImage;
            }
            if (noteTransform.localEulerAngles.y > 180.0f)
            {
                animState = 2;
                fadeScreen.StartFadeOut("StageSelectScene");
            }
        }
        else if (animState == 2)
        {
            //cameraのsizeを5から2にする
            //同時にフェードアウトする
            timeElapsed += Time.unscaledDeltaTime;

            Camera.main.orthographicSize = Mathf.Lerp(5.0f, camSize, timeElapsed/camTime);
        }

    }
}
