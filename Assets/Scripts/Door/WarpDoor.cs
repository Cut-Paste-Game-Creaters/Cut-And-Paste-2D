using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WarpDoor : MonoBehaviour
{
    public string stageName;
    public StageManager stageMgr;
    public bool stopLoad = false;

    //コンストラクタ
    public WarpDoor(WarpDoor wpDoor)
    {
        stageName = wpDoor.stageName;
        stageMgr = wpDoor.stageMgr;
        stopLoad = wpDoor.stopLoad;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" && Input.GetKeyDown(KeyCode.W))
        {

            //シーン遷移したときにもしDontDestroyOnLoadに何か残ってたら削除
            foreach (var obj in stageMgr.EraseObjects)
            {
                Destroy(obj);
            }
            stageMgr.EraseObjects = new List<GameObject>();
            if (stopLoad) return;
            SceneManager.LoadScene(stageName);
        }
    }
}
