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




    }
}
