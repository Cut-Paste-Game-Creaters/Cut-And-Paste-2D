using UnityEngine;
using UnityEngine.EventSystems;

public class PageTurnDetector : MonoBehaviour, IDragHandler, IEndDragHandler
{
    private Vector2 dragStart;

    public void OnDrag(PointerEventData eventData)
    {
        // �����ł͉������Ȃ�
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.delta.x < -100)  // ���փX���C�h�Ńy�[�W�߂���Ɣ��f
        {
            Debug.Log("�y�[�W���߂���܂����I");
            PageTurnManager.Instance.StartPageTurnSequence();
        }
    }
}


