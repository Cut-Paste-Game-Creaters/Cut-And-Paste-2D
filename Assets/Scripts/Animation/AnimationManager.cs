using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    //アニメーションをたくさん読み込むのは重いのでsingletonにする
    public static AnimationManager Instance { get; private set; }

    //インスペクターで設定されるアニメーションクリップ
    [SerializeField] private List<SpriteAnimationData> animationClips;

    //アニメーションクリップのデータ保管辞書
    private Dictionary<string, SpriteAnimationData> animationDict = new();
    //現在再生中のアニメーションが保管されるリスト
    private List<AnimationInstance> activeAnimations = new();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var clip in animationClips)
        {
            if (!animationDict.ContainsKey(clip.name))
            {
                animationDict.Add(clip.name, clip);
            }
        }
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;
        for (int i = activeAnimations.Count - 1; i >= 0; i--)
        {
            var anim = activeAnimations[i];
            anim.Update(deltaTime);

            if (anim.IsFinished)
            {
                activeAnimations.RemoveAt(i);  // アニメーション終了後は削除
            }
        }
    }

    //アニメーションの実行を指示する関数
    public void Play(string name, SpriteRenderer targetRenderer,bool overlapAnim=false)
    {
        if (!animationDict.TryGetValue(name, out var clip))
        {
            Debug.LogWarning($"Animation '{name}' not found!");
            return;
        }
        if(overlapAnim)     //これがtrueなら、同じspriteRendererに対して同じアニメーション呼び出ししたら無視
        {
            for (int i = activeAnimations.Count - 1; i >= 0; i--)
            {
                if (activeAnimations[i].TargetRenderer == targetRenderer
                    && activeAnimations[i].GetAnimationName()==name)
                {
                    return;
                }
            }
        }

        Stop(targetRenderer);

        AnimationInstance instance = new AnimationInstance(clip, targetRenderer);
        activeAnimations.Add(instance);
    }

    public void Stop(SpriteRenderer renderer)
    {
        for (int i = activeAnimations.Count - 1; i >= 0; i--)
        {
            if (activeAnimations[i].TargetRenderer == renderer)
            {
                activeAnimations[i].Stop();
                activeAnimations.RemoveAt(i); // 即座に消去
                break;
            }
        }
    }
}
