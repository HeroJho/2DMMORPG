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
        Button_X,
        Button_1,
        Button_2,
        Button_3
    }

    enum Texts
    {
        PointsText
    }

    [SerializeField]
    GameObject _tree1;
    [SerializeField]
    GameObject _tree2;
    [SerializeField]
    GameObject _tree3;

    [SerializeField]
    GameObject _grid1;
    [SerializeField]
    GameObject _grid2;
    [SerializeField]
    GameObject _grid3;

    private Vector3 _opendButtonPos1;
    private Vector3 _closedButtonPos1;
    private Vector3 _opendButtonPos2;
    private Vector3 _closedButtonPos2;
    private Vector3 _opendButtonPos3;
    private Vector3 _closedButtonPos3;

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));

        _opendButtonPos1 = Get<Button>((int)Buttons.Button_1).gameObject.transform.position;
        _closedButtonPos1 = Get<Button>((int)Buttons.Button_1).gameObject.transform.position + new Vector3(0, -10, 0);
        _opendButtonPos2 = Get<Button>((int)Buttons.Button_2).gameObject.transform.position + new Vector3(0, 10, 0);
        _closedButtonPos2 = Get<Button>((int)Buttons.Button_2).gameObject.transform.position;
        _opendButtonPos3 = Get<Button>((int)Buttons.Button_3).gameObject.transform.position + new Vector3(0, 10, 0);
        _closedButtonPos3 = Get<Button>((int)Buttons.Button_3).gameObject.transform.position;

        _tree1.transform.SetAsLastSibling();
        _tree2.transform.SetAsFirstSibling();
        _tree3.transform.SetAsFirstSibling();

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

        // SkillTreeButtons
        BindEvent(Get<Button>((int)Buttons.Button_1).gameObject, (e) =>
        {
            Get<Button>((int)Buttons.Button_1).gameObject.transform.position = _opendButtonPos1;
            Get<Button>((int)Buttons.Button_2).gameObject.transform.position = _closedButtonPos2;
            Get<Button>((int)Buttons.Button_3).gameObject.transform.position = _closedButtonPos3;

            _tree1.transform.SetAsLastSibling();
            _tree2.transform.SetAsFirstSibling();
            _tree3.transform.SetAsFirstSibling();

        }, Define.UIEvent.LeftClick);
        BindEvent(Get<Button>((int)Buttons.Button_2).gameObject, (e) =>
        {
            Get<Button>((int)Buttons.Button_1).gameObject.transform.position = _closedButtonPos1;
            Get<Button>((int)Buttons.Button_2).gameObject.transform.position = _opendButtonPos2;
            Get<Button>((int)Buttons.Button_3).gameObject.transform.position = _closedButtonPos3;

            _tree1.transform.SetAsFirstSibling();
            _tree2.transform.SetAsLastSibling();
            _tree3.transform.SetAsFirstSibling();

        }, Define.UIEvent.LeftClick);
        BindEvent(Get<Button>((int)Buttons.Button_3).gameObject, (e) =>
        {
            Get<Button>((int)Buttons.Button_1).gameObject.transform.position = _closedButtonPos1;
            Get<Button>((int)Buttons.Button_2).gameObject.transform.position = _closedButtonPos2;
            Get<Button>((int)Buttons.Button_3).gameObject.transform.position = _opendButtonPos3;

            _tree1.transform.SetAsFirstSibling();
            _tree2.transform.SetAsFirstSibling();
            _tree3.transform.SetAsLastSibling();

        }, Define.UIEvent.LeftClick);

    }

    bool _isInit = false;
    public void RefreshUI()
    {
        if(!_isInit)
        {
            Bind<Text>(typeof(Texts));
            _isInit = true;
        }

        Get<Text>((int)Texts.PointsText).text = "Points: " + Managers.Skill.MyPoints;

        foreach (Transform child in _grid1.transform)
            Destroy(child.gameObject);
        foreach (Transform child in _grid2.transform)
            Destroy(child.gameObject);
        foreach (Transform child in _grid3.transform)
            Destroy(child.gameObject);

        foreach (int skillId in Managers.Skill.SkillPointInfos.Keys)
        {
            if (skillId > 3000) // 3차
            {
                GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_SkillSlot", _grid3.transform);
                UI_Skill_Slot skillSlot = go.GetComponent<UI_Skill_Slot>();

                skillSlot.SetUI(skillId, Managers.Skill.GetSkillPoint(skillId));
            }
            else if(skillId > 2000) // 2차
            {
                GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_SkillSlot", _grid2.transform);
                UI_Skill_Slot skillSlot = go.GetComponent<UI_Skill_Slot>();

                skillSlot.SetUI(skillId, Managers.Skill.GetSkillPoint(skillId));
            }
            else if(skillId > 1000) // 1차
            {
                GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_SkillSlot", _grid1.transform);
                UI_Skill_Slot skillSlot = go.GetComponent<UI_Skill_Slot>();

                skillSlot.SetUI(skillId, Managers.Skill.GetSkillPoint(skillId));
            }

        }

    }

}
