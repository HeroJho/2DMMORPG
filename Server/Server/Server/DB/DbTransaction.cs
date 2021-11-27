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

            // Hp같은 경우는 버프로 MaxHp가 증가해서 초과 할 수도 있음
            if (player.Hp > player.Stat.MaxHp)
                player.Hp = player.Stat.MaxHp;

            // Me (GameRoom)
            PlayerDb playerDb = new PlayerDb();
            {
                // TODO : 여러던전 고려
                if(room.Map.MapId == 1)
                {
                    playerDb.PosX = player.PosInfo.PosX;
                    playerDb.PosY = player.PosInfo.PosY;
                }
                else // 던전 안에서 나갓다
                {
                    playerDb.PosX = -33;
                    playerDb.PosY = -36;
                }

                playerDb.PlayerDbId = player.PlayerDbId;
                playerDb.Gold = player.Info.Gold;
                playerDb.Attack = player.Stat.Attack;
                playerDb.Defence = player.Stat.Defence;
                playerDb.MaxHp = player.Stat.MaxHp;
                playerDb.MaxMp = player.Stat.MaxMp;
                playerDb.Hp = player.Stat.Hp;
                playerDb.Mp = player.Stat.Mp;
                playerDb.Str = player.Stat.Str;
                playerDb.Int = player.Stat.Int;
                playerDb.Level = player.Stat.Level;
                playerDb.TotalExp = player.Stat.TotalExp;

                playerDb.JobClassType = (int)player.Stat.JobClassType;
                playerDb.CanUpClass = player.Stat.CanUpClass;
                playerDb.StatPoints = player.Stat.StatPoints;                
            }

            SkillDb skillDb = new SkillDb();
            {
                skillDb.SkillDbId = player.Skill.SkillDbId;
                skillDb.SkillPoints = player.Skill.SkillPoint;
                ConvertIntStringData converData = new ConvertIntStringData();
                converData.SkillPoints = player.Skill.SkillTree.SkillPoints;
                skillDb.ConvertIntToString(converData);
            }

            // You (Db)
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(playerDb).State = EntityState.Unchanged;

                    db.Entry(playerDb).Property(nameof(PlayerDb.PosX)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.PosY)).IsModified = true;

                    db.Entry(playerDb).Property(nameof(PlayerDb.Gold)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Attack)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Defence)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Mp)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.MaxHp)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.MaxMp)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Str)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Int)).IsModified = true;

                    db.Entry(playerDb).Property(nameof(PlayerDb.Level)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.TotalExp)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.JobClassType)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.CanUpClass)).IsModified = true;
                    db.Entry(playerDb).Property(nameof(PlayerDb.StatPoints)).IsModified = true;

                    db.Entry(skillDb).State = EntityState.Unchanged;
                    db.Entry(skillDb).Property(nameof(SkillDb.SkillPoints)).IsModified = true;
                    db.Entry(skillDb).Property(nameof(SkillDb.SkillLevelData)).IsModified = true;

                    bool success = db.SaveChangesEx();
                    if (success)
                    {// 성공했다면 일감을 나한테 돌려줘
                     // Me (GameRoom)
                        room.Push(() => Console.WriteLine($"({playerDb.PlayerDbId})'s Info is saved"));
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
        
        public static void BuyItem(Player player, ItemData iData, int count, bool stackable, GameRoom room)
        {
            // TODO : 거래같은 경우는 아이템과 돈이 한번에 같이 DB에 저장이 되야함.
            // 둘 다 선 메모리가 아니라 Db저장후 메모리 저장으로 해야함

            if (player == null || iData == null || room == null)
                return;

            // 골드 충분한지 먼저 확인
            int gold = 0;
            int miusGold = 0;
            if (stackable)
            {
                gold = player.Info.Gold - (iData.gold * count);
                miusGold = -(iData.gold * count);
            }
            else
            {
                gold = player.Info.Gold - iData.gold;
                miusGold = -iData.gold;
                count = 0;
            }
            if (gold < 0)
                return;

            RewardData rewardData = new RewardData()
            {
                itemId = iData.id,
                count = count
            };

            bool isSusscess = false;
            // count가 없는 일반 보상은 그냥 추가
            if (rewardData.count <= 0)
            {
                isSusscess = Instance.AddNewSlot(player, rewardData, room);
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

                // 장비칸이 충분한지 확인
                if(totalCount > consumableData.maxCount)
                {
                    int emptySlotCount = player.Inven.GetCountEmptySlot();
                    int needSlotCount = totalCount / consumableData.maxCount;
                    if (emptySlotCount <= needSlotCount)
                        isSusscess = false;
                    else
                        isSusscess = true;

                    while (totalCount > consumableData.maxCount && isSusscess)
                    {
                        totalCount -= consumableData.maxCount;

                        RewardData rewardDataCopy = new RewardData()
                        {
                            probability = rewardData.probability,
                            itemId = rewardData.itemId,
                            count = consumableData.maxCount
                        };

                        // 카운터가 넘어서 슬롯을 늘린다면
                        Instance.AddNewSlot(player, rewardDataCopy, room);
                    }

                }

                if (totalCount <= consumableData.maxCount)
                {
                    RewardData rewardDataCopy = new RewardData()
                    {
                        probability = rewardData.probability,
                        itemId = rewardData.itemId,
                        count = totalCount
                    };

                    if (item == null)
                        isSusscess = Instance.AddNewSlot(player, rewardDataCopy, room);
                    else
                    {
                        Instance.AddCountSlot(player, rewardDataCopy, room, item);
                        isSusscess = true;
                    }

                }

            }

            // 아무이상 없이 DB저장 되면 머니 적용
            if(isSusscess)
                player.GetGold(miusGold);

        }
        public static void SellItem(Player player, GameRoom room, Item item, int count)
        {
            if (player == null || room == null || item == null)
                return;

            int totalGetGold = 0;
            ItemData itemData = null;
            if (DataManager.ItemDict.TryGetValue(item.TemplateId, out itemData) == false)
                return;

            totalGetGold = itemData.gold * count;

            // 스택이 가능한가
            if(item.Stackable)
            {
                RemoveCountItem(player, item, count, room);
                player.GetGold(totalGetGold);
            }
            else
            {
                RemoveItem(player, item, room);
                player.GetGold(totalGetGold);
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
        public bool AddNewSlot(Player player, RewardData rewardData, GameRoom room)
        {
            if (player == null || rewardData == null || room == null)
                return false;

            // 살짝 문제가 있긴 하다... > 해결
            int? slot = player.Inven.GetEmptySlot();
            if (slot == null)
                return false;


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

            return true;
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

            // 던전이라면 아이템을 떨구지 않는다
            if (room.RoomId != 1)
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
                    bool success = false;
                    db.Items.Add(itemDb);
                    success = db.SaveChangesEx();
                  
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


