using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Skill : UI_Base
{
    enum Images
    {
        DragBar,
    }

    enum Buttons
    {
        Button_X
    }

    [SerializeField]
    GameObject _grid;

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));

        BindEvent();
        
    }

    void BindEvent()
    {
        // 드레그로 창 이동
        BindEvent(Get<Image>((int)Images.DragBar).gameObject, (e) =>
        {
            gameObject.transform.position = e.position;

        }, Define.UIEvent.Drag);

        // X버튼 누르면 끈다
        BindEvent(Get<Button>((int)Buttons.Button_X).gameObject, (e) =>
        {
            gameObject.SetActive(false);

        }, Define.UIEvent.LeftClick);

    }

    public void RefreshUI()
    {
        foreach (Transform child in _grid.transform)
            Destroy(child.gameObject);

        foreach (int skillId in Managers.Skill.SkillPointInfos.Keys)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_SkillSlot", _grid.transform);
            UI_Skill_Slot skillSlot = go.GetComponent<UI_Skill_Slot>();

            skillSlot.SetUI(skillId, Managers.Skill.GetSkillPoint(skillId));
        }

    }

}
