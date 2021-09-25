using Data;
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

    public void WriteSkillDescriptionText(Skill skillData, int point)
    {
        DescriptionText.text =
           "데미지: " + skillData.skillPointInfos[point].damage +
            System.Environment.NewLine +
            "쿨타임: " + skillData.skillPointInfos[point].cooldown +
            System.Environment.NewLine +
            "MP소모량: " + skillData.skillPointInfos[point].mp +
             System.Environment.NewLine;

        if (point + 1 >= skillData.skillPointInfos.Count)
        {
            DescriptionText.text +=
                System.Environment.NewLine +
                skillData.description +
                System.Environment.NewLine;
            return;
        }

        DescriptionText.text +=
            "다음 스킬 레벨" +
            System.Environment.NewLine +
            "데미지: " + skillData.skillPointInfos[point + 1].damage +
             System.Environment.NewLine +
             "쿨타임: " + skillData.skillPointInfos[point + 1].cooldown +
             System.Environment.NewLine +
             "MP소모량: " + skillData.skillPointInfos[point + 1].mp +
             System.Environment.NewLine +
             System.Environment.NewLine +
             skillData.description +
             System.Environment.NewLine;

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
