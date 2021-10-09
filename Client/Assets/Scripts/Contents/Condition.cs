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

    private Animator _posionEffect;

    private float _attackSpeed = 0;

    public void Start()
    {
        _creatureController = GetComponent<CreatureController>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        // 일일히 크리쳐한테 파싱 ㄴㄴ
        _posionEffect = Managers.Resource.Instantiate("Effect/BuffEffect/Debuff_Poision", gameObject.transform).GetComponent<Animator>();

        _posionEffect.gameObject.SetActive(false);
    }

    public void UpdateCondition(S_ChangeConditionInfo changeConditionPacket)
    {
        int timeValue = changeConditionPacket.Time;
        ConditionType conditionType = changeConditionPacket.ConditionType;

        switch (conditionType)
        {
            case ConditionType.ConditionChilled:
                Chilled(changeConditionPacket.MoveSpeed, changeConditionPacket.AttackSpeed, timeValue);
                break;
            case ConditionType.ConditionPoison:
                break;
            case ConditionType.ConditionStun:
                break;
            default:
                break;
        }
    }

    Coroutine _chilledAnim = null;
    public void Chilled(float slowMoveValue, float slowAttackValue, int timeValue)
    {
        if(_chilledAnim != null)
        {
            StopCoroutine(_chilledAnim);
            _chilledAnim = null;
        }

        SlowSpeed(slowMoveValue, timeValue);
        SlowAttackSpeed(slowAttackValue, timeValue);

        // 이펙트
        _spriteRenderer.color = new Color(0, 255, 255);
        _animator.SetFloat("AttackSpeed", 0.5f);

        // 시간후에 원래속도 되돌림
        _chilledAnim = StartCoroutine(CoolTime(timeValue, () =>
        {
            _spriteRenderer.color = new Color(255, 255, 255);
            _animator.SetFloat("AttackSpeed", 1);
        }));
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
        if (_slowJob != null)
        {
            StopCoroutine(_slowJob);
            _creatureController.Speed = _originSpeed;
            _slowJob = null;
        }

        // 원래 속도 저장 후 감소
        _originSpeed = _creatureController.Speed;
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
        if (_slowAttackJob != null)
        {
            StopCoroutine(_slowAttackJob);
            _creatureController.Stat.AttackSpeed = _originAttackSpeed;
            _slowAttackJob = null;
        }

        _originAttackSpeed = _creatureController.Stat.AttackSpeed;
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

    public void Stun(int timeValue)
    {

    }

    public void BackCondition()
    { 
        
    }
}
