using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager
{
    public Dictionary<int, Item> Items { get; } = new Dictionary<int, Item>();

    public void Add(Item item)
    {
        Items.Add(item.ItemDbId, item);
    }

    public Item Get(int itemDbId)
    {
        Item item = null;
        Items.TryGetValue(itemDbId, out item);
        return item;
    }

    public Item Find(Func<Item, bool> condition)
    {
        foreach (Item item in Items.Values)
        {
            if (condition.Invoke(item))
                return item;
        }

        return null;
    }

    public bool Remove(int itemDbId)
    {
        if (Items.Remove(itemDbId) == false)
            return false;

        return true;
    }

    public void TryToRemoveItem(int itemDbId)
    {
        C_RemoveItem removeItem = new C_RemoveItem();
        removeItem.ItemDbId = itemDbId;

        Managers.Network.Send(removeItem);
    }

    public List<Item> FindToList(Func<Item, bool> condition)
    {
        List<Item> items = new List<Item>();

        foreach (Item item in Items.Values)
        {
            if (condition.Invoke(item))
                items.Add(item);
        }

        return items;
    }

    public void Clear()
    {
        Items.Clear();
    }

    public void ChangeSlot(int itemDbId, int slot)
    {
        Item item = Get(itemDbId);

        Item changeItem = FindItemBySlot(slot);
        if (changeItem == null)
        {
            item.Slot = slot;
        }
        else
        {
            changeItem.Slot = item.Slot;
            item.Slot = slot;
        }

        C_ChangeSlot changeSlotPacket = new C_ChangeSlot();
        changeSlotPacket.ItemDbId = itemDbId;
        changeSlotPacket.Slot = slot;
        Managers.Network.Send(changeSlotPacket);
    }

    public Item FindItemBySlot(int slot)
    {
        Item item = Items.Values.FirstOrDefault(i => i.Slot == slot);
        return item;
    }
}
