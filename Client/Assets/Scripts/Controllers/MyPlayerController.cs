using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MyPlayerController : PlayerController
{
	UI_GameScene _gameSceneUI = null;
	bool _moveKeyPressed = false;

    public override StatInfo Stat 
	{
		get { return base.Stat; }
		set 
		{ 
			base.Stat = value;

			RefreshAdditionanlStat();
			AddExBar();
			InitLevelUI();
		}
	}

    public int Exp
    {
        get { return Stat.TotalExp; }
		set { Stat.TotalExp = value; }
    }

	private int LevelUpExp
	{
		get { return Level * 25; }
	}

	public int WeaponDamage { get; private set; }
	public int ArmorDefence { get; private set; }

	protected override void Init()
    {
        base.Init();

		_gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		RefreshAdditionanlStat();
		AddExBar();
		InitLevelUI();
	}

    protected override void AddHpBar()
    {
		UpdateHpBar();
	}

    protected override void UpdateHpBar()
    {
		float ratio = 0.0f;
		if (Stat.MaxHp > 0)
		{
			ratio = ((float)Hp / Stat.MaxHp);
		}

		UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
		if (gameScene == null)
			return;

		gameScene.StatBarUI.SetHpBar(ratio);
	}

	protected override void AddMpBar()
	{
		UpdateMpBar();
	}

	protected override void UpdateMpBar()
	{
		float ratio = 0.0f;
		if (Stat.MaxMp > 0)
		{
			ratio = ((float)Mp / Stat.MaxMp);
		}

		UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
		if (gameScene == null)
			return;

		gameScene.StatBarUI.SetMpBar(ratio);
	}

	private void AddExBar()
    {
		UpdateExBar();
	}

	public void UpdateExBar()
    {
		float ratio = 0.0f;
		if (LevelUpExp > 0)
		{
			ratio = ((float)Exp / LevelUpExp);
		}

		UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
		if (gameScene == null)
			return;

		gameScene.StatBarUI.SetExBar(ratio);
	}

	private void InitLevelUI()
	{
		UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
		if (gameScene == null)
			return;

		gameScene.StatBarUI.SetLevel(Level);
	}

    public override void LevelUp(int level)
    {
        base.LevelUp(level);

		UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
		if (gameScene == null)
			return;

		gameScene.StatBarUI.SetLevel(level);

		// 퀘스트 여부 체크
		Managers.Quest.CheckCondition();
	}

	public Vector3Int TestPos;
	public bool TestMapDo = false;

    protected override void UpdateController()
    {
		if(TestMapDo)
        {
			CellPos = TestPos;
        }

		if (State == CreatureState.Dead)
			return;

		GetUIKeyInput();
		GetQuickSlotInput();

		switch (State)
		{
			case CreatureState.Idle:
				GetDirInput();
				GetSkillKeyInput();
				GetKeyInput();
				break;
			case CreatureState.Moving:
				GetDirInput();
				GetSkillKeyInput();
				GetKeyInput();
				break;
		}

		base.UpdateController();
    }

	protected override void UpdateIdle()
	{
		// 이동 상태로 갈지 확인
		if (_moveKeyPressed)
		{
			State = CreatureState.Moving;
			return;
		}
	}

	// 키보드 입력
	void GetDirInput()
	{
		_moveKeyPressed = true;

		if (Input.GetKey(KeyCode.UpArrow))
		{
			Dir = MoveDir.Up;
		}
		else if (Input.GetKey(KeyCode.DownArrow))
		{
			Dir = MoveDir.Down;
		}
		else if (Input.GetKey(KeyCode.LeftArrow))
		{
			Dir = MoveDir.Left;
		}
		else if (Input.GetKey(KeyCode.RightArrow))
		{
			Dir = MoveDir.Right;
		}
		else
		{
			_moveKeyPressed = false;
		}
	}

	void GetSkillKeyInput()
    {

	}

	void GetKeyInput()
    {
		// 아이템 먹기
		if(Input.GetKeyDown(KeyCode.Z))
        {
			ItemController item = Managers.Object.FindItemFromGround(CellPos);

			if (item != null)
			{
				C_GetDropItem dropItemPacket = new C_GetDropItem()
				{
					PosInfo = new PositionInfo(),
					ItemInfo = new ItemInfo()
				};
				dropItemPacket.PosInfo.PosX = CellPos.x;
				dropItemPacket.PosInfo.PosY = CellPos.y;
				dropItemPacket.ItemInfo.MergeFrom(item.itemInfo);

				Managers.Network.Send(dropItemPacket);
			}
        }
		
		if(Input.GetKeyDown(KeyCode.Space))
        {
			Managers.Quest.ViewQuest(GetFrontCellPos());
        }

	}

	void GetUIKeyInput()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
			UI_Inventory invenUI = _gameSceneUI.InvenUI;

            if (invenUI.gameObject.activeSelf) // 활성화 여부
            {
				invenUI.gameObject.SetActive(false);
			}
            else
            {
				invenUI.gameObject.SetActive(true);
				invenUI.RefreshUI(); // 켤 때 갱신
			}
		}
        else if (Input.GetKeyDown(KeyCode.C))
        {
			UI_Stat statUI = _gameSceneUI.StatUI;

			if (statUI.gameObject.activeSelf) // 활성화 여부
			{
				statUI.gameObject.SetActive(false);
			}
			else
			{
				statUI.gameObject.SetActive(true);
				statUI.RefreshUI(); // 켤 때 갱신
			}
		}
		else if(Input.GetKeyDown(KeyCode.K))
        {
			UI_Skill skillUI = _gameSceneUI.SkillUI;

			if (skillUI.gameObject.activeSelf) // 활성화 여부
			{
				skillUI.gameObject.SetActive(false);
			}
			else
			{
				skillUI.gameObject.SetActive(true);
			}
		}
	}

	void GetQuickSlotInput()
	{ // 단축키

		if (Input.GetKeyDown(KeyCode.Q))
			_gameSceneUI.ShortcutKeyUI.GetKeyQ();
		if (Input.GetKeyDown(KeyCode.W))
			_gameSceneUI.ShortcutKeyUI.GetKeyW();
		if (Input.GetKeyDown(KeyCode.E))
			_gameSceneUI.ShortcutKeyUI.GetKeyE();
		if (Input.GetKeyDown(KeyCode.R))
			_gameSceneUI.ShortcutKeyUI.GetKeyR();
		if (Input.GetKeyDown(KeyCode.A))
			_gameSceneUI.ShortcutKeyUI.GetKeyA();
		if (Input.GetKeyDown(KeyCode.S))
			_gameSceneUI.ShortcutKeyUI.GetKeyS();
		if (Input.GetKeyDown(KeyCode.D))
			_gameSceneUI.ShortcutKeyUI.GetKeyD();
		if (Input.GetKeyDown(KeyCode.F))
			_gameSceneUI.ShortcutKeyUI.GetKeyF();

	}

	void LateUpdate()
	{
		Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
	}

    protected override void MoveToNextPos()
    {
		if (!_moveKeyPressed)
		{
			State = CreatureState.Idle;
			CheckUpdatedFlag();
			return;
		}

		Vector3Int destPos = CellPos;

		switch (Dir)
		{
			case MoveDir.Up:
				destPos += Vector3Int.up;
				break;
			case MoveDir.Down:
				destPos += Vector3Int.down;
				break;
			case MoveDir.Left:
				destPos += Vector3Int.left;
				break;
			case MoveDir.Right:
				destPos += Vector3Int.right;
				break;
		}

		if (Managers.Map.CanGo(destPos))
		{
			if (Managers.Object.FindCollsion(destPos) == null)
			{
				CellPos = destPos;
			}
		}

		CheckUpdatedFlag();
	}

	protected override void CheckUpdatedFlag()
    {
		if(_updated)
        {
			C_Move movePacket = new C_Move();
			movePacket.PosInfo = PosInfo;
			Managers.Network.Send(movePacket);

			_updated = false;
		}
    }

	public void RefreshAdditionanlStat()
	{
		WeaponDamage = 0;
		ArmorDefence = 0;

		foreach (Item item in Managers.Inven.Items.Values)
		{
			if (item.Equipped == false)
				continue;

			switch (item.ItemType)
			{
				case ItemType.Weapon:
					WeaponDamage += ((Weapon)item).Damage;
					break;
				case ItemType.Armor:
					ArmorDefence += ((Armor)item).Defence;
					break;
			}
		}
	}

}
