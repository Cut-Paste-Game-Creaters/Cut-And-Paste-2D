using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player_Function : MonoBehaviour
{
    private GameOverFunction gameOverFunc;
    private ClearFunction clearFunc;
    StageManager stageMgr;

    private Tilemap tilemap;
    private float fallingLine_y;
    private SpriteRenderer sr;

    public int fallLine_FromMin = 10;
    //private RigidBody rb;

    private float fadeDuration = 0.6f; // 消えるまでの時間（秒）

    // Start is called before the first frame update
    void Start()
    {
        //rb = GetComponent<RigidBody>();
        gameOverFunc = FindObjectOfType<GameOverFunction>();
        clearFunc = FindObjectOfType<ClearFunction>();
        stageMgr = FindObjectOfType<StageManager>();
        sr = GetComponent<SpriteRenderer>();

        Tilemap[] maps = FindObjectsOfType<Tilemap>();
        foreach (var map in maps)
        {
            if (map.gameObject.tag == "Tilemap")
            {
                tilemap = map;
                break;
            }
        }
        fallingLine_y = tilemap.cellBounds.min.y - fallLine_FromMin;
    }

    // Update is called once per frame
    void Update()
    {
        if (stageMgr == null) stageMgr = FindObjectOfType<StageManager>();
        if (stageMgr.isPlayerDead == false)
        {
            Color currentColor = sr.color;
            currentColor.a = stageMgr.isPlayerDamaged ? 0.5f : 1.0f;
            sr.color = currentColor;
        }
        /*テスト用に4ボタン押すと死んだことになる 本来はhpが0 もしくは　ステージから落ちたら*/
        if(this.gameObject.transform.position.y < fallingLine_y || stageMgr.player_HP <= 0)
        {

            stageMgr.player_HP = 0;
            if (!stageMgr.isPlayerDead)
            {
                StartCoroutine(Die()); ;
                if (this.gameObject.transform.position.y < fallingLine_y)
                {
                    SEManager.instance.ClipAtPointSE(SEManager.instance.diveSE);
                }
            }
        }
    }

    /*
    #攻撃処理
    void AttackToEnemy()
    {

    }
    */

    /*
    #ダメージのあるものに衝突したらhpを減らす処理
    
    */

    //死亡処理
    IEnumerator Die()
    {
        //ゲームオーバーファンクション呼び出し
        stageMgr.isPlayerDead = true;
        if(gameOverFunc==null)gameOverFunc = FindObjectOfType<GameOverFunction>();

        

        StartCoroutine(gameOverFunc.GameOver());
        yield return new WaitForSecondsRealtime(1f);
        StartCoroutine(FadeOut());
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if(other.gameObject.tag == "Goal")
        {
            StartCoroutine(clearFunc.GameClear());
        }
    }



    public IEnumerator FadeOut(float duration = -1f)
    {
        if (duration <= 0) duration = fadeDuration;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // TimeScale=0でも進む
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);

            if (sr != null)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }

            yield return null;
        }

        // 最終的に完全に透明に
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
        }
    }

    /// <summary>
    /// フェードアウト後に見た目をリセット（PauseOffで使う）
    /// </summary>
    public void ResetAlpha()
    {
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
    }

}
