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

		if (Managers.Object.MyPlayer.Id == movePacket.ObjectId)
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

		cc.UseSkill(skillPacket.Info.SkillId);

	}

	public static void S_ChangeHpMpHandler(PacketSession session, IMessage packet)
    {
		S_ChangeHpMp changeHpMpPacket = (S_ChangeHpMp)packet;

		GameObject go = Managers.Object.FindById(changeHpMpPacket.ObjectId);
		if (go == null)
			return;

		CreatureController cc = go.GetComponent<CreatureController>();
		if(cc == null)
			return;

		if (changeHpMpPacket.Hp != 0)
			cc.Hp = changeHpMpPacket.Hp;
		else
		{
			PlayerController pc = (PlayerController)cc;
			if (pc == null)
				return;

			pc.Mp = changeHpMpPacket.Mp;
		}

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

	}

	public static void S_EquipItemHandler(PacketSession session, IMessage packet)
	{
		S_EquipItem equipItemOk = (S_EquipItem)packet;

		// 메모리에 아이템 정보 적용
		Item item = Managers.Inven.Get(equipItemOk.ItemDbId);
		if (item == null)
			return;

		item.Equipped = equipItemOk.Equipped;
		Debug.Log("아이템 착용변경!");

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

		mpc.CanUseSkill = skillmanagePacket.CanUseSkill;

	}
}
