using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MyPlayerController : PlayerController
{
	bool _moveKeyPressed = false;
	int _currentSkill = 1;
	public bool CanUseSkill { get; set; }

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

		CanUseSkill = true;
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
	}

    protected override void UpdateController()
    {
		GetUIKeyInput();

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

		if (Input.GetKey(KeyCode.W))
		{
			Dir = MoveDir.Up;
		}
		else if (Input.GetKey(KeyCode.S))
		{
			Dir = MoveDir.Down;
		}
		else if (Input.GetKey(KeyCode.A))
		{
			Dir = MoveDir.Left;
		}
		else if (Input.GetKey(KeyCode.D))
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
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			_currentSkill = 1;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			_currentSkill = 2;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			_currentSkill = 3;
		}


		// 스킬 상태로 갈지 확인
		if (_coSkillCooltime == null && Input.GetKey(KeyCode.Space) && CanUseSkill)
		{
			C_Skill skill = new C_Skill() { Info = new SkillInfo() };
			skill.Info.SkillId = _currentSkill;
			Managers.Network.Send(skill);

			_coSkillCooltime = StartCoroutine("CoInputCooltime",
				Managers.Data.SkillDict[_currentSkill].cooldown);
		}

	}


	void GetKeyInput()
    {
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

    }

	void GetUIKeyInput()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
			UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
			UI_Inventory invenUI = gameSceneUI.InvenUI;

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
			UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
			UI_Stat statUI = gameSceneUI.StatUI;

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
	}

	Coroutine _coSkillCooltime;
	IEnumerator CoInputCooltime(float time)
    {
		yield return new WaitForSeconds(time);
		_coSkillCooltime = null;
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
			if (Managers.Object.FindCreature(destPos) == null)
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
