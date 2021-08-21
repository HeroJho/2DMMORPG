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
    Image _frame;

    [SerializeField]
    Text _countText;

    public int ItemDbId { get; private set; }
    public int TemplateId { get; private set; }
    public int Count { get; private set; }
    public bool Stackable { get; private set; }
    public bool Equipped { get; private set; }

    public override void Init()
    {
        BindEvent(_icon.gameObject, (e) =>
        {
            Debug.Log("Click Item");

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
        });
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
}
