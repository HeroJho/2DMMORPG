using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Stat : UI_Base
{
    enum Images
    {
        Slot_Helmet,
        Slot_Armor,
        Slot_Boots,
        Slot_Weapon,
        Slot_Shield,
        Slot_Helmet_BG,
        Slot_Armor_BG,
        Slot_Boots_BG,
        Slot_Weapon_BG,
        Slot_Shield_BG

    }

    enum Texts
    {
        NameText,
        AttackValueText,
        DefenceValueText
    }

    bool _init = false;
    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));

        // 아이콘이 진해서 일단 꺼둠
        Get<Image>((int)Images.Slot_Armor).enabled = false;
        Get<Image>((int)Images.Slot_Boots).enabled = false;
        Get<Image>((int)Images.Slot_Helmet).enabled = false;
        Get<Image>((int)Images.Slot_Shield).enabled = false;
        Get<Image>((int)Images.Slot_Weapon).enabled = false;

        BindEvent();

        _init = true;
    }


    public void RefreshUI()
    {
        if (_init == false)
            return;

        // 우선 다 가린다
        Get<Image>((int)Images.Slot_Armor).enabled = false;
        Get<Image>((int)Images.Slot_Boots).enabled = false;
        Get<Image>((int)Images.Slot_Helmet).enabled = false;
        Get<Image>((int)Images.Slot_Shield).enabled = false;
        Get<Image>((int)Images.Slot_Weapon).enabled = false;

        // 채워준다
        foreach (Item item in Managers.Inven.Items.Values)
        {
            if (item.Equipped == false)
                continue;

            ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(item.TemplateId, out itemData);
            Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);

            if(item.ItemType == ItemType.Weapon)
            {
                Get<Image>((int)Images.Slot_Weapon).enabled = true;
                Get<Image>((int)Images.Slot_Weapon).sprite = icon;
            }
            else if(item.ItemType == ItemType.Armor)
            {
                Armor armor = (Armor)item;

                switch (armor.ArmorType)
                {
                    case ArmorType.Helmet:
                        Get<Image>((int)Images.Slot_Helmet).enabled = true;
                        Get<Image>((int)Images.Slot_Helmet).sprite = icon;
                        break;
                    case ArmorType.Armor:
                        Get<Image>((int)Images.Slot_Armor).enabled = true;
                        Get<Image>((int)Images.Slot_Armor).sprite = icon;
                        break;
                    case ArmorType.Boots:
                        Get<Image>((int)Images.Slot_Boots).enabled = true;
                        Get<Image>((int)Images.Slot_Boots).sprite = icon;
                        break;
                }
            }
        }

        // Text
        MyPlayerController player = Managers.Object.MyPlayer;
        player.RefreshAdditionanlStat();

        Get<Text>((int)Texts.NameText).text = player.name;

        int totalDamage = player.Stat.Attack + player.WeaponDamage;
        Get<Text>((int)Texts.AttackValueText).text = $"{totalDamage}(+{player.WeaponDamage})";
        Get<Text>((int)Texts.DefenceValueText).text = $"{player.ArmorDefence}";
    }

    public void BindEvent()
    {
        // Drop이벤트 등록
        BindEvent(Get<Image>((int)Images.Slot_Helmet_BG).gameObject, (e) =>
        {
            UI_Inventory_Item itemInfo = e.pointerDrag.GetComponentInParent<UI_Inventory_Item>();

            Data.ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(itemInfo.TemplateId, out itemData);
            if (itemData == null)
                return;

            if (itemData.itemType != ItemType.Armor)
                return;

            if (itemInfo.ArmorType != ArmorType.Helmet)
                return;

            C_EquipItem equipPacket = new C_EquipItem();
            equipPacket.ItemDbId = itemInfo.ItemDbId;
            equipPacket.Equipped = true;

            Managers.Network.Send(equipPacket);

        }, Define.UIEvent.Drop);
        BindEvent(Get<Image>((int)Images.Slot_Armor_BG).gameObject, (e) =>
        {
            UI_Inventory_Item itemInfo = e.pointerDrag.GetComponentInParent<UI_Inventory_Item>();

            Data.ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(itemInfo.TemplateId, out itemData);
            if (itemData == null)
                return;

            if (itemData.itemType != ItemType.Armor)
                return;

            if (itemInfo.ArmorType != ArmorType.Armor)
                return;

            C_EquipItem equipPacket = new C_EquipItem();
            equipPacket.ItemDbId = itemInfo.ItemDbId;
            equipPacket.Equipped = true;

            Managers.Network.Send(equipPacket);

        }, Define.UIEvent.Drop);
        BindEvent(Get<Image>((int)Images.Slot_Boots_BG).gameObject, (e) =>
        {
            UI_Inventory_Item itemInfo = e.pointerDrag.GetComponentInParent<UI_Inventory_Item>();

            Data.ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(itemInfo.TemplateId, out itemData);
            if (itemData == null)
                return;

            if (itemData.itemType != ItemType.Armor)
                return;

            if (itemInfo.ArmorType != ArmorType.Boots)
                return;

            C_EquipItem equipPacket = new C_EquipItem();
            equipPacket.ItemDbId = itemInfo.ItemDbId;
            equipPacket.Equipped = true;

            Managers.Network.Send(equipPacket);

        }, Define.UIEvent.Drop);
        BindEvent(Get<Image>((int)Images.Slot_Weapon_BG).gameObject, (e) =>
        {
            UI_Inventory_Item itemInfo = e.pointerDrag.GetComponentInParent<UI_Inventory_Item>();

            Data.ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(itemInfo.TemplateId, out itemData);
            if (itemData == null)
                return;

            if (itemData.itemType != ItemType.Weapon)
                return;

            C_EquipItem equipPacket = new C_EquipItem();
            equipPacket.ItemDbId = itemInfo.ItemDbId;
            equipPacket.Equipped = true;

            Managers.Network.Send(equipPacket);

        }, Define.UIEvent.Drop);
        BindEvent(Get<Image>((int)Images.Slot_Shield_BG).gameObject, (e) =>
        {
            UI_Inventory_Item itemInfo = e.pointerDrag.GetComponentInParent<UI_Inventory_Item>();

            Data.ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(itemInfo.TemplateId, out itemData);
            if (itemData == null)
                return;

            if (itemData.itemType != ItemType.Weapon)
                return;

            C_EquipItem equipPacket = new C_EquipItem();
            equipPacket.ItemDbId = itemInfo.ItemDbId;
            equipPacket.Equipped = true;

            Managers.Network.Send(equipPacket);

        }, Define.UIEvent.Drop);

    }
}
