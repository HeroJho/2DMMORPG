using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Shop_Player_Item : UI_Base
{
    enum Texts
    {
        NameText,
        GoldText,
        CountText
    }

    enum Images
    {
        ItemImage
    }

    enum Buttons
    {
        SellButton
    }

    UI_DescriptionBox _descriptionBox = null;
    Item _item = null;

    bool _isInit = false;
    public override void Init()
    {
        if (_isInit)
            return;

        _descriptionBox = transform.root.GetComponentInChildren<UI_DescriptionBox>();


        Bind<Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));

        BindEvent();
        _isInit = true;
    }

    private void BindEvent()
    {
        // 아이템 설명
        BindEvent(Get<Image>((int)Images.ItemImage).gameObject, (e) =>
        {
            if (_item == null)
                return;

            _descriptionBox.WriteNameText(_item.Name);
            _descriptionBox.WriteDescriptionText(_item.Description);
            _descriptionBox.ModifyPosition(e);

        }, Define.UIEvent.Enter);
        BindEvent(Get<Image>((int)Images.ItemImage).gameObject, (e) =>
        {
            if (_item == null)
                return;

            _descriptionBox.ClosePosition();

        }, Define.UIEvent.Exit);


        // 아이템 팔기
        BindEvent(Get<Button>((int)Buttons.SellButton).gameObject, (e) =>
        {
            if (_item == null)
                return;

            C_SellItem sellItemPacket = new C_SellItem();
            sellItemPacket.ItemDbId = _item.ItemDbId;

            if(_item.Stackable)
            {
                // TODO : 몇개를 팔거냐
                sellItemPacket.Count = 1;
            }
            else
            {
                sellItemPacket.Count = 1;
            }

            Managers.Network.Send(sellItemPacket);

        }, Define.UIEvent.LeftClick);

    }

    public void SetInfo(Item data)
    {
        Init();

        // 초기화
        _item = null;

        Get<Image>((int)Images.ItemImage).sprite = null;
        Get<Text>((int)Texts.NameText).text = "";
        Get<Text>((int)Texts.GoldText).text = "골드: ";
        Get<Text>((int)Texts.CountText).text = "x";

        // 셋팅
        _item = data;

        ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(data.TemplateId, out itemData);

        Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
        Get<Image>((int)Images.ItemImage).sprite = icon;

        Get<Text>((int)Texts.NameText).text = itemData.name;
        Get<Text>((int)Texts.GoldText).text = "골드: " + itemData.gold;

        if(data.Stackable)
        {
            Get<Text>((int)Texts.CountText).gameObject.SetActive(true);
            Get<Text>((int)Texts.CountText).text = "x" + data.Count;
        }
        else
        {
            Get<Text>((int)Texts.CountText).gameObject.SetActive(false);
        }


    }
}
