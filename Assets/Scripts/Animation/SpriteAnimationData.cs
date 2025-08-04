using System.Collections.Generic;
using UnityEngine;


//クリップ1つ分の構造体のようなクラス
[System.Serializable]
public class SpriteAnimationData
{
    public string name;
    public List<Sprite> frames;
    public float frameRate = 12f;
    public bool isLoop = false;  // ループするか（デフォルトfalse）

    public float FrameDuration => 1f / frameRate;
}
