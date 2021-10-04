using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition : MonoBehaviour
{

    CreatureController _creatureController;
    SpriteRenderer _spriteRenderer;

    public void Start()
    {
        _creatureController = GetComponent<CreatureController>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void StartBuff(ConditionType type, int value, int time)
    {
        switch (type)
        {
            case ConditionType.ConditionChilled:
                SlowSpeed(value, time);
                break;
            case ConditionType.ConditionPoison:
                break;
            case ConditionType.ConditionStun:
                break;
        }
    }

    Coroutine _slowJob = null;
    float _originSpeed;
    public void SlowSpeed(int slowValue, int timeValue)
    {
        if (_slowJob != null)
            return;

        // 애니
        _spriteRenderer.color = new Color(0, 255, 255);

        // 원래 속도 저장 후 감소
        _originSpeed = _creatureController.Speed;
        _creatureController.Speed -= slowValue;

        // 시간후에 원래속도 되돌림
        _slowJob = StartCoroutine(CoolTime(timeValue, () =>
        {
            _creatureController.Speed = _originSpeed;
            _spriteRenderer.color = new Color(255, 255, 255);
        }));
    }
    IEnumerator CoolTime(int timeValue, Action func)
    {

        yield return new WaitForSeconds(timeValue);

        func.Invoke();
        _slowJob = null;
    }

    public void SlowAttackSpeed(int slowValue, int timeValue)
    {

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
