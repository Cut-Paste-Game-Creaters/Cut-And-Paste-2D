using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverFunction : MonoBehaviour
{
    private Canvas canvas;
    private GameObject player;
    private Vector3 initPlayerPos; private FadeScreen fadeCurtain;

    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponent<Canvas>();
        player = GameObject.FindWithTag("Player");
        initPlayerPos = player.transform.position;
        canvas.enabled = false;
        fadeCurtain = FindObjectOfType<FadeScreen>();
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position =
            new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z + 1);
    }

    public void GameOver()
    {
        SEManager.instance.ClipAtPointSE(SEManager.instance.deathbiribiriSE);
        SEManager.instance.ClipAtPointSE(SEManager.instance.deathyoinSE);
        canvas.enabled = true;
        PlayerInput.isPausing = true;
        Time.timeScale = 0f;
    }

    public void PauseOff()
    {
        player.transform.position = initPlayerPos;
        canvas.enabled = false;
        PlayerInput.isPausing = false;
        Time.timeScale = 1.0f;
    }

    public void LoadStageSelect()
    {
        PauseOff();
        fadeCurtain.StartFadeOut("StageSelectScene");
        //SceneManager.LoadScene("StageSelectScene");
    }
}
