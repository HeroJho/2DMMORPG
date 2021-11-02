
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class PacketManager
    {
        #region Singleton
        static PacketManager _instance = new PacketManager();
	    public static PacketManager Instance { get { return _instance; } }
        #endregion

        PacketManager()
        {
            Register();
        }

        Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
        Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();

        public Action<PacketSession, IMessage, ushort> CustomHandler { get; set; }

        public void Register()
        {
            
            _onRecv.Add((ushort)MsgId.SEnterGame, MakePacket<S_EnterGame>);
            _handler.Add((ushort)MsgId.SEnterGame, PacketHandler.S_EnterGameHandler);

            _onRecv.Add((ushort)MsgId.SLeaveGame, MakePacket<S_LeaveGame>);
            _handler.Add((ushort)MsgId.SLeaveGame, PacketHandler.S_LeaveGameHandler);

            _onRecv.Add((ushort)MsgId.SSpawn, MakePacket<S_Spawn>);
            _handler.Add((ushort)MsgId.SSpawn, PacketHandler.S_SpawnHandler);

            _onRecv.Add((ushort)MsgId.SDespawn, MakePacket<S_Despawn>);
            _handler.Add((ushort)MsgId.SDespawn, PacketHandler.S_DespawnHandler);

            _onRecv.Add((ushort)MsgId.SMove, MakePacket<S_Move>);
            _handler.Add((ushort)MsgId.SMove, PacketHandler.S_MoveHandler);

            _onRecv.Add((ushort)MsgId.SSkill, MakePacket<S_Skill>);
            _handler.Add((ushort)MsgId.SSkill, PacketHandler.S_SkillHandler);

            _onRecv.Add((ushort)MsgId.SChangeHp, MakePacket<S_ChangeHp>);
            _handler.Add((ushort)MsgId.SChangeHp, PacketHandler.S_ChangeHpHandler);

            _onRecv.Add((ushort)MsgId.SChangeMp, MakePacket<S_ChangeMp>);
            _handler.Add((ushort)MsgId.SChangeMp, PacketHandler.S_ChangeMpHandler);

            _onRecv.Add((ushort)MsgId.SDie, MakePacket<S_Die>);
            _handler.Add((ushort)MsgId.SDie, PacketHandler.S_DieHandler);

            _onRecv.Add((ushort)MsgId.SConnected, MakePacket<S_Connected>);
            _handler.Add((ushort)MsgId.SConnected, PacketHandler.S_ConnectedHandler);

            _onRecv.Add((ushort)MsgId.SLogin, MakePacket<S_Login>);
            _handler.Add((ushort)MsgId.SLogin, PacketHandler.S_LoginHandler);

            _onRecv.Add((ushort)MsgId.SCreatePlayer, MakePacket<S_CreatePlayer>);
            _handler.Add((ushort)MsgId.SCreatePlayer, PacketHandler.S_CreatePlayerHandler);

            _onRecv.Add((ushort)MsgId.SItemList, MakePacket<S_ItemList>);
            _handler.Add((ushort)MsgId.SItemList, PacketHandler.S_ItemListHandler);

            _onRecv.Add((ushort)MsgId.SAddItem, MakePacket<S_AddItem>);
            _handler.Add((ushort)MsgId.SAddItem, PacketHandler.S_AddItemHandler);

            _onRecv.Add((ushort)MsgId.SEquipItem, MakePacket<S_EquipItem>);
            _handler.Add((ushort)MsgId.SEquipItem, PacketHandler.S_EquipItemHandler);

            _onRecv.Add((ushort)MsgId.SChangeEx, MakePacket<S_ChangeEx>);
            _handler.Add((ushort)MsgId.SChangeEx, PacketHandler.S_ChangeExHandler);

            _onRecv.Add((ushort)MsgId.SLevelUp, MakePacket<S_LevelUp>);
            _handler.Add((ushort)MsgId.SLevelUp, PacketHandler.S_LevelUpHandler);

            _onRecv.Add((ushort)MsgId.SPing, MakePacket<S_Ping>);
            _handler.Add((ushort)MsgId.SPing, PacketHandler.S_PingHandler);

            _onRecv.Add((ushort)MsgId.SManageSkill, MakePacket<S_ManageSkill>);
            _handler.Add((ushort)MsgId.SManageSkill, PacketHandler.S_ManageSkillHandler);

            _onRecv.Add((ushort)MsgId.SSetCountConsumable, MakePacket<S_SetCountConsumable>);
            _handler.Add((ushort)MsgId.SSetCountConsumable, PacketHandler.S_SetCountConsumableHandler);

            _onRecv.Add((ushort)MsgId.SRemoveItem, MakePacket<S_RemoveItem>);
            _handler.Add((ushort)MsgId.SRemoveItem, PacketHandler.S_RemoveItemHandler);

            _onRecv.Add((ushort)MsgId.SSpawnNpc, MakePacket<S_SpawnNpc>);
            _handler.Add((ushort)MsgId.SSpawnNpc, PacketHandler.S_SpawnNpcHandler);

            _onRecv.Add((ushort)MsgId.SAddQuest, MakePacket<S_AddQuest>);
            _handler.Add((ushort)MsgId.SAddQuest, PacketHandler.S_AddQuestHandler);

            _onRecv.Add((ushort)MsgId.SRefreshHuntingQuest, MakePacket<S_RefreshHuntingQuest>);
            _handler.Add((ushort)MsgId.SRefreshHuntingQuest, PacketHandler.S_RefreshHuntingQuestHandler);

            _onRecv.Add((ushort)MsgId.SCanCompleteQuest, MakePacket<S_CanCompleteQuest>);
            _handler.Add((ushort)MsgId.SCanCompleteQuest, PacketHandler.S_CanCompleteQuestHandler);

            _onRecv.Add((ushort)MsgId.SCompleteQuest, MakePacket<S_CompleteQuest>);
            _handler.Add((ushort)MsgId.SCompleteQuest, PacketHandler.S_CompleteQuestHandler);

            _onRecv.Add((ushort)MsgId.SRespawn, MakePacket<S_Respawn>);
            _handler.Add((ushort)MsgId.SRespawn, PacketHandler.S_RespawnHandler);

            _onRecv.Add((ushort)MsgId.SSpawnObstacle, MakePacket<S_SpawnObstacle>);
            _handler.Add((ushort)MsgId.SSpawnObstacle, PacketHandler.S_SpawnObstacleHandler);

            _onRecv.Add((ushort)MsgId.SDespawnObstacle, MakePacket<S_DespawnObstacle>);
            _handler.Add((ushort)MsgId.SDespawnObstacle, PacketHandler.S_DespawnObstacleHandler);

            _onRecv.Add((ushort)MsgId.SSkillPoint, MakePacket<S_SkillPoint>);
            _handler.Add((ushort)MsgId.SSkillPoint, PacketHandler.S_SkillPointHandler);

            _onRecv.Add((ushort)MsgId.SStatPoint, MakePacket<S_StatPoint>);
            _handler.Add((ushort)MsgId.SStatPoint, PacketHandler.S_StatPointHandler);

            _onRecv.Add((ushort)MsgId.SClassUp, MakePacket<S_ClassUp>);
            _handler.Add((ushort)MsgId.SClassUp, PacketHandler.S_ClassUpHandler);

            _onRecv.Add((ushort)MsgId.SChangeConditionInfo, MakePacket<S_ChangeConditionInfo>);
            _handler.Add((ushort)MsgId.SChangeConditionInfo, PacketHandler.S_ChangeConditionInfoHandler);

            _onRecv.Add((ushort)MsgId.SPartyList, MakePacket<S_PartyList>);
            _handler.Add((ushort)MsgId.SPartyList, PacketHandler.S_PartyListHandler);

            _onRecv.Add((ushort)MsgId.SChat, MakePacket<S_Chat>);
            _handler.Add((ushort)MsgId.SChat, PacketHandler.S_ChatHandler);

        }

        public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
        {
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            Action<PacketSession, ArraySegment<byte>, ushort> action = null;
            if (_onRecv.TryGetValue(id, out action))
                action.Invoke(session, buffer, id);
        }

        void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
        {
            T pkt = new T();
            pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

            if (CustomHandler != null)
            {
                CustomHandler.Invoke(session, pkt, id);
            }
            else
            {
                Action<PacketSession, IMessage> action = null;
                if (_handler.TryGetValue(id, out action))
                    action.Invoke(session, pkt);
            }
        }

        public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
        {
            Action<PacketSession, IMessage> action = null;
            if (_handler.TryGetValue(id, out action))
                return action;
            return null;
        }
    }
}
