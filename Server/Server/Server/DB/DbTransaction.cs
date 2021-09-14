﻿using System;
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

        // Me (GameRoom) -> You (Db) -> Me (GameRoom)
        public static void SavePlayerQuests_AllInOne(Player player, GameRoom room)
        {
            if (player == null || room == null)
                return;

            // Me (GameRoom)
            QuestManager questManager = player.Quest;

            List<QuestDb> addQuestDbList = new List<QuestDb>();
            List<QuestDb> editQuestList = new List<QuestDb>();

            // 변경, 추가해야하는 퀘스트
            foreach (Quest quest in questManager.Quests.Values)
            {
                // DB에 저장된 퀘스트가 아니다 > 새로운 퀘스트다
                if (quest.QuestDbId == 0)
                {
                    QuestDb questDb = new QuestDb();
                    {
                        questDb.OwnerDbId = player.PlayerDbId;
                        questDb.TmeplateId = quest.QuestId;
                        questDb.QuestState = (int)quest.QuestState;

                        switch (quest.QuestType)
                        {
                            case QuestType.Hunting:
                                {
                                    HuntingQuest huntQuest = (HuntingQuest)quest;
                                    questDb.CurrentNumber = huntQuest.CurrentNumber;
                                }
                                break;
                            case QuestType.Collection:
                                {
                                    CollectionQuest collectionQuest = (CollectionQuest)quest;
                                    questDb.CurrentNumber = collectionQuest.CurrentNumber;
                                }
                                break;
                            case QuestType.Complete:
                                {
                                    CompletingQuest collectionQuest = (CompletingQuest)quest;
                                    // 퀘스트를 완료할 때마다 완료 퀘스트 목록에서 쫙 확인함
                                }
                                break;

                        }

                    }

                    addQuestDbList.Add(questDb);
                }
                else
                {
                    QuestDb questDb = new QuestDb();
                    {
                        questDb.QuestDbId = quest.QuestDbId;
                        questDb.OwnerDbId = player.PlayerDbId;
                        questDb.TmeplateId = quest.QuestId;

                        questDb.QuestState = (int)quest.QuestState;

                        switch (quest.QuestType)
                        {
                            case QuestType.Hunting:
                                {
                                    HuntingQuest huntQuest = (HuntingQuest)quest;
                                    questDb.CurrentNumber = huntQuest.CurrentNumber;
                                }
                                break;
                            case QuestType.Collection:
                                {
                                    CollectionQuest collectionQuest = (CollectionQuest)quest;
                                    questDb.CurrentNumber = collectionQuest.CurrentNumber;
                                }
                                break;
                            case QuestType.Complete:
                                {
                                    CompletingQuest collectionQuest = (CompletingQuest)quest;
                                    // 퀘스트를 완료할 때마다 완료 퀘스트 목록에서 쫙 확인함
                                }
                                break;

                        }
                    }

                    editQuestList.Add(questDb);
                }
                
            }
            // 완료한 퀘스트 > State만 변경됨
            foreach (Quest quest in questManager.CompletedQuests.Values)
            {
                // DB에 저장된 퀘스트가 아니다 > 새로운 퀘스트다
                if (quest.QuestDbId == 0)
                {
                    QuestDb questDb = new QuestDb();
                    {
                        questDb.OwnerDbId = player.PlayerDbId;
                        questDb.TmeplateId = quest.QuestId;
                        questDb.QuestState = (int)quest.QuestState;
                    }

                    addQuestDbList.Add(questDb);
                }
                else
                {
                    QuestDb questDb = new QuestDb();
                    {
                        questDb.QuestDbId = quest.QuestDbId;
                        questDb.OwnerDbId = player.PlayerDbId;
                        questDb.TmeplateId = quest.QuestId;

                        questDb.QuestState = (int)quest.QuestState;

                    }

                    editQuestList.Add(questDb);
                }
            }



            // You (Db)
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    // 추가 되어야할 퀘스트 목록
                    db.Quests.AddRange(addQuestDbList);

                    // 변경되어야할 퀘스트 목록
                    foreach (QuestDb questDb in editQuestList)
                    {
                        db.Entry(questDb).State = EntityState.Unchanged;
                        db.Entry(questDb).Property(nameof(QuestDb.QuestState)).IsModified = true;
                        db.Entry(questDb).Property(nameof(QuestDb.CurrentNumber)).IsModified = true;
                    }

                    bool success = db.SaveChangesEx();
                    if (success)
                    {// 성공했다면 일감을 나한테 돌려줘
                     // Me (GameRoom)
                        room.Push(() => Console.WriteLine($"Quest Saved!"));

                    }
                }
            });
        }

        // 보상 아이템 추가
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
        public static void RewardQuestPlayer(Player player, QuestRewardData rewardData, GameRoom room)
        {
            if (player == null || rewardData == null || room == null)
                return;

            // count가 없는 일반 보상은 그냥 추가
            if (rewardData.count <= 0)
            {
                RewardData rewardDataCopy = new RewardData()
                {
                    itemId = rewardData.itemId,
                };

                Instance.AddNewSlot(player, rewardDataCopy, room);
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
                        itemId = rewardData.itemId,
                        count = consumableData.maxCount
                    };

                    Instance.AddNewSlot(player, rewardDataCopy, room);
                }

                if (totalCount <= consumableData.maxCount)
                {
                    RewardData rewardDataCopy = new RewardData()
                    {
                        itemId = rewardData.itemId,
                        count = totalCount
                    };

                    if (item == null)
                        Instance.AddNewSlot(player, rewardDataCopy, room);
                    else
                        Instance.AddCountSlot(player, rewardDataCopy, room, item);
                }

            }
        }

        // 땅에 떨어진 아이템 추가
        public static void AddItemPlayer(Player player, Item newItem, GameRoom room)
        {
            if (player == null || newItem == null || room == null)
            {
                room.Map.PuseItems.Remove(newItem);
                return;
            }


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

                int maxCount = 0;
                switch (itemData.itemType)
                {
                    case ItemType.Consumable:
                        ConsumableData consumableData = itemData as ConsumableData;
                        maxCount = consumableData.maxCount;
                        break;
                    case ItemType.Collection:
                        CollectionData collectionData = itemData as CollectionData;
                        maxCount = collectionData.maxCount;
                        break;
                    default:
                        return;
                }


                // 동일 종류이고 최대갯수가 아닌 아이템
                Item item = player.Inven.Find(i =>
                i.TemplateId == newItem.TemplateId && i.Count < maxCount);

                int totalCount = 0;
                if (item == null)
                    totalCount = newItem.Count;
                else
                    totalCount = item.Count + newItem.Count;

                while (totalCount > maxCount)
                {
                    totalCount -= maxCount;

                    RewardData rewardDataCopy = new RewardData()
                    {
                        probability = 0,
                        itemId = newItem.TemplateId,
                        count = maxCount
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
                OwnerDbId = player.PlayerDbId,
                PosX = 0,
                PosY = 0,
                RoomId = 0
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

                            // Client Noti
                            {
                                S_AddItem addItemPacket = new S_AddItem();
                                ItemInfo itemInfo = new ItemInfo();
                                itemInfo.MergeFrom(newItem.ItemInfo);
                                addItemPacket.Items.Add(itemInfo);

                                player.Session.Send(addItemPacket);
                                player.Inven.SlotPuse.Remove(newItem.Slot);

                                player.Quest.ProceddWithQuest(newItem.TemplateId);
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

                                player.Quest.ProceddWithQuest(item.TemplateId);
                            }
                        });
                    }
                }
            });
        }

        // AddPlayer
        public void AddNewSlot(Player player, Item newItem, GameRoom room)
        {
            if (player == null || newItem == null || room == null)
                return;

            // TODO : 살짝 문제가 있긴 하다... > 해결
            int? slot = player.Inven.GetEmptySlot();
            if (slot == null)
            {
                room.Map.PuseItems.Remove(newItem);
                return;
            }


            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = newItem.ItemDbId,
                Slot = slot.Value,
                OwnerDbId = player.PlayerDbId,
                RoomId = 0
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
                            room.Map.DeleteGroundItem(newItem);

                            // TODO : Client Noti
                            {
                                S_AddItem addItemPacket = new S_AddItem();
                                ItemInfo itemInfo = new ItemInfo();
                                itemInfo.MergeFrom(newItem.ItemInfo);
                                addItemPacket.Items.Add(itemInfo);

                                player.Session.Send(addItemPacket);
                                player.Inven.SlotPuse.Remove(newItem.Slot);

                                player.Quest.ProceddWithQuest(newItem.TemplateId);
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
            {
                room.Map.PuseItems.Remove(newItem);
                return;
            }

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
                            room.Map.DeleteGroundItem(newItem);

                            // TODO : Client Noti
                            {
                                S_AddItem addItemPacket = new S_AddItem();
                                ItemInfo itemInfo = new ItemInfo();
                                itemInfo.MergeFrom(newItem.ItemInfo);
                                addItemPacket.Items.Add(itemInfo);

                                player.Session.Send(addItemPacket);
                                player.Inven.SlotPuse.Remove(newItem.Slot);

                                player.Quest.ProceddWithQuest(newItem.TemplateId);
                            }
                        });
                    }
                }
            });
        }

        public void MergeCountSlot(Player player, Item item, Item newItem, GameRoom room, int totalCount)
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
                ItemDbId = newItem.ItemDbId,
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
                            room.Map.DeleteGroundItem(newItem);

                            // Client Noti
                            {
                                S_SetCountConsumable usingConsumablePacket = new S_SetCountConsumable();
                                usingConsumablePacket.ItemDbId = item.ItemDbId;
                                usingConsumablePacket.Count = item.Count;

                                player.Session.Send(usingConsumablePacket);

                                player.Quest.ProceddWithQuest(newItem.TemplateId);
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
                PosX = pos.x,
                PosY = pos.y,
                RoomId = room.RoomId
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
                            newItem.Id = ObjectManager.Instance.GenerateId(newItem.ObjectType);
                            room.Map.DropItemToMap(pos, newItem);

                            // 이제 떨어진 아이템을 바로 보내는게 아니라
                            // 방에 입장해서 Zone적용 시킴
                            room.EnterGame(newItem);

                        });
                    }
                }
            });
        }

        public static void RemoveItem(Player player, Item item, GameRoom room)
        {
            if (player == null || item == null || room == null)
                return;

            // Me
            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = item.ItemDbId
            };

            // You
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Items.Remove(itemDb);

                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        // Me
                        room.Push(() =>
                        {
                            player.Inven.Remove(item.ItemDbId);

                            // Client Noti
                            {
                                S_RemoveItem removeItemPacket = new S_RemoveItem();
                                removeItemPacket.ItemDbId = item.ItemDbId;

                                player.Session.Send(removeItemPacket);

                                player.Quest.ProceddWithQuest(item.TemplateId);
                            }
                        });
                    }
                }
            });

        }

        public static void RemoveCountItem(Player player, Item item, int count, GameRoom room)
        {
            if (player == null || item == null || room == null)
                return;

            int totalCount = item.Count - count;

            if (totalCount < 0)
                return;
            else if(totalCount == 0)
            {
                RemoveItem(player, item, room);
                return;
            }

            // Me
            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = item.ItemDbId,
                Count = totalCount
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
                            item.Count = totalCount;

                            // Client Noti
                            {
                                S_SetCountConsumable usingConsumablePacket = new S_SetCountConsumable();
                                usingConsumablePacket.ItemDbId = item.ItemDbId;
                                usingConsumablePacket.Count = item.Count;

                                player.Session.Send(usingConsumablePacket);

                                // 똑같은 아이템 퀘스트에서 갯수 이하로 떨어지면 완료 불가능 해야하니
                                player.Quest.ProceddWithQuest(item.TemplateId);
                            }
                        });
                    }
                }
            });

        }
    }
}


