using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;


class PacketHandler
{
	public static void S_EnterGameHandler(PacketSession session, IMessage packet)
	{
		S_EnterGame enterPacket = packet as S_EnterGame;
	}

	public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
	{
		S_LeaveGame leavePacket = packet as S_LeaveGame;
	}

	public static void S_SpawnHandler(PacketSession session, IMessage packet)
	{
		S_Spawn spawnPacket = packet as S_Spawn;
	}

	public static void S_DespawnHandler(PacketSession session, IMessage packet)
	{
		S_Despawn despawnPacket = packet as S_Despawn;
	}

	public static void S_MoveHandler(PacketSession session, IMessage packet)
	{
		S_Move movePacket = packet as S_Move;
	}

	public static void S_SkillHandler(PacketSession session, IMessage packet)
	{
		S_Skill skillPacket = packet as S_Skill;
	}

	public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
	{
		S_ChangeHp changeHpPacket = (S_ChangeHp)packet;
	}

	public static void S_ChangeMpHandler(PacketSession session, IMessage packet)
	{
		S_ChangeMp changeMpPacket = (S_ChangeMp)packet;
	}

	public static void S_DieHandler(PacketSession session, IMessage packet)
	{
		S_Die diePacket = packet as S_Die;
	}

	public static void S_ConnectedHandler(PacketSession session, IMessage packet)
	{
		C_Login loginPacket = new C_Login();
		ServerSession serverSession = (ServerSession)session;

		loginPacket.UniqueId = $"DummyClient_{serverSession.DummyId.ToString("0000")}";
		serverSession.Send(loginPacket);
	}

	public static void S_LoginHandler(PacketSession session, IMessage packet)
	{
		S_Login loginPacket = (S_Login)packet;
		ServerSession serverSession = (ServerSession)session;

		// 해당 식별자로는 캐릭터가 없다
		if (loginPacket.Players == null || loginPacket.Players.Count == 0)
        {
			C_CreatePlayer createPacket = new C_CreatePlayer();
			createPacket.Name = $"Player_{serverSession.DummyId.ToString("0000")}";
			serverSession.Send(createPacket);
        }
        else
        {
			// 무조건 첫버째 캐릭터로 로그인
			LobbyPlayerInfo info = loginPacket.Players[0];
			C_EnterGame enterGamePacket = new C_EnterGame();
			enterGamePacket.Name = info.Name;
			serverSession.Send(enterGamePacket);
        }
	}

	public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
	{
		S_CreatePlayer createPlayerPacket = (S_CreatePlayer)packet;
		ServerSession serverSession = (ServerSession)session;

		if(createPlayerPacket.Player == null)
        {

        }
        else
		{
			C_EnterGame enterGamePacket = new C_EnterGame();
			enterGamePacket.Name = createPlayerPacket.Player.Name;
			serverSession.Send(enterGamePacket);
        }
	}

	public static void S_ItemListHandler(PacketSession session, IMessage packet)
	{
		S_ItemList itemList = (S_ItemList)packet;
	}

	public static void S_AddItemHandler(PacketSession session, IMessage packet)
	{
		S_AddItem itemList = (S_AddItem)packet;
	}

	public static void S_EquipItemHandler(PacketSession session, IMessage packet)
	{
		S_EquipItem equipItemOk = (S_EquipItem)packet;
	}

	public static void S_ChangeExHandler(PacketSession session, IMessage packet)
	{
		S_ChangeEx changeExPacket = (S_ChangeEx)packet;
	}

	public static void S_LevelUpHandler(PacketSession session, IMessage packet)
	{
		S_LevelUp levelUpPacket = (S_LevelUp)packet;
	}

	public static void S_PingHandler(PacketSession session, IMessage packet)
	{
		C_Pong pongPacket = new C_Pong();
	}

	public static void S_ManageSkillHandler(PacketSession session, IMessage packet)
	{
		S_ManageSkill skillmanagePacket = (S_ManageSkill)packet;
	}

	public static void S_SetCountConsumableHandler(PacketSession session, IMessage packet)
	{
		S_SetCountConsumable useConsumablePacket = (S_SetCountConsumable)packet;
	}

	public static void S_RemoveItemHandler(PacketSession session, IMessage packet)
	{
		S_RemoveItem removeItemPacket = (S_RemoveItem)packet;
	}

	public static void S_SpawnNpcHandler(PacketSession session, IMessage packet)
	{
		S_SpawnNpc spawnNpcPacket = (S_SpawnNpc)packet;
	}

	public static void S_AddQuestHandler(PacketSession session, IMessage packet)
	{
		S_AddQuest addQuestPacket = (S_AddQuest)packet;
	}

	public static void S_RefreshHuntingQuestHandler(PacketSession session, IMessage packet)
	{
		S_RefreshHuntingQuest refreshHuntingQuestPacket = (S_RefreshHuntingQuest)packet;
	}

	public static void S_CanCompleteQuestHandler(PacketSession session, IMessage packet)
	{
		S_CanCompleteQuest completeQuestPacket = (S_CanCompleteQuest)packet;
	}

	public static void S_CompleteQuestHandler(PacketSession session, IMessage packet)
	{
		S_CompleteQuest completeQuestPacket = (S_CompleteQuest)packet;
	}

	public static void S_RespawnHandler(PacketSession session, IMessage packet)
	{

	}

	public static void S_SpawnObstacleHandler(PacketSession session, IMessage packet)
	{
		S_SpawnObstacle spawnObjstaclePacket = (S_SpawnObstacle)packet;
	}

	public static void S_DespawnObstacleHandler(PacketSession session, IMessage packet)
	{
		S_DespawnObstacle spawnObjstaclePacket = (S_DespawnObstacle)packet;
	}

	public static void S_SkillPointHandler(PacketSession session, IMessage packet)
	{
		S_SkillPoint skillPointPacket = (S_SkillPoint)packet;
	}

	public static void S_StatPointHandler(PacketSession session, IMessage packet)
	{
		S_StatPoint statPointPacket = (S_StatPoint)packet;
	}

	public static void S_ClassUpHandler(PacketSession session, IMessage packet)
	{
		S_ClassUp classUpPacket = (S_ClassUp)packet;
	}

	public static void S_ChangeConditionInfoHandler(PacketSession session, IMessage packet)
	{
		S_ChangeConditionInfo changeConditionPacket = (S_ChangeConditionInfo)packet;
	}

	public static void S_PartyListHandler(PacketSession session, IMessage packet)
	{
		S_PartyList partyListPacket = (S_PartyList)packet;
	}

	public static void S_ChatHandler(PacketSession session, IMessage packet)
	{
		S_Chat chatPacket = (S_Chat)packet;
	}

	public static void S_TryGetInDungunHandler(PacketSession session, IMessage packet)
	{
		S_TryGetInDungun dungunPacket = (S_TryGetInDungun)packet;
	}

	public static void S_ChangeGoldHandler(PacketSession session, IMessage packet)
	{
		S_ChangeGold changeGoldPacket = (S_ChangeGold)packet;
	}

	public static void S_SendMassageHandler(PacketSession session, IMessage packet)
	{
		S_SendMassage massagePacket = (S_SendMassage)packet;
	}
}

