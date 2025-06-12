using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseInStage : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    // Start is called before the first frame update
    void Start()
    {
        canvas.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position =
            new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z + 1);
        if (PlayerInput.GetKeyDown(KeyCode.Escape))
        {
            canvas.enabled = true;
            PlayerInput.isPausing = true;
            Time.timeScale = 0f;
        }
    }

    public void PauseOff()
    {
        canvas.enabled = false;
        PlayerInput.isPausing = false;
        Time.timeScale = 1.0f;
    }
}
