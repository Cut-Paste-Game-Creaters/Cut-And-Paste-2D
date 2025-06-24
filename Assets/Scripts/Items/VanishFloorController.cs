using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VanishFloor : MonoBehaviour
{
    public float stayTime = 2.0f; //消えるまでの時間
    SpriteRenderer sprite;
    Color32 color;
    
    //自身の幅
    //float width;
    float height;

    float elapsed = 0.0f;
    bool isCollision;

    // Start is called before the first frame update
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        //width = sprite.bounds.size.x;
        height = sprite.bounds.size.y;
    }

    // Update is called once per frame
    void Update()
    {
        if(isCollision)
        {
            elapsed += PlayerInput.GetDeltaTime();
            sprite.material.color = new Color32(255,255,255,(byte)(255 * (1 - (elapsed / stayTime))));
            if(elapsed >= stayTime)
            {
                isCollision = false;
                Destroy(this.gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if(other.gameObject.tag == "Player")
        {
            if(other.gameObject.transform.position.y > this.transform.position.y + (height / 2))
            isCollision = true;
        }
    }

    /*IEnumerator DestroySelf()
    {
        for ( int i = 0 ; i < 255 ; i++ ){
            sprite.material.color = sprite.material.color - new Color32(0,0,0,1);
            yield return new WaitForSeconds(stayTime / 255);
        }
        Destroy(this.gameObject); //自身を削除
        Debug.Log("自身を削除しました.");
    }*/
}
