using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory_Item : UI_Base
{
    public Image Icon;

    [SerializeField]
    Image _dragIcon;
    CanvasGroup _canvasGroup;
    Image _frontDragIcon;

    [SerializeField]
    Image _frame;

    [SerializeField]
    Text _countText;

    public int ItemDbId { get; private set; }
    public int TemplateId { get; private set; }
    public int Count { get; private set; }
    public bool Stackable { get; private set; }
    public ArmorType ArmorType { get; private set; }
    public bool Equipped { get; private set; }


    public override void Init()
    {
        // 드래그 UI가 가림 > 최상위 부모에 생성된 UI로 덮어줌
        _frontDragIcon = transform.root.GetComponentInChildren<UI_FrontDragIcon>().GetComponent<Image>();
        
        _canvasGroup = _dragIcon.GetComponent<CanvasGroup>();

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

            Icon.gameObject.SetActive(false);
            _dragIcon.gameObject.SetActive(false);
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
            
            // UI_Stat Drop할 때 확인용
            if(item.ItemType == ItemType.Armor)
            {
                Armor armor = (Armor)item;
                ArmorType = armor.ArmorType;
            }

            Data.ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);

            Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
            Icon.sprite = icon;
            _dragIcon.sprite = icon;

            Icon.gameObject.SetActive(true);
            _dragIcon.gameObject.SetActive(true);
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
        BindEvent(_dragIcon.gameObject, (e) =>
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
        }, Define.UIEvent.RightClick);

        // 드래그
        BindEvent(_dragIcon.gameObject, (e) =>
        {
            _frontDragIcon.transform.position = e.position;
            _frontDragIcon.sprite = Icon.sprite;
            _frontDragIcon.enabled = true;

            _dragIcon.transform.position = e.position;
            _canvasGroup.blocksRaycasts = false;

        }, Define.UIEvent.Drag);
        // 드래그를 풀면 제자리로
        BindEvent(_dragIcon.gameObject, (e) =>
        {
            _frontDragIcon.enabled = false;
            _frontDragIcon.transform.position = Vector3.zero;

            _canvasGroup.blocksRaycasts = true;
            _dragIcon.transform.position = Icon.transform.position;

        }, Define.UIEvent.Click_Up);

    }

    IEnumerator ReSpawnDragIcon()
    {
        yield return new WaitForSeconds(1f);

        if (_dragIcon.transform.position != Icon.transform.position)
        {
            _dragIcon.transform.position = Icon.transform.position;
            _canvasGroup.blocksRaycasts = true;
        }
    }
}
