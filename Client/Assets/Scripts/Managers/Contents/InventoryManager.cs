using System;
using System.Collections;
using System.Collections.Generic;
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
}
