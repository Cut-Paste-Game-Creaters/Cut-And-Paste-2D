using UnityEngine;
using UnityEngine.EventSystems;

public class PageTurnDetector : MonoBehaviour, IDragHandler, IEndDragHandler
{
    private Vector2 dragStart;

    public void OnDrag(PointerEventData eventData)
    {
        // ここでは何もしない
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.delta.x < -100)  // 左へスライドでページめくりと判断
        {
            Debug.Log("ページをめくりました！");
            PageTurnManager.Instance.StartPageTurnSequence();
        }
    }
}


