using UnityEngine;

//�������̃A�j���[�V�������Ǘ�����N���X
public class AnimationInstance
{
    //�A�j���[�V�����N���b�v
    private SpriteAnimationData data;
    //�A�j���[�V�������s���Ώہ@AnimationManager.Play�Ŏ擾����
    public SpriteRenderer renderer;
    private float timer = 0f;
    private int frameIndex = 0;
    private bool isFinished = false;

    public bool IsFinished => isFinished;
    public SpriteRenderer TargetRenderer => renderer; // renderer��ǂݎ���p�Ō��J����
    public string GetAnimationName() { return data.name; }

    public AnimationInstance(SpriteAnimationData data, SpriteRenderer renderer)
    {
        this.data = data;
        this.renderer = renderer;

        // ������Ԃŕ\��
        if (data.frames.Count > 0)
            renderer.sprite = data.frames[0];
    }

    //�A�j���[�V�������������ꂽ��Update����
    public void Update(float deltaTime)
    {
        //�����I�����Ă���A�j���[�V������������A�A�j���̉摜��0��������return
        if (IsFinished || data.frames.Count == 0 || renderer==null)
            return;

        timer += deltaTime;

        //�o�ߎ��Ԃ��t���[���Ԃ̎��Ԃɕς���
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
