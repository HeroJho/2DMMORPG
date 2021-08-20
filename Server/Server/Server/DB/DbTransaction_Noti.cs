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
                    db.Entry(itemDb).State = EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(ItemDb.Count)).IsModified = true;

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
