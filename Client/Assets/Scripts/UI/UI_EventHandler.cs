using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_EventHandler : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler, IPointerUpHandler, IDropHandler 
{
    public Action<PointerEventData> OnRightClickHandler = null;
    public Action<PointerEventData> OnLeftClickHandler = null;
    public Action<PointerEventData> OnDragHandler = null;
    public Action<PointerEventData> OnDragEndHandler = null;
    public Action<PointerEventData> OnUpHandler = null;
    public Action<PointerEventData> OnDropHandler = null;


    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (OnLeftClickHandler == null)
                return;

            OnLeftClickHandler.Invoke(eventData);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (OnRightClickHandler == null)
                return;

            OnRightClickHandler.Invoke(eventData);
        }

    }

    public void OnDrag(PointerEventData eventData)
    {
        if (OnDragHandler != null)
            OnDragHandler.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (OnDragEndHandler != null)
            OnDragEndHandler.Invoke(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (OnUpHandler != null)
            OnUpHandler.Invoke(eventData);
    }

    // OnDrop은 마우스를 누른 오브젝트를 드롭할 때 인자로 불러올 수 있음
    // 위치를 굳이 옮길 필요가 없음
    public void OnDrop(PointerEventData eventData)
    {
        if (OnDropHandler != null)
            OnDropHandler.Invoke(eventData);
    }


}

