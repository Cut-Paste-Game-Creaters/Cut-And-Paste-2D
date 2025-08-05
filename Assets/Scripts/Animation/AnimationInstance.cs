using UnityEngine;

//ある特定のアニメーションを管理するクラス
public class AnimationInstance
{
    //アニメーションクリップ
    private SpriteAnimationData data;
    //アニメーションを行う対象　AnimationManager.Playで取得する
    public SpriteRenderer renderer;
    private float timer = 0f;
    private int frameIndex = 0;
    private bool isFinished = false;

    public bool IsFinished => isFinished;
    public SpriteRenderer TargetRenderer => renderer; // rendererを読み取り専用で公開する
    public string GetAnimationName() { return data.name; }

    public AnimationInstance(SpriteAnimationData data, SpriteRenderer renderer)
    {
        this.data = data;
        this.renderer = renderer;

        // 初期状態で表示
        if (data.frames.Count > 0)
            renderer.sprite = data.frames[0];
    }

    //アニメーションが生成されたらUpdateする
    public void Update(float deltaTime)
    {
        //もし終了しているアニメーションだったり、アニメの画像が0だったらreturn
        if (IsFinished || data.frames.Count == 0 || renderer==null)
            return;

        timer += deltaTime;

        //経過時間をフレーム間の時間に変える
        if (timer >= data.FrameDuration)
        {
            timer -= data.FrameDuration;
            frameIndex++;

            if (frameIndex >= data.frames.Count)
            {
                if (data.isLoop)
                {
                    frameIndex = 0;
                }
                else
                {
                    isFinished = true;
                    return;
                }
            }

            renderer.sprite = data.frames[frameIndex];
        }
    }

    public void Stop()
    {
        isFinished = true;
    }
}
