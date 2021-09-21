using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_Inventory : UI_Base
{
    public List<UI_Inventory_Item> Items { get; } = new List<UI_Inventory_Item>();

    public override void Init()
    {
        Items.Clear();

        GameObject grid = transform.Find("ItemGrid").gameObject;
        foreach (Transform child in grid.transform)
            Destroy(child.gameObject);

        for (int i = 0; i < 20; i++)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Inventory_Item", grid.transform);
            UI_Inventory_Item item = go.GetComponent<UI_Inventory_Item>();
            item.Slot = i;
            Items.Add(item);
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (Items.Count == 0)
            return;

        List<Item> items = Managers.Inven.Items.Values.ToList();
        items.Sort((left, right) => { return left.Slot - right.Slot; });

        for (int i = 0; i < 20; i++)
        {
            Items[i].SetItem(null);
        }

        foreach (Item item in items)
        {
            if (item.Slot < 0 || item.Slot >= 20)
                continue;

            Items[item.Slot].SetItem(item);
        }

    }

    public void SetCount(Item item)
    {
        UI_Inventory_Item UIitem = Items.Find(i => i.ItemDbId == item.ItemDbId);
        if (UIitem == null)
            return;

        if(item.Count > 0)
            Items[item.Slot].SetItem(item);
        else
            Items[item.Slot].SetItem(null);

    }

}
