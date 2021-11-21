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
        public static void EquipItemNoti(Player player, Item item)
        {
            if (player == null || item == null)
                return;

            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = item.ItemDbId,
                Equipped = item.Equipped
            };

            // You
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(itemDb).State = EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(ItemDb.Equipped)).IsModified = true;

                    bool success = db.SaveChangesEx();
                    if (!success)
                    {
                        // 실패 > Kick
                    }
                }
            });
        }

        public static void UseConsumableItemNoti(Player player, Item item)
        {
            // 갯수와함께 사용했다는 패킷 전송
            // 선 메모리 적용 DB 통보
            // *포션 같은 경우는 빠르게 사용 되어야 하니 선 메모리 적용
            // *주문서 같은 경우는 DB 적용후 메모리

            if (player == null || item == null)
                return;

            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = item.ItemDbId,
                Count = item.Count
            };

            // You
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    if(itemDb.Count > 0)
                    {
                        db.Entry(itemDb).State = EntityState.Unchanged;
                        db.Entry(itemDb).Property(nameof(ItemDb.Count)).IsModified = true;
                    }
                    else // 아이템 전부 소진시 DB에서 삭제
                    {
                        db.Items.Remove(itemDb);
                    }

                    bool success = db.SaveChangesEx();
                    if (!success)
                    {
                        // 실패 > Kick
                    }
                }
            });
        }

        public static void ChangeItemSlotNoti(Player player, Item item)
        {
            if (player == null || item == null)
                return;

            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = item.ItemDbId,
                Slot = item.Slot
            };

            // You
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(itemDb).State = EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(ItemDb.Slot)).IsModified = true;
 
                    bool success = db.SaveChangesEx();
                    if (!success)
                    {
                        // 실패 > Kick
                    }
                }
            });
        }

        public static void ChangeGoldNoti(Player player, int gold)
        {
            if (player == null || gold == 0)
                return;

            // 메모리 선 저장
            player.Info.Gold += gold;

            // Me (GameRoom)
            PlayerDb playerDb = new PlayerDb();
            {
                playerDb.PlayerDbId = player.PlayerDbId;
                playerDb.Gold = player.Info.Gold;
            }

            // You
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(playerDb).State = EntityState.Unchanged;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Gold)).IsModified = true;

                    bool success = db.SaveChangesEx();
                    if (!success)
                    {
                        // 실패 > Kick
                    }
                }
            });
        }

    }
}
