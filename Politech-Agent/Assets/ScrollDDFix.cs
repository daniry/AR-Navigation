using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollDDFix : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool isOver = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isOver = true;
        Debug.Log("OnPointerEnter: " + isOver);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isOver = false;
        Debug.Log("OnPointerExit: " + isOver);
    }
}
