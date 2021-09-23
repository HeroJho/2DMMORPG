using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Skill_Slot : UI_Base
{
    public int TemplateId { get; private set; }
    private int _point;

    enum Images
    {
        Icon,
    }

    Image _frontDragIcon;
    Sprite _sprite;
    UI_DescriptionBox _descriptionBox;
    Skill _skillData;

    public override void Init()
    {

    }

    public void SetUI(int skillId, int point)
    {
        TemplateId = skillId;

        // 드래그 UI가 가림 > 최상위 부모에 생성된 UI로 덮어줌
        _frontDragIcon = transform.root.GetComponentInChildren<UI_FrontDragIcon>().GetComponent<Image>();
        _descriptionBox = transform.root.GetComponentInChildren<UI_DescriptionBox>();


        Bind<Image>(typeof(Images));

        Managers.Data.SkillDict.TryGetValue(TemplateId, out _skillData);

        _sprite = Managers.Resource.Load<Sprite>(_skillData.iconPath);
        Get<Image>((int)Images.Icon).sprite = _sprite;

        BindEvent();
    }

    void BindEvent()
    {
        // 드래그해서 슬롯에 등록
        BindEvent(Get<Image>((int)Images.Icon).gameObject, (e) =>
        {
            _frontDragIcon.transform.position = e.position;
            _frontDragIcon.sprite = _sprite;
            _frontDragIcon.enabled = true;

            _frontDragIcon.transform.position = e.position;

        }, Define.UIEvent.Drag);
        BindEvent(Get<Image>((int)Images.Icon).gameObject, (e) =>
        {
            _frontDragIcon.enabled = false;
            _frontDragIcon.transform.position = Vector3.zero;

        }, Define.UIEvent.Click_Up);


        // 아이템 설명
        BindEvent(Get<Image>((int)Images.Icon).gameObject, (e) =>
        {
            if (_skillData == null)
                return;

            _descriptionBox.WriteNameText(_skillData.name);
            _descriptionBox.WriteDescriptionText(
                "데미지: " + _skillData.skillPointInfos[_point].damage +
                System.Environment.NewLine +
                "쿨타임: " + _skillData.skillPointInfos[_point].cooldown +
                System.Environment.NewLine +
                "MP소모량: " + _skillData.skillPointInfos[_point].mp +
                System.Environment.NewLine +
                System.Environment.NewLine +
                _skillData.description
                );
            _descriptionBox.ModifyPosition(e);

        }, Define.UIEvent.Enter);
        BindEvent(Get<Image>((int)Images.Icon).gameObject, (e) =>
        {
            if (_skillData == null)
                return;

            _descriptionBox.ClosePosition();

        }, Define.UIEvent.Exit);
    }

}
