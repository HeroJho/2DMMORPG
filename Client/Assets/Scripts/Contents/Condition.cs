using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition : MonoBehaviour
{
    private CreatureController _creatureController;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private UI_GameScene _gameSceneUI;

    private Animator _posionEffect;
    private Animator _stunEffect;
    private Animator _healingEffect;
    private Animator _magicGuardEffect;
    private Animator _hyperBodyEffect;
    private Animator _ironBodyEffect;

    private float _attackSpeed = 0;

    bool _isInit = false;
    public void Start()
    {
        if (_isInit == true)
            return;

        _gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        _creatureController = GetComponent<CreatureController>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        // 일일히 크리쳐한테 파싱 ㄴㄴ
        _posionEffect = Managers.Resource.Instantiate("Effect/BuffEffect/Debuff_Poision", gameObject.transform).GetComponent<Animator>();
        _stunEffect = Managers.Resource.Instantiate("Effect/BuffEffect/Debuff_Stun", gameObject.transform).GetComponent<Animator>();
        _healingEffect = Managers.Resource.Instantiate("Effect/BuffEffect/Buff_Healing", gameObject.transform).GetComponent<Animator>();
        _magicGuardEffect = Managers.Resource.Instantiate("Effect/BuffEffect/Buff_MagicGuard", gameObject.transform).GetComponent<Animator>();
        _hyperBodyEffect = Managers.Resource.Instantiate("Effect/BuffEffect/Buff_HyperBody", gameObject.transform).GetComponent<Animator>();
        _ironBodyEffect = Managers.Resource.Instantiate("Effect/BuffEffect/Buff_IronBody", gameObject.transform).GetComponent<Animator>();


        _posionEffect.gameObject.SetActive(false);
        _stunEffect.gameObject.SetActive(false);
        _healingEffect.gameObject.SetActive(false);
        _magicGuardEffect.gameObject.SetActive(false);
        _hyperBodyEffect.gameObject.SetActive(false);
        _ironBodyEffect.gameObject.SetActive(false);
        _isInit = true;

        _originSpeed = _creatureController.Speed;
        _originAttackSpeed = _creatureController.Stat.AttackSpeed;
    }

    public void Init()
    {
        if (_isInit == true)
            return;

        _gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        _creatureController = GetComponent<CreatureController>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        // 일일히 크리쳐한테 파싱 ㄴㄴ
        _posionEffect = Managers.Resource.Instantiate("Effect/BuffEffect/Debuff_Poision", gameObject.transform).GetComponent<Animator>();
        _stunEffect = Managers.Resource.Instantiate("Effect/BuffEffect/Debuff_Stun", gameObject.transform).GetComponent<Animator>();
        _healingEffect = Managers.Resource.Instantiate("Effect/BuffEffect/Buff_Healing", gameObject.transform).GetComponent<Animator>();
        _magicGuardEffect = Managers.Resource.Instantiate("Effect/BuffEffect/Buff_MagicGuard", gameObject.transform).GetComponent<Animator>();
        _hyperBodyEffect = Managers.Resource.Instantiate("Effect/BuffEffect/Buff_HyperBody", gameObject.transform).GetComponent<Animator>();
        _ironBodyEffect = Managers.Resource.Instantiate("Effect/BuffEffect/Buff_IronBody", gameObject.transform).GetComponent<Animator>();


        _posionEffect.gameObject.SetActive(false);
        _stunEffect.gameObject.SetActive(false);
        _healingEffect.gameObject.SetActive(false);
        _magicGuardEffect.gameObject.SetActive(false);
        _hyperBodyEffect.gameObject.SetActive(false);
        _ironBodyEffect.gameObject.SetActive(false);
        _isInit = true;


        _originSpeed = _creatureController.Speed;
        _originAttackSpeed = _creatureController.Stat.AttackSpeed;
    }

    public void UpdateCondition(ConditionType conditionType, int objId, int skillId, int time, float attackSpeed, float moveSpeed)
    {
        int timeValue = time;

        switch (conditionType)
        {
            case ConditionType.ConditionChilled:
                Chilled(moveSpeed, attackSpeed, timeValue);
                break;
            case ConditionType.ConditionPoison:
                Poison(time);
                break;
            case ConditionType.ConditionStun:
                Stun(time);
                break;
            case ConditionType.ConditionHealing:
                Healing(time);
                break;
            case ConditionType.ConditionBuff:
                if(skillId == 2006)
                    MagicGuard(time);
                else if (skillId == 2007)
                    HyperBody(time);
                else if (skillId == 2008)
                    IronBody(time);

                // 버프UI는 내 플레어만 띄우면 됨
                if (Managers.Object.MyPlayer.Id == objId)
                    _gameSceneUI.BuffUI.AddBuff(skillId, time);
                break;
            default:
                break;
        }
    }

    Coroutine _chilledAnim = null;
    public void Chilled(float slowMoveValue, float slowAttackValue, int timeValue)
    {
        Init();

        if (_chilledAnim != null)
        {
            StopCoroutine(_chilledAnim);
            _chilledAnim = null;
        }

        SlowSpeed(slowMoveValue, timeValue);
        SlowAttackSpeed(slowAttackValue, timeValue);

        // 이펙트
        _spriteRenderer.color = new Color(0, 255, 255);
        if(_animator != null)
            _animator.SetFloat("AttackSpeed", 0.5f);

        // 시간후에 원래속도 되돌림
        _chilledAnim = StartCoroutine(CoolTime(timeValue, () =>
        {
            _spriteRenderer.color = new Color(255, 255, 255);
            if (_animator != null)
                _animator.SetFloat("AttackSpeed", 1);
        }));
    }

    Coroutine _poisonAnim = null;
    public void Poison(int timeValue)
    {
        Init();

        if (_poisonAnim != null)
        {
            StopCoroutine(_poisonAnim);
            _poisonAnim = null;
        }

        // 이펙트
        _spriteRenderer.color = new Color(0, 255, 255);
        _posionEffect.gameObject.SetActive(true);

        // 시간후에 원래속도 되돌림
        _poisonAnim = StartCoroutine(CoolTime(timeValue, () =>
        {
            _spriteRenderer.color = new Color(255, 255, 255);
            _posionEffect.gameObject.SetActive(false);
        }));
    }

    Coroutine _healingAnim = null;
    public void Healing(int timeValue)
    {
        Init();

        if (_healingAnim != null)
        {
            StopCoroutine(_healingAnim);
            _healingAnim = null;
        }

        // 이펙트
        _healingEffect.gameObject.SetActive(true);

        // 시간후에 원래속도 되돌림
        _healingAnim = StartCoroutine(CoolTime(timeValue, () =>
        {
            _healingEffect.gameObject.SetActive(false);
        }));
    }

    Coroutine _magicGuardAnim = null;
    public void MagicGuard(int timeValue)
    {
        Init();

        if (_magicGuardAnim != null)
        {
            StopCoroutine(_magicGuardAnim);
            _magicGuardAnim = null;
        }

        // 이펙트
        _magicGuardEffect.gameObject.SetActive(true);

        // 시간후에 원래속도 되돌림
        _magicGuardAnim = StartCoroutine(CoolTime(timeValue, () =>
        {
            _magicGuardEffect.gameObject.SetActive(false);
        }));
    }

    Coroutine _hyperBodyAnim = null;
    public void HyperBody(int timeValue)
    {
        Init();

        if (_hyperBodyAnim != null)
        {
            StopCoroutine(_hyperBodyAnim);
            _hyperBodyAnim = null;
        }

        // 이펙트
        _hyperBodyEffect.gameObject.SetActive(true);

        // 시간후에 원래속도 되돌림
        _hyperBodyAnim = StartCoroutine(CoolTime(timeValue, () =>
        {
            _hyperBodyEffect.gameObject.SetActive(false);
        }));
    }

    Coroutine _ironBodyAnim = null;
    public void IronBody(int timeValue)
    {
        Init();

        if (_ironBodyAnim != null)
        {
            StopCoroutine(_ironBodyAnim);
            _ironBodyAnim = null;
        }

        // 이펙트
        _ironBodyEffect.gameObject.SetActive(true);

        // 시간후에 원래속도 되돌림
        _ironBodyAnim = StartCoroutine(CoolTime(timeValue, () =>
        {
            _ironBodyEffect.gameObject.SetActive(false);
        }));
    }

    public void DeBuffAll()
    {
        if(_slowJob != null)
        {
            StopCoroutine(_slowJob);
            _creatureController.Speed = _originSpeed;
            _slowJob = null;
        }
        if (_slowAttackJob != null)
        {
            StopCoroutine(_slowAttackJob);
            _creatureController.Stat.AttackSpeed = _originAttackSpeed;
            _slowAttackJob = null;
        }
        if (_stunAnim != null)
        {
            StopCoroutine(_stunAnim);
            _stunAnim = null;
            _stunEffect.gameObject.SetActive(false);
            _creatureController.State = CreatureState.Idle;
        }
        if (_chilledAnim != null)
        {
            StopCoroutine(_chilledAnim);
            _chilledAnim = null;
            _spriteRenderer.color = new Color(255, 255, 255);
            if (_animator != null)
                _animator.SetFloat("AttackSpeed", 1);
        }
        if (_poisonAnim != null)
        {
            StopCoroutine(_poisonAnim);
            _poisonAnim = null;
            _spriteRenderer.color = new Color(255, 255, 255);
            _posionEffect.gameObject.SetActive(false);
        }

    }

    // 쿨타임 메서드
    IEnumerator CoolTime(int timeValue, Action func)
    {

        yield return new WaitForSeconds(timeValue);

        func.Invoke();
        _slowJob = null;
    }



    Coroutine _slowJob = null;
    float _originSpeed;
    public void SlowSpeed(float slowMoveValue, int timeValue)
    {
        Init();

        if (_slowJob != null)
        {
            StopCoroutine(_slowJob);
            _creatureController.Speed = _originSpeed;
            _slowJob = null;
        }

        // 원래 속도 저장 후 감소
        _creatureController.Speed = slowMoveValue;

        // 시간후에 원래속도 되돌림
        _slowJob = StartCoroutine(CoolTime(timeValue, () =>
        {
            _creatureController.Speed = _originSpeed;
        }));
    }

    Coroutine _slowAttackJob = null;
    float _originAttackSpeed;
    public void SlowAttackSpeed(float slowAttackValue, int timeValue)
    {
        Init();

        if (_slowAttackJob != null)
        {
            StopCoroutine(_slowAttackJob);
            _creatureController.Stat.AttackSpeed = _originAttackSpeed;
            _slowAttackJob = null;
        }

        _creatureController.Stat.AttackSpeed = slowAttackValue;

        // 시간후에 원래속도 되돌림
        _slowAttackJob = StartCoroutine(CoolTime(timeValue, () =>
        {
            _creatureController.Stat.AttackSpeed = _originAttackSpeed;
        }));

    }

    public void TickDamage(int damageValue, int timeValue)
    {

    }

    Coroutine _stunAnim = null;
    public void Stun(int timeValue)
    {
        Init();

        if (_stunAnim != null)
        {
            StopCoroutine(_stunAnim);
            _stunAnim = null;
            _creatureController.State = CreatureState.Idle;
        }

        _creatureController.State = CreatureState.Stun;

        // 이펙트
        _stunEffect.gameObject.SetActive(true);

        // 시간후에 원래속도 되돌림
        _stunAnim = StartCoroutine(CoolTime(timeValue, () =>
        {
            _stunEffect.gameObject.SetActive(false);
            _creatureController.State = CreatureState.Idle;
        }));
    }

    public void BackCondition()
    {
        if(_chilledAnim != null)
            StopCoroutine(_chilledAnim);
        if (_poisonAnim != null)
            StopCoroutine(_poisonAnim);
        if (_healingAnim != null)
            StopCoroutine(_healingAnim);
        if (_stunAnim != null)
            StopCoroutine(_stunAnim);
        if (_magicGuardAnim != null)
            StopCoroutine(_magicGuardAnim);
        if (_hyperBodyAnim != null)
            StopCoroutine(_hyperBodyAnim);
        if (_ironBodyAnim != null)
            StopCoroutine(_ironBodyAnim);
        _chilledAnim = null;
        _poisonAnim = null;
        _healingAnim = null;
        _stunAnim = null;
        _magicGuardAnim = null;
        _hyperBodyAnim = null;
        _ironBodyAnim = null;

        _spriteRenderer.color = new Color(255, 255, 255);
        _posionEffect.gameObject.SetActive(false);
        _stunEffect.gameObject.SetActive(false);
        _healingEffect.gameObject.SetActive(false);
        _magicGuardEffect.gameObject.SetActive(false);
        _hyperBodyEffect.gameObject.SetActive(false);
        _ironBodyEffect.gameObject.SetActive(false);


        if (_slowJob != null)
        {
            _creatureController.Speed = _originSpeed;
            StopCoroutine(_slowJob);
            _slowJob = null;
        }
        if (_slowAttackJob != null)
        {
            _creatureController.Stat.AttackSpeed = _originAttackSpeed;
            StopCoroutine(_slowAttackJob);
            _slowAttackJob = null;
        }

    }
}
