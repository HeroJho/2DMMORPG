using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory_Item : UI_Base
{
    [SerializeField]
    Image _icon;
    [SerializeField]
    Image _backGround;

    CanvasGroup _canvasGroup;
    Image _frontDragIcon;
    UI_DescriptionBox _descriptionBox;

    [SerializeField]
    Image _frame;

    [SerializeField]
    Text _countText;

    public int Slot { get; set; }

    public int ItemDbId { get; private set; }
    public int TemplateId { get; private set; }
    public int Count { get; private set; }
    public bool Stackable { get; private set; }
    public ItemType ItemType { get; private set; }
    public ArmorType ArmorType { get; private set; }
    public bool Equipped { get; private set; }

    public string Name { get; private set; }
    public string Description { get; private set; }


    public override void Init()
    {
        // 드래그 UI가 가림 > 최상위 부모에 생성된 UI로 덮어줌
        _frontDragIcon = transform.root.GetComponentInChildren<UI_FrontDragIcon>().GetComponent<Image>();
        _descriptionBox = transform.root.GetComponentInChildren<UI_DescriptionBox>();


        BindEvent();
    }

    public void SetItem(Item item)
    {
        if (item == null)
        {
            ItemDbId = 0;
            TemplateId = 0;
            Count = 0;
            Stackable = false;
            Equipped = false;
            ItemType = ItemType.None;
            ArmorType = ArmorType.None;

            Name = null;
            Description = null;

            _icon.gameObject.SetActive(false);
            _frame.gameObject.SetActive(false);
            _countText.gameObject.SetActive(false);
        }
        else
        {
            ItemDbId = item.ItemDbId;
            TemplateId = item.TemplateId;
            Count = item.Count;
            Stackable = item.Stackable;
            Equipped = item.Equipped;
            ItemType = item.ItemType;

            Name = item.Name;
            Description = item.Description;

            // UI_Stat Drop할 때 확인용
            if (item.ItemType == ItemType.Armor)
            {
                Armor armor = (Armor)item;
                ArmorType = armor.ArmorType;
            }


            Data.ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);

            Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
            _icon.sprite = icon;

            _icon.gameObject.SetActive(true);
            _frame.gameObject.SetActive(Equipped);


            SetCount();
        }

    }

    public void SetCount()
    {
        if (Stackable)
        {
            _countText.gameObject.SetActive(true);
            _countText.text = $"x{Count}";
        }
        else
        {
            _countText.text = "";
            _countText.gameObject.SetActive(false);
        }
    }

    public void BindEvent()
    {
        // 오른 클릭시 장착패킷
        BindEvent(_backGround.gameObject, (e) =>
        {
            Data.ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);
            if (itemData == null)
                return;

            if (itemData.itemType == ItemType.Consumable)
            {
                C_SetCountConsumable useConsumablePacket = new C_SetCountConsumable();
                useConsumablePacket.ItemDbId = ItemDbId;

                Managers.Network.Send(useConsumablePacket);
                return;
            }

            C_EquipItem equipPacket = new C_EquipItem();
            equipPacket.ItemDbId = ItemDbId;
            equipPacket.Equipped = !Equipped;

            Managers.Network.Send(equipPacket);
        }, Define.UIEvent.RightClick);

        // 드래그
        BindEvent(_backGround.gameObject, (e) =>
        {
            if (ItemDbId == 0)
                return;

            _frontDragIcon.transform.position = e.position;
            _frontDragIcon.sprite = _icon.sprite;
            _frontDragIcon.enabled = true;

            _frontDragIcon.transform.position = e.position;

        }, Define.UIEvent.Drag);
        // 드래그를 풀면 제자리로
        BindEvent(_backGround.gameObject, (e) =>
        {
            if (ItemDbId == 0)
                return;

            _frontDragIcon.enabled = false;
            _frontDragIcon.transform.position = Vector3.zero;

        }, Define.UIEvent.Click_Up);

        
        // 슬롯 변경 드랍
        BindEvent(_backGround.gameObject, (e) =>
        {
            UI_Inventory_Item itemInfo = e.pointerDrag.GetComponentInParent<UI_Inventory_Item>();
            if (itemInfo == null || itemInfo.ItemDbId == 0)
                return;

            Managers.Inven.ChangeSlot(itemInfo.ItemDbId, Slot);
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            gameSceneUI.InvenUI.RefreshUI();

        }, Define.UIEvent.Drop);


        // 아이템 설명
        BindEvent(_backGround.gameObject, (e) =>
        {
            if (ItemDbId == 0)
                return;

            _descriptionBox.WriteNameText(Name);
            _descriptionBox.WriteDescriptionText(Description);
            _descriptionBox.ModifyPosition(e);

        }, Define.UIEvent.Enter);
        BindEvent(_backGround.gameObject, (e) =>
        {
            if (ItemDbId == 0)
                return;

            _descriptionBox.WriteNameText(Name);
            _descriptionBox.WriteDescriptionText(Description);
            _descriptionBox.ClosePosition();

        }, Define.UIEvent.Exit);

    }

}
