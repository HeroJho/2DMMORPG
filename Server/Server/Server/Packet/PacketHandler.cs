using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.DB;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    class PacketHandler
    {
        public static void C_MoveHandler(PacketSession session, IMessage packet)
        {
            C_Move movePacket = (C_Move)packet;
            ClientSession clientSession = (ClientSession)session;

            //Console.WriteLine($"PlayerID :{clientSession.MyPlayer.Info.ObjectId} ({movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY})");

            Player player = clientSession.MyPlayer;
            if (player == null)
                return;

            GameRoom room = player.Room;
            if (room == null)
                return;

            room.Push(room.HandleMove, player, movePacket);
        }

        public static void C_SkillHandler(PacketSession session, IMessage packet)
        {
            C_Skill skillPacket = (C_Skill)packet;
            ClientSession clientSession = (ClientSession)session;

            Player player = clientSession.MyPlayer;
            if (player == null)
                return;

            GameRoom room = player.Room;
            if (room == null)
                return;

            room.Push(room.HandleSkill, player, skillPacket);
        }

        public static void C_LoginHandler(PacketSession session, IMessage packet)
        {
            C_Login loginPacket = (C_Login)packet;
            ClientSession clientSession = (ClientSession)session;

            clientSession.HandleLogin(loginPacket);
        }

        public static void C_EnterGameHandler(PacketSession session, IMessage packet)
        {
            C_EnterGame enterPacket = (C_EnterGame)packet;
            ClientSession clientSession = (ClientSession)session;

            clientSession.HandleEnterGame(enterPacket);
        }

        public static void C_CreatePlayerHandler(PacketSession session, IMessage packet)
        {
            C_CreatePlayer createPacket = (C_CreatePlayer)packet;
            ClientSession clientSession = (ClientSession)session;

            clientSession.HandleCreatePlayer(createPacket);
        }

        public static void C_EquipItemHandler(PacketSession session, IMessage packet)
        {
            C_EquipItem equipPacket = (C_EquipItem)packet;
            ClientSession clientSession = (ClientSession)session;

            Player player = clientSession.MyPlayer;
            if (player == null)
                return;

            GameRoom room = player.Room;
            if (room == null)
                return;

            room.Push(room.HandleEquipItem, player, equipPacket);
        }

        public static void C_PongHandler(PacketSession session, IMessage packet)
        {
            ClientSession clientSession = (ClientSession)session;
            clientSession.HandlePong();
        }

        public static void C_SetCountConsumableHandler(PacketSession session, IMessage packet)
        {
            ClientSession clientSession = (ClientSession)session;
            C_SetCountConsumable useConsumablePacket = (C_SetCountConsumable)packet;

            Player player = clientSession.MyPlayer;
            if (player == null)
                return;

            GameRoom room = player.Room;
            if (room == null)
                return;

            room.Push(room.HandleConsumeable, player, useConsumablePacket);
        }

        public static void C_GetDropItemHandler(PacketSession session, IMessage packet)
        {
            ClientSession clientSession = (ClientSession)session;
            C_GetDropItem dropItemPacket = (C_GetDropItem)packet;

            Player player = clientSession.MyPlayer;
            if (player == null)
                return;

            GameRoom room = player.Room;
            if (room == null)
                return;

            room.Push(room.HandleDropItem, player, dropItemPacket);
        }

        public static void C_RemoveItemHandler(PacketSession session, IMessage packet)
        {
            ClientSession clientSession = (ClientSession)session;
            C_RemoveItem removeItemPacket = (C_RemoveItem)packet;

            Player player = clientSession.MyPlayer;
            if (player == null)
                return;

            GameRoom room = player.Room;
            if (room == null)
                return;

            room.Push(room.HandleRemoveItem, player, removeItemPacket);
        }

        public static void C_AddQuestHandler(PacketSession session, IMessage packet)
        {
            ClientSession clientSession = (ClientSession)session;
            C_AddQuest addQuestPacket = (C_AddQuest)packet;

            Player player = clientSession.MyPlayer;
            if (player == null)
                return;

            GameRoom room = player.Room;
            if (room == null)
                return;

            room.Push(room.HandleAcceptQuest, player, addQuestPacket);
        }

        public static void C_TryCompleteQuestHandler(PacketSession session, IMessage packet)
        {
            ClientSession clientSession = (ClientSession)session;
            C_TryCompleteQuest tryCompleteQuestPacket = (C_TryCompleteQuest)packet;

            Player player = clientSession.MyPlayer;
            if (player == null)
                return;

            GameRoom room = player.Room;
            if (room == null)
                return;

            room.Push(room.HandleTryCompleteQuest, player, tryCompleteQuestPacket);
        }

        public static void C_RespawnHandler(PacketSession session, IMessage packet)
        {
            ClientSession clientSession = (ClientSession)session;

            Player player = clientSession.MyPlayer;
            if (player == null)
                return;

            GameRoom room = player.Room;
            if (room == null)
                return;

            room.Push(room.HandleRespawn, player);
        }

        public static void C_ChangeSlotHandler(PacketSession session, IMessage packet)
        {
            ClientSession clientSession = (ClientSession)session;
            C_ChangeSlot changeSlotPacket = (C_ChangeSlot)packet;

            Player player = clientSession.MyPlayer;
            if (player == null)
                return;

            GameRoom room = player.Room;
            if (room == null)
                return;

            room.Push(room.HandleChangeSlot, player, changeSlotPacket);
        }

        public static void C_ChangeSkillPointHandler(PacketSession session, IMessage packet)
        {
            ClientSession clientSession = (ClientSession)session;
            C_ChangeSkillPoint changeSkillPointPacket = (C_ChangeSkillPoint)packet;

            Player player = clientSession.MyPlayer;
            if (player == null)
                return;

            GameRoom room = player.Room;
            if (room == null)
                return;

            room.Push(room.HandleSkillPoint, player, changeSkillPointPacket);
        }

        public static void C_ChangeStatPointHandler(PacketSession session, IMessage packet)
        {
            ClientSession clientSession = (ClientSession)session;
            C_ChangeStatPoint changeStatPointPacket = (C_ChangeStatPoint)packet;

            Player player = clientSession.MyPlayer;
            if (player == null)
                return;

            GameRoom room = player.Room;
            if (room == null)
                return;

            room.Push(room.HandleStatPoint, player, changeStatPointPacket);
        }

        public static void C_ClassUpHandler(PacketSession session, IMessage packet)
        {
            ClientSession clientSession = (ClientSession)session;
            C_ClassUp classUpPacket = (C_ClassUp)packet;

            Player player = clientSession.MyPlayer;
            if (player == null)
                return;

            GameRoom room = player.Room;
            if (room == null)
                return;

            room.Push(room.HandleClassUp, player, classUpPacket);
        }

        public static void C_InvitePlayerHandler(PacketSession session, IMessage packet)
        {
            ClientSession clientSession = (ClientSession)session;
            C_InvitePlayer invitePlayerPacket = (C_InvitePlayer)packet;

            Player player = clientSession.MyPlayer;
            if (player == null)
                return;

            GameRoom room = player.Room;
            if (room == null)
                return;

            room.Push(room.HandleInvitePlayerToParty, player, invitePlayerPacket);
        }

        public static void C_QuitPartyHandler(PacketSession session, IMessage packet)
        {
            ClientSession clientSession = (ClientSession)session;
            C_QuitParty quitPartyPacket = (C_QuitParty)packet;

            Player player = clientSession.MyPlayer;
            if (player == null)
                return;

            GameRoom room = player.Room;
            if (room == null)
                return;

            room.Push(room.HandleQuitParty, player, quitPartyPacket);
        }

        public static void C_ChatHandler(PacketSession session, IMessage packet)
        {
            ClientSession clientSession = (ClientSession)session;
            C_Chat chatPacket = (C_Chat)packet;

            Player player = clientSession.MyPlayer;
            if (player == null)
                return;

            GameRoom room = player.Room;
            if (room == null)
                return;

            room.Push(room.HandleChat, player, chatPacket);
        }

        public static void C_TryGetInDungunHandler(PacketSession session, IMessage packet)
        {
            ClientSession clientSession = (ClientSession)session;
            C_TryGetInDungun tryDungunPacket = (C_TryGetInDungun)packet;

            Player player = clientSession.MyPlayer;
            if (player == null)
                return;

            GameRoom room = player.Room;
            if (room == null)
                return;

            room.Push(room.HandleTryDungun, player, tryDungunPacket);
        }

        public static void C_GetInDungunHandler(PacketSession session, IMessage packet)
        {
            ClientSession clientSession = (ClientSession)session;
            C_GetInDungun dungunPacket = (C_GetInDungun)packet;

            Player player = clientSession.MyPlayer;
            if (player == null)
                return;

            // 던전 입장은 이전에 Room을 나간 상태이기 때문에
            // null이 아니라면 종료
            GameRoom room = player.Room;
            if (room != null)
                return;

            player.Session.HandleChangeRoom();
        }

        public static void C_BuyItemHandler(PacketSession session, IMessage packet)
        {
            ClientSession clientSession = (ClientSession)session;
            C_BuyItem buyItemPacket = (C_BuyItem)packet;

            Player player = clientSession.MyPlayer;
            if (player == null)
                return;

            GameRoom room = player.Room;
            if (room == null)
                return;

            room.Push(room.HandleBuyItem, player, buyItemPacket);
        }


    }
}
