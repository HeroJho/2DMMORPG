using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BanBanController : MonsterController
{
	protected Coroutine _coSkill;
	private UI_GameScene _gameSceneUI;

	[SerializeField]
	private GameObject WarningSkill_7;
	[SerializeField]
	private GameObject WarningSkill_10_Right;
	[SerializeField]
	private GameObject WarningSkill_10_Left;
	[SerializeField]
	private GameObject WarningSkill_10_Up;
	[SerializeField]
	private GameObject WarningSkill_10_Down;

    protected override void AddHpBar()
    {
		_gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		_gameSceneUI.BossHpUI.gameObject.SetActive(true);
	}

    protected override void UpdateHpBar()
    {
		if (_gameSceneUI == null)
			return;
		if (_gameSceneUI.BossHpUI == null && _gameSceneUI.BossHpUI.gameObject.activeSelf)
			return;

		float ratio = 0.0f;
		if (Stat.MaxHp > 0)
		{
			ratio = ((float)Hp / Stat.MaxHp);
		}

		_gameSceneUI.BossHpUI.SetHpBar(ratio);
	}

    public override void UseSkill(int skillId, int skillLevel)
    {
		if (_animator == null)
			return;

        switch (skillId)
        {
			case 0:
				_coSkill = StartCoroutine("CoStartMotionSkill_0", 0);
				break;
			case 1:
				_coSkill = StartCoroutine("CoStartMotionSkill_1", 0);
				break;
			case 5:
				_coSkill = StartCoroutine("CoStartMotionSkill_5", 0);
				break;
			case 7:
				_coSkill = StartCoroutine("CoStartMotionSkill_7", 0);
				break;
			case 10:
				MoveDir dir = (MoveDir)skillLevel;

				if(dir == MoveDir.Right)
					_coSkill = StartCoroutine("CoStartMotionSkill_10_Right", 0);
				else if(dir == MoveDir.Left)
					_coSkill = StartCoroutine("CoStartMotionSkill_10_Left", 0);
				else if (dir == MoveDir.Up)
					_coSkill = StartCoroutine("CoStartMotionSkill_10_Up", 0);
				else if (dir == MoveDir.Down)
					_coSkill = StartCoroutine("CoStartMotionSkill_10_Down", 0);
				break;
			default:
                break;
        }

    }

	IEnumerator CoStartMotionSkill_0(int skillLevel)
	{
		State = CreatureState.Skill; // 서버에서 허락을 맡음

		switch (Dir)
		{
			case MoveDir.Up:
				_animator.Play("ATTACK_BACK");
				_sprite.flipX = false;
				break;
			case MoveDir.Down:
				_animator.Play("ATTACK_FRONT");
				_sprite.flipX = false;
				break;
			case MoveDir.Left:
				_animator.Play("ATTACK_RIGHT");
				_sprite.flipX = true;
				break;
			case MoveDir.Right:
				_animator.Play("ATTACK_RIGHT");
				_sprite.flipX = false;
				break;
		}

		yield return new WaitForSeconds(2f);

		State = CreatureState.Idle;
		_coSkill = null;
	}

	IEnumerator CoStartMotionSkill_1(int skillLevel)
	{
		State = CreatureState.Skill; // 서버에서 허락을 맡음
		_animator.Play("SKILL_1");

		yield return new WaitForSeconds(2f);

		State = CreatureState.Idle;
		_coSkill = null;
	}

	IEnumerator CoStartMotionSkill_5(int skillLevel)
	{
		State = CreatureState.Skill; // 서버에서 허락을 맡음
		_animator.Play("SKILL_5");

		yield return new WaitForSeconds(1.8f);

		State = CreatureState.Idle;
		_coSkill = null;
	}

	IEnumerator CoStartMotionSkill_7(int skillLevel)
	{
		State = CreatureState.Skill; // 서버에서 허락을 맡음
		_animator.Play("SKILL_7");
		WarningSkill_7.SetActive(true);

		yield return new WaitForSeconds(1.8f);

		WarningSkill_7.SetActive(false);
		State = CreatureState.Idle;
		_coSkill = null;
	}

	IEnumerator CoStartMotionSkill_10_Right(int skillLevel)
	{
		State = CreatureState.Skill; // 서버에서 허락을 맡음
		_animator.Play("SKILL_7");
		WarningSkill_10_Right.SetActive(true);

		yield return new WaitForSeconds(1.8f);

		WarningSkill_10_Right.SetActive(false);
		State = CreatureState.Idle;
		_coSkill = null;
	}
	IEnumerator CoStartMotionSkill_10_Left(int skillLevel)
	{
		State = CreatureState.Skill; // 서버에서 허락을 맡음
		_animator.Play("SKILL_7");
		WarningSkill_10_Left.SetActive(true);

		yield return new WaitForSeconds(1.8f);

		WarningSkill_10_Left.SetActive(false);
		State = CreatureState.Idle;
		_coSkill = null;
	}
	IEnumerator CoStartMotionSkill_10_Up(int skillLevel)
	{
		State = CreatureState.Skill; // 서버에서 허락을 맡음
		_animator.Play("SKILL_7");
		WarningSkill_10_Up.SetActive(true);

		yield return new WaitForSeconds(1.8f);

		WarningSkill_10_Up.SetActive(false);
		State = CreatureState.Idle;
		_coSkill = null;
	}
	IEnumerator CoStartMotionSkill_10_Down(int skillLevel)
	{
		State = CreatureState.Skill; // 서버에서 허락을 맡음
		_animator.Play("SKILL_7");
		WarningSkill_10_Down.SetActive(true);

		yield return new WaitForSeconds(1.8f);

		WarningSkill_10_Down.SetActive(false);
		State = CreatureState.Idle;
		_coSkill = null;
	}

}
