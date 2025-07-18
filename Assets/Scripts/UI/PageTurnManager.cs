using UnityEngine;
using UnityEngine.SceneManagement;

public class PageTurnManager : MonoBehaviour
{
    public static PageTurnManager Instance;

    public Animator bookAnimator;  // �y�[�W�߂���A�j���[�V����
    public Camera mainCamera;
    public Transform zoomTarget;   // ���̃V�[���̒��S�_
    public float zoomDuration = 2f;
    public float zoomSize = 2f;
    private float originalSize;

    void Awake()
    {
        Instance = this;
    }

    public void StartPageTurnSequence()
    {
        originalSize = mainCamera.orthographicSize;
        bookAnimator.SetTrigger("TurnPage");
        Invoke(nameof(StartZoom), 1f); // �A�j���[�V������ɃY�[��
    }

    void StartZoom()
    {
        StartCoroutine(ZoomAndMoveCamera());
    }

    System.Collections.IEnumerator ZoomAndMoveCamera()
    {
        Vector3 startPos = mainCamera.transform.position;
        Vector3 endPos = new Vector3(zoomTarget.position.x, zoomTarget.position.y, startPos.z);

        float startSize = originalSize;
        float endSize = zoomSize;

        float t = 0f;
        while (t < zoomDuration)
        {
            t += Time.deltaTime;
            float progress = t / zoomDuration;

            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, progress);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, endSize, progress);

            yield return null;
        }

        // �Ō�ɃV�[���J��
        SceneManager.LoadScene("Scene2");
    }
}

