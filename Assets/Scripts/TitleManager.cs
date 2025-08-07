using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] GameObject noteBook;
    [SerializeField] float rotSpeed = 1.0f;
    [SerializeField] Sprite frontImage;
    [SerializeField] Sprite backImage;

    private RectTransform noteTransform;
    private bool isAnimStart = false;
    private Image noteImage;

    void Start()
    {
        noteTransform = noteBook.GetComponent<RectTransform>();
        noteImage = noteBook.transform.GetChild(0).GetComponent<Image>();
        noteImage.sprite=frontImage;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            noteTransform.localEulerAngles = new Vector3(0, 0, 0);
            isAnimStart = true;
        }

        if(isAnimStart)
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
                isAnimStart = false;
            }
            Debug.Log("rotY:" + noteTransform.localEulerAngles.y);
        }
    }
}
