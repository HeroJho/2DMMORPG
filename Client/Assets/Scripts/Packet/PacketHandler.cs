using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PacketHandler
{
	public static void S_EnterGameHandler(PacketSession session, IMessage packet)
	{
		S_EnterGame enterPacket = packet as S_EnterGame;

		Debug.Log("Enter!");
		Managers.Object.Add(enterPacket.Player, myPlayer: true);
	}

	public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
	{
		S_LeaveGame leavePacket = packet as S_LeaveGame;

		Managers.Object.Clear();
	}

	public static void S_SpawnHandler(PacketSession session, IMessage packet)
	{
		S_Spawn spawnPacket = packet as S_Spawn;

		foreach (ObjectInfo info in spawnPacket.Objects)
        {
			Managers.Object.Add(info, myPlayer: false);
		}
	}

	public static void S_DespawnHandler(PacketSession session, IMessage packet)
	{
		S_Despawn despawnPacket = packet as S_Despawn;

		foreach (int id in despawnPacket.ObjectIds)
		{
			Managers.Object.Remove(id);
		}
	}

	public static void S_MoveHandler(PacketSession session, IMessage packet)
	{
		S_Move movePacket = packet as S_Move;

		GameObject go = Managers.Object.FindById(movePacket.ObjectId);
		if (go == null)
			return;

		if (movePacket.IncludingMe == false && Managers.Object.MyPlayer.Id == movePacket.ObjectId)
			return;

		BaseController bc = go.GetComponent<BaseController>();
		if (bc == null)
			return;

		bc.PosInfo = movePacket.PosInfo;
	}

	public static void S_SkillHandler(PacketSession session, IMessage packet)
	{
		S_Skill skillPacket = packet as S_Skill;

		GameObject go = Managers.Object.FindById(skillPacket.ObjectId);
		if (go == null)
			return;

		CreatureController cc = go.GetComponent<CreatureController>();
		if (cc == null)
			return;

		cc.UseSkill(skillPacket.Info.SkillId, skillPacket.Info.Point);

	}

	public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {
		S_ChangeHp changeHpPacket = (S_ChangeHp)packet;

		GameObject go = Managers.Object.FindById(changeHpPacket.ObjectId);
		if (go == null)
			return;

		CreatureController cc = go.GetComponent<CreatureController>();
		if(cc == null)
			return;

		cc.Stat.MaxHp = changeHpPacket.MaxHp;
		cc.Hp = changeHpPacket.Hp;

		// 스텟창 갱신
		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		gameSceneUI.StatUI.RefreshUI();
	}

	public static void S_ChangeMpHandler(PacketSession session, IMessage packet)
	{
		S_ChangeMp changeMpPacket = (S_ChangeMp)packet;

		GameObject go = Managers.Object.FindById(changeMpPacket.ObjectId);
		if (go == null)
			return;

		PlayerController pc = go.GetComponent<PlayerController>();
		if (pc == null)
			return;

		pc.Stat.MaxMp = changeMpPacket.MaxMp;
		pc.Mp = changeMpPacket.Mp;

		// 스텟창 갱신
		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		gameSceneUI.StatUI.RefreshUI();
	}

	public static void S_DieHandler(PacketSession session, IMessage packet)
    {
		S_Die diePacket = packet as S_Die;

		GameObject go = Managers.Object.FindById(diePacket.ObjectId);
		if (go == null)
			return;

		CreatureController cc = go.GetComponent<CreatureController>();
		if(cc != null)
        {
			cc.Hp = 0;
			cc.OnDead();

			// 내 플레이어라면 창 띄우기
			if(cc.Id == Managers.Object.MyPlayer.Id)
            {
				Managers.UI.ShowPopupUI<UI_Die>();
			}

        }
	}

	public static void S_ConnectedHandler(PacketSession session, IMessage packet)
	{
		Debug.Log("S_ConnectedHandler");
		C_Login loginPacket = new C_Login();
		// Unique한 값을 뱉음
		string path = Application.dataPath;
		loginPacket.UniqueId = path.GetHashCode().ToString();
		Managers.Network.Send(loginPacket);
	}

	public static void S_LoginHandler(PacketSession session, IMessage packet)
	{
		S_Login loginPacket = (S_Login)packet;
		Debug.Log($"LoginOk({loginPacket.LoginOk})");

		// TODO : 로비 UI에서 캐릭터 보여주고, 선택할 수 있도록

		// 해당 식별자로는 캐릭터가 없다
		if(loginPacket.Players == null || loginPacket.Players.Count == 0)
        {
			C_CreatePlayer createPlayerPacket = new C_CreatePlayer();
			createPlayerPacket.Name = $"Player_{Random.Range(0, 10000).ToString("0000")}";
			Managers.Network.Send(createPlayerPacket);
		}
        else
        {
			// 무조건 첫번째 캐릭터로 로그인
			LobbyPlayerInfo info = loginPacket.Players[0];
			C_EnterGame enterGamePacket = new C_EnterGame();
			enterGamePacket.Name = info.Name;
			Managers.Network.Send(enterGamePacket);
        }
	}

	public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
    {
		S_CreatePlayer createPlayerPacket = (S_CreatePlayer)packet;

        // 뭔가 문제가 있다(이름 중복이라든지..) > 다시 만들기 시도
        if (createPlayerPacket.Player == null)
        {
			C_CreatePlayer createPlayer = new C_CreatePlayer();
			createPlayer.Name = $"Player_{Random.Range(0, 10000).ToString("0000")}";
			Managers.Network.Send(createPlayer);
		}
		else // 캐릭터 만들기 성공
        {
			C_EnterGame enterGamePacket = new C_EnterGame();
			enterGamePacket.Name = createPlayerPacket.Player.Name;
			Managers.Network.Send(enterGamePacket);
        }
    }

	public static void S_ItemListHandler(PacketSession session, IMessage packet)
    {
		S_ItemList itemList = (S_ItemList)packet;

		Managers.Inven.Clear(); // 패킷 여러번 오면 기존 아이템 삭제

        // 메모리에 아이템 정보 적용
        foreach (ItemInfo itemInfo in itemList.Items)
        {
			Item item = Item.MakeItem(itemInfo);
			Managers.Inven.Add(item);
        }

		if (Managers.Object.MyPlayer != null)
			Managers.Object.MyPlayer.RefreshAdditionanlStat();
    }

	public static void S_AddItemHandler(PacketSession session, IMessage packet)
	{
		S_AddItem itemList = (S_AddItem)packet;

		// 메모리에 아이템 정보 적용
		foreach (ItemInfo itemInfo in itemList.Items)
		{
			Item item = Item.MakeItem(itemInfo);
			Managers.Inven.Add(item);
		}

		Debug.Log("아이템을 획득 했습니다!");

		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		gameSceneUI.InvenUI.RefreshUI();
		gameSceneUI.StatUI.RefreshUI();
		gameSceneUI.ShortcutKeyUI.RefreshUI();
	}

	public static void S_EquipItemHandler(PacketSession session, IMessage packet)
	{
		S_EquipItem equipItemOk = (S_EquipItem)packet;

		// 메모리에 아이템 정보 적용
		Item item = Managers.Inven.Get(equipItemOk.ItemDbId);
		if (item == null)
			return;

		item.Equipped = equipItemOk.Equipped;

		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		gameSceneUI.InvenUI.RefreshUI();
		gameSceneUI.StatUI.RefreshUI();

		if (Managers.Object.MyPlayer != null)
			Managers.Object.MyPlayer.RefreshAdditionanlStat();
	}

	public static void S_ChangeExHandler(PacketSession session, IMessage packet)
    {
		S_ChangeEx changeExPacket = (S_ChangeEx)packet;

		Debug.Log($"경험치{changeExPacket.Ex} / {changeExPacket.LevelEx}");

		MyPlayerController mpc = Managers.Object.MyPlayer;
		if (mpc == null)
			return;

		mpc.Exp = changeExPacket.Ex;
		mpc.UpdateExBar();
	}

	public static void S_LevelUpHandler(PacketSession session, IMessage packet)
    {
		S_LevelUp levelUpPacket = (S_LevelUp)packet;

		GameObject go = Managers.Object.FindById(levelUpPacket.Id);
		if (go == null)
			return;
		PlayerController pc = go.GetComponent<PlayerController>();
		if (pc == null)
			return;

		pc.LevelUp(levelUpPacket.Level);
    }

	public static void S_PingHandler(PacketSession session, IMessage packet)
    {
		C_Pong pongPacket = new C_Pong();
		//Debug.Log("[Server] PingCheck");
		Managers.Network.Send(pongPacket);
    }

	public static void S_ManageSkillHandler(PacketSession session, IMessage packet)
	{
		S_ManageSkill skillmanagePacket = (S_ManageSkill)packet;

		MyPlayerController mpc = Managers.Object.MyPlayer;
		if (mpc == null)
			return;

		Managers.Skill.ResetCooltime(skillmanagePacket.TemplateId);

	}

	public static void S_SetCountConsumableHandler(PacketSession session, IMessage packet)
    {
		S_SetCountConsumable useConsumablePacket = (S_SetCountConsumable)packet;

		Item item = Managers.Inven.Get(useConsumablePacket.ItemDbId);
		if (item == null)
			return;

		item.Count = useConsumablePacket.Count;

		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;

		gameSceneUI.InvenUI.SetCount(item);
		gameSceneUI.ShortcutKeyUI.RefreshUI();

		if (item.Count <= 0)
			Managers.Inven.Items.Remove(item.ItemDbId);
	}

	public static void S_RemoveItemHandler(PacketSession session, IMessage packet)
	{
		S_RemoveItem removeItemPacket = (S_RemoveItem)packet;

		Debug.Log("아이템을 버렸습니다!");

		Managers.Inven.Remove(removeItemPacket.ItemDbId);

		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		gameSceneUI.InvenUI.RefreshUI();
		//gameSceneUI.StatUI.RefreshUI();
		//gameSceneUI.ShortcutKeyUI.RefreshUI();
	}

	public static void S_SpawnNpcHandler(PacketSession session, IMessage packet)
	{
		S_SpawnNpc spawnNpcPacket = (S_SpawnNpc)packet;

		Managers.Quest.ParsingQuest(spawnNpcPacket.QuestInfo);

        foreach (ObjectInfo info in spawnNpcPacket.NpcInfos)
        {
			Managers.Object.SpawnNpc(info);
        }

		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		gameSceneUI.QuestUI.RefreshUI();
	}

	public static void S_AddQuestHandler(PacketSession session, IMessage packet)
    {
		S_AddQuest addQuestPacket = (S_AddQuest)packet;

		Managers.Quest.ReceiveQuest(addQuestPacket);
    }

	public static void S_RefreshHuntingQuestHandler(PacketSession session, IMessage packet)
	{
		S_RefreshHuntingQuest refreshHuntingQuestPacket = (S_RefreshHuntingQuest)packet;

		Managers.Quest.RefreshQuest(refreshHuntingQuestPacket);
	}

	public static void S_CanCompleteQuestHandler(PacketSession session, IMessage packet)
	{
		S_CanCompleteQuest completeQuestPacket = (S_CanCompleteQuest)packet;

		GameObject go = Managers.Object.FindNpcWithId(completeQuestPacket.NpcId);
		QuestGiver npc = go.GetComponent<QuestGiver>();

		Quest quest = null;
		npc.QuestList.TryGetValue(completeQuestPacket.QuestId, out quest);

		Managers.Quest.CanCompleteQuests.Add(quest.QuestId, quest);
		quest.QuestState = QuestState.Cancomplete;

		npc.RefreshMark();
	}

	public static void S_CompleteQuestHandler(PacketSession session, IMessage packet)
    {
		S_CompleteQuest completeQuestPacket = (S_CompleteQuest)packet;

		Managers.Quest.CompleteQuest(completeQuestPacket.QuestId);
	}

	public static void S_RespawnHandler(PacketSession session, IMessage packet)
	{
		
	}

	public static void S_SpawnObstacleHandler(PacketSession session, IMessage packet)
    {
		S_SpawnObstacle spawnObjstaclePacket = (S_SpawnObstacle)packet;

        foreach (int id in spawnObjstaclePacket.TemplateId)
        {
			Managers.Object.SpawnObstacle(id);
        }
    }

	public static void S_DespawnObstacleHandler(PacketSession session, IMessage packet)
	{
		S_DespawnObstacle spawnObjstaclePacket = (S_DespawnObstacle)packet;

		foreach (int id in spawnObjstaclePacket.TemplateId)
		{
			Managers.Object.DespawnObstacle(id);
		}
	}

	public static void S_SkillPointHandler(PacketSession session, IMessage packet)
    {
		S_SkillPoint skillPointPacket = (S_SkillPoint)packet;

		Managers.Skill.MyPoints = skillPointPacket.Points;
        foreach (SkillInfo info in skillPointPacket.SkillInfos)
        {
			Managers.Skill.RefreshSkillPointInfo(info.SkillId, info.Point);
		}

		// 스킬창 갱신
		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		gameSceneUI.SkillUI.RefreshUI();
	}

	public static void S_StatPointHandler(PacketSession session, IMessage packet)
	{
		S_StatPoint statPointPacket = (S_StatPoint)packet;

		Managers.Object.MyPlayer.Stat = statPointPacket.StatInfo;

		// 스텟창 갱신
		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		gameSceneUI.StatUI.RefreshUI();
	}

	public static void S_ClassUpHandler(PacketSession session, IMessage packet)
	{
		S_ClassUp classUpPacket = (S_ClassUp)packet;

		Managers.Object.MyPlayer.Stat.CanUpClass = true;

		Managers.UI.ShowPopupUI<UI_ClassUp>();

		// 전직 아이콘
		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		gameSceneUI.ClassUp.gameObject.SetActive(true);
	}

	public static void S_ChangeConditionInfoHandler(PacketSession session, IMessage packet)
    {
		S_ChangeConditionInfo changeConditionPacket = (S_ChangeConditionInfo)packet;

		GameObject go = Managers.Object.FindById(changeConditionPacket.Id);
		if (go == null)
			return;
		Condition condition = go.GetComponent<Condition>();
		if (condition == null)
			return;

		condition.UpdateCondition(changeConditionPacket.ConditionType, changeConditionPacket.Id, changeConditionPacket.SkillId, changeConditionPacket.Time, changeConditionPacket.AttackSpeed, changeConditionPacket.MoveSpeed);
	}

	public static void S_PartyListHandler(PacketSession session, IMessage packet)
	{
		S_PartyList partyListPacket = (S_PartyList)packet;

		Managers.Communication.Party.LeaderPlayer = partyListPacket.LeaderPlayer;

		if(partyListPacket.PlayerInfos != null)
		{
			Managers.Communication.Party.PartyList.Clear();
			foreach (ObjectInfo info in partyListPacket.PlayerInfos)
			{
				Managers.Communication.Party.AddPlayer(info);
			}
		}
		else
        {
			Managers.Communication.Party.PartyList.Clear();
		}

        // 이젠 커뮤니티 매니저를 거쳐서 UI 표시
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.PartyPanelUI.SetPartyInfos();
    }

	public static void S_ChatHandler(PacketSession session, IMessage packet)
    {
		S_Chat chatPacket = (S_Chat)packet;

		Managers.Chat.SendChatToBox(chatPacket.Id, chatPacket.Str);
    }

	public static void S_TryGetInDungunHandler(PacketSession session, IMessage packet)
	{
		S_TryGetInDungun dungunPacket = (S_TryGetInDungun)packet;

		if (dungunPacket.Ok == 0)
        {
			// TODO : 파티를 해야합니다!
        }
		else if (dungunPacket.Ok == 1)
        {
			// TODO : 조건이 안됩니다!
        }
		else if (dungunPacket.Ok == 2)
        {
			// TODO : 파티장이 아닙니다!
        }
		else if (dungunPacket.Ok == 3)
        {
			UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
			gameSceneUI.ChangeUI.ChangeRoom();
		}


	}

	public static void S_ChangeGoldHandler(PacketSession session, IMessage packet)
	{
		S_ChangeGold changeGoldPacket = (S_ChangeGold)packet;

		Managers.Inven.Gold = changeGoldPacket.Gold;

		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		gameSceneUI.InvenUI.RefreshUI();

	}

}
