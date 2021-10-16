using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreatureController : BaseController
{
	HpBar _hpBar;
	Condition _condition;

	private TextMeshPro _nameBox = null;
	public string Name 
	{
		get
        {
			return Name;
        }
		set
        {
			if(_nameBox == null)
				_nameBox = Managers.Resource.Instantiate("UI/NameBox", gameObject.transform).GetComponentInChildren<TextMeshPro>();

			_nameBox.text = value;
        }
	}

	public CreatureController()
	{
		CanCollision = true;
	}

	public override StatInfo Stat
	{
		get { return _stat; }
		set
		{
			base.Stat = value;

			UpdateHpBar();
		}
	}

	public override int Hp
    {
        get { return Stat.Hp; }
		set
        {
			base.Hp = value;

			UpdateHpBar();
		}
    }

	protected override void Init()
    {
		base.Init();

		AddHpBar();
		_condition = GetComponent<Condition>();
    }

	protected virtual void AddHpBar()
    {
		GameObject go = Managers.Resource.Instantiate("UI/HpBar", transform);
		go.transform.localPosition = new Vector3(0, 0.5f, 0);
		go.name = "HpBar";
		_hpBar = go.GetComponent<HpBar>();
		UpdateHpBar();
    }

	protected virtual void UpdateHpBar()
    {
		if (_hpBar == null)
			return;

		float ratio = 0.0f;
		if(Stat.MaxHp > 0)
        {
			ratio = ((float)Hp / Stat.MaxHp);
        }

		_hpBar.SetHpBar(ratio);
    }

	protected override void UpdateDead()
	{

	}

	public virtual void OnDamaged()
	{

	}

	public virtual void OnDead()
    {
		// 모든 컨디션 초기화
		_condition.BackCondition();

		State = CreatureState.Dead;
		GameObject effect = Managers.Resource.Instantiate("Effect/DieEffect");
		effect.transform.position = transform.position;
		effect.GetComponent<Animator>().Play("START");
		GameObject.Destroy(effect, 0.5f);
	}

	public virtual void UseSkill(int skillId, int skillLevel)
	{
		
	}
}
