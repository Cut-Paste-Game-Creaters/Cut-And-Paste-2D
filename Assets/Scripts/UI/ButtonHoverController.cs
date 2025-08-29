using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private GameUIController UIController;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (UIController == null) UIController = FindAnyObjectByType<GameUIController>();
        UIController.OnHoverStart();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (UIController == null) UIController = FindAnyObjectByType<GameUIController>();
        UIController.OnHoverExit();
    }
}
