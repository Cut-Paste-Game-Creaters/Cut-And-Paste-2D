using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ClearFunction : MonoBehaviour
{
    private Canvas canvas;
    private bool isClear = false;
    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponent<Canvas>();
        canvas.enabled = false;
        isClear = false;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position =
            new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z + 1);
    }

    public bool GetisClear()
    {
        return isClear;
    }

    public void GameClear()
    {
        canvas.enabled = true;
        PlayerInput.isPausing = true;
        isClear = true;
        Time.timeScale = 0f;
    }

    public void PauseOff()
    {
        canvas.enabled = false;
        PlayerInput.isPausing = false;
        Time.timeScale = 1.0f;
    }

    public void LoadStageSelect()
    {
        PauseOff();
        SceneManager.LoadScene("StageSelectScene");
    }
}
