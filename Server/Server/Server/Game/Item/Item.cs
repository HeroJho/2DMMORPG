using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Item : GameObject
    {
        public ItemInfo ItemInfo { get; } = new ItemInfo();

        public int ItemDbId
        {
            get { return ItemInfo.ItemDbId; }
            set { ItemInfo.ItemDbId = value; }
        }

        public int TemplateId
        {
            get { return ItemInfo.TemplateId; }
            set { ItemInfo.TemplateId = value; }
        }

        public int Count
        {
            get { return ItemInfo.Count; }
            set { ItemInfo.Count = value; }
        }

        public int Slot
        {
            get { return ItemInfo.Slot; }
            set { ItemInfo.Slot = value; }
        }

        public bool Equipped
        {
            get { return ItemInfo.Equipped; }
            set { ItemInfo.Equipped = value; }
        }

        public ItemType ItemType { get; private set; }
        public bool Stackable { get; protected set; }

        public Item(ItemType itemType)
        {
            ObjectType = GameObjectType.Item;
            ItemType = itemType;
            Info.ItemInfo = ItemInfo;
        }

        public static Item MakeItem(ItemDb itemDb)
        {
            Item item = null;

            ItemData itemData = null;
            DataManager.ItemDict.TryGetValue(itemDb.TemplateId, out itemData);

            if (itemData == null)
                return null;

            switch (itemData.itemType)
            {
                case ItemType.Weapon:
                    item = new Weapon(itemDb.TemplateId);
                    break;
                case ItemType.Armor:
                    item = new Armor(itemDb.TemplateId);
                    break;
                case ItemType.Consumable:
                    item = new Consumable(itemDb.TemplateId);
                    break;
            }

            if(item != null)
            {
                item.ItemDbId = itemDb.ItemDbId;
                item.Count = itemDb.Count;
                item.Slot = itemDb.Slot;
                item.Equipped = itemDb.Equipped;

                // 떨어진 아이템 위치
                item.PosInfo.PosX = itemDb.PosX.Value;
                item.PosInfo.PosY = itemDb.PosY.Value;
            }

            return item;
        }

        public static Item CopyItem(Item newItem)
        {
            Item item = null;

            ItemData itemData = null;
            DataManager.ItemDict.TryGetValue(newItem.TemplateId, out itemData);

            if (itemData == null)
                return null;

            switch (itemData.itemType)
            {
                case ItemType.Weapon:
                    item = new Weapon(newItem.TemplateId);
                    break;
                case ItemType.Armor:
                    item = new Armor(newItem.TemplateId);
                    break;
                case ItemType.Consumable:
                    item = new Consumable(newItem.TemplateId);
                    break;
            }

            if (item != null)
            {
                //item.ItemDbId = rewardData.ItemDbId;
                item.Count = newItem.Count;
                //item.Slot = 0;
                item.Equipped = false;
            }

            return item;
        }
    }

    public class Weapon : Item
    {
        public WeaponType WeaponType { get; private set; }
        public int Damage { get; private set; }

        public Weapon(int templateId) : base(ItemType.Weapon)
        {
            Init(templateId);
        }

        void Init(int templateId)
        {
            ItemData itemData = null;
            DataManager.ItemDict.TryGetValue(templateId, out itemData);
            if (itemData.itemType != ItemType.Weapon)
                return;

            WeaponData data = (WeaponData)itemData;
            {
                TemplateId = data.id;
                Count = 1;
                WeaponType = data.weaponType;
                Damage = data.damage;
                Stackable = false;
            }
        }
    }

    public class Armor : Item
    {
        public ArmorType ArmorType { get; private set; }
        public int Defence { get; private set; }

        public Armor(int templateId) : base(ItemType.Armor)
        {
            Init(templateId);
        }

        void Init(int templateId)
        {
            ItemData itemData = null;
            DataManager.ItemDict.TryGetValue(templateId, out itemData);
            if (itemData.itemType != ItemType.Armor)
                return;

            ArmorData data = (ArmorData)itemData;
            {
                TemplateId = data.id;
                Count = 1;
                ArmorType = data.armorType;
                Defence = data.defence;
                Stackable = false;
            }
        }
    }

    public class Consumable : Item
    {
        public ConsumableType ConsumableType { get; private set; }
        public int MaxCount { get; private set; }

        public Consumable(int templateId) : base(ItemType.Consumable)
        {
            Init(templateId);
        }

        void Init(int templateId)
        {
            ItemData itemData = null;
            DataManager.ItemDict.TryGetValue(templateId, out itemData);
            if (itemData.itemType != ItemType.Consumable)
                return;

            ConsumableData data = (ConsumableData)itemData;
            {
                TemplateId = data.id;
                Count = 1;
                ConsumableType = data.consumableType;
                MaxCount = data.maxCount;
                Stackable = (data.maxCount > 1);
            }
        }
    }

}
