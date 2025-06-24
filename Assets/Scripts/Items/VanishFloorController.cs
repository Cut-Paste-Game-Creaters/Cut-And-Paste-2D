using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VanishFloor : MonoBehaviour
{
    public float stayTime = 2.0f; //消えるまでの時間
    SpriteRenderer sprite;
    
    //自身の幅
    //float width;
    float height;

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
        
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if(other.gameObject.tag == "Player")
        {
            if(other.gameObject.transform.position.y > this.transform.position.y + (height / 2))
            StartCoroutine(DestroySelf());
        }
    }

    IEnumerator DestroySelf()
    {
        for ( int i = 0 ; i < 255 ; i++ ){
            sprite.material.color = sprite.material.color - new Color32(0,0,0,1);
            yield return new WaitForSeconds(stayTime / 255);
        }
        Destroy(this.gameObject); //自身を削除
        Debug.Log("自身を削除しました.");
    }
}
