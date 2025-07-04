using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


public class CaptureCopyZone : MonoBehaviour
{
    private Rect captureArea = new Rect(100, 100, 300, 200); // x, y, width, height
    private Camera cam;
    private int originalMask;

    [SerializeField] Image image;

    void Start()
    {
        image.enabled = false; 
    }

    public void CaptureImage(Vector3 startPos, Vector3 endPos)
    {
        int dir = culcDir(startPos,endPos);

        /*
         ①Startposとendposを引数で取得する
        ②それをdirによって位置を調整する
        ここでChangeVecToIntと4方向のベクトルをつかう
        ③それらをスクリーン座標にしてx,yをしゅとくする
        ④さらにstartとendのx,yからwidthとheightを計算する
         */
        Vector3Int temp_start = ChangeVecToInt(startPos);
        temp_start.z = 0;
        Vector3Int temp_end = ChangeVecToInt(endPos);
        temp_end.z = 0;
        Vector3Int tempVec_start = Vector3Int.zero;
        Vector3Int tempVec_end = Vector3Int.zero;

        switch (dir)
        {
            case 0:
                tempVec_start = new Vector3Int(temp_start.x, temp_start.y, temp_start.z);
                tempVec_end = new Vector3Int(temp_end.x+1, temp_end.y+1, temp_end.z);
                break;
            case 1:
                tempVec_start = new Vector3Int(temp_start.x, temp_start.y+1, temp_start.z);
                tempVec_end = new Vector3Int(temp_end.x + 1, temp_end.y, temp_end.z);
                break;
            case 2:
                tempVec_start = new Vector3Int(temp_start.x+1, temp_start.y + 1, temp_start.z);
                tempVec_end = new Vector3Int(temp_end.x, temp_end.y, temp_end.z);
                break;
            case 3:
                tempVec_start = new Vector3Int(temp_start.x + 1, temp_start.y, temp_start.z);
                tempVec_end = new Vector3Int(temp_end.x, temp_end.y+1, temp_end.z);
                break;
            default:break;
        }

        float startX_screen = Camera.main.WorldToScreenPoint(tempVec_start).x;
        float startY_screen = Camera.main.WorldToScreenPoint(tempVec_start).y;
        float endX_screen = Camera.main.WorldToScreenPoint(tempVec_end).x;
        float endY_screen = Camera.main.WorldToScreenPoint(tempVec_end).y;
        captureArea.x = startX_screen < endX_screen ? startX_screen : endX_screen;
        captureArea.y = startY_screen < endY_screen ? startY_screen : endY_screen;
        captureArea.width = Mathf.Abs(startX_screen - endX_screen);
        captureArea.height = Mathf.Abs(startY_screen - endY_screen);


        // 元のカリングマスクを保存
        cam = Camera.main;
        originalMask = cam.cullingMask;

        // 指定レイヤーを無視（ビット演算で除外）
        int ignoreLayer = LayerMask.NameToLayer("UI");
        cam.cullingMask &= ~(1 << ignoreLayer);

        StartCoroutine(CaptureAndSave());
    }

    //範囲選択して、コピーまたはカットを選択した瞬間にこの関数を呼び出す

    System.Collections.IEnumerator CaptureAndSave()
    {
        // フレームのレンダリング完了を待つ
        yield return new WaitForEndOfFrame();

        // テクスチャを作成
        Texture2D tex = new Texture2D((int)captureArea.width, (int)captureArea.height, TextureFormat.RGB24, false);
        tex.ReadPixels(captureArea, 0, 0); // スクリーンの一部を読み込む
        tex.Apply();

        // Texture2D → Sprite に変換
        Sprite capturedSprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f) // pivot (中央)
        );

        // カリングマスクを元に戻す
        cam.cullingMask = originalMask;

        // UI Image に反映
        image.sprite = capturedSprite;
        SetImageScaleToFit(image,capturedSprite);
        image.enabled = true;

        ///////////////////////////////////////////
        /*
        // PNGデータに変換
        byte[] pngData = tex.EncodeToPNG();

        // 保存先パス
        string path = Application.dataPath + "/CapturedImage.png";
        File.WriteAllBytes(path, pngData);

        Debug.Log("画像を保存しました: " + path);
        */
    }

    void SetImageScaleToFit(Image image, Sprite sprite)
    {
        // 画像の元のサイズ（ピクセル単位）
        float imgWidth = sprite.rect.width;
        float imgHeight = sprite.rect.height;

        Vector3 scale = Vector3.one;

        if (imgHeight >= imgWidth)
        {
            // 縦長：Yを基準にして3.6、Xは比率で縮小
            scale.y = 3.6f;
            scale.x = 3.6f * (imgWidth / imgHeight);
        }
        else
        {
            // 横長：Xを基準にして3.6、Yは比率で縮小
            scale.x = 3.6f;
            scale.y = 3.6f * (imgHeight / imgWidth);
        }

        // scaleをImageオブジェクトに適用
        image.transform.localScale = scale;
    }


    //マウスの座標をタイルの座標に変換する関数
    private Vector3Int ChangeVecToInt(Vector3 v)
    {
        Vector3Int pos = Vector3Int.zero;
        pos.x = (int)Mathf.Floor(v.x);
        pos.y = (int)Mathf.Floor(v.y);
        //pos.z = (int)Mathf.Floor(v.z);

        return pos;
    }

    private int culcDir(Vector3 _startPos,Vector3 _endPos)
    {
        //コピーの向きを取得する
        int direction = 0;
        if (_endPos.x - _startPos.x >= 0)//右向き
        {
            if (_endPos.y - _startPos.y >= 0) //上向き
            {
                direction = 0;
            }
            else
            {
                direction = 1;
            }
        }
        else  //左向き
        {
            if (_endPos.y - _startPos.y >= 0) //上向き
            {
                direction = 3;
            }
            else
            {
                direction = 2;
            }
        }
        return direction;
    }

    public void disableImage()
    {
        image.enabled = false;
    }
}
