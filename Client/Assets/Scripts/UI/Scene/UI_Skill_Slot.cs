using Data;
using Google.Protobuf.Protocol;
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
    enum Texts
    {
        LevelText,
    }
    enum Buttons
    {
        PointUpButton,
    }

    Image _frontDragIcon;
    Sprite _sprite;
    UI_DescriptionBox _descriptionBox;
    Skill _skillData;

    public override void Init()
    {

    }

    bool _isBind = false;
    public void SetUI(int skillId, int point)
    {
        TemplateId = skillId;
        _point = point + 1;
        if(!_isBind)
        {
            // 드래그 UI가 가림 > 최상위 부모에 생성된 UI로 덮어줌
            _frontDragIcon = transform.root.GetComponentInChildren<UI_FrontDragIcon>().GetComponent<Image>();
            _descriptionBox = transform.root.GetComponentInChildren<UI_DescriptionBox>();

            Bind<Image>(typeof(Images));
            Bind<Text>(typeof(Texts));
            Bind<Button>(typeof(Buttons));

            BindEvent();
            _isBind = true;
        }

        Managers.Data.SkillDict.TryGetValue(TemplateId, out _skillData);

        _sprite = Managers.Resource.Load<Sprite>(_skillData.iconPath);
        Get<Image>((int)Images.Icon).sprite = _sprite;

        if(_point >= _skillData.skillPointInfos.Count)
        {
            Get<Text>((int)Texts.LevelText).text = "Level: MAX";
            Get<Button>((int)Buttons.PointUpButton).gameObject.SetActive(false);
        }    
        else
        {
            Get<Text>((int)Texts.LevelText).text = "Level: " + _point;
            Get<Button>((int)Buttons.PointUpButton).gameObject.SetActive(true);
        }

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
            _descriptionBox.WriteSkillDescriptionText(_skillData, _point);
            _descriptionBox.ModifyPosition(e);

        }, Define.UIEvent.Enter);
        BindEvent(Get<Image>((int)Images.Icon).gameObject, (e) =>
        {
            if (_skillData == null)
                return;

            _descriptionBox.ClosePosition();

        }, Define.UIEvent.Exit);

        // 스킬포인트 버튼
        BindEvent(Get<Button>((int)Buttons.PointUpButton).gameObject, (e) =>
        {
            if (_skillData == null)
                return;

            C_ChangeSkillPoint changeSkillPointPacket = new C_ChangeSkillPoint();
            changeSkillPointPacket.SkillInfo = new SkillInfo()
            {
                SkillId = TemplateId,
                Point = _point
            };           

            Managers.Network.Send(changeSkillPointPacket);

        }, Define.UIEvent.LeftClick);

    }

}
