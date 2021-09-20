using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_DescriptionBox : MonoBehaviour
{
    [SerializeField]
    private Text NameText;
    [SerializeField]
    private Text DescriptionText;
    [SerializeField]
    private Vector3 ClosePos;

    public void WriteNameText(string text)
    {
        NameText.text = text;
    }

    public void WriteDescriptionText(string text)
    {
        DescriptionText.text = text;
    }

    public void ModifyPosition(PointerEventData e)
    {
        transform.position = e.position + new Vector2(100, -115);
    }

    public void ClosePosition()
    {
        transform.position = ClosePos;
    }
}
