using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Server.Data;
using Google.Protobuf.Protocol;

namespace Server.DB
{
    public partial class DbTransaction : JobSerializer
    {
        public static DbTransaction Instance { get; } = new DbTransaction();

        // Me (GameRoom) -> You (Db) -> Me (GameRoom)
        public static void SavePlayerStatus_AllInOne(Player player, GameRoom room)
        {
            if (player == null || room == null)
                return;

            // Me (GameRoom)
            PlayerDb playerDb = new PlayerDb();
            {
                playerDb.PlayerDbId = player.PlayerDbId;
                playerDb.Hp = player.Stat.Hp;
                playerDb.Mp = player.Stat.Mp;
                playerDb.Level = player.Stat.Level;
                playerDb.TotalExp = player.Stat.TotalExp;
            }

            // You (Db)
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(playerDb).State = EntityState.Unchanged;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Mp)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Level)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.TotalExp)).IsModified = true;

                    bool success = db.SaveChangesEx();
                    if (success)
                    {// 성공했다면 일감을 나한테 돌려줘
                     // Me (GameRoom)
                        room.Push(() => Console.WriteLine($"Hp Saved({playerDb.Hp})"));
                        room.Push(() => Console.WriteLine($"Mp Saved({playerDb.Mp})"));
                    }
                }
            });
        }

        public static void RewardPlayer(Player player, RewardData rewardData, GameRoom room)
        {
            if (player == null || rewardData == null || room == null)
                return;

            // count가 없는 일반 보상은 그냥 추가
            if (rewardData.count <= 0)
            {
                Instance.AddNewSlot(player, rewardData, room);
            }
            else // count가 있다면 countMax따져서 추가
            {
                ItemData itemData = null;
                DataManager.ItemDict.TryGetValue(rewardData.itemId, out itemData);
                if (itemData == null)
                    return;
                ConsumableData consumableData = itemData as ConsumableData;
                if (consumableData == null)
                    return;

                // 동일 종류이고 최대갯수가 아닌 아이템
                Item item = player.Inven.Find(i =>
                i.TemplateId == rewardData.itemId && i.Count < consumableData.maxCount);

                int totalCount = 0;
                if (item == null)
                    totalCount = rewardData.count;
                else
                    totalCount = item.Count + rewardData.count;

                while (totalCount > consumableData.maxCount)
                {
                    totalCount -= consumableData.maxCount;

                    RewardData rewardDataCopy = new RewardData()
                    {
                        probability = rewardData.probability,
                        itemId = rewardData.itemId,
                        count = consumableData.maxCount
                    };

                    Instance.AddNewSlot(player, rewardDataCopy, room);
                }

                if (totalCount <= consumableData.maxCount)
                {
                    RewardData rewardDataCopy = new RewardData()
                    {
                        probability = rewardData.probability,
                        itemId = rewardData.itemId,
                        count = totalCount
                    };

                    if(item == null)
                        Instance.AddNewSlot(player, rewardDataCopy, room);
                    else
                        Instance.AddCountSlot(player, rewardDataCopy, room, item);
                }

            }
        }

        public static void AddItemPlayer(Player player, Item newItem, GameRoom room)
        {
            if (player == null || newItem == null || room == null)
                return;

            // 쌓을 수 있는가
            if (!newItem.Stackable)
            {
                Instance.AddNewSlot(player, newItem, room);
            }
            else
            {
                ItemData itemData = null;
                DataManager.ItemDict.TryGetValue(newItem.TemplateId, out itemData);
                if (itemData == null)
                    return;
                ConsumableData consumableData = itemData as ConsumableData;
                if (consumableData == null)
                    return;

                // 동일 종류이고 최대갯수가 아닌 아이템
                Item item = player.Inven.Find(i =>
                i.TemplateId == newItem.TemplateId && i.Count < consumableData.maxCount);

                int totalCount = 0;
                if (item == null)
                    totalCount = newItem.Count;
                else
                    totalCount = item.Count + newItem.Count;

                while (totalCount > consumableData.maxCount)
                {
                    totalCount -= consumableData.maxCount;

                    RewardData rewardDataCopy = new RewardData()
                    {
                        probability = 0,
                        itemId = newItem.TemplateId,
                        count = consumableData.maxCount
                    };

                    Instance.AddNewSlot(player, rewardDataCopy, room);
                }

                if (item == null)
                    Instance.AddNewSlot(player, newItem, room, totalCount);
                else
                {
                    Instance.MergeCountSlot(player, item, newItem, room, totalCount);
                }

            }
        }

        // RewardPlayer
        public void AddNewSlot(Player player, RewardData rewardData, GameRoom room)
        {
            if (player == null || rewardData == null || room == null)
                return;

            // TODO : 살짝 문제가 있긴 하다... > 해결
            int? slot = player.Inven.GetEmptySlot();
            if (slot == null)
                return;

            // Me
            ItemDb itemDb = new ItemDb()
            {
                TemplateId = rewardData.itemId,
                Count = rewardData.count,
                Slot = slot.Value,
                OwnerDbId = player.PlayerDbId
            };

            // You
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Items.Add(itemDb);
                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        // Me
                        room.Push(() =>
                        {
                            Item newItem = Item.MakeItem(itemDb);
                            player.Inven.Add(newItem);

                            // TODO : Client Noti
                            {
                                S_AddItem addItemPacket = new S_AddItem();
                                ItemInfo itemInfo = new ItemInfo();
                                itemInfo.MergeFrom(newItem.Info);
                                addItemPacket.Items.Add(itemInfo);

                                player.Session.Send(addItemPacket);
                                player.Inven.SlotPuse.Remove(newItem.Slot);
                            }
                        });
                    }
                }
            });
        }

        public void AddCountSlot(Player player, RewardData rewardData, GameRoom room, Item item)
        {
            // Me
            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = item.ItemDbId,
                Count = rewardData.count,
            };

            // You
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(itemDb).State = EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(ItemDb.Count)).IsModified = true;

                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        // Me
                        room.Push(() =>
                        {
                            item.Count = rewardData.count;

                            // TODO : Client Noti
                            {
                                S_SetCountConsumable usingConsumablePacket = new S_SetCountConsumable();
                                usingConsumablePacket.ItemDbId = item.ItemDbId;
                                usingConsumablePacket.Count = item.Count;

                                player.Session.Send(usingConsumablePacket);
                            }
                        });
                    }
                }
            });
        }

        // AddPlayer
        public void MergeCountSlot(Player player, Item item, Item newitem, GameRoom room, int totalCount)
        {
            //newitem은 Db에서 제거해줌

            // Me
            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = item.ItemDbId,
                Count = totalCount
            };

            ItemDb newItemDb = new ItemDb()
            {
                ItemDbId = newitem.ItemDbId,
            };

            // You
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(itemDb).State = EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(ItemDb.Count)).IsModified = true;
                                        
                    db.Items.Remove(newItemDb);

                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        // Me
                        room.Push(() =>
                        {
                            item.Count = totalCount;

                            // TODO : Client Noti
                            {
                                S_SetCountConsumable usingConsumablePacket = new S_SetCountConsumable();
                                usingConsumablePacket.ItemDbId = item.ItemDbId;
                                usingConsumablePacket.Count = item.Count;

                                player.Session.Send(usingConsumablePacket);

                                S_DeleteGroundItem deletePacket = new S_DeleteGroundItem();
                                deletePacket.ItemDbId = newItemDb.ItemDbId;

                                player.Session.Send(deletePacket);
                            }
                        });
                    }
                }
            });
        }

        public void AddNewSlot(Player player, Item newItem, GameRoom room)
        {
            if (player == null || newItem == null || room == null)
                return;

            // TODO : 살짝 문제가 있긴 하다... > 해결
            int? slot = player.Inven.GetEmptySlot();
            if (slot == null)
                return;

            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = newItem.ItemDbId,
                Slot = slot.Value,
                OwnerDbId = player.PlayerDbId
            };

            // You
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(itemDb).State = EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(ItemDb.Slot)).IsModified = true;
                    db.Entry(itemDb).Property(nameof(ItemDb.OwnerDbId)).IsModified = true;

                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        // Me
                        room.Push(() =>
                        {
                            newItem.Slot = slot.Value;
                            player.Inven.Add(newItem);

                            // TODO : Client Noti
                            {
                                S_AddItem addItemPacket = new S_AddItem();
                                ItemInfo itemInfo = new ItemInfo();
                                itemInfo.MergeFrom(newItem.Info);
                                addItemPacket.Items.Add(itemInfo);

                                player.Session.Send(addItemPacket);
                                player.Inven.SlotPuse.Remove(newItem.Slot);

                                S_DeleteGroundItem deletePacket = new S_DeleteGroundItem();
                                deletePacket.ItemDbId = newItem.ItemDbId;

                                player.Session.Send(deletePacket);
                            }
                        });
                    }
                }
            });
        }

        public void AddNewSlot(Player player, Item newItem, GameRoom room, int totalCount)
        {
            if (player == null || newItem == null || room == null)
                return;

            // TODO : 살짝 문제가 있긴 하다... > 해결
            int? slot = player.Inven.GetEmptySlot();
            if (slot == null)
                return;

            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = newItem.ItemDbId,
                Slot = slot.Value,
                OwnerDbId = player.PlayerDbId,
                Count = totalCount
            };

            // You
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(itemDb).State = EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(ItemDb.Slot)).IsModified = true;
                    db.Entry(itemDb).Property(nameof(ItemDb.OwnerDbId)).IsModified = true;
                    db.Entry(itemDb).Property(nameof(ItemDb.Count)).IsModified = true;

                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        // Me
                        room.Push(() =>
                        {
                            newItem.Slot = slot.Value;
                            newItem.Count = totalCount;
                            player.Inven.Add(newItem);

                            // TODO : Client Noti
                            {
                                S_AddItem addItemPacket = new S_AddItem();
                                ItemInfo itemInfo = new ItemInfo();
                                itemInfo.MergeFrom(newItem.Info);
                                addItemPacket.Items.Add(itemInfo);

                                player.Session.Send(addItemPacket);
                                player.Inven.SlotPuse.Remove(newItem.Slot);

                                S_DeleteGroundItem deletePacket = new S_DeleteGroundItem();
                                deletePacket.ItemDbId = newItem.ItemDbId;

                                player.Session.Send(deletePacket);
                            }
                        });
                    }
                }
            });
        }


        public static void DropItem(Vector2Int pos, RewardData rewardData, GameRoom room)
        {
            if (rewardData == null || room == null)
                return;

            // Me
            ItemDb itemDb = new ItemDb()
            {
                TemplateId = rewardData.itemId,
                Count = rewardData.count,
            };

            // You
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Items.Add(itemDb);
                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        // Me
                        room.Push(() =>
                        {
                            Item newItem = Item.MakeItem(itemDb);
                            room.Map.DropItemToMap(pos, newItem);

                            //Client Noti
                            {
                                S_DropItem dropItemPacket = new S_DropItem()
                                {
                                    PosInfo = new PositionInfo(),
                                    ItemInfo = new ItemInfo()
                                };
                                dropItemPacket.PosInfo.PosX = pos.x;
                                dropItemPacket.PosInfo.PosY = pos.y;
                                dropItemPacket.ItemInfo.MergeFrom(newItem.Info);

                                room.Broadcast(pos, dropItemPacket);
                            }
                        });
                    }
                }
            });
        }
    }
}


