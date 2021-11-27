using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public partial class GameRoom : JobSerializer
    {
        public void HandleEquipItem(Player player, C_EquipItem equipPacket)
        {
            if (player == null)
                return;

            player.HandleEquipItem(equipPacket);
        }

        public void HandleConsumeable(Player player, C_SetCountConsumable useConsumablePacket)
        {
            if (player == null)
                return;

            // 인벤에 확인
            Item item = player.Inven.Find(i =>
            {
                if (i.ItemDbId == useConsumablePacket.ItemDbId)
                    return true;
                return false;
            });

            if (item == null)
                return;

            // 갯수 확인
            if (item.Count <= 0)
                return;

            Consumable consumableItem = (Consumable)item;
            if (consumableItem == null)
                return;

            ItemData itemData = null;
            if (DataManager.ItemDict.TryGetValue(consumableItem.TemplateId, out itemData) == false)
                return;
            if (itemData == null)
                return;

            ConsumableData consumableData = (ConsumableData)itemData;
            if (consumableData == null)
                return;

            // 아이템 Type분기
            switch (consumableItem.ConsumableType)
            {
                // 아이템 사용
                // *체력 같은 경우는 몬스터도 회복가능
                // *GameObject에 함수를 만들어 오버라이드해서 사용하기
                case ConsumableType.Potion:
                    {
                        if(consumableData.posionType == PosionType.Hp)
                            player.RecoveryHp(consumableData.recovery);
                        else if (consumableData.posionType == PosionType.Mp)
                            player.RecoveryMp(consumableData.recovery);
                    }
                    break;
            }

            // 갯수와함께 사용했다는 패킷 전송
            // 선 메모리 적용 DB 통보
            // *포션 같은 경우는 빠르게 사용 되어야 하니 선 메모리 적용
            // *주문서 같은 경우는 DB 적용후 메모리
            item.Count--;

            // 소진하면 마찬가지로
            // 선 메모리 삭제 후 DB처리
            if(item.Count <= 0)
                player.Inven.Items.Remove(item.ItemDbId);

            DbTransaction.UseConsumableItemNoti(player, item);

            // 아이템 사용 클라 통보
            S_SetCountConsumable usingConsumablePacket = new S_SetCountConsumable();
            usingConsumablePacket.ItemDbId = item.ItemDbId;
            usingConsumablePacket.Count = item.Count;

            player.Session.Send(usingConsumablePacket);
            // 아이템 사용시 퀘스트 진행
            player.Quest.ProceddWithQuest(item.TemplateId);
        }

        public void HandleDropItem(Player player, C_GetDropItem dropItempacket)
        {
            GameRoom room = player.Room;
            if (player == null || room == null)
                return;

            Vector2Int pos = new Vector2Int(dropItempacket.PosInfo.PosX, dropItempacket.PosInfo.PosY);

            // 아이템을 찾는다
            Item item = Map.FindGroundItem(pos, dropItempacket.ItemInfo);
            if (item == null)
                return;

            // 연속으로 같은 아이템 Db에 시도하는 걸 방지 EX) Slot
            if (room.Map.PuseItems.Contains(item))
                return;
            room.Map.PuseItems.Add(item);

            // DB저장 후 인벤 추가 & 해당 좌표 템 삭제
            DbTransaction.AddItemPlayer(player, item, room);
        }

        public void HandleRemoveItem(Player player, C_RemoveItem removeItemPacket)
        {
            GameRoom room = player.Room;

            if (player == null || room == null)
                return;

            int itemDbId = removeItemPacket.ItemDbId;
            Item item = player.Inven.Get(itemDbId);

            if (item == null)
                return;

            DbTransaction.RemoveItem(player, item, room);

        }
    
        public void HandleChangeSlot(Player player, C_ChangeSlot changeSlotPacket)
        {
            // 슬롯 같은 경우는 이동 패킷처럼 클라에 먼저 적용하고 서버에 적용
            GameRoom room = player.Room;

            if (player == null || room == null)
                return;

            // 메모리 상 이동
            int itemDbId = changeSlotPacket.ItemDbId;
            int changeSlot = changeSlotPacket.Slot;

            Item item = player.Inven.Get(itemDbId);
            if (item == null)
                return;

            Item changeItem = player.Inven.FindItemBySlot(changeSlot);
            if(changeItem == null)
            {
                item.Slot = changeSlot;
                DbTransaction.ChangeItemSlotNoti(player, item);
            }
            else
            {
                changeItem.Slot = item.Slot;
                item.Slot = changeSlot;
                DbTransaction.ChangeItemSlotNoti(player, item);
                DbTransaction.ChangeItemSlotNoti(player, changeItem);
            }

        }
    
        public void HandleBuyItem(Player player, C_BuyItem buyItemPacket)
        {
            GameRoom room = player.Room;
            if (player == null || room == null)
                return;

            ItemData itemData = null;
            if (DataManager.ItemDict.TryGetValue(buyItemPacket.ItemId, out itemData) == false)
                return;

            // 중첩이 가능한 아이템이냐
            int gold = 0;
            if (buyItemPacket.Stackable)
                gold = player.Info.Gold - (itemData.gold * buyItemPacket.Count);
            else
                gold = player.Info.Gold - itemData.gold;

            if (gold < 0)
            {
                player.SendMassage("골드가 모자랍니다!", false);
                return;
            }

            DbTransaction.BuyItem(player, itemData, buyItemPacket.Count, buyItemPacket.Stackable, room);

        }

        public void HandleSellItem(Player player, C_SellItem sellItemPacket)
        {
            GameRoom room = player.Room;
            if (player == null || room == null)
                return;

            // 해당 아이템이 있는지
            Item item = player.Inven.Get(sellItemPacket.ItemDbId);
            if (item == null)
                return;

            // 스택이 가능한지
            if(item.Stackable)
            {
                // 갯수가 충분한지
                if (item.Count < sellItemPacket.Count)
                    return;
            }

            // 적용
            DbTransaction.SellItem(player, room, item, sellItemPacket.Count);

        }

    }
    
}
