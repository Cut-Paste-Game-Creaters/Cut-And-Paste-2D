using System.Collections.Generic;
using UnityEngine;

public class RandomRingWarp2D : MonoBehaviour
{
    [Header("必須")]
    [SerializeField] private Camera cam;                          // 未設定なら Awake で MainCamera
    [SerializeField] private Transform stageRoot;                 // 背景親。配下のSpriteRenderer/Collider2Dから合成Bounds

    [Header("ワープ配置（カメラ外）")]
    [SerializeField, Range(0f, 0.4f)] private float viewportMargin = 0.4f; // 画面端からどれだけ外に出すか（ビューポート）
    [SerializeField] private float extraWorldPadding = 0f;                   // ワールドでさらに外へ
    [SerializeField, Min(1)] private int maxTries = 12;                      // 収まるまで再抽選

    [Header("傾き")]
    public float maxTiltAngle = 15f;
    [SerializeField] private bool addTiltToCurrent = true;

    [Header("境界の更新")]
    [SerializeField] private bool recomputeBoundsEachWarp = false; // 背景が動く/差し替わるならON

    private Bounds combinedBounds;
    private bool hasCombinedBounds;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        RecomputeStageBounds();
    }

    /// <summary>
    /// カメラ外（ぎり見えない場所）にワープ。背景合成Boundsの内側に収まるまで抽選。
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
            int edge = Random.Range(0, 4); // 0=左,1=右,2=下,3=上
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

        // 失敗時は合成境界内にクランプ（カメラ外優先はできないが、はみ出しは防ぐ）
        if (!placed && hasCombinedBounds)
        {
            chosen = ClampToBounds(transform.position, combinedBounds);

            // 可能なら最後に片軸だけカメラ外へ寄せる（境界内にクランプしつつ）
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

        // ランダム傾き
        float rand = Random.Range(-maxTiltAngle, maxTiltAngle);
        transform.rotation = addTiltToCurrent
            ? Quaternion.Euler(0f, 0f, transform.eulerAngles.z + rand)
            : Quaternion.Euler(0f, 0f, rand);
    }

    // ===== 合成Bounds（親配下の SpriteRenderer / Collider2D を統合） =====
    public void RecomputeStageBounds()
    {
        hasCombinedBounds = false;
        if (stageRoot == null) return;

        bool any = false;
        Bounds sum = new Bounds();

        // 1) Collider2D 優先（あるなら形に近い）
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

        // 2) なければ/あっても 追加で SpriteRenderer も取り込み（広くカバー）
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
            const float shrink = 20f;       // 幅・高さから差し引く量
            const float minSize = 0.01f;     // 反転防止の下限

            Vector3 size = sum.size;
            size.x = Mathf.Max(size.x - shrink, minSize);
            size.y = Mathf.Max(size.y - shrink, minSize);
            // 2DならZはそのまま（必要なら同様に size.z -= 500f も可）

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
    // Sceneビューで合成境界を可視化（選択時）
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
