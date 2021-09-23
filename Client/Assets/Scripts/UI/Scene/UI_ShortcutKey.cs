using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ShortcutKey : UI_Base
{
    enum Images
    {
        Slot,
        DragIcon,
        CoolTime
    }

    enum Texts
    {
        Count
    }

    public int TemplateId { get; private set; } = 0;
    public int SlotType { get; private set; } = 0;
    int _count = 0;
    int _itemDbId = 0;
    Image _frontDragIcon;
    Sprite _icon = null;


    public override void Init()
    {
        // 드래그 UI가 가림 > 최상위 부모에 생성된 UI로 덮어줌
        _frontDragIcon = transform.root.GetComponentInChildren<UI_FrontDragIcon>().GetComponent<Image>();

        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));

        Get<Image>((int)Images.Slot).enabled = true;

        Get<Image>((int)Images.CoolTime).enabled = false;
        Get<Image>((int)Images.CoolTime).fillAmount = 1;

        Get<Text>((int)Texts.Count).enabled = false;
        Get<Text>((int)Texts.Count).text = "x" + 0;


        BindEvent();
    }

    void BindEvent()
    {
        // 클릭시 아이템 사용
        BindEvent(Get<Image>((int)Images.Slot).gameObject, (e) =>
        {
            GetKey();

        }, Define.UIEvent.LeftClick);


        // 드래그
        BindEvent(Get<Image>((int)Images.Slot).gameObject, (e) =>
        {
            _frontDragIcon.transform.position = e.position;
            _frontDragIcon.sprite = _icon;
            _frontDragIcon.enabled = true;

            _frontDragIcon.transform.position = e.position;

        }, Define.UIEvent.Drag);
        // 드래그를 풀면 제자리로
        BindEvent(Get<Image>((int)Images.Slot).gameObject, (e) =>
        {
            _frontDragIcon.enabled = false;
            _frontDragIcon.transform.position = Vector3.zero;

        }, Define.UIEvent.Click_Up);


        // Drop이벤트 등록
        BindEvent(Get<Image>((int)Images.Slot).gameObject, (e) =>
        {
            UI_Inventory_Item itemInfo = e.pointerDrag.GetComponentInParent<UI_Inventory_Item>();
            UI_Skill_Slot skillInfo = e.pointerDrag.GetComponentInParent<UI_Skill_Slot>();
            
            UI_ShortcutKey keyInfo = e.pointerDrag.GetComponentInParent<UI_ShortcutKey>();

            if (itemInfo == null && keyInfo == null && skillInfo == null)
                return;


            if (itemInfo != null)
            {
                // 일단 소모품만 킥슬롯 등록 가능
                if (itemInfo.ItemType != ItemType.Consumable)
                {
                    return;
                }
                else
                {
                    TemplateId = itemInfo.TemplateId;
                    SlotType = 1;
                }

            }
            else if(skillInfo != null)
            {
                TemplateId = skillInfo.TemplateId;
                SlotType = 2;
            }


            // 기존 등록키 옮김
            if (keyInfo != null)
            {
                TemplateId = keyInfo.TemplateId;
                SlotType = keyInfo.SlotType;
                keyInfo.RemoveSlot();
            }

            if (TemplateId == 0)
                return;

            // 슬롯에 등록
            RefleshSlot(TemplateId);

        }, Define.UIEvent.Drop);

    }

    public void RefleshSlot(int templateId)
    {
        TemplateId = templateId;

        switch (SlotType)
        {
            case 1:
                {
                    ItemData itemData = null;
                    if (Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData) == false)
                        return;

                    _icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
                    Get<Image>((int)Images.Slot).sprite = _icon;

                    RefleshCount();

                    Get<Image>((int)Images.Slot).enabled = true;
                    Get<Text>((int)Texts.Count).enabled = true;
                    // 이벤트바인딩 때문에 enable불가능 > 알파값으로 조절
                    Color color = Get<Image>((int)Images.Slot).color;
                    color.a = 255;
                    Get<Image>((int)Images.Slot).color = color;
                }
                break;
            case 2:
                {
                    Skill skillData = null;
                    if (Managers.Data.SkillDict.TryGetValue(TemplateId, out skillData) == false)
                        return;

                    _icon = Managers.Resource.Load<Sprite>(skillData.iconPath);
                    Get<Image>((int)Images.Slot).sprite = _icon;

                    Get<Image>((int)Images.Slot).enabled = true;
                    Get<Text>((int)Texts.Count).enabled = false;
                    // 이벤트바인딩 때문에 enable불가능 > 알파값으로 조절
                    Color color = Get<Image>((int)Images.Slot).color;
                    color.a = 255;
                    Get<Image>((int)Images.Slot).color = color;
                }
                break;
            default:
                break;
        }

    }

    public void RefleshCount()
    {
        _count = 0;
        List<Item> items = Managers.Inven.FindToList(i => i.TemplateId == TemplateId);

        if (items.Count <= 0)
        {
            _count = 0;
            return;
        }

        _itemDbId = items[0].ItemDbId;

        foreach (Item item in items)
        {
            _count += item.Count;
        }

        Get<Text>((int)Texts.Count).text = "x" + _count.ToString();
    }

    public void GetKey()
    {
        switch (SlotType)
        {
            case 1: // 아이템
                {
                    C_SetCountConsumable useConsumablePacket = new C_SetCountConsumable();
                    useConsumablePacket.ItemDbId = _itemDbId;

                    Managers.Network.Send(useConsumablePacket);
                    RefleshCount();
                }
                break;
            case 2: // 스킬
                {
                    MyPlayerController mpc = Managers.Object.MyPlayer;

                    if (!Managers.Skill.UseSkill(TemplateId))
                        return;

                    // 스킬쿨 표시 돌림
                    Skill skillData = null;
                    Managers.Data.SkillDict.TryGetValue(TemplateId, out skillData);
                    int point = Managers.Skill.GetSkillPoint(TemplateId);

                    if(_coCool != null)
                        StopCoroutine(_coCool);
                    StartCool(skillData.skillPointInfos[point].cooldown);

                }
                break;
            default:
                break;
        }

    }

    public void RemoveSlot()
    {
        TemplateId = 0;
        SlotType = 0;
        _count = 0;
        _itemDbId = 0;
        _icon = null;

        Get<Image>((int)Images.Slot).sprite = _icon;

        // 이벤트바인딩 때문에 enable불가능 > 알파값으로 조절
        Color color = Get<Image>((int)Images.Slot).color;
        color.a = 0;
        Get<Image>((int)Images.Slot).color = color;

        RefleshCount();

        Get<Image>((int)Images.Slot).enabled = true;
        Get<Text>((int)Texts.Count).enabled = false;
    }


    // CoolTime 함수
    Coroutine _coCool = null;
    float _duration;
    float _currentTime;
    public void StartCool(float du)
    {
        Get<Image>((int)Images.CoolTime).enabled = true;
        Get<Image>((int)Images.CoolTime).fillAmount = 1;

        _duration = du;
        _currentTime = du;
                
        _coCool = StartCoroutine(Activation());
    }

    IEnumerator Activation()
    {
        while (_currentTime > 0)
        {
            _currentTime -= 0.05f;
            Get<Image>((int)Images.CoolTime).fillAmount = _currentTime / _duration;
            yield return new WaitForSeconds(0.05f);
        }
        
        Get<Image>((int)Images.CoolTime).enabled = false;
        _duration = 0;
        _currentTime = 0;
        _coCool = null;
    }

}
