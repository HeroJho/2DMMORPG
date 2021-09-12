using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Skill_Slot : UI_Base
{
    public int TemplateId;

    enum Images
    {
        SkillSlot,
        Selected
    }

    Image _frontDragIcon;

    public override void Init()
    {
        // 드래그 UI가 가림 > 최상위 부모에 생성된 UI로 덮어줌
        _frontDragIcon = transform.root.GetComponentInChildren<UI_FrontDragIcon>().GetComponent<Image>();

        Bind<Image>(typeof(Images));
        Get<Image>((int)Images.Selected).enabled = false;

        BindEvent();

    }

    void BindEvent()
    {
        // 드래그해서 슬롯에 등록
        BindEvent(Get<Image>((int)Images.SkillSlot).gameObject, (e) =>
        {
            Skill skillData = null;
            Managers.Data.SkillDict.TryGetValue(TemplateId, out skillData);


            Sprite _icon = Managers.Resource.Load<Sprite>(skillData.iconPath);

            _frontDragIcon.transform.position = e.position;
            _frontDragIcon.sprite = _icon;
            _frontDragIcon.enabled = true;

            _frontDragIcon.transform.position = e.position;

        }, Define.UIEvent.Drag);
        BindEvent(Get<Image>((int)Images.SkillSlot).gameObject, (e) =>
        {
            _frontDragIcon.enabled = false;
            _frontDragIcon.transform.position = Vector3.zero;

        }, Define.UIEvent.Click_Up);
    }
}
