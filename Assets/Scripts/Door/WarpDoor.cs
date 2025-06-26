using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WarpDoor : MonoBehaviour
{
    public string stageName;
    public StageManager stageMgr;
    public bool stopLoad = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {

            //ƒV[ƒ“‘JˆÚ‚µ‚½‚Æ‚«‚É‚à‚µDontDestroyOnLoad‚É‰½‚©c‚Á‚Ä‚½‚çíœ
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
