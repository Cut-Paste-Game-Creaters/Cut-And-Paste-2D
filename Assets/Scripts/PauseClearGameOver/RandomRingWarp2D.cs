using System.Collections.Generic;
using UnityEngine;

public class RandomRingWarp2D : MonoBehaviour
{
    [Header("�K�{")]
    [SerializeField] private Camera cam;                          // ���ݒ�Ȃ� Awake �� MainCamera
    [SerializeField] private Transform stageRoot;                 // �w�i�e�B�z����SpriteRenderer/Collider2D���獇��Bounds

    [Header("���[�v�z�u�i�J�����O�j")]
    [SerializeField, Range(0f, 0.4f)] private float viewportMargin = 0.4f; // ��ʒ[����ǂꂾ���O�ɏo�����i�r���[�|�[�g�j
    [SerializeField] private float extraWorldPadding = 0f;                   // ���[���h�ł���ɊO��
    [SerializeField, Min(1)] private int maxTries = 12;                      // ���܂�܂ōĒ��I

    [Header("�X��")]
    public float maxTiltAngle = 15f;
    [SerializeField] private bool addTiltToCurrent = true;

    [Header("���E�̍X�V")]
    [SerializeField] private bool recomputeBoundsEachWarp = false; // �w�i������/�����ւ��Ȃ�ON

    private Bounds combinedBounds;
    private bool hasCombinedBounds;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        RecomputeStageBounds();
    }

    /// <summary>
    /// �J�����O�i���茩���Ȃ��ꏊ�j�Ƀ��[�v�B�w�i����Bounds�̓����Ɏ��܂�܂Œ��I�B
    /// </summary>
    public void WarpRandomAroundCamera2D()
    {
        if (cam == null) return;
        if (recomputeBoundsEachWarp) RecomputeStageBounds();

        float depth = cam.orthographic
            ? Mathf.Abs(transform.position.z - cam.transform.position.z)
            : cam.WorldToViewportPoint(transform.position).z;
        if (depth <= 0f) depth = cam.nearClipPlane + 0.01f;

        Vector3 chosen = transform.position;
        bool placed = false;

        for (int i = 0; i < maxTries; i++)
        {
            int edge = Random.Range(0, 4); // 0=��,1=�E,2=��,3=��
            Vector2 vp = new Vector2(Random.value, Random.value);

            switch (edge)
            {
                case 0: vp.x = -viewportMargin; break;
                case 1: vp.x = 1f + viewportMargin; break;
                case 2: vp.y = -viewportMargin; break;
                case 3: vp.y = 1f + viewportMargin; break;
            }

            Vector3 world = cam.ViewportToWorldPoint(new Vector3(vp.x, vp.y, depth));

            if (extraWorldPadding != 0f)
            {
                switch (edge)
                {
                    case 0: world += -cam.transform.right * extraWorldPadding; break;
                    case 1: world += cam.transform.right * extraWorldPadding; break;
                    case 2: world += -cam.transform.up * extraWorldPadding; break;
                    case 3: world += cam.transform.up * extraWorldPadding; break;
                }
            }

            world.z = transform.position.z;

            if (!hasCombinedBounds || combinedBounds.Contains(new Vector3(world.x, world.y, combinedBounds.center.z)))
            {
                chosen = world;
                placed = true;
                break;
            }
        }

        // ���s���͍������E���ɃN�����v�i�J�����O�D��͂ł��Ȃ����A�͂ݏo���͖h���j
        if (!placed && hasCombinedBounds)
        {
            chosen = ClampToBounds(transform.position, combinedBounds);

            // �\�Ȃ�Ō�ɕЎ������J�����O�֊񂹂�i���E���ɃN�����v���j
            if (Random.value < 0.5f)
            {
                float vpX = (Random.value < 0.5f) ? -viewportMargin : 1f + viewportMargin;
                Vector3 w = cam.ViewportToWorldPoint(new Vector3(vpX, 0.5f, depth)); w.z = transform.position.z;
                chosen.x = Mathf.Clamp(w.x, combinedBounds.min.x, combinedBounds.max.x);
            }
            else
            {
                float vpY = (Random.value < 0.5f) ? -viewportMargin : 1f + viewportMargin;
                Vector3 w = cam.ViewportToWorldPoint(new Vector3(0.5f, vpY, depth)); w.z = transform.position.z;
                chosen.y = Mathf.Clamp(w.y, combinedBounds.min.y, combinedBounds.max.y);
            }
        }

        transform.position = chosen;

        // �����_���X��
        float rand = Random.Range(-maxTiltAngle, maxTiltAngle);
        transform.rotation = addTiltToCurrent
            ? Quaternion.Euler(0f, 0f, transform.eulerAngles.z + rand)
            : Quaternion.Euler(0f, 0f, rand);
    }

    // ===== ����Bounds�i�e�z���� SpriteRenderer / Collider2D �𓝍��j =====
    public void RecomputeStageBounds()
    {
        hasCombinedBounds = false;
        if (stageRoot == null) return;

        bool any = false;
        Bounds sum = new Bounds();

        // 1) Collider2D �D��i����Ȃ�`�ɋ߂��j
        var cols = stageRoot.GetComponentsInChildren<Collider2D>(includeInactive: true);
        foreach (var c in cols)
        {
            if (!any)
            {
                sum = c.bounds; any = true;
            }
            else
            {
                sum.Encapsulate(c.bounds);
            }
        }

        // 2) �Ȃ����/�����Ă� �ǉ��� SpriteRenderer ����荞�݁i�L���J�o�[�j
        var srs = stageRoot.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        foreach (var sr in srs)
        {
            if (!any)
            {
                sum = sr.bounds; any = true;
            }
            else
            {
                sum.Encapsulate(sr.bounds);
            }
        }

        combinedBounds = sum;
        hasCombinedBounds = any;
        if (any)
        {
            const float shrink = 20f;       // ���E�������獷��������
            const float minSize = 0.01f;     // ���]�h�~�̉���

            Vector3 size = sum.size;
            size.x = Mathf.Max(size.x - shrink, minSize);
            size.y = Mathf.Max(size.y - shrink, minSize);
            // 2D�Ȃ�Z�͂��̂܂܁i�K�v�Ȃ瓯�l�� size.z -= 500f ���j

            combinedBounds = new Bounds(sum.center, size);
            hasCombinedBounds = true;
        }
    }

    private Vector3 ClampToBounds(Vector3 pos, Bounds b)
    {
        float x = Mathf.Clamp(pos.x, b.min.x, b.max.x);
        float y = Mathf.Clamp(pos.y, b.min.y, b.max.y);
        return new Vector3(x, y, pos.z);
    }

#if UNITY_EDITOR
    // Scene�r���[�ō������E�������i�I�����j
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            if (cam == null) cam = Camera.main;
            RecomputeStageBounds();
        }
        if (hasCombinedBounds)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
            Gizmos.DrawCube(combinedBounds.center, combinedBounds.size);
            Gizmos.color = new Color(0f, 0.7f, 0f, 1f);
            Gizmos.DrawWireCube(combinedBounds.center, combinedBounds.size);
        }
    }
#endif
}
