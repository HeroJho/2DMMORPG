using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : CreatureController
{
	private MpBar _mpBar;
	protected Coroutine _coSkill;
	protected bool _rangedSkill = false;

	public int Mp
	{
        get { return Stat.Mp; }
		set
        {
			Stat.Mp = value;

			UpdateMpBar();
        }
	}

	protected int Level
    {
        get { return Stat.Level; }
        set { Stat.Level = value; }
    }

	protected virtual void AddMpBar()
	{
		GameObject go = Managers.Resource.Instantiate("UI/MpBar", transform);
		go.transform.localPosition = new Vector3(0, 0.4f, 0);
		go.name = "MpBar";
		_mpBar = go.GetComponent<MpBar>();
		UpdateMpBar();
	}

	protected virtual void UpdateMpBar()
	{
		if (_mpBar == null)
			return;

		float ratio = 0.0f;
		if (Stat.MaxMp > 0)
		{
			ratio = ((float)Mp / Stat.MaxMp);
		}

		_mpBar.SetMpBar(ratio);
	}

	protected override void Init()
    {
        base.Init();

		AddMpBar();
	}

	protected override void UpdateAnimation()
	{
		if (_animator == null || _sprite == null)
			return;

		Color color = _sprite.color;
		color.a = 1f;
		_sprite.color = color;

		if (State == CreatureState.Idle)
		{
			switch (Dir)
			{
				case MoveDir.Up:
					_animator.Play("IDLE_BACK");
					_sprite.flipX = false;
					break;
				case MoveDir.Down:
					_animator.Play("IDLE_FRONT");
					_sprite.flipX = false;
					break;
				case MoveDir.Left:
					_animator.Play("IDLE_RIGHT");
					_sprite.flipX = true;
					break;
				case MoveDir.Right:
					_animator.Play("IDLE_RIGHT");
					_sprite.flipX = false;
					break;
			}
		}
		else if (State == CreatureState.Moving)
		{
			switch (Dir)
			{
				case MoveDir.Up:
					_animator.Play("WALK_BACK");
					_sprite.flipX = false;
					break;
				case MoveDir.Down:
					_animator.Play("WALK_FRONT");
					_sprite.flipX = false;
					break;
				case MoveDir.Left:
					_animator.Play("WALK_RIGHT");
					_sprite.flipX = true;
					break;
				case MoveDir.Right:
					_animator.Play("WALK_RIGHT");
					_sprite.flipX = false;
					break;
			}
		}
		else if (State == CreatureState.Skill)
		{
			switch (Dir)
			{
				case MoveDir.Up:
					_animator.Play(_rangedSkill ? "ATTACK_WEAPON_BACK" : "ATTACK_BACK");
					_sprite.flipX = false;
					break;
				case MoveDir.Down:
					_animator.Play(_rangedSkill ? "ATTACK_WEAPON_FRONT" : "ATTACK_FRONT");
					_sprite.flipX = false;
					break;
				case MoveDir.Left:
					_animator.Play(_rangedSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK_RIGHT");
					_sprite.flipX = true;
					break;
				case MoveDir.Right:
					_animator.Play(_rangedSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK_RIGHT");
					_sprite.flipX = false;
					break;
			}
		}
		else if (State == CreatureState.Dead)
		{
			_animator.Play("IDLE_FRONT");
			color.a = 0.5f;
			_sprite.color = color;
		}
	}

	protected override void UpdateController()
	{

		base.UpdateController();
	}

	public override void UseSkill(int skillId)
    {
		if(skillId == 1001)
        {
			_coSkill = StartCoroutine("CoStartPunch");
        }
		else if(skillId == 1002)
        {
			_coSkill = StartCoroutine("CoStartShootArrow");
        }
		else if (skillId == 2001)
		{
			_coSkill = StartCoroutine("CoStartShootArrow");
		}
		else if(skillId == 2002)
        {
			_coSkill = StartCoroutine("CoStartExplosion");
        }
		else if(skillId == 2003)
        {
			_coSkill = StartCoroutine("CoStartPoisonSmokeReady");
        }
    }

	public virtual void LevelUp(int level)
    {
		Level = level;

		GameObject effect = Managers.Resource.Instantiate("Effect/LevelUpEffect");
		effect.transform.position = transform.position - new Vector3(0, 0.5f);
		effect.GetComponent<Animator>().Play("LEVELUP_START");
		GameObject.Destroy(effect, 2f);
	}

	protected virtual void CheckUpdatedFlag()
	{
		
	}

	IEnumerator CoStartPunch()
	{
		// 대기 시간
		_rangedSkill = false;
		State = CreatureState.Skill; // 서버에서 허락을 맡음
		yield return new WaitForSeconds(0.5f);
		State = CreatureState.Idle;
		_coSkill = null;

		CheckUpdatedFlag();
	}

    IEnumerator CoStartShootArrow()
    {
        _rangedSkill = true;
		State = CreatureState.Skill;
        yield return new WaitForSeconds(0.5f);
        State = CreatureState.Idle;
        _coSkill = null;

		CheckUpdatedFlag();
    }

	IEnumerator CoStartExplosion()
    {
		_rangedSkill = false;
		State = CreatureState.Skill; // 서버에서 허락을 맡음

		GameObject go = Managers.Resource.Instantiate("Effect/SkillEffect/ExplosionEffect");
		go.transform.position = transform.position;
		
		// 이펙트 크기 조정
		int skillLevel = Managers.Skill.GetSkillPoint(2002);
		Skill skillData = null;
		Managers.Data.SkillDict.TryGetValue(2002, out skillData);
		int radian = skillData.explosion.explosionPointInfos[skillLevel].radian;
		go.transform.localScale = new Vector3(
			radian,
			radian,
			1);

		go.GetComponent<Animator>().Play("EXPLOSION_START");
		Destroy(go, 1);

		yield return new WaitForSeconds(1f);

		State = CreatureState.Idle;
		_coSkill = null;

		CheckUpdatedFlag();
	}

	IEnumerator CoStartPoisonSmokeReady()
	{
		_rangedSkill = false;
		State = CreatureState.Skill; // 서버에서 허락을 맡음

		GameObject go = Managers.Resource.Instantiate("Effect/SkillEffect/PoisonSmokeReadyEffect");
		go.transform.position = transform.position;

		go.GetComponent<Animator>().Play("POISON_SMOKE_READY_EFFECT_START");
		Destroy(go, 2);

		yield return new WaitForSeconds(2f);

		State = CreatureState.Idle;
		_coSkill = null;

		CheckUpdatedFlag();
	}

	public override void OnDamaged()
	{
		Debug.Log("Player HIT !");
	}

	public override void OnDead()
	{
		base.OnDead();

	}
}
