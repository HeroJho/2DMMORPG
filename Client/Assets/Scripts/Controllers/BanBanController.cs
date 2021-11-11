using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BanBanController : MonsterController
{
	protected Coroutine _coSkill;

	[SerializeField]
	private GameObject WarningSkill_7;

	public override void UseSkill(int skillId, int skillLevel)
    {
		if (_animator == null)
			return;

        switch (skillId)
        {
            case 1:
				_coSkill = StartCoroutine("CoStartMotionSkill_1", 0);
				break;
			case 5:
				_coSkill = StartCoroutine("CoStartMotionSkill_5", 0);
				break;
			case 7:
				_coSkill = StartCoroutine("CoStartMotionSkill_7", 0);
				break;
			default:
                break;
        }

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

}
