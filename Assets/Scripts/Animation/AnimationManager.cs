using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    //�A�j���[�V��������������ǂݍ��ނ̂͏d���̂�singleton�ɂ���
    public static AnimationManager Instance { get; private set; }

    //�C���X�y�N�^�[�Őݒ肳���A�j���[�V�����N���b�v
    [SerializeField] private List<SpriteAnimationData> animationClips;

    //�A�j���[�V�����N���b�v�̃f�[�^�ۊǎ���
    private Dictionary<string, SpriteAnimationData> animationDict = new();
    //���ݍĐ����̃A�j���[�V�������ۊǂ���郊�X�g
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
                activeAnimations.RemoveAt(i);  // �A�j���[�V�����I����͍폜
            }
        }
    }

    //�A�j���[�V�����̎��s���w������֐�
    public void Play(string name, SpriteRenderer targetRenderer,bool overlapAnim=false)
    {
        if (!animationDict.TryGetValue(name, out var clip))
        {
            Debug.LogWarning($"Animation '{name}' not found!");
            return;
        }
        if(overlapAnim)     //���ꂪtrue�Ȃ�A����spriteRenderer�ɑ΂��ē����A�j���[�V�����Ăяo�������疳��
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
                activeAnimations.RemoveAt(i); // �����ɏ���
                break;
            }
        }
    }
}
